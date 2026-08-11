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

using System.Security.Claims;
using OpenTrack.Core.Enums;

namespace OpenTrack.Infrastructure.Authorization;

/// <summary>The signed-in user's id and global role, extracted from their claims.</summary>
public readonly record struct AccessIdentity(int UserId, UserRole GlobalRole);

public static class ClaimsPrincipalExtensions
{
    /// <summary>The claim key that carries the user's global OpenTrack role (see RoleClaimsPrincipalFactory).</summary>
    public const string RoleClaimType = "OpenTrack.Role";

    /// <summary>
    /// Reads the user id and global role from the principal. Returns null if the principal is not
    /// a valid signed-in OpenTrack user. A missing/unparseable role claim resolves to the LOWEST
    /// privilege (Viewer) — fail closed, never fail open.
    /// </summary>
    public static AccessIdentity? GetAccessIdentity(this ClaimsPrincipal user)
    {
        var idValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idValue is null || !int.TryParse(idValue, out var id))
            return null;

        var roleValue = user.FindFirst(RoleClaimType)?.Value;
        var role = Enum.TryParse<UserRole>(roleValue, out var parsed) ? parsed : UserRole.Viewer;
        return new AccessIdentity(id, role);
    }
}
