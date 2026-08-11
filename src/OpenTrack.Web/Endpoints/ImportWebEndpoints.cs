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

using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Import;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// Cookie-authenticated MantisBT import upload for the web host. The Backup &amp; export page posts a
/// MantisBT XML file here (static SSR multipart form with an antiforgery token). The shared
/// <see cref="MantisImporter"/> enforces the Manager requirement and does the work; the result is
/// reported back to the page via redirect query parameters.
/// </summary>
public static class ImportWebEndpoints
{
    private const long MaxBytes = 25 * 1024 * 1024; // 25 MB

    public static void MapImportWebEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/import").RequireAuthorization();

        group.MapPost("/mantis", async (IFormFile? file, HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var identity = http.User.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();

            if (file is null || file.Length == 0) return Results.Redirect("/backup?imp=empty");
            if (file.Length > MaxBytes) return Results.Redirect("/backup?imp=toobig");

            string xml;
            using (var reader = new StreamReader(file.OpenReadStream()))
                xml = await reader.ReadToEndAsync(ct);

            var access = await AccessSnapshot.LoadAsync(db, identity.Value, ct);
            try
            {
                var s = await MantisImporter.ImportAsync(db, access, xml, ct);
                return Results.Redirect($"/backup?imp=ok&pc={s.ProjectsCreated}&pm={s.ProjectsMatched}&ic={s.IssuesImported}&sk={s.IssuesSkipped}&nc={s.NotesImported}&tc={s.TagsLinked}");
            }
            catch (UnauthorizedAccessException) { return Results.Redirect("/backup?imp=forbidden"); }
            catch (FormatException) { return Results.Redirect("/backup?imp=badfile"); }
        });
        // Antiforgery is enforced by UseAntiforgery(): the upload form on the Backup page includes an
        // <AntiforgeryToken />, exactly like the attachment-upload form.
    }
}
