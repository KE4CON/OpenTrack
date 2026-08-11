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
using OpenTrack.Infrastructure.CustomFields;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Webhooks;
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
                project.IsPublic, project.OwnerId, project.CreatedAt, project.RowVersion));
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
                new ProjectDetailDto(project.Id, project.Name, project.Description, project.IsPublic, project.OwnerId, project.CreatedAt, project.RowVersion));
        }).RequireAuthorization(AuthorizationPolicies.RequireManager);

        group.MapPut("/{id:int}", async (int id, UpdateProjectRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(project.Id).CanViewProject(project.IsPublic))
                return Results.NotFound();
            if (!access.For(project.Id).CanEditProject()) return Results.Forbid();

            db.Entry(project).Property(p => p.RowVersion).OriginalValue = req.RowVersion;
            project.Name = req.Name;
            project.Description = req.Description;
            project.IsPublic = req.IsPublic;
            project.RowVersion = Guid.NewGuid();
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(OpenTrack.Core.ConcurrencyConflictException.DefaultMessage); }
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

        // ---- Member management (Manager+ on the project) ----

        group.MapGet("/{id:int}/member-details", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(id).CanViewProject(project.IsPublic)) return Results.NotFound();
            if (!access.For(id).CanManageProject()) return Results.Forbid();

            var rows = await db.ProjectMemberships.AsNoTracking()
                .Where(m => m.ProjectId == id).OrderBy(m => m.User.UserName)
                .Select(m => new ProjectMemberDetailDto(m.UserId, m.User.UserName ?? "unknown", m.Role, m.UserId == project.OwnerId))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapPost("/{id:int}/members", async (int id, AddProjectMemberRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(id).CanViewProject(project.IsPublic)) return Results.NotFound();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            if (!IsAssignableProjectRole(req.Role)) return Results.BadRequest("Invalid project role.");

            var normalized = req.Email.Trim().ToUpperInvariant();
            var target = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
            if (target is null) return Results.BadRequest($"No user with email '{req.Email}'.");
            if (await db.ProjectMemberships.AnyAsync(m => m.ProjectId == id && m.UserId == target.Id, ct))
                return Results.BadRequest("That user is already a member of this project.");

            db.ProjectMemberships.Add(new ProjectMembership { ProjectId = id, UserId = target.Id, Role = req.Role });
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPut("/{id:int}/members/{userId:int}", async (int id, int userId, SetMemberRoleRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null) return Results.NotFound();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            if (!IsAssignableProjectRole(req.Role)) return Results.BadRequest("Invalid project role.");
            if (userId == project.OwnerId && (int)req.Role < (int)UserRole.Manager)
                return Results.BadRequest("The project owner must remain a Manager.");

            var membership = await db.ProjectMemberships.FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId, ct);
            if (membership is null) return Results.NotFound();
            membership.Role = req.Role;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}/members/{userId:int}", async (int id, int userId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null) return Results.NotFound();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            if (userId == project.OwnerId) return Results.BadRequest("The project owner cannot be removed.");

            var membership = await db.ProjectMemberships.FirstOrDefaultAsync(m => m.ProjectId == id && m.UserId == userId, ct);
            if (membership is null) return Results.NotFound();
            db.ProjectMemberships.Remove(membership);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static bool IsAssignableProjectRole(UserRole role) =>
        (int)role >= (int)UserRole.Viewer && (int)role <= (int)UserRole.Manager;

    // ---- Category & version management (Manager+ on the project) ----

    public static void MapProjectSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").RequireAuthorization().WithTags("Projects");

        group.MapPost("/{id:int}/categories", async (int id, CreateCategoryRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var name = req.Name.Trim();
            if (string.IsNullOrEmpty(name)) return Results.BadRequest("Category name is required.");
            if (await db.Categories.AnyAsync(c => c.ProjectId == id && c.Name == name, ct))
                return Results.BadRequest("A category with that name already exists.");
            db.Categories.Add(new Category { ProjectId = id, Name = name });
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}/categories/{categoryId:int}", async (int id, int categoryId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.ProjectId == id, ct);
            if (category is null) return Results.NotFound();
            db.Categories.Remove(category);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapGet("/{id:int}/versions", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            if (project is null || !access.For(id).CanViewProject(project.IsPublic)) return Results.NotFound();
            var rows = await db.Versions.AsNoTracking()
                .Where(v => v.ProjectId == id).OrderBy(v => v.Name)
                .Select(v => new ProjectVersionDto(v.Id, v.Name, v.Description, v.ReleaseDate, v.IsReleased))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapPost("/{id:int}/versions", async (int id, CreateVersionRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var name = req.Name.Trim();
            if (string.IsNullOrEmpty(name)) return Results.BadRequest("Version name is required.");
            if (await db.Versions.AnyAsync(v => v.ProjectId == id && v.Name == name, ct))
                return Results.BadRequest("A version with that name already exists.");
            db.Versions.Add(new ProjectVersion { ProjectId = id, Name = name, Description = req.Description, ReleaseDate = req.ReleaseDate, IsReleased = req.IsReleased });
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapDelete("/{id:int}/versions/{versionId:int}", async (int id, int versionId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var version = await db.Versions.FirstOrDefaultAsync(v => v.Id == versionId && v.ProjectId == id, ct);
            if (version is null) return Results.NotFound();
            db.Versions.Remove(version);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        // ---- Custom field definitions (Manager+ to change; anyone who can view the project can read) ----

        group.MapGet("/{id:int}/custom-fields", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var rows = await CustomFieldOperations.ListDefinitionsAsync(db, access, id, ct);
            return Results.Ok(rows.Select(d => new CustomFieldDefinitionDto(d.Id, d.ProjectId, d.Name, d.Type, d.EnumOptions, d.Required, d.DisplayOrder)));
        });

        group.MapPost("/{id:int}/custom-fields", async (int id, CreateCustomFieldRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            // Manage-check on the route project first — same shape as categories/versions, so an
            // unauthorized caller can't tell an existing project from a missing one.
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var error = await CustomFieldOperations.CreateDefinitionAsync(db, access, id, req.Name, req.Type, req.EnumOptions, req.Required, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapPut("/{id:int}/custom-fields/{fieldId:int}", async (int id, int fieldId, UpdateCustomFieldRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var error = await CustomFieldOperations.UpdateDefinitionAsync(db, access, id, fieldId, req.Name, req.EnumOptions, req.Required, req.DisplayOrder, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapDelete("/{id:int}/custom-fields/{fieldId:int}", async (int id, int fieldId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var removed = await CustomFieldOperations.DeleteDefinitionAsync(db, access, id, fieldId, ct);
            return removed ? Results.NoContent() : Results.NotFound();
        });

        // ---- Project webhooks (Manager only) ----

        group.MapGet("/{id:int}/webhooks", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var rows = await WebhookOperations.ListForProjectAsync(db, access, id, ct);
            return Results.Ok(rows.Select(w => new { w.Id, w.Url, w.Format, w.IsActive }));
        });

        group.MapPost("/{id:int}/webhooks", async (int id, CreateWebhookRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var error = await WebhookOperations.AddAsync(db, access, id, req.Url, req.Format, ct);
            return error is null ? Results.NoContent() : Results.BadRequest(error);
        });

        group.MapDelete("/{id:int}/webhooks/{hookId:int}", async (int id, int hookId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (!access.For(id).CanManageProject()) return Results.Forbid();
            var removed = await WebhookOperations.DeleteAsync(db, access, id, hookId, ct);
            return removed ? Results.NoContent() : Results.NotFound();
        });
    }

    internal static int? GetUserId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}
