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

using OpenTrack.Infrastructure.Activity;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Endpoints;

/// <summary>Cookie-authenticated change-token for the smart-poll auto-refresh (see auto-refresh.js).
/// Returns a tiny per-user token that changes when a visible issue is created/edited/deleted.</summary>
public static class ActivityWebEndpoints
{
    public static void MapActivityWebEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/activity/token", async (HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var identity = http.User.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var access = await AccessSnapshot.LoadAsync(db, identity.Value, ct);
            return Results.Text(await ActivityToken.ComputeAsync(db, access, ct));
        }).RequireAuthorization();
    }
}
