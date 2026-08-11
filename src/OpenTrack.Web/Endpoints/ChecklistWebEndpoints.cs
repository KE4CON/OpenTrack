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

using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Checklist;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// A cookie-authenticated, same-origin endpoint that sets one checklist item's status. The bug-hunt
/// checklist page normally posts its Pass/Fail/N-A via static-SSR forms; this endpoint is what the
/// offline layer (checklist-offline.js) replays queued changes to when the tablet comes back online.
/// It delegates to the same shared, ACL-checked ChecklistOperations, so an offline replay can't do
/// anything the online form couldn't. Antiforgery is enforced (the JS sends the page's token).
/// </summary>
public static class ChecklistWebEndpoints
{
    public static void MapChecklistWebEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/checklist/{projectId:int}/{itemId:int}/status",
            async (int projectId, int itemId, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var identity = http.User.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();

            var form = await http.Request.ReadFormAsync(ct);
            if (!Enum.TryParse<ChecklistItemStatus>(form["status"], out var status))
                return Results.BadRequest("Unknown status.");

            var access = await AccessSnapshot.LoadAsync(db, identity.Value, ct);
            try
            {
                var error = await ChecklistOperations.SetStatusAsync(db, access, projectId, itemId, status, null, ct);
                return error is null ? Results.NoContent() : Results.BadRequest(error);
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        }).RequireAuthorization();
    }
}
