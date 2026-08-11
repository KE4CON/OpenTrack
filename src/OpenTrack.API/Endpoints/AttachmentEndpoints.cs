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
using OpenTrack.API.Contracts;
using OpenTrack.Core.Entities;
using OpenTrack.Infrastructure.Attachments;
using OpenTrack.Infrastructure.Data;
using OpenTrack.API;

namespace OpenTrack.API.Endpoints;

/// <summary>
/// Bearer-authenticated attachment endpoints for the desktop thin-client. Every operation re-checks
/// the Phase 2 per-project ACL; downloads are always served as an attachment with nosniff so an
/// uploaded HTML/SVG can't execute in a viewer.
/// </summary>
public static class AttachmentEndpoints
{
    public static void MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization().WithTags("Attachments");

        group.MapGet("/issues/{id:int}/attachments", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();
            if (!access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();

            var rows = await db.IssueAttachments.AsNoTracking()
                .Where(a => a.IssueId == id).OrderBy(a => a.UploadedAt)
                .Select(a => new AttachmentDto(a.Id, a.FileName, a.FileSize, a.ContentType, a.UploadedBy.UserName ?? "unknown", a.UploadedAt))
                .ToListAsync(ct);
            return Results.Ok(rows);
        });

        group.MapPost("/issues/{id:int}/attachments", async (int id, IFormFile file, ClaimsPrincipal user, AppDbContext db, IAttachmentStorage storage, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
            if (access is null) return Results.Unauthorized();
            var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (issue is null) return Results.NotFound();
            var ctx = access.For(issue.ProjectId);
            if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
                return Results.NotFound();
            if (!ctx.CanAddNote()) return Results.Forbid();
            if (file.Length == 0) return Results.BadRequest("Empty file.");
            if (file.Length > storage.MaxBytes) return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

            string key;
            await using (var upload = file.OpenReadStream())
            {
                try { key = await storage.SaveAsync(upload, ct); }
                catch (AttachmentTooLargeException) { return Results.StatusCode(StatusCodes.Status413PayloadTooLarge); }
            }

            var attachment = new IssueAttachment
            {
                IssueId = id,
                FileName = SafeDisplayName(file.FileName),
                FilePath = key,
                FileSize = file.Length,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                UploadedById = access.UserId,
                UploadedAt = DateTime.UtcNow
            };
            db.IssueAttachments.Add(attachment);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/attachments/{attachment.Id}/download", attachment.Id);
        }).DisableAntiforgery(); // bearer auth, no cookie — antiforgery N/A

        group.MapGet("/attachments/{id:int}/download", async (int id, ClaimsPrincipal user, AppDbContext db, IAttachmentStorage storage, HttpContext http, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
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

        group.MapDelete("/attachments/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db, IAttachmentStorage storage, CancellationToken ct) =>
        {
            var access = await ApiAccess.LoadAsync(user, db, ct);
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

            var key = attachment.FilePath;
            db.IssueAttachments.Remove(attachment);
            await db.SaveChangesAsync(ct);
            await storage.DeleteAsync(key, ct);
            return Results.NoContent();
        });
    }

    /// <summary>Keeps only the file name portion of a client-supplied name and caps its length, so a
    /// path or an overlong string can't reach the DB display column. (Storage never uses this value.)</summary>
    private static string SafeDisplayName(string? clientName)
    {
        var name = Path.GetFileName(clientName ?? "").Trim();
        if (string.IsNullOrEmpty(name)) name = "attachment";
        return name.Length > 255 ? name[^255..] : name;
    }
}
