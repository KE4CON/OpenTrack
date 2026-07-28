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

using Microsoft.AspNetCore.Authorization;
using OpenTrack.Core.Enums;

namespace OpenTrack.API;

/// <summary>Policy name constants for the API, matching the web/desktop policy names.</summary>
public static class AuthorizationPolicies
{
    public const string RequireUpdater = "RequireUpdater";
    public const string RequireDeveloper = "RequireDeveloper";
    public const string RequireManager = "RequireManager";
    public const string RequireAdministrator = "RequireAdministrator";
}

/// <summary>Shared role-threshold check used by the API's authorization policies.</summary>
public static class ApiRoleCheck
{
    public static bool HasRoleAtLeast(AuthorizationHandlerContext ctx, UserRole minimum)
    {
        var roleClaim = ctx.User.FindFirst("OpenTrack.Role")?.Value;
        return roleClaim is not null
            && Enum.TryParse<UserRole>(roleClaim, out var role)
            && (int)role >= (int)minimum;
    }
}
