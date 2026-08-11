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
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Reporting;

namespace OpenTrack.API.Endpoints;

/// <summary>Reporting figures over the issues the caller may see (ACL enforced by ReportQuery).</summary>
public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports", async (int? projectId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var r = await ReportQuery.BuildAsync(db, access, projectId, DateTime.UtcNow, ct);
            return Results.Ok(r);
        }).RequireAuthorization().WithTags("Reports");
    }
}
