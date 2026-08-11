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
using OpenTrack.Core.Authorization;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Authorization;

/// <summary>
/// A signed-in user's access state, loaded once per request: their global role plus every
/// project they are a member of and the role they hold there. From this we can (a) build an
/// <see cref="AccessContext"/> for any single project via <see cref="For"/>, and (b) feed the
/// pre-computed id lists into the <see cref="VisibilityQueries"/> filters so list endpoints
/// filter in the database instead of in memory.
/// </summary>
public sealed class AccessSnapshot
{
    public int UserId { get; }
    public UserRole GlobalRole { get; }

    private readonly IReadOnlyDictionary<int, UserRole> _projectRoles;

    /// <summary>Ids of every project the user is a member of (any role).</summary>
    public IReadOnlyList<int> MemberProjectIds { get; }

    /// <summary>Ids of projects where the user's membership role alone grants Developer+ (can see private issues).</summary>
    public IReadOnlyList<int> DeveloperProjectIds { get; }

    public bool IsGlobalAdmin => (int)GlobalRole >= (int)UserRole.Administrator;
    public bool GlobalAtLeast(UserRole role) => (int)GlobalRole >= (int)role;

    private AccessSnapshot(int userId, UserRole globalRole, IReadOnlyDictionary<int, UserRole> projectRoles)
    {
        UserId = userId;
        GlobalRole = globalRole;
        _projectRoles = projectRoles;
        MemberProjectIds = projectRoles.Keys.ToList();
        DeveloperProjectIds = projectRoles
            .Where(kv => (int)kv.Value >= (int)UserRole.Developer)
            .Select(kv => kv.Key)
            .ToList();
    }

    /// <summary>Builds the pure per-project decision context for one project.</summary>
    public AccessContext For(int projectId) =>
        new(UserId, GlobalRole, _projectRoles.TryGetValue(projectId, out var role) ? role : null);

    public static async Task<AccessSnapshot> LoadAsync(
        AppDbContext db, AccessIdentity identity, CancellationToken ct = default)
    {
        var memberships = await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.UserId == identity.UserId)
            .Select(m => new { m.ProjectId, m.Role })
            .ToListAsync(ct);

        var roles = memberships.ToDictionary(m => m.ProjectId, m => m.Role);
        return new AccessSnapshot(identity.UserId, identity.GlobalRole, roles);
    }
}
