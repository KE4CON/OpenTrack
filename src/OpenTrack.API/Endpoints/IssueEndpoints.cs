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
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Queries;
using OpenTrack.Infrastructure.Relationships;
using OpenTrack.API;

namespace OpenTrack.API.Endpoints;

public static class IssueEndpoints
{
    public static void MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Issues");

        // Global list, optionally filtered by project. Row-level filtered to what the caller may see.
        group.MapGet("/issues", async (
            int? projectId, IssueStatus? status, IssueSeverity? severity, IssuePriority? priority,
            int? assigneeId, int? categoryId, string? text, IssueSort? sort,
            ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var filter = new IssueFilter(projectId, status, severity, priority, assigneeId, categoryId, text,
                sort ?? IssueSort.UpdatedDesc);

            var rows = await db.Issues.AsNoTracking()
                .WhereVisibleTo(access)   // ACL first — filtering can only narrow the visible set
                .ApplyFilter(filter)
                .Select(i => new IssueDto(
                    i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status, i.Severity, i.Priority,
                    i.Reporter.UserName ?? "unknown", i.Assignee != null ? i.Assignee.UserName : null, i.UpdatedAt))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapGet("/issues/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var issue = await db.Issues.AsNoTracking()
                .Include(i => i.Project).Include(i => i.Category)
                .Include(i => i.Reporter).Include(i => i.Assignee)
                .Include(i => i.AffectsVersion).Include(i => i.FixVersion)
                .Include(i => i.Notes).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();

            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound(); // don't leak existence of a private issue

            return Results.Ok(new IssueDetailDto(
                issue.Id, issue.ProjectId, issue.Project.Name, issue.Title, issue.Description,
                issue.StepsToReproduce, issue.ExpectedBehavior, issue.ActualBehavior,
                issue.Status, issue.Severity, issue.Priority,
                issue.Reproducibility, issue.Resolution,
                issue.ReporterId, issue.Reporter.UserName ?? "unknown",
                issue.AssigneeId, issue.Assignee?.UserName,
                issue.CategoryId, issue.Category?.Name, issue.IsSticky, issue.IsPrivate,
                issue.CreatedAt, issue.UpdatedAt, issue.DueDate,
                issue.AffectsVersionId, issue.AffectsVersion?.Name, issue.FixVersionId, issue.FixVersion?.Name,
                issue.RowVersion,
                issue.Notes.Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new IssueNoteDto(n.Id, n.Author.UserName ?? "unknown", n.Text, n.IsPrivate, n.CreatedAt))
                    .ToList()));
        });

