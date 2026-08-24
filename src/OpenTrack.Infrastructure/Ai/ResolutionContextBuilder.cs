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
using OpenTrack.Infrastructure.Attachments;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Queries;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>
/// Assembles the grounded <see cref="ResolutionContext"/> for the "Suggest a fix" feature: the issue text,
/// its ACL-filtered notes, text/log attachment excerpts, and similar RESOLVED issues (with how they were
/// handled). Shared by the Web API and the web/EF data service so BOTH hosts feed the AI identical,
/// correctly access-filtered grounding — the same "one shared layer" discipline as the authorization code.
/// Returns null if the issue is missing or the caller may not see it (so a private issue never leaks).
/// </summary>
public static class ResolutionContextBuilder
{
    public static async Task<ResolutionContext?> BuildAsync(
        AppDbContext db, AccessSnapshot access, IAttachmentStorage storage, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.Notes).ThenInclude(n => n.Author)
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return null;

        var ctx = access.For(issue.ProjectId);
        if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return null; // don't leak the existence of a private issue

        // Only ever feed the model notes this caller may see.
        var notes = issue.Notes
            .Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
            .OrderBy(n => n.CreatedAt)
            .Select(n => $"{n.Author.UserName ?? "unknown"}: {n.Text}")
            .ToList();

        var logs = await ExtractLogsAsync(issue.Attachments, storage, ct);
        var similar = await FindSimilarResolvedAsync(db, access, issue.ProjectId, issue.Title, issueId, ct);

        return new ResolutionContext(issue.Title, issue.Description, notes, logs, similar);
    }

    /// <summary>Best-effort text from an issue's text/log attachments (size-capped). Non-text attachments and
    /// any unreadable file are skipped, so a bad attachment never blocks the suggestion.</summary>
    private static async Task<IReadOnlyList<string>> ExtractLogsAsync(
        IEnumerable<IssueAttachment> attachments, IAttachmentStorage storage, CancellationToken ct)
    {
        var result = new List<string>();
        foreach (var a in attachments.OrderByDescending(a => a.UploadedAt))
        {
            if (result.Count >= AiResolution.MaxLogExcerpts) break;
            if (!LooksTextual(a.FileName, a.ContentType)) continue;
            try
            {
                await using var stream = await storage.OpenAsync(a.FilePath, ct);
                if (stream is null) continue;
                var text = await ReadCappedTextAsync(stream, AiResolution.MaxLogChars, ct);
                if (!string.IsNullOrWhiteSpace(text))
                    result.Add($"{a.FileName}:\n{text}");
            }
            catch
            {
                // Best-effort: an unreadable attachment never blocks the suggestion.
            }
        }
        return result;
    }

    private static bool LooksTextual(string fileName, string contentType)
    {
        if (contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) return true;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".log" or ".txt" or ".json" or ".xml" or ".yaml" or ".yml"
            or ".csv" or ".trace" or ".stacktrace" or ".out" or ".err" or ".md";
    }

    private static async Task<string> ReadCappedTextAsync(Stream stream, int maxChars, CancellationToken ct)
    {
        using var reader = new StreamReader(stream);
        var buffer = new char[maxChars];
        var read = await reader.ReadBlockAsync(buffer.AsMemory(0, maxChars), ct);
        return new string(buffer, 0, read);
    }

    /// <summary>Similar issues that are already RESOLVED/CLOSED, with a short "how it was handled" hint (the
    /// resolution plus the latest public note, which is usually where the fix was explained). Similarity uses
    /// the same ACL-aware query as duplicate detection, so only visible issues are ever considered.</summary>
    private static async Task<IReadOnlyList<ResolvedReference>> FindSimilarResolvedAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string title, int excludeIssueId, CancellationToken ct)
    {
        var similar = await SimilarIssueQuery.FindAsync(db, access, projectId, title, excludeIssueId, ct: ct);
        var resolvedIds = similar
            .Where(s => s.Status is IssueStatus.Resolved or IssueStatus.Closed)
            .Select(s => s.Id)
            .Take(AiResolution.MaxSimilar)
            .ToList();
        if (resolvedIds.Count == 0) return [];

        var rows = await db.Issues.AsNoTracking()
            .Where(i => resolvedIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Title, i.Resolution })
            .ToListAsync(ct);

        // The latest non-private note on each is usually where the fix was explained. Grouped in memory
        // (resolvedIds is <= MaxSimilar) to avoid a GroupBy-with-First that EF/SQLite can't translate.
        var noteRows = await db.IssueNotes.AsNoTracking()
            .Where(n => resolvedIds.Contains(n.IssueId) && !n.IsPrivate)
            .Select(n => new { n.IssueId, n.CreatedAt, n.Text })
            .ToListAsync(ct);
        var latestNote = noteRows
            .GroupBy(n => n.IssueId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(n => n.CreatedAt).First().Text);

        // Preserve the similarity ranking order from SimilarIssueQuery.
        return resolvedIds
            .Select(id => rows.FirstOrDefault(r => r.Id == id))
            .Where(r => r is not null)
            .Select(r =>
            {
                var resolution = latestNote.TryGetValue(r!.Id, out var note) && !string.IsNullOrWhiteSpace(note)
                    ? $"resolved as {r.Resolution}; last note: {note}"
                    : $"resolved as {r.Resolution}";
                return new ResolvedReference(r.Id, r.Title, resolution);
            })
            .ToList();
    }
}
