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
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Dashboard;

public readonly record struct DashboardProjectTally(int ProjectId, string ProjectName, int OpenCount, int OverdueCount);
public readonly record struct DashboardSeverityTally(IssueSeverity Severity, int Count);
public readonly record struct DashboardRecentIssue(
    int Id, int ProjectId, string ProjectName, string Title,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    string ReporterName, string? AssigneeName, DateTime UpdatedAt);
public readonly record struct DashboardResult(
    int TotalOpen, int TotalOverdue,
    IReadOnlyList<DashboardProjectTally> Projects,
    IReadOnlyList<DashboardSeverityTally> OpenBySeverity,
    IReadOnlyList<DashboardRecentIssue> Recent);

/// <summary>
/// Builds the cross-project dashboard overview, shared by the web/EF data service and the Web API so
/// both compute identical numbers. Everything is scoped through <see cref="VisibilityQueries"/> first,
/// so a user never sees a tally or a row for an issue they couldn't open directly.
/// </summary>
public static class DashboardQuery
{
    // "Open" = any status below Resolved (i.e. not Resolved and not Closed). Inlined in the query
    // below as `(int)i.Status < (int)IssueStatus.Resolved` so EF Core can translate it to SQL.

    public static async Task<DashboardResult> BuildAsync(
        AppDbContext db, AccessSnapshot access, DateTime nowUtc, CancellationToken ct = default)
    {
        var visible = db.Issues.AsNoTracking().WhereVisibleTo(access);
        var open = visible.Where(i => (int)i.Status < (int)IssueStatus.Resolved);

        var projects = await open
            .GroupBy(i => new { i.ProjectId, ProjectName = i.Project.Name })
            .Select(g => new DashboardProjectTally(
                g.Key.ProjectId,
                g.Key.ProjectName,
                g.Count(),
                g.Count(i => i.DueDate != null && i.DueDate < nowUtc)))
            .ToListAsync(ct);
        var orderedProjects = projects
            .OrderByDescending(p => p.OverdueCount).ThenByDescending(p => p.OpenCount).ThenBy(p => p.ProjectName)
            .ToList();

        var bySeverity = await open
            .GroupBy(i => i.Severity)
            .Select(g => new DashboardSeverityTally(g.Key, g.Count()))
            .ToListAsync(ct);

        var recent = await visible
            .OrderByDescending(i => i.UpdatedAt)
            .Take(10)
            .Select(i => new DashboardRecentIssue(
                i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status, i.Severity, i.Priority,
                i.Reporter.UserName ?? "unknown", i.Assignee != null ? i.Assignee.UserName : null, i.UpdatedAt))
            .ToListAsync(ct);

        return new DashboardResult(
            orderedProjects.Sum(p => p.OpenCount),
            orderedProjects.Sum(p => p.OverdueCount),
            orderedProjects,
            bySeverity.OrderByDescending(s => (int)s.Severity).ToList(),
            recent);
    }
}
