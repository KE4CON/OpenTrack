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

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Git;
using OpenTrack.Infrastructure.Notifications;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// Inbound GitHub webhook receiver. Unauthenticated (GitHub can't hold an OpenTrack session) but every
/// request is HMAC-verified against the project's stored secret before anything is read from the payload,
/// and the route is rate-limited. On a verified push, commit messages are scanned for issue references and
/// linked / auto-resolved via <see cref="GitIntegrationOperations"/>.
/// </summary>
public static class GitWebhookEndpoints
{
    public static void MapGitWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/git/webhook/{projectId:int}", async (
            int projectId, HttpRequest request, AppDbContext db, NotificationDispatch notifications, CancellationToken ct) =>
        {
            // Read the raw body once — the signature is computed over the exact bytes.
            using var ms = new MemoryStream();
            await request.Body.CopyToAsync(ms, ct);
            var body = ms.ToArray();

            var config = await db.GitIntegrations.AsNoTracking().FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
            if (config is null || !config.Enabled)
                return Results.NotFound(); // don't reveal whether the project exists / is configured

            var signature = request.Headers["X-Hub-Signature-256"].ToString();
            if (!GitSignature.IsValid(config.WebhookSecret, body, signature))
                return Results.NotFound(); // same response as unconfigured — don't reveal which projects have Git enabled

            var githubEvent = request.Headers["X-GitHub-Event"].ToString();
            if (string.Equals(githubEvent, "ping", StringComparison.OrdinalIgnoreCase))
                return Results.Ok(new { message = "pong" });
            if (!string.Equals(githubEvent, "push", StringComparison.OrdinalIgnoreCase))
                return Results.Ok(new { ignored = githubEvent }); // other events are accepted but ignored

            var commits = ParseCommits(body);
            if (commits.Count == 0) return Results.Ok(new { linked = 0, resolved = 0 });

            var result = await GitIntegrationOperations.ProcessPushAsync(db, notifications, projectId, commits, ct);
            return Results.Ok(new { linked = result.Linked, resolved = result.Resolved });
        })
        .RequireRateLimiting("intake")
        .WithTags("GitWebhook");
    }

    /// <summary>Pull (sha, message, url, author, timestamp) out of a GitHub push payload's commits[].</summary>
    private static List<GitCommitInfo> ParseCommits(byte[] body)
    {
        var list = new List<GitCommitInfo>();
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("commits", out var commits) || commits.ValueKind != JsonValueKind.Array)
                return list;

            foreach (var c in commits.EnumerateArray())
            {
                var sha = GetString(c, "id");
                var message = GetString(c, "message");
                if (string.IsNullOrEmpty(sha) || string.IsNullOrEmpty(message)) continue;

                var url = GetString(c, "url");
                string? author = null;
                if (c.TryGetProperty("author", out var a) && a.ValueKind == JsonValueKind.Object)
                    author = GetString(a, "name") is { Length: > 0 } n ? n : GetString(a, "username");

                var committedAt = DateTime.UtcNow;
                if (GetString(c, "timestamp") is { Length: > 0 } ts && DateTimeOffset.TryParse(ts, out var dto))
                    committedAt = dto.UtcDateTime;

                list.Add(new GitCommitInfo(sha, message, string.IsNullOrEmpty(url) ? null : url, author, committedAt));
            }
        }
        catch (JsonException)
        {
            // Malformed payload → treat as no commits (the signature already passed, so just no-op).
        }
        return list;
    }

    private static string GetString(JsonElement e, string prop) =>
        e.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
}
