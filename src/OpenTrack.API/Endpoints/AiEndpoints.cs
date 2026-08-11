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
using OpenTrack.Infrastructure.Ai;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Endpoints;

/// <summary>Optional AI-assist endpoints (opt-in). Off unless the server is configured with an Anthropic
/// key; every call is authenticated and scoped by the caller's project access.</summary>
public static class AiEndpoints
{
    public record TriageRequest(int ProjectId, string Title, string? Description);
    public record SearchRequest(string Query);
    public record SummarizeRequest(int IssueId);

    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai").RequireAuthorization().WithTags("Ai");

        group.MapGet("/enabled", (IAiAssistant ai) => Results.Ok(ai.IsEnabled));

        group.MapPost("/triage", async (TriageRequest req, ClaimsPrincipal user, AppDbContext db, IAiAssistant ai, CancellationToken ct) =>
        {
            if (!ai.IsEnabled) return Results.Ok((object?)null);
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct);
            if (project is null || !access.For(req.ProjectId).CanViewProject(project.IsPublic)) return Results.Ok((object?)null);

            var categories = await db.Categories.AsNoTracking().Where(c => c.ProjectId == req.ProjectId).Select(c => c.Name).ToListAsync(ct);
            var s = await ai.SuggestTriageAsync(req.Title, req.Description, categories, ct);
            return s is { } sg ? Results.Ok(new { sg.Severity, sg.Priority, sg.Category, sg.Tags }) : Results.Ok((object?)null);
        });

        group.MapPost("/search", async (SearchRequest req, ClaimsPrincipal user, AppDbContext db, IAiAssistant ai, CancellationToken ct) =>
        {
            if (!ai.IsEnabled || string.IsNullOrWhiteSpace(req.Query)) return Results.Ok((object?)null);
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            // Constrain any project match to the caller's visible projects.
            var projectNames = await db.Projects.AsNoTracking()
                .WhereVisibleTo(access).OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(ct);
            var c = await ai.InterpretSearchAsync(req.Query, projectNames, ct);
            return c is { } sc
                ? Results.Ok(new { sc.Status, sc.Severity, sc.Priority, sc.Text, sc.Stale, sc.Sort, ProjectName = sc.ProjectName })
                : Results.Ok((object?)null);
        });

        group.MapPost("/summarize", async (SummarizeRequest req, ClaimsPrincipal user, AppDbContext db, IAiAssistant ai, CancellationToken ct) =>
        {
            if (!ai.IsEnabled) return Results.Ok(new { Summary = (string?)null });
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();

            var issue = await db.Issues.AsNoTracking()
                .Include(i => i.Project)
                .Include(i => i.Notes).ThenInclude(n => n.Author)
                .FirstOrDefaultAsync(i => i.Id == req.IssueId, ct);
            if (issue is null) return Results.NotFound();
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound(); // don't leak existence of a private issue

            // Only feed the model notes this caller may see.
            var notes = issue.Notes.Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
                .OrderBy(n => n.CreatedAt)
                .Select(n => $"{n.Author.UserName ?? "unknown"}: {n.Text}")
                .ToList();
            var summary = await ai.SummarizeIssueAsync(issue.Title, issue.Description, notes, ct);
            return Results.Ok(new { Summary = summary });
        });
    }
}