        group.MapPost("/projects/{projectId:int}/issues", async (int projectId, CreateIssueRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
            if (project is null) return Results.NotFound();

            var ctx = access.For(projectId);
            if (!ctx.CanViewProject(project.IsPublic)) return Results.NotFound();
            if (!ctx.CanCreateIssue(project.IsPublic)) return Results.Forbid();

            var issue = new Issue
            {
                ProjectId = projectId, Title = req.Title, Description = req.Description,
                StepsToReproduce = req.StepsToReproduce, ExpectedBehavior = req.ExpectedBehavior,
                ActualBehavior = req.ActualBehavior, CategoryId = req.CategoryId,
                Severity = req.Severity, Priority = req.Priority, Reproducibility = req.Reproducibility,
                DueDate = req.DueDate, AffectsVersionId = req.AffectsVersionId, FixVersionId = req.FixVersionId,
                ReporterId = access.UserId, Status = IssueStatus.New,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            // History via navigation => single atomic SaveChanges.
            issue.History.Add(new IssueHistory
            {
                UserId = access.UserId, FieldChanged = "Status",
                OldValue = null, NewValue = IssueStatus.New.ToString(), ChangedAt = DateTime.UtcNow
            });
            db.Issues.Add(issue);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/issues/{issue.Id}", issue.Id);
        });

        group.MapPut("/issues/{id:int}", async (int id, UpdateIssueRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var issue = await db.Issues.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();

            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();
            if (!ctx.CanEditIssue()) return Results.Forbid();

            db.Entry(issue).Property(i => i.RowVersion).OriginalValue = req.RowVersion;
            issue.RowVersion = Guid.NewGuid();

            var originalStatus = issue.Status;
            var originalAssigneeId = issue.AssigneeId;

            issue.Title = req.Title;
            issue.Description = req.Description;
            issue.StepsToReproduce = req.StepsToReproduce;
            issue.ExpectedBehavior = req.ExpectedBehavior;
            issue.ActualBehavior = req.ActualBehavior;
            issue.Status = req.Status;
            issue.Severity = req.Severity;
            issue.Priority = req.Priority;
            issue.Reproducibility = req.Reproducibility;
            issue.Resolution = req.Resolution;
            issue.CategoryId = req.CategoryId;
            issue.DueDate = req.DueDate;
            issue.AffectsVersionId = req.AffectsVersionId;
            issue.FixVersionId = req.FixVersionId;

            // Privileged fields: ignore (keep existing) unless the caller is authorized, so a crafted
            // request body cannot escalate past what the UI exposes for the caller's role.
            if (ctx.CanAssignIssue() && await IsAssignableAsync(db, issue.ProjectId, req.AssigneeId, ct))
                issue.AssigneeId = req.AssigneeId;
            if (ctx.CanSetIssuePrivacy())
                issue.IsPrivate = req.IsPrivate;
            if (ctx.CanSetIssueSticky())
                issue.IsSticky = req.IsSticky;

            issue.UpdatedAt = DateTime.UtcNow;

            if (issue.Status != originalStatus)
                issue.History.Add(new IssueHistory
                {
                    UserId = access.UserId, FieldChanged = "Status",
                    OldValue = originalStatus.ToString(), NewValue = issue.Status.ToString(), ChangedAt = DateTime.UtcNow
                });
            if (issue.AssigneeId != originalAssigneeId)
                issue.History.Add(new IssueHistory
                {
                    UserId = access.UserId, FieldChanged = "Assignee",
                    OldValue = originalAssigneeId?.ToString(), NewValue = issue.AssigneeId?.ToString(), ChangedAt = DateTime.UtcNow
                });

            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(OpenTrack.Core.ConcurrencyConflictException.DefaultMessage); }
            return Results.NoContent();
        });

        group.MapPost("/issues/{id:int}/notes", async (int id, AddIssueNoteRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(req.Text)) return Results.BadRequest("Note text is required.");

            var issue = await db.Issues.AsNoTracking().Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();

            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();
            if (!ctx.CanAddNote()) return Results.Forbid();
            var effectivePrivate = req.IsPrivate && ctx.CanAddPrivateNote();

            var note = new IssueNote { IssueId = id, AuthorId = access.UserId, Text = req.Text, IsPrivate = effectivePrivate, CreatedAt = DateTime.UtcNow };
            db.IssueNotes.Add(note);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/issues/{id}", note.Id);
        });

        group.MapGet("/issues/{id:int}/relationships", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var items = await RelationshipOperations.ListAsync(db, access, id, ct);
            return Results.Ok(items.Select(i => new IssueRelationshipDto(i.Id, i.OtherIssueId, i.OtherIssueTitle, i.OtherProjectName, i.Label)));
        });

        group.MapPost("/issues/{id:int}/relationships", async (int id, AddRelationshipRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await RelationshipOperations.AddAsync(db, access, id, req.TargetIssueId, req.Type, ct);
                return error is null ? Results.NoContent() : Results.BadRequest(error);
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapDelete("/relationships/{relationshipId:int}", async (int relationshipId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var removed = await RelationshipOperations.RemoveAsync(db, access, relationshipId, ct);
                return removed ? Results.NoContent() : Results.NotFound();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapGet("/issues/{id:int}/history", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var issue = await db.Issues.AsNoTracking().Include(i => i.Project)
                .FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();

            var rows = await db.IssueHistories.AsNoTracking()
                .Where(h => h.IssueId == id).OrderByDescending(h => h.ChangedAt)
                .Select(h => new IssueHistoryDto(h.Id, h.User.UserName ?? "unknown", h.FieldChanged, h.OldValue, h.NewValue, h.ChangedAt))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });
    }

    private static async Task<bool> IsAssignableAsync(AppDbContext db, int projectId, int? assigneeId, CancellationToken ct)
    {
        if (assigneeId is null) return true;
        return await db.ProjectMemberships.AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId, ct);
    }
}
