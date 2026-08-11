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

using System.Text;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Export;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// Cookie-authenticated export downloads for the web host. The shared Backup &amp; export page links
/// here; the file content is built by the shared <see cref="ExportBuilder"/>, so it contains only what
/// the signed-in user may see. Plain GET links, so they work in any browser (including a tablet).
/// </summary>
public static class ExportWebEndpoints
{
    public static void MapExportWebEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/export").RequireAuthorization();

        group.MapGet("/issues.csv", async (int? projectId, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var access = await LoadAccess(http, db, ct);
            if (access is null) return Results.Unauthorized();
            var csv = await ExportBuilder.BuildIssuesCsvAsync(db, access, projectId, ct);
            var name = projectId is { } p ? $"issues-project-{p}.csv" : "issues.csv";
            return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", fileDownloadName: name);
        });

        group.MapGet("/project/{id:int}.json", async (int id, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var access = await LoadAccess(http, db, ct);
            if (access is null) return Results.Unauthorized();
            var json = await ExportBuilder.BuildProjectJsonAsync(db, access, id, ct);
            return json is null
                ? Results.NotFound()
                : Results.File(Encoding.UTF8.GetBytes(json), "application/json", fileDownloadName: $"project-{id}.json");
        });
    }

    private static async Task<AccessSnapshot?> LoadAccess(HttpContext http, AppDbContext db, CancellationToken ct)
    {
        var identity = http.User.GetAccessIdentity();
        return identity is null ? null : await AccessSnapshot.LoadAsync(db, identity.Value, ct);
    }
}
