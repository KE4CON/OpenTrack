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
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Automation;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Endpoints;

/// <summary>Per-project automation-rule CRUD (Manager-gated via <see cref="AutomationRuleOperations"/>).</summary>
public static class AutomationEndpoints
{
    public record AutomationRuleDto(
        int Id, string Name, bool IsEnabled, int SortOrder,
        string? WhenTextContains, IssueSeverity? WhenSeverity, IssuePriority? WhenPriority, int? WhenCategoryId,
        IssueSeverity? SetSeverity, IssuePriority? SetPriority, IssueStatus? SetStatus,
        int? AssignToUserId, string? AddTag);

    public static void MapAutomationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Automation");

        group.MapGet("/projects/{projectId:int}/automation-rules",
            async (int projectId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var rules = await AutomationRuleOperations.ListForProjectAsync(db, access, projectId, ct);
            return Results.Ok(rules.Select(r => new AutomationRuleDto(
                r.Id, r.Name, r.IsEnabled, r.SortOrder,
                r.WhenTextContains, r.WhenSeverity, r.WhenPriority, r.WhenCategoryId,
                r.SetSeverity, r.SetPriority, r.SetStatus, r.AssignToUserId, r.AddTag)).ToList());
        });

        group.MapPost("/projects/{projectId:int}/automation-rules",
            async (int projectId, AutomationRuleDto dto, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await AutomationRuleOperations.CreateAsync(db, access, projectId, ToInput(dto), ct);
                return Results.Ok(new { error });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapPut("/automation-rules/{id:int}",
            async (int id, AutomationRuleDto dto, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await AutomationRuleOperations.UpdateAsync(db, access, id, ToInput(dto), ct);
                return Results.Ok(new { error });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapDelete("/automation-rules/{id:int}",
            async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var ok = await AutomationRuleOperations.DeleteAsync(db, access, id, ct);
                return ok ? Results.Ok() : Results.NotFound();
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });
    }

    private static AutomationRuleInput ToInput(AutomationRuleDto d) => new(
        d.Name, d.IsEnabled, d.SortOrder,
        d.WhenTextContains, d.WhenSeverity, d.WhenPriority, d.WhenCategoryId,
        d.SetSeverity, d.SetPriority, d.SetStatus, d.AssignToUserId, d.AddTag);
}
