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

using OpenTrack.Core.Entities;
using OpenTrack.Core.Querying;

namespace OpenTrack.Infrastructure.Queries;

/// <summary>
/// Applies an <see cref="IssueFilter"/> (filtering + sorting) to an issue query. Both the Web API and
/// the web/EF data service call this, so search behaves identically on both surfaces and can't drift.
/// Call it AFTER <c>WhereVisibleTo</c> so filtering only ever narrows the rows a user may already see.
/// Sticky issues are always pinned first, then the chosen sort.
/// </summary>
public static class IssueQueries
{
    public static IQueryable<Issue> ApplyFilter(this IQueryable<Issue> query, IssueFilter filter)
    {
        if (filter.ProjectId is { } projectId) query = query.Where(i => i.ProjectId == projectId);
        if (filter.Status is { } status) query = query.Where(i => i.Status == status);
        if (filter.Severity is { } severity) query = query.Where(i => i.Severity == severity);
        if (filter.Priority is { } priority) query = query.Where(i => i.Priority == priority);
        if (filter.AssigneeId is { } assigneeId) query = query.Where(i => i.AssigneeId == assigneeId);
        if (filter.CategoryId is { } categoryId) query = query.Where(i => i.CategoryId == categoryId);
        if (filter.TagId is { } tagId) query = query.Where(i => i.IssueTags.Any(it => it.TagId == tagId));

        if (!string.IsNullOrWhiteSpace(filter.Text))
        {
            // Case-insensitive contains: EF translates string.Contains to a case-SENSITIVE match on
            // SQLite (instr), so lower-case both sides to get the search behaviour users expect.
            var text = filter.Text.Trim().ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(text) || i.Description.ToLower().Contains(text));
        }

        // Sticky first, then the requested order (with a stable UpdatedAt tie-break where relevant).
        return filter.Sort switch
        {
            IssueSort.UpdatedAsc => query.OrderByDescending(i => i.IsSticky).ThenBy(i => i.UpdatedAt),
            IssueSort.CreatedDesc => query.OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.CreatedAt),
            IssueSort.CreatedAsc => query.OrderByDescending(i => i.IsSticky).ThenBy(i => i.CreatedAt),
            IssueSort.PriorityDesc => query.OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.Priority).ThenByDescending(i => i.UpdatedAt),
            IssueSort.SeverityDesc => query.OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.Severity).ThenByDescending(i => i.UpdatedAt),
            IssueSort.StatusAsc => query.OrderByDescending(i => i.IsSticky).ThenBy(i => i.Status).ThenByDescending(i => i.UpdatedAt),
            IssueSort.IdDesc => query.OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.Id),
            IssueSort.IdAsc => query.OrderByDescending(i => i.IsSticky).ThenBy(i => i.Id),
            _ => query.OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.UpdatedAt), // UpdatedDesc (default)
        };
    }
}
