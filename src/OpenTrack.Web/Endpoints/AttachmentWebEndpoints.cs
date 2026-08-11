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
using OpenTrack.Infrastructure.Attachments;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// Cookie-authenticated attachment endpoints for the web host. The shared issue page posts a plain
/// multipart form here to upload (static SSR has no interactivity) and links here to download. Every
/// operation re-checks the Phase 2 per-project ACL; downloads are served as an attachment with
/// nosniff so an uploaded HTML/SVG can't execute. Antiforgery is enforced by UseAntiforgery() on the
/// POST forms (the forms include an &lt;AntiforgeryToken /&gt;).
/// </summary>
public static class AttachmentWebEndpoints
{
    public static void MapAttachmentWebEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/attachments").RequireAuthorization();

        group.MapPost("/{issueId:int}/upload", async (int issueId, IFormFile? file, HttpContext http, AppDbContext db, IAttachmentStorage storage, CancellationToken ct) =>
        {
            var access = await LoadAccess(http, db, ct);
            if (access is null) return Results.Unauthorized();
            var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
            if (issue is null) return Results.NotFound();
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();
            if (!ctx.CanAddNote()) return Results.Forbid();

            if (file is { Length: > 0 } && file.Length <= storage.MaxBytes)
            {
                string key;
                await using (var upload = file.OpenReadStream())
                {
                    try { key = await storage.SaveAsync(upload, ct); }
                    catch (AttachmentTooLargeException) { return Results.Redirect($"/issues/{issueId}"); }
                }
                db.IssueAttachments.Add(new IssueAttachment
                {
                    IssueId = issueId,
                    FileName = SafeDisplayName(file.FileName),
                    FilePath = key,
                    FileSize = file.Length,
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                    UploadedById = access.UserId,
                    UploadedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync(ct);
            }
            return Results.Redirect($"/issues/{issueId}");
        });

        group.MapGet("/{id:int}/download", async (int id, HttpContext http, AppDbContext db, IAttachmentStorage storage, CancellationToken ct) =>
        {
            var access = await LoadAccess(http, db, ct);
            if (access is null) return Results.Unauthorized();
            var attachment = await db.IssueAttachments.AsNoTracking().Include(a => a.Issue).ThenInclude(i => i.Project)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
            if (attachment is null) return Results.NotFound();
            var issue = attachment.Issue;
            if (!access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();

            var stream = await storage.OpenAsync(attachment.FilePath, ct);
            if (stream is null) return Results.NotFound();

            http.Response.Headers["X-Content-Type-Options"] = "nosniff";
            return Results.File(stream, attachment.ContentType, fileDownloadName: attachment.FileName);
        });

        group.MapPost("/{id:int}/delete", async (int id, HttpContext http, AppDbContext db, IAttachmentStorage storage, CancellationToken ct) =>
        {
            var access = await LoadAccess(http, db, ct);
            if (access is null) return Results.Unauthorized();
            var attachment = await db.IssueAttachments.Include(a => a.Issue).ThenInclude(i => i.Project)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
            if (attachment is null) return Results.NotFound();
            var issue = attachment.Issue;
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();
            if (attachment.UploadedById != access.UserId && !ctx.CanEditIssue())
                return Results.Forbid();

            var issueId = issue.Id;
            var key = attachment.FilePath;
            db.IssueAttachments.Remove(attachment);
            await db.SaveChangesAsync(ct);
            await storage.DeleteAsync(key, ct);
            return Results.Redirect($"/issues/{issueId}");
        });
    }

    private static Task<AccessSnapshot?> LoadAccess(HttpContext http, AppDbContext db, CancellationToken ct)
    {
        var identity = http.User.GetAccessIdentity();
        return identity is null
            ? Task.FromResult<AccessSnapshot?>(null)
            : LoadSnapshot(db, identity.Value, ct);
    }

    private static async Task<AccessSnapshot?> LoadSnapshot(AppDbContext db, AccessIdentity identity, CancellationToken ct)
        => await AccessSnapshot.LoadAsync(db, identity, ct);

    private static string SafeDisplayName(string? clientName)
    {
        var name = Path.GetFileName(clientName ?? "").Trim();
        if (string.IsNullOrEmpty(name)) name = "attachment";
        return name.Length > 255 ? name[^255..] : name;
    }
}
