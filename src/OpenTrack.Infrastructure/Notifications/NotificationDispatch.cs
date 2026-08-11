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
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Authorization;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Email;

namespace OpenTrack.Infrastructure.Notifications;

/// <summary>
/// Delivers notifications for an issue change to its reporter, assignee, and explicit monitors (minus
/// the person who made the change). Crucially it re-checks each recipient's CURRENT view access to the
/// issue, so a monitor who has since lost access (e.g. the issue went private, or they were removed
/// from a private project) is dropped — the notification text contains the issue title and must never
/// leak to someone who can no longer see it. Writes in-app notification rows and, if a mail server is
/// configured, sends a best-effort email (a mail failure is logged, never thrown).
/// </summary>
public sealed class NotificationDispatch(IEmailService email, ILogger<NotificationDispatch> logger)
{
    /// <summary>Best-effort: notification delivery must never fail the change that triggered it, so the
    /// whole thing is wrapped and any error is logged rather than propagated to the caller.</summary>
    public async Task NotifyIssueChangedAsync(AppDbContext db, int actorUserId, int issueId, string summary, CancellationToken ct = default)
    {
        try
        {
            await DispatchAsync(db, actorUserId, issueId, summary, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to dispatch notifications for issue {IssueId}.", issueId);
        }
    }

    private async Task DispatchAsync(AppDbContext db, int actorUserId, int issueId, string summary, CancellationToken ct)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return;

        var candidateIds = new HashSet<int> { issue.ReporterId };
        if (issue.AssigneeId is { } assigneeId) candidateIds.Add(assigneeId);
        candidateIds.UnionWith(await db.IssueMonitors.AsNoTracking()
            .Where(m => m.IssueId == issueId).Select(m => m.UserId).ToListAsync(ct));
        candidateIds.Remove(actorUserId); // never notify the actor of their own change
        if (candidateIds.Count == 0) return;

        // Load each candidate's global role + email, and their membership role on this project.
        var users = await db.Users.AsNoTracking().Where(u => candidateIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Role, u.Email }).ToListAsync(ct);
        var memberRoles = await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.ProjectId == issue.ProjectId && candidateIds.Contains(m.UserId))
            .ToDictionaryAsync(m => m.UserId, m => m.Role, ct);

        var recipients = new List<(int Id, string? Email)>();
        foreach (var u in users)
        {
            var ctx = new AccessContext(u.Id, u.Role, memberRoles.TryGetValue(u.Id, out var role) ? role : (UserRole?)null);
            if (ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                recipients.Add((u.Id, u.Email));
        }
        if (recipients.Count == 0) return;

        var text = $"Issue #{issueId} — {issue.Title}: {summary}";
        foreach (var (uid, _) in recipients)
            db.Notifications.Add(new Notification { UserId = uid, IssueId = issueId, Text = text, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);

        if (email.IsConfigured)
        {
            foreach (var addr in recipients.Select(r => r.Email).Where(e => !string.IsNullOrWhiteSpace(e)))
                await email.SendAsync(addr!, $"[OpenTrack] Issue #{issueId} updated", text, ct);
        }
    }
}
