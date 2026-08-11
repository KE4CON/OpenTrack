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
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Preferences;

namespace OpenTrack.API.Endpoints;

/// <summary>The signed-in user's personal preferences, scoped to their own user id.</summary>
public static class PreferenceEndpoints
{
    public record SavePreferencesRequest(int? DefaultProjectId, IssueSort? DefaultSort);

    public static void MapPreferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/preferences").RequireAuthorization().WithTags("Preferences");

        group.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var p = await UserPreferenceOperations.GetAsync(db, identity.Value.UserId, ct);
            return Results.Ok(new { p.DefaultProjectId, p.DefaultSort });
        });

        group.MapPut("/", async (SavePreferencesRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            await UserPreferenceOperations.SaveAsync(db, identity.Value.UserId, req.DefaultProjectId, req.DefaultSort, ct);
            return Results.NoContent();
        });
    }
}
