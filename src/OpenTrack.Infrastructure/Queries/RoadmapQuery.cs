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

namespace OpenTrack.Infrastructure.Queries;

public readonly record struct RoadmapIssueRow(int Id, string Title, IssueStatus Status, IssueSeverity Severity, bool Done);
public readonly record struct RoadmapVersionRow(
    int VersionId, string Name, bool IsReleased, DateTime? ReleaseDate, int Total, int Done, IReadOnlyList<RoadmapIssueRow> Issues);

/// <summary>
/// Builds a project's roadmap and changelog from its versions and the issues targeted to be fixed in
/// each (Issue.FixVersionId). Un-released versions become the roadmap (with progress); released versions
/// become the changelog. ACL first: only issues the caller may see are counted or listed, and a project
/// the caller can't view yields nothing. Shared by the Web API and the web/EF data service.
/// </summary>
public static class RoadmapQuery
{
    public static async Task<IReadOnlyList<RoadmapVersionRow>> BuildAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return [];

        var versions = await db.Versions.AsNoTracking()
            .Where(v => v.ProjectId == projectId)
            .Select(v => new { v.Id, v.Name, v.IsReleased, v.ReleaseDate })
            .ToListAsync(ct);
        if (versions.Count == 0) return [];

        var issues = await db.Issues.AsNoTracking().WhereVisibleTo(access)
            .Where(i => i.ProjectId == projectId && i.FixVersionId != null)
            .Select(i => new { i.Id, i.Title, i.Status, i.Severity, FixVersionId = i.FixVersionId!.Value })
            .ToListAsync(ct);

        var byVersion = issues.ToLookup(i => i.FixVersionId);

        return versions
            .Select(v =>
            {
                var rows = byVersion[v.Id]
                    .OrderBy(i => i.Id)
                    .Select(i => new RoadmapIssueRow(i.Id, i.Title, i.Status, i.Severity, (int)i.Status >= (int)IssueStatus.Resolved))
                    .ToList();
                return new RoadmapVersionRow(v.Id, v.Name, v.IsReleased, v.ReleaseDate,
                    rows.Count, rows.Count(r => r.Done), rows);
            })
            // Roadmap (unreleased) first, then changelog (released) newest-first.
            .OrderBy(v => v.IsReleased)
            .ThenByDescending(v => v.ReleaseDate ?? DateTime.MinValue)
            .ThenBy(v => v.Name)
            .ToList();
    }
}
