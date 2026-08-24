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
using OpenTrack.Core.Text;
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Intake;

public readonly record struct IntakeResult(int? IssueId, string? Error);
public readonly record struct IntakeStatus(int IssueId, string Title, IssueStatus Status, string? Key);

/// <summary>
/// The public trouble-ticket intake — the one issue-creation path that is NOT authenticated. It is
/// gated instead by the project's <see cref="Project.PublicIntakeEnabled"/> flag (off by default; a
/// Manager turns it on). A submission becomes a normal issue attributed to the project owner, with the
/// submitter's name/email captured; that email later lets the submitter look up just their own ticket's
/// status. Length caps and the project gate live here; the web layer adds rate-limiting and a honeypot.
/// </summary>
public static class PublicIntakeOperations
{
    public static async Task<IntakeResult> SubmitAsync(
        AppDbContext db, int projectId, string? name, string? email, string? title, string? description,
        CancellationToken ct = default)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !project.PublicIntakeEnabled)
            return new IntakeResult(null, "This project isn't accepting public submissions.");

        title = title?.Trim() ?? "";
        if (title.Length == 0) return new IntakeResult(null, "Please describe the problem in the summary.");
        title = Truncate(title, FieldLimits.IssueTitle);

        name = Blank(name) ? null : Truncate(name!.Trim(), FieldLimits.IntakeName);
        // Cap the free-text description — this is an UNAUTHENTICATED endpoint, so an anonymous submitter
        // could otherwise store a multi-megabyte body (bounded only by Kestrel's request limit).
        description = Blank(description) ? null : Truncate(description!, FieldLimits.IntakeDescription);
        email = Blank(email) ? null : email!.Trim();
        if (email is not null && (!email.Contains('@') || email.Length > FieldLimits.IntakeEmail))
            return new IntakeResult(null, "Enter a valid email address, or leave it blank.");

        var issue = new Issue
        {
            ProjectId = projectId,
            Title = title,
            Description = BuildDescription(name, email, description),
            ReporterId = project.OwnerId, // a valid reporter; the real submitter is captured below
            Status = IssueStatus.New,
            Severity = IssueSeverity.Minor,
            Priority = IssuePriority.Normal,
            Reproducibility = IssueReproducibility.HaveNotTried,
            IntakeName = name,
            IntakeEmail = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        issue.History.Add(new IssueHistory
        {
            UserId = project.OwnerId,
            FieldChanged = "Status",
            OldValue = null,
            NewValue = IssueStatus.New.ToString(),
            ChangedAt = DateTime.UtcNow,
        });
        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct);
        return new IntakeResult(issue.Id, null);
    }

    /// <summary>
    /// Create a ticket from an inbound email, routed to a project by its <c>Key</c> (derived from the
    /// recipient address, e.g. <c>tickets+APRS@…</c> → the "APRS" project). Only projects that have public
    /// intake enabled are eligible, so email intake can never reach a project that isn't already accepting
    /// public tickets. Reuses <see cref="SubmitAsync"/> so the same caps and description formatting apply.
    /// </summary>
    public static async Task<IntakeResult> SubmitByProjectKeyAsync(
        AppDbContext db, string? projectKey, string? name, string? email, string? title, string? description,
        CancellationToken ct = default)
    {
        var key = TicketNumber.NormalizeKey(projectKey);
        if (key is null)
            return new IntakeResult(null, "Could not tell which project this email is for (no project key in the address).");

        var projectId = await db.Projects.AsNoTracking()
            .Where(p => p.Key != null && p.Key.ToUpper() == key && p.PublicIntakeEnabled)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(ct);
        if (projectId is null)
            return new IntakeResult(null, "No project is accepting email tickets for that address.");

        return await SubmitAsync(db, projectId.Value, name, email, title, description, ct);
    }

    /// <summary>Look up a submitted ticket's status. Requires the reference AND the exact email used, so
    /// one submitter can't read another's ticket, and it only ever returns public-intake issues.</summary>
    public static async Task<IntakeStatus?> LookupAsync(AppDbContext db, int reference, string? email, CancellationToken ct = default)
    {
        if (Blank(email)) return null;
        var e = email!.Trim();
        var row = await db.Issues.AsNoTracking()
            .Where(i => i.Id == reference && i.IntakeEmail != null && i.IntakeEmail.ToLower() == e.ToLower())
            .Select(i => new { i.Id, i.Title, i.Status, ProjectKey = i.Project.Key })
            .FirstOrDefaultAsync(ct);
        return row is null ? null : new IntakeStatus(row.Id, row.Title, row.Status, row.ProjectKey);
    }

    private static string BuildDescription(string? name, string? email, string? description)
    {
        var parts = new List<string> { "*Submitted via the public “Report a problem” form.*" };
        var who = string.Join(" ", new[] { name, email is null ? null : $"<{email}>" }.Where(s => !string.IsNullOrEmpty(s)));
        if (who.Length > 0) parts.Add($"Reported by: {who}");
        parts.Add(Blank(description) ? "(No further details were provided.)" : description!.Trim());
        return string.Join("\n\n", parts);
    }

    private static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);
    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
