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
using OpenTrack.Core.Enums;

namespace OpenTrack.Infrastructure.Authorization;

/// <summary>
/// Server-side visibility filters applied to list queries. Both the web/EF data service and the
/// Web API call these, so the two surfaces enforce identical row-level security. The predicates
/// are written entirely in terms of local id lists (from <see cref="AccessSnapshot"/>) so EF Core
/// translates them to SQL rather than pulling rows into memory to filter.
/// </summary>
public static class VisibilityQueries
{
    /// <summary>Restricts a projects query to those the user may see (public, member, or global admin).</summary>
    public static IQueryable<Project> WhereVisibleTo(this IQueryable<Project> query, AccessSnapshot access)
    {
        if (access.IsGlobalAdmin)
            return query;

        var memberIds = access.MemberProjectIds;
        return query.Where(p => p.IsPublic || memberIds.Contains(p.Id));
    }

    /// <summary>
    /// Restricts an issues query to those the user may see: the containing project must be viewable,
    /// AND the issue must not be private unless the user is its reporter/assignee, has global
    /// Developer+, or has Developer+ membership on that project.
    /// </summary>
    public static IQueryable<Issue> WhereVisibleTo(this IQueryable<Issue> query, AccessSnapshot access)
    {
        if (access.IsGlobalAdmin)
            return query;

        var memberIds = access.MemberProjectIds;
        var developerProjectIds = access.DeveloperProjectIds;
        var globalDeveloperPlus = access.GlobalAtLeast(UserRole.Developer);
        var userId = access.UserId;

        return query.Where(i =>
            (i.Project.IsPublic || memberIds.Contains(i.ProjectId))
            && (!i.IsPrivate
                || globalDeveloperPlus
                || i.ReporterId == userId
                || i.AssigneeId == userId
                || developerProjectIds.Contains(i.ProjectId)));
    }
}
