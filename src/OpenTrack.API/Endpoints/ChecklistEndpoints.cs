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
using OpenTrack.API.Contracts;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Checklist;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Endpoints;

/// <summary>Per-project bug-hunt checklist. Reading needs project view; defining items is Manager-only
/// (checked on the route project first, like categories/custom fields); working items (status/convert)
/// needs Updater and is enforced inside the shared <see cref="ChecklistOperations"/>.</summary>
public static class ChecklistEndpoints
{
    public static void MapChecklistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").RequireAuthorization().WithTags("Checklist");

        group.MapGet("/{id:int}/checklist", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var items = await ChecklistOperations.ListForProjectAsync(db, access, id, ct);
            return Results.Ok(items.Select(c => new ChecklistItemDtoContract(
                c.Id, c.ProjectId, c.Title, c.Details, c.Area, c.Status, c.Notes, c.LinkedIssueId, c.DisplayOrder)));
        });

        group.MapPost("/{id:int}/checklist", async (int id, AddChecklistItemRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var error = await ChecklistOperations.AddItemAsync(db, access, id, req.Title, req.Details, req.Area, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapPost("/{id:int}/checklist/import", async (int id, ImportChecklistRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var added = await ChecklistOperations.ImportAsync(db, access, id, req.Text, ct);
            return Results.Ok(new { Added = added });
        });

        group.MapPut("/{id:int}/checklist/{itemId:int}", async (int id, int itemId, UpdateChecklistItemRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var error = await ChecklistOperations.UpdateItemAsync(db, access, id, itemId, req.Title, req.Details, req.Area, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapDelete("/{id:int}/checklist/{itemId:int}", async (int id, int itemId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var removed = await ChecklistOperations.DeleteItemAsync(db, access, id, itemId, ct);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        group.MapPut("/{id:int}/checklist/{itemId:int}/status", async (int id, int itemId, SetChecklistStatusRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await ChecklistOperations.SetStatusAsync(db, access, id, itemId, req.Status, req.Notes, ct);
                return error is null ? Results.NoContent() : Results.BadRequest(error);
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapPost("/{id:int}/checklist/{itemId:int}/convert", async (int id, int itemId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var issueId = await ChecklistOperations.ConvertToIssueAsync(db, access, id, itemId, ct);
                return issueId is null ? Results.NotFound() : Results.Ok(new { IssueId = issueId });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });
    }
}
