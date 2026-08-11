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
using Microsoft.EntityFrameworkCore;
using OpenTrack.API.Contracts;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Endpoints;

/// <summary>The signed-in user's own in-app notifications. Every query/mutation is scoped to the
/// caller's user id, so one user can never read or modify another's notifications.</summary>
public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").RequireAuthorization().WithTags("Notifications");

        group.MapGet("/", async (bool? unreadOnly, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var uid = identity.Value.UserId;
            var q = db.Notifications.AsNoTracking().Where(n => n.UserId == uid);
            if (unreadOnly == true) q = q.Where(n => !n.IsRead);
            var rows = await q.OrderByDescending(n => n.CreatedAt).Take(200)
                .Select(n => new NotificationDto(n.Id, n.IssueId, n.Text, n.IsRead, n.CreatedAt))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapGet("/unread-count", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var uid = identity.Value.UserId;
            return Results.Ok(await db.Notifications.AsNoTracking().CountAsync(n => n.UserId == uid && !n.IsRead, ct));
        });

        group.MapPost("/{id:int}/read", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var uid = identity.Value.UserId;
            var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid, ct);
            if (n is null) return Results.NotFound();
            if (!n.IsRead) { n.IsRead = true; await db.SaveChangesAsync(ct); }
            return Results.NoContent();
        });

        group.MapPost("/read-all", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var identity = user.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();
            var uid = identity.Value.UserId;
            await db.Notifications.Where(n => n.UserId == uid && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
            return Results.NoContent();
        });
    }
}
