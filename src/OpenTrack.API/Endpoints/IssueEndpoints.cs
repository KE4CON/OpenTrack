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
using OpenTrack.Infrastructure.Data;
using OpenTrack.API;

namespace OpenTrack.API.Endpoints;

public static class IssueEndpoints
{
    public static void MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Issues");

        // Global list, optionally filtered by project.
        group.MapGet("/issues", async (int? projectId, AppDbContext db) =>
        {
            var query = db.Issues.AsNoTracking().AsQueryable();
            if (projectId is not null) query = query.Where(i => i.ProjectId == projectId);

            return await query
                .OrderByDescending(i => i.UpdatedAt)
                .Select(i => new IssueDto(
                    i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status, i.Severity, i.Priority,
                    i.Reporter.UserName ?? "unknown", i.Assignee != null ? i.Assignee.UserName : null, i.UpdatedAt))
                .ToListAsync();
        });

        group.MapGet("/issues/{id:int}", async (int id, AppDbContext db) =>
        {
            var issue = await db.Issues.AsNoTracking()
                .Include(i => i.Project).Include(i => i.Category)
                .Include(i => i.Reporter).Include(i => i.Assignee)
                .Include(i => i.Notes).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (issue is null) return Results.NotFound();

            return Results.Ok(new IssueDetailDto(
                issue.Id, issue.ProjectId, issue.Project.Name, issue.Title, issue.Description,
                issue.StepsToReproduce, issue.Status, issue.Severity, issue.Priority,
                issue.Reproducibility, issue.Resolution,
                issue.ReporterId, issue.Reporter.UserName ?? "unknown",
                issue.AssigneeId, issue.Assignee?.UserName,
                issue.CategoryId, issue.Category?.Name, issue.IsSticky, issue.IsPrivate,
                issue.CreatedAt, issue.UpdatedAt,
                issue.Notes.OrderBy(n => n.CreatedAt)
                    .Select(n => new IssueNoteDto(n.Id, n.Author.UserName ?? "unknown", n.Text, n.CreatedAt))
                    .ToList()));
        });

        group.MapPost("/projects/{projectId:int}/issues", async (int projectId, CreateIssueRequest req, ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = ProjectEndpoints.GetUserId(user);
            if (userId is null) return Results.Unauthorized();

            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            if (project is null) return Results.NotFound();

            var issue = new Issue
            {
                ProjectId = projectId, Title = req.Title, Description = req.Description,
                StepsToReproduce = req.StepsToReproduce, CategoryId = req.CategoryId,
                Severity = req.Severity, Priority = req.Priority, Reproducibility = req.Reproducibility,
                ReporterId = userId.Value, Status = IssueStatus.New,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Issues.Add(issue);
            await db.SaveChangesAsync();

            db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id, UserId = userId.Value, FieldChanged = "Status",
                OldValue = null, NewValue = IssueStatus.New.ToString(), ChangedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            return Results.Created($"/api/issues/{issue.Id}", issue.Id);
        });

        group.MapPut("/issues/{id:int}", async (int id, UpdateIssueRequest req, ClaimsPrincipal user, AppDbContext db) =>
        {
            var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == id);
            if (issue is null) return Results.NotFound();

            var userId = ProjectEndpoints.GetUserId(user) ?? 0;
            var originalStatus = issue.Status;
            var originalAssigneeId = issue.AssigneeId;

            issue.Title = req.Title;
            issue.Description = req.Description;
            issue.Status = req.Status;
            issue.Severity = req.Severity;
            issue.Priority = req.Priority;
            issue.Resolution = req.Resolution;
            issue.AssigneeId = req.AssigneeId;
            issue.CategoryId = req.CategoryId;
            issue.IsSticky = req.IsSticky;
            issue.IsPrivate = req.IsPrivate;
            issue.UpdatedAt = DateTime.UtcNow;

            if (issue.Status != originalStatus)
            {
                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.Id, UserId = userId, FieldChanged = "Status",
                    OldValue = originalStatus.ToString(), NewValue = issue.Status.ToString(), ChangedAt = DateTime.UtcNow
                });
            }
            if (issue.AssigneeId != originalAssigneeId)
            {
                db.IssueHistories.Add(new IssueHistory
                {
                    IssueId = issue.Id, UserId = userId, FieldChanged = "Assignee",
                    OldValue = originalAssigneeId?.ToString(), NewValue = issue.AssigneeId?.ToString(), ChangedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.RequireUpdater);

        group.MapPost("/issues/{id:int}/notes", async (int id, AddIssueNoteRequest req, ClaimsPrincipal user, AppDbContext db) =>
        {
            var userId = ProjectEndpoints.GetUserId(user);
            if (userId is null) return Results.Unauthorized();
            if (string.IsNullOrWhiteSpace(req.Text)) return Results.BadRequest("Note text is required.");

            var issueExists = await db.Issues.AnyAsync(i => i.Id == id);
            if (!issueExists) return Results.NotFound();

            var note = new IssueNote { IssueId = id, AuthorId = userId.Value, Text = req.Text, CreatedAt = DateTime.UtcNow };
            db.IssueNotes.Add(note);
            await db.SaveChangesAsync();
            return Results.Created($"/api/issues/{id}", note.Id);
        });
    }
}
