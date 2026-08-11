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
using OpenTrack.Infrastructure.Filters;

namespace OpenTrack.API.Endpoints;

/// <summary>The signed-in user's saved issue filters. Every query/mutation is scoped to the caller's
/// user id, so one user can never read or change another's.</summary>
public static class SavedFilterEndpoints
{
    public record SaveFilterRequest(string Name, string Query);

    public static void MapSavedFilterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/saved-filters").RequireAuthorization().WithTags("SavedFilters");

        group.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var items = await SavedFilterOperations.ListForUserAsync(db, identity.Value.UserId, ct);
            return Results.Ok(items.Select(f => new { f.Id, f.Name, f.Query }));
        });

        group.MapPost("/", async (SaveFilterRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var error = await SavedFilterOperations.SaveAsync(db, identity.Value.UserId, req.Name, req.Query, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var removed = await SavedFilterOperations.DeleteAsync(db, identity.Value.UserId, id, ct);
            return removed ? Results.NoContent() : Results.NotFound();
        });
    }
}
