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
using OpenTrack.Core.Enums;
using OpenTrack.Core.Sla;
using OpenTrack.Infrastructure.Authorization;

namespace OpenTrack.Infrastructure.Sla;

/// <summary>One open issue that is at-risk or breached against its SLA target.</summary>
public sealed record SlaBoardRow(
    int Id, int ProjectId, string ProjectName, string Title, IssuePriority Priority,
    string? AssigneeName, DateTime CreatedAtUtc, DateTime DueUtc, SlaStatus Status);

/// <summary>Breached and at-risk open issues, breached first and each most-overdue-first.</summary>
public sealed record SlaBoardResult(IReadOnlyList<SlaBoardRow> Breached, IReadOnlyList<SlaBoardRow> AtRisk);

/// <summary>
/// Builds the SLA board over the issues a user may see: loads the open (clock-running) visible issues and
/// the SLA targets for their projects, then applies the pure <see cref="SlaCalculator"/> in memory (the
/// due-by math — created + target hours per priority — isn't worth expressing in SQL). The ACL is applied
/// first via <see cref="VisibilityQueries.WhereVisibleTo"/>, so the board can only ever show issues the
/// caller is already allowed to see.
/// </summary>
public static class SlaBoard
{
    public static async Task<SlaBoardResult> BuildAsync(
        Data.AppDbContext db, AccessSnapshot access, DateTime nowUtc, CancellationToken ct = default)
    {
        // Only open issues have a running clock.
        var open = await db.Issues.AsNoTracking()
            .WhereVisibleTo(access)
            .Where(i => (int)i.Status < (int)IssueStatus.Resolved)
            .Select(i => new
            {
                i.Id, i.ProjectId, ProjectName = i.Project.Name, i.Title, i.Priority,
                AssigneeName = i.Assignee != null ? i.Assignee.UserName : null,
                i.CreatedAt,
            })
            .ToListAsync(ct);
        if (open.Count == 0) return new SlaBoardResult([], []);

        // Load the targets for just the projects in play → dictionary keyed by (project, priority).
        var projectIds = open.Select(i => i.ProjectId).Distinct().ToList();
        var targets = await db.SlaPolicies.AsNoTracking()
            .Where(p => projectIds.Contains(p.ProjectId))
            .ToDictionaryAsync(p => (p.ProjectId, p.Priority), p => p.TargetHours, ct);

        var rows = new List<SlaBoardRow>();
        foreach (var i in open)
        {
            targets.TryGetValue((i.ProjectId, i.Priority), out var hours);
            var a = SlaCalculator.Evaluate(i.Priority, i.CreatedAt, isOpen: true, nowUtc, hours == 0 ? null : hours);
            if (a.Status is SlaStatus.AtRisk or SlaStatus.Breached && a.DueUtc is { } due)
                rows.Add(new SlaBoardRow(i.Id, i.ProjectId, i.ProjectName ?? "", i.Title, i.Priority,
                    i.AssigneeName, i.CreatedAt, due, a.Status));
        }

        return new SlaBoardResult(
            rows.Where(r => r.Status == SlaStatus.Breached).OrderBy(r => r.DueUtc).ToList(),
            rows.Where(r => r.Status == SlaStatus.AtRisk).OrderBy(r => r.DueUtc).ToList());
    }
}
