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
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.API;

namespace OpenTrack.API.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").RequireAuthorization().WithTags("Projects");

        group.MapGet("/", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var rows = await db.Projects.AsNoTracking()
                .WhereVisibleTo(access)
                .OrderBy(p => p.Name)
                .Select(p => new ProjectDto(
                    p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId,
                    p.Issues.Count(i => i.Status != IssueStatus.Closed)))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(project.Id).CanViewProject(project.IsPublic))
                return Results.NotFound();

            return Results.Ok(new ProjectDetailDto(project.Id, project.Name, project.Description,
                project.IsPublic, project.OwnerId, project.CreatedAt));
        });

        // Creating a project is not scoped to an existing project: require global Manager+.
        group.MapPost("/", async (CreateProjectRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.GlobalAtLeast(UserRole.Manager)) return Results.Forbid();

            var project = new Project { Name = req.Name, Description = req.Description, IsPublic = req.IsPublic, OwnerId = access.UserId };
            project.Members.Add(new ProjectMembership { UserId = access.UserId, Role = UserRole.Manager });

            db.Projects.Add(project);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/projects/{project.Id}",
                new ProjectDetailDto(project.Id, project.Name, project.Description, project.IsPublic, project.OwnerId, project.CreatedAt));
        }).RequireAuthorization(AuthorizationPolicies.RequireManager);

        group.MapPut("/{id:int}", async (int id, UpdateProjectRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(project.Id).CanViewProject(project.IsPublic))
                return Results.NotFound();
            if (!access.For(project.Id).CanEditProject()) return Results.Forbid();

            project.Name = req.Name;
            project.Description = req.Description;
            project.IsPublic = req.IsPublic;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        // Lookups for the issue create/edit forms (categories to pick from, members to assign to).
        group.MapGet("/{id:int}/categories", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(id).CanViewProject(project.IsPublic)) return Results.NotFound();

            var rows = await db.Categories.AsNoTracking()
                .Where(c => c.ProjectId == id).OrderBy(c => c.Name)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapGet("/{id:int}/members", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(id).CanViewProject(project.IsPublic)) return Results.NotFound();

            var memberIds = await db.ProjectMemberships.AsNoTracking()
                .Where(m => m.ProjectId == id).Select(m => m.UserId).ToListAsync(ct);
            var rows = await db.Users.AsNoTracking()
                .Where(u => memberIds.Contains(u.Id)).OrderBy(u => u.UserName)
                .Select(u => new ProjectMemberDto(u.Id, u.UserName ?? "unknown"))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });
    }

    internal static int? GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}
