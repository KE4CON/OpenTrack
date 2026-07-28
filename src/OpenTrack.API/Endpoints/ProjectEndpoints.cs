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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTrack.API.Contracts;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;
using OpenTrack.API;

namespace OpenTrack.API.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").RequireAuthorization().WithTags("Projects");

        group.MapGet("/", async (AppDbContext db) =>
            await db.Projects.AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProjectDto(
                    p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId,
                    p.Issues.Count(i => i.Status != IssueStatus.Closed)))
                .ToListAsync());

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            return project is null
                ? Results.NotFound()
                : Results.Ok(new ProjectDetailDto(project.Id, project.Name, project.Description,
                    project.IsPublic, project.OwnerId, project.CreatedAt));
        });

        group.MapPost("/", async (CreateProjectRequest req, ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = GetUserId(user);
            if (userId is null) return Results.Unauthorized();

            var project = new Project { Name = req.Name, Description = req.Description, IsPublic = req.IsPublic, OwnerId = userId.Value };
            project.Members.Add(new ProjectMembership { UserId = userId.Value, Role = UserRole.Manager });

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return Results.Created($"/api/projects/{project.Id}",
                new ProjectDetailDto(project.Id, project.Name, project.Description, project.IsPublic, project.OwnerId, project.CreatedAt));
        }).RequireAuthorization(AuthorizationPolicies.RequireManager);

        group.MapPut("/{id:int}", async (int id, UpdateProjectRequest req, AppDbContext db) =>
        {
            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project is null) return Results.NotFound();

            project.Name = req.Name;
            project.Description = req.Description;
            project.IsPublic = req.IsPublic;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.RequireManager);

        // Lookups for the issue create/edit forms (categories to pick from, members to assign to).
        group.MapGet("/{id:int}/categories", async (int id, AppDbContext db) =>
            await db.Categories.AsNoTracking()
                .Where(c => c.ProjectId == id).OrderBy(c => c.Name)
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync());

        group.MapGet("/{id:int}/members", async (int id, AppDbContext db) =>
        {
            var memberIds = await db.ProjectMemberships.AsNoTracking()
                .Where(m => m.ProjectId == id).Select(m => m.UserId).ToListAsync();
            return await db.Users.AsNoTracking()
                .Where(u => memberIds.Contains(u.Id)).OrderBy(u => u.UserName)
                .Select(u => new ProjectMemberDto(u.Id, u.UserName ?? "unknown"))
                .ToListAsync();
        });
    }

    internal static int? GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}
