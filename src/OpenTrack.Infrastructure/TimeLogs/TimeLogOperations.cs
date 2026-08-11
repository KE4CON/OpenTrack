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
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.TimeLogs;

public readonly record struct TimeLogItem(int Id, int Minutes, string? Note, DateTime WorkedOn, string AuthorName, int AuthorId);

/// <summary>
/// ACL-aware time/work-log operations on an issue, shared by the Web API and the web/EF data service.
/// Viewing entries requires view access to the issue; logging time requires edit access (Updater+);
/// deleting an entry is allowed to its author or to an Updater+ on the project.
/// </summary>
public static class TimeLogOperations
{
    public static async Task<IReadOnlyList<TimeLogItem>> ListForIssueAsync(
        AppDbContext db, AccessSnapshot access, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return [];
        return await db.TimeLogs.AsNoTracking()
            .Where(t => t.IssueId == issueId)
            .OrderByDescending(t => t.WorkedOn).ThenByDescending(t => t.Id)
            .Select(t => new TimeLogItem(t.Id, t.Minutes, t.Note, t.WorkedOn, t.User.UserName ?? "unknown", t.UserId))
            .ToListAsync(ct);
    }

    public static async Task<string?> AddAsync(
        AppDbContext db, AccessSnapshot access, int issueId, int minutes, string? note, DateTime? workedOn,
        CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return "Issue not found.";
        if (!access.For(issue.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("Logging time requires the Updater role on this project.");

        if (minutes <= 0) return "Enter how long you worked.";
        note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (note is { Length: > FieldLimits.TimeLogNote }) note = note[..FieldLimits.TimeLogNote];

        db.TimeLogs.Add(new TimeLog
        {
            IssueId = issueId,
            UserId = access.UserId,
            Minutes = minutes,
            Note = note,
            WorkedOn = workedOn ?? DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteAsync(AppDbContext db, AccessSnapshot access, int logId, CancellationToken ct = default)
    {
        var log = await db.TimeLogs.Include(t => t.Issue).ThenInclude(i => i.Project).FirstOrDefaultAsync(t => t.Id == logId, ct);
        if (log is null || !CanView(access, log.Issue)) return false;
        // The author can remove their own entry; an Updater+ can remove anyone's.
        if (log.UserId != access.UserId && !access.For(log.Issue.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("You can only remove your own time entries.");
        db.TimeLogs.Remove(log);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static bool CanView(AccessSnapshot access, Issue issue) =>
        access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId);
}
