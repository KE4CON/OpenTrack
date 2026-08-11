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
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Dashboard;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Endpoints;

/// <summary>The cross-project dashboard overview. Aggregation and its row-level ACL live in the shared
/// <see cref="DashboardQuery"/>, so the API and the web host return identical numbers.</summary>
public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").RequireAuthorization().WithTags("Dashboard");

        group.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var result = await DashboardQuery.BuildAsync(db, access, DateTime.UtcNow, ct);
            return Results.Ok(result);
        });
    }
}
