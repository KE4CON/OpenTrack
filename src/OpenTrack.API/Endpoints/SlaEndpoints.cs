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
using OpenTrack.Core.Enums;
using OpenTrack.Core.Sla;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Sla;

namespace OpenTrack.API.Endpoints;

/// <summary>SLA target management (Manager-gated) plus the read-only SLA board and per-issue status.</summary>
public static class SlaEndpoints
{
    public record SlaTargetDto(IssuePriority Priority, int TargetHours);
    public record SetTargetRequest(int? Hours);
    public record SlaBoardItemDto(
        int Id, int ProjectId, string ProjectName, string Title, IssuePriority Priority,
        string? AssigneeName, DateTime CreatedAtUtc, DateTime DueUtc, SlaStatus Status);
    public record SlaBoardDto(IReadOnlyList<SlaBoardItemDto> Breached, IReadOnlyList<SlaBoardItemDto> AtRisk);
    public record SlaIssueDto(SlaStatus Status, DateTime? DueUtc);

    public static void MapSlaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Sla");

        group.MapGet("/projects/{projectId:int}/sla-policies",
            async (int projectId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var items = await SlaPolicyOperations.ListForProjectAsync(db, access, projectId, ct);
            return Results.Ok(items.Select(p => new SlaTargetDto(p.Priority, p.TargetHours)).ToList());
        });

        group.MapPut("/projects/{projectId:int}/sla-policies/{priority}",
            async (int projectId, IssuePriority priority, SetTargetRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await SlaPolicyOperations.SetAsync(db, access, projectId, priority, req.Hours, ct);
                return Results.Ok(new { error });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapGet("/sla-board", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var board = await SlaBoard.BuildAsync(db, access, DateTime.UtcNow, ct);
            static List<SlaBoardItemDto> Map(IReadOnlyList<SlaBoardRow> rows) =>
                rows.Select(r => new SlaBoardItemDto(r.Id, r.ProjectId, r.ProjectName, r.Title, r.Priority,
                    r.AssigneeName, r.CreatedAtUtc, r.DueUtc, r.Status)).ToList();
            return Results.Ok(new SlaBoardDto(Map(board.Breached), Map(board.AtRisk)));
        });

        group.MapGet("/issues/{id:int}/sla", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();

            int? hours = await db.SlaPolicies.AsNoTracking()
                .Where(p => p.ProjectId == issue.ProjectId && p.Priority == issue.Priority)
                .Select(p => (int?)p.TargetHours).FirstOrDefaultAsync(ct);
            var a = SlaCalculator.Evaluate(issue.Priority, issue.CreatedAt, SlaCalculator.IsOpen(issue.Status), DateTime.UtcNow, hours);
            return Results.Ok(new SlaIssueDto(a.Status, a.DueUtc));
        });
    }
}
