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

using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Webhooks;

/// <summary>
/// Delivers issue-event notifications to a project's configured outgoing webhooks (Slack/Discord/
/// generic JSON). Best-effort and non-blocking: it does one quick DB read for the active hooks, then
/// fires the HTTP POSTs without awaiting them, so a slow or dead webhook can never delay or fail the
/// edit that triggered it. Each POST is bounded by the HttpClient timeout and its failures are logged.
/// </summary>
public sealed class WebhookDispatch(HttpClient http, ILogger<WebhookDispatch> logger)
{
    public async Task DispatchAsync(
        AppDbContext db, int projectId, string projectName, int issueId, string issueTitle, string status,
        string eventText, CancellationToken ct = default)
    {
        var hooks = await db.ProjectWebhooks.AsNoTracking()
            .Where(w => w.ProjectId == projectId && w.IsActive)
            .Select(w => new { w.Url, w.Format })
            .ToListAsync(ct);
        if (hooks.Count == 0) return;

        var message = $"[{projectName}] #{issueId} \"{issueTitle}\" — {eventText} (status: {status})";
        foreach (var h in hooks)
        {
            object payload = h.Format switch
            {
                WebhookFormat.Slack => new { text = message },
                WebhookFormat.Discord => new { content = message },
                _ => new
                {
                    @event = eventText,
                    projectId,
                    projectName,
                    issueId,
                    title = issueTitle,
                    status,
                    occurredAtUtc = DateTime.UtcNow,
                },
            };
            _ = SendAsync(h.Url, payload); // fire-and-forget; must not delay the caller
        }
    }

    private async Task SendAsync(string url, object payload)
    {
        try
        {
            using var res = await http.PostAsJsonAsync(url, payload);
            if (!res.IsSuccessStatusCode)
                logger.LogWarning("Webhook POST to {Url} returned {Status}.", url, (int)res.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Webhook POST to {Url} failed.", url);
        }
    }
}
