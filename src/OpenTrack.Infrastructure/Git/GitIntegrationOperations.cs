// OpenTrack — open-source issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version. This program is distributed WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License <https://www.gnu.org/licenses/> for
// more details.

using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Core.Git;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Notifications;
using OpenTrack.Infrastructure.Workflow;

namespace OpenTrack.Infrastructure.Git;

/// <summary>One commit extracted from a push webhook payload.</summary>
public sealed record GitCommitInfo(string Sha, string Message, string? Url, string? Author, DateTime CommittedAt);

/// <summary>How many links were created and issues auto-resolved by one push.</summary>
public sealed record GitPushResult(int Linked, int Resolved);

/// <summary>
/// Manager-gated Git-integration config (mirrors the other per-project operations classes) plus the
/// webhook processing that links commits to issues and optionally auto-resolves them. Processing is a
/// trusted system action (the webhook was signature-verified), so it has no signed-in user — history/notify
/// are attributed to the project owner / the "system" actor id 0.
/// </summary>
public static class GitIntegrationOperations
{
    // ---- Manager-gated config ----

    public static async Task<GitIntegration?> GetForProjectAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject()) return null;
        return await db.GitIntegrations.AsNoTracking().FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
    }

    public static async Task<string?> SetAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId,
        bool enabled, string? webhookSecret, bool autoResolve, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing Git integration requires the Manager role on this project.");
        if (enabled && string.IsNullOrWhiteSpace(webhookSecret))
            return "A webhook secret is required to enable Git integration.";

        var config = await db.GitIntegrations.FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
        if (config is null)
        {
            config = new GitIntegration { ProjectId = projectId };
            db.GitIntegrations.Add(config);
        }
        config.Enabled = enabled;
        // Store the secret EXACTLY as entered (no Trim): GitHub signs over the exact bytes you paste into
        // its webhook Secret field, so trimming here would silently break verification for a padded secret.
        config.WebhookSecret = string.IsNullOrWhiteSpace(webhookSecret) ? null : webhookSecret;
        config.AutoResolve = autoResolve;
        await db.SaveChangesAsync(ct);
        return null;
    }

    // ---- Per-issue commit list (ACL: caller must be able to view the issue) ----

    public static async Task<IReadOnlyList<IssueCommitLink>> ListForIssueAsync(
        Data.AppDbContext db, AccessSnapshot access, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return [];
        if (!access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return [];
        return await db.IssueCommitLinks.AsNoTracking()
            .Where(l => l.IssueId == issueId)
            .OrderByDescending(l => l.CommittedAt)
            .ToListAsync(ct);
    }

    // ---- Webhook processing (system actor, no ACL — the signature was already verified) ----

    public static async Task<GitPushResult> ProcessPushAsync(
        Data.AppDbContext db, NotificationDispatch notifications, int projectId,
        IReadOnlyList<GitCommitInfo> commits, CancellationToken ct = default)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return new GitPushResult(0, 0);
        var autoResolve = await db.GitIntegrations
            .Where(g => g.ProjectId == projectId).Select(g => g.AutoResolve).FirstOrDefaultAsync(ct);

        var now = DateTime.UtcNow;
        var linked = 0;
        var resolvedIssueIds = new List<int>();
        var seen = new HashSet<(int IssueId, string Sha)>();

        foreach (var c in commits)
        {
            var refs = GitRefParser.Parse(c.Message);
            if (refs.Count == 0) continue;
            var firstLine = FirstLine(c.Message);

            foreach (var r in refs)
            {
                // The referenced issue must belong to THIS project.
                var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == r.IssueId && i.ProjectId == projectId, ct);
                if (issue is null) continue;

                // Consider a (issue, sha) already linked if it's in the DB OR was added earlier in THIS
                // push (the change tracker isn't visible to AnyAsync) — prevents a crafted payload with a
                // duplicate commit from violating the unique index and aborting the whole push.
                var key = (issue.Id, c.Sha);
                var alreadyLinked = seen.Contains(key)
                    || await db.IssueCommitLinks.AnyAsync(l => l.IssueId == issue.Id && l.Sha == c.Sha, ct);
                var newlyLinked = false;
                if (!alreadyLinked)
                {
                    db.IssueCommitLinks.Add(new IssueCommitLink
                    {
                        IssueId = issue.Id, Sha = c.Sha, Message = firstLine,
                        Author = c.Author, Url = c.Url, Closing = r.Closing, CommittedAt = c.CommittedAt,
                    });
                    seen.Add(key);
                    linked++;
                    newlyLinked = true;
                }

                // Auto-resolve only for a NEWLY linked closing reference on an open issue the workflow
                // permits — gating on newlyLinked means a replayed (already-linked) push can't re-resolve a
                // manually reopened issue.
                if (r.Closing && newlyLinked && autoResolve && (int)issue.Status < (int)IssueStatus.Resolved
                    && !resolvedIssueIds.Contains(issue.Id)
                    && await WorkflowOperations.IsAllowedAsync(db, projectId, issue.Status, IssueStatus.Resolved, ct))
                {
                    var old = issue.Status;
                    issue.Status = IssueStatus.Resolved;
                    issue.UpdatedAt = now;
                    issue.History.Add(new IssueHistory
                    {
                        UserId = project.OwnerId, FieldChanged = "Status",
                        OldValue = old.ToString(), NewValue = IssueStatus.Resolved.ToString(), ChangedAt = now,
                    });
                    resolvedIssueIds.Add(issue.Id);
                }
            }
        }

        if (linked == 0 && resolvedIssueIds.Count == 0) return new GitPushResult(0, 0);
        await db.SaveChangesAsync(ct);

        // Notify watchers of each auto-resolve (best-effort; actor 0 = system, so everyone is notified).
        foreach (var id in resolvedIssueIds)
            await notifications.NotifyIssueChangedAsync(db, 0, id, "resolved automatically by a linked commit", ct);

        return new GitPushResult(linked, resolvedIssueIds.Count);
    }

    private static string FirstLine(string message)
    {
        var line = (message ?? "").Split('\n', 2)[0].Trim();
        return line.Length > 500 ? line[..500] : line;
    }
}
