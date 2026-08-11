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
using OpenTrack.Core.Bulk;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Notifications;
using OpenTrack.Infrastructure.Tags;

namespace OpenTrack.Infrastructure.Bulk;

/// <summary>
/// Applies one action to many issues, enforcing the SAME per-issue authorization as a single edit:
/// an issue the caller can't see, or can't perform this action on, is silently SKIPPED (never a
/// partial leak or a blanket allow). Shared by the Web API and the web/EF data service. Each changed
/// issue records history and fires a notification, exactly like an individual edit.
/// </summary>
public static class BulkOperations
{
    public static async Task<BulkResult> ApplyAsync(
        AppDbContext db, AccessSnapshot access, IReadOnlyCollection<int> issueIds, BulkAction action,
        NotificationDispatch notifications, CancellationToken ct = default)
    {
        int updated = 0, skipped = 0;

        foreach (var id in issueIds.Distinct())
        {
            var issue = await db.Issues.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) { skipped++; continue; }
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            { skipped++; continue; }

            var originalStatus = issue.Status;
            var originalAssignee = issue.AssigneeId;
            string? summary = null;

            switch (action.Type)
            {
                case BulkActionType.SetStatus when action.Status is { } s:
                    if (!ctx.CanEditIssue()) { skipped++; continue; }
                    issue.Status = s; summary = $"status set to {s}";
                    break;
                case BulkActionType.Close:
                    if (!ctx.CanEditIssue()) { skipped++; continue; }
                    issue.Status = IssueStatus.Closed; summary = "closed";
                    break;
                case BulkActionType.Assign when action.AssigneeId is { } aid:
                    if (!ctx.CanAssignIssue()) { skipped++; continue; }
                    if (!await db.ProjectMemberships.AsNoTracking().AnyAsync(m => m.ProjectId == issue.ProjectId && m.UserId == aid, ct))
                    { skipped++; continue; } // assignee must be a member of the issue's project
                    issue.AssigneeId = aid; summary = "reassigned";
                    break;
                case BulkActionType.Unassign:
                    if (!ctx.CanAssignIssue()) { skipped++; continue; }
                    issue.AssigneeId = null; summary = "unassigned";
                    break;
                case BulkActionType.AddTag when !string.IsNullOrWhiteSpace(action.Tag):
                    if (!ctx.CanEditIssue()) { skipped++; continue; }
                    // Reuse the tag ACL/create-on-assign logic; it re-checks edit and is idempotent.
                    try { await TagOperations.AddAsync(db, access, id, action.Tag, ct); }
                    catch (UnauthorizedAccessException) { skipped++; continue; }
                    updated++;
                    continue; // tag add saves itself and doesn't need issue history/notification here
                default:
                    skipped++; continue; // nothing to do / missing value for the action
            }

            issue.UpdatedAt = DateTime.UtcNow;
            if (issue.Status != originalStatus)
                issue.History.Add(new IssueHistory { UserId = access.UserId, FieldChanged = "Status", OldValue = originalStatus.ToString(), NewValue = issue.Status.ToString(), ChangedAt = DateTime.UtcNow });
            if (issue.AssigneeId != originalAssignee)
                issue.History.Add(new IssueHistory { UserId = access.UserId, FieldChanged = "Assignee", OldValue = originalAssignee?.ToString(), NewValue = issue.AssigneeId?.ToString(), ChangedAt = DateTime.UtcNow });

            await db.SaveChangesAsync(ct);
            if (summary is not null)
                await notifications.NotifyIssueChangedAsync(db, access.UserId, id, summary, ct);
            updated++;
        }

        return new BulkResult(updated, skipped);
    }
}
