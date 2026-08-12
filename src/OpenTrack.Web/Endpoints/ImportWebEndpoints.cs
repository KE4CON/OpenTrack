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

using OpenTrack.Core.Import;
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

        // Generic issue import (CSV / Jira CSV / GitHub JSON) into a chosen existing project. The Import
        // page posts projectId + source + file; the shared IssueImporter enforces the per-project Manager
        // requirement and dedups on re-import.
        group.MapPost("/issues", async (HttpContext http, AppDbContext db, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, CancellationToken ct) =>
        {
            // Reads the form manually, so UseAntiforgery doesn't auto-validate — do it explicitly.
            try { await antiforgery.ValidateRequestAsync(http); }
            catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException) { return Results.BadRequest("Invalid antiforgery token."); }

            var identity = http.User.GetAccessIdentity();
            if (identity is null) return Results.Unauthorized();

            var form = await http.Request.ReadFormAsync(ct);
            if (!int.TryParse(form["projectId"], out var projectId) || projectId <= 0)
                return Results.Redirect("/import?imp=noproject");
            var source = form["source"].ToString();
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0) return Results.Redirect("/import?imp=empty");
            if (file.Length > MaxBytes) return Results.Redirect("/import?imp=toobig");

            string text;
            using (var reader = new StreamReader(file.OpenReadStream()))
                text = await reader.ReadToEndAsync(ct);

            var access = await AccessSnapshot.LoadAsync(db, identity.Value, ct);
            try
            {
                var (issues, label) = source switch
                {
                    "github" => ((IReadOnlyList<ImportedIssue>)GitHubIssueImport.Parse(text), "GitHub"),
                    "jira" => (CsvIssueImport.Parse(text), "Jira"),
                    _ => (CsvIssueImport.Parse(text), "CSV"),
                };
                var s = await IssueImporter.ImportAsync(db, access, projectId, label, issues, ct);
                return Results.Redirect($"/import?imp=ok&pid={projectId}&ic={s.Imported}&sk={s.Skipped}&tc={s.TagsLinked}");
            }
            catch (UnauthorizedAccessException) { return Results.Redirect($"/import?imp=forbidden&pid={projectId}"); }
            catch (FormatException) { return Results.Redirect($"/import?imp=badfile&pid={projectId}"); }
        });
        // Antiforgery is enforced by UseAntiforgery(): the upload form on the Backup page includes an
        // <AntiforgeryToken />, exactly like the attachment-upload form.
    }
}
