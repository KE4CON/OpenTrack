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

using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Webhooks;

public readonly record struct WebhookItem(int Id, string Url, WebhookFormat Format, bool IsActive);

/// <summary>
/// ACL-aware CRUD for a project's outgoing webhooks. Because a webhook URL can carry a secret token
/// (Slack/Discord URLs do), reading and managing them both require the Manager role on the project —
/// the same authority as other project settings. Shared by the Web API and the web/EF data service.
/// </summary>
public static class WebhookOperations
{
    public static async Task<IReadOnlyList<WebhookItem>> ListForProjectAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject()) return [];
        return await db.ProjectWebhooks.AsNoTracking()
            .Where(w => w.ProjectId == projectId)
            .OrderBy(w => w.Id)
            .Select(w => new WebhookItem(w.Id, w.Url, w.Format, w.IsActive))
            .ToListAsync(ct);
    }

    public static async Task<string?> AddAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string url, WebhookFormat format, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing webhooks requires the Manager role on this project.");
        if (!await db.Projects.AnyAsync(p => p.Id == projectId, ct)) return "Project not found.";

        url = url.Trim();
        if (url.Length == 0) return "The webhook URL is required.";
        if (url.Length > FieldLimits.WebhookUrl) return $"The URL must be {FieldLimits.WebhookUrl} characters or fewer.";
        // Only http/https — and never point a webhook at an internal address you don't control (SSRF).
        // Webhooks are Manager-configured on a self-hosted server, so this is a trust boundary, not a
        // sandbox; we still require a well-formed absolute http(s) URL.
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return "Enter a valid http(s) URL.";

        db.ProjectWebhooks.Add(new ProjectWebhook { ProjectId = projectId, Url = url, Format = format, IsActive = true });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int id, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing webhooks requires the Manager role on this project.");
        var hook = await db.ProjectWebhooks.FirstOrDefaultAsync(w => w.Id == id && w.ProjectId == projectId, ct);
        if (hook is null) return false;
        db.ProjectWebhooks.Remove(hook);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
