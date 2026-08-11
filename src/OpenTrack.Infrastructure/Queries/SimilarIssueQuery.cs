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
using OpenTrack.Core.Text;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Queries;

public readonly record struct SimilarIssue(int Id, int ProjectId, string ProjectName, string Title, IssueStatus Status);

/// <summary>
/// Finds likely-duplicate issues for a proposed title. ACL first (only issues the user could open are
/// ever considered), then a coarse word-overlap match ranked in memory. Shared by the Web API and the
/// web/EF data service.
/// </summary>
public static class SimilarIssueQuery
{
    public static async Task<IReadOnlyList<SimilarIssue>> FindAsync(
        AppDbContext db, AccessSnapshot access, int? projectId, string? title, int? excludeIssueId,
        int take = 5, CancellationToken ct = default)
    {
        var words = IssueSimilarity.SignificantWords(title).Take(6).ToList();
        if (words.Count == 0) return [];

        var baseQuery = db.Issues.AsNoTracking().WhereVisibleTo(access);
        if (projectId is { } pid) baseQuery = baseQuery.Where(i => i.ProjectId == pid);
        if (excludeIssueId is { } ex) baseQuery = baseQuery.Where(i => i.Id != ex);

        // Union a "title contains this word" query per significant word (EF translates each to a LIKE).
        var candidates = words
            .Select(w => baseQuery.Where(i => i.Title.ToLower().Contains(w)))
            .Aggregate((a, b) => a.Union(b));

        var rows = await candidates
            .Select(i => new SimilarIssue(i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status))
            .ToListAsync(ct);

        // Rank by how many significant words the titles share; keep the strongest few.
        return rows
            .Select(r => (Row: r, Score: IssueSimilarity.Overlap(title, r.Title)))
            .Where(x => x.Score >= 1)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Row.Id)
            .Take(take)
            .Select(x => x.Row)
            .ToList();
    }
}
