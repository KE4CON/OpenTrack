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
    }
}
