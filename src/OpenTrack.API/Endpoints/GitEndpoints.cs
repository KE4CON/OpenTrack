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
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Git;

namespace OpenTrack.API.Endpoints;

/// <summary>Git-integration config (Manager-gated) and the per-issue linked-commit list. The webhook
/// RECEIVER is not here — it's an unauthenticated, signature-verified endpoint in the web host.</summary>
public static class GitEndpoints
{
    public record GitIntegrationDto(bool Enabled, string? WebhookSecret, bool AutoResolve);
    public record SetGitRequest(bool Enabled, string? WebhookSecret, bool AutoResolve);
    public record IssueCommitDto(
        string Sha, string ShortSha, string Message, string? Author, string? Url, bool Closing, DateTime CommittedAtUtc);

    public static void MapGitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Git");

        group.MapGet("/projects/{projectId:int}/git-integration",
            async (int projectId, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var config = await GitIntegrationOperations.GetForProjectAsync(db, access, projectId, ct);
            return config is null
                ? Results.Ok((object?)null)
                : Results.Ok(new GitIntegrationDto(config.Enabled, config.WebhookSecret, config.AutoResolve));
        });

        group.MapPut("/projects/{projectId:int}/git-integration",
            async (int projectId, SetGitRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            try
            {
                var error = await GitIntegrationOperations.SetAsync(db, access, projectId, req.Enabled, req.WebhookSecret, req.AutoResolve, ct);
                return Results.Ok(new { error });
            }
            catch (UnauthorizedAccessException) { return Results.Forbid(); }
        });

        group.MapGet("/issues/{id:int}/commits", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var items = await GitIntegrationOperations.ListForIssueAsync(db, access, id, ct);
            return Results.Ok(items.Select(l => new IssueCommitDto(
                l.Sha, l.Sha.Length >= 7 ? l.Sha[..7] : l.Sha, l.Message, l.Author, l.Url, l.Closing, l.CommittedAt)).ToList());
        });
    }
}
