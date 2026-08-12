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
using OpenTrack.Core.Import;
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Import;

/// <summary>What a generic import did.</summary>
public sealed record IssueImportSummary(int Imported, int Skipped, int TagsLinked, IReadOnlyList<string> Warnings);

/// <summary>
/// Imports source-agnostic <see cref="ImportedIssue"/>s (from CSV / GitHub / Jira parsers) into an EXISTING
/// project chosen by the caller — unlike the MantisBT importer, which creates projects by name. Requires the
/// Manager role on that project. Source authors are matched to OpenTrack accounts by username where possible
/// (otherwise attributed to the importer, with the original name kept in the text).
/// <para>Re-import de-duplication depends on each row carrying a stable id/key column (GitHub number, Jira
/// key, or an id/key column in a CSV): rows WITH one get an <see cref="Issue.ImportedExternalKey"/> and are
/// skipped on re-import; rows WITHOUT one can't be de-duplicated and will be added again — the summary warns
/// how many such rows there were.</para>
/// </summary>
public static class IssueImporter
{
    public static async Task<IssueImportSummary> ImportAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string sourceLabel,
        IReadOnlyList<ImportedIssue> issues, CancellationToken ct = default)
    {
        // Authorize BEFORE revealing whether the project exists — a non-manager (incl. for a nonexistent
        // project id) gets the same "forbidden", so the endpoint can't be used to probe which ids exist.
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Importing into a project requires the Manager role on it.");
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return new IssueImportSummary(0, 0, 0, ["The selected project no longer exists."]);

        var warnings = new List<string>();
        int imported = 0, skipped = 0, tagsLinked = 0, noId = 0;
        var now = DateTime.UtcNow;

        var users = await db.Users.AsNoTracking().Where(u => u.UserName != null)
            .ToDictionaryAsync(u => u.UserName!.ToLower(), u => u.Id, ct);
        // Only attribute imported issues/notes to users who are actually MEMBERS of the target project,
        // so an importer can't forge authorship as an arbitrary account (e.g. an administrator).
        var memberIds = await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.ProjectId == projectId).Select(m => m.UserId).ToHashSetAsync(ct);
        int? MapUser(string? name) =>
            name is not null && users.TryGetValue(name.Trim().ToLowerInvariant(), out var id) && memberIds.Contains(id)
                ? id : null;

        var categories = await db.Categories.Where(c => c.ProjectId == projectId).ToListAsync(ct);
        Category? FindOrAddCategory(string? catName)
        {
            if (string.IsNullOrWhiteSpace(catName)) return null;
            var trimmed = Truncate(catName.Trim(), FieldLimits.CategoryName);
            var existing = categories.FirstOrDefault(c => string.Equals(c.Name, trimmed, StringComparison.OrdinalIgnoreCase));
            if (existing is not null) return existing;
            var created = new Category { ProjectId = projectId, Name = trimmed };
            db.Categories.Add(created);
            categories.Add(created);
            return created;
        }

        var alreadyImported = await db.Issues
            .Where(i => i.ProjectId == projectId && i.ImportedExternalKey != null)
            .Select(i => i.ImportedExternalKey!)
            .ToHashSetAsync(ct);
        var tagCache = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);

        foreach (var m in issues)
        {
            // Namespace the source key so ids from different tools can't collide (e.g. "github:123").
            var key = string.IsNullOrWhiteSpace(m.ExternalId) ? null : Truncate($"{sourceLabel}:{m.ExternalId.Trim()}", 200);
            if (key is null) noId++; // can't be de-duplicated on a future re-import
            if (key is not null && alreadyImported.Contains(key)) { skipped++; continue; }

            var reporterMapped = MapUser(m.Reporter) is not null;
            var issue = new Issue
            {
                ProjectId = projectId,
                Title = Truncate(m.Title, FieldLimits.IssueTitle),
                Description = BuildDescription(m, sourceLabel, reporterMapped),
                Status = m.Status ?? IssueStatus.New,
                Severity = m.Severity ?? IssueSeverity.Minor,
                Priority = m.Priority ?? IssuePriority.Normal,
                Reproducibility = IssueReproducibility.HaveNotTried,
                Resolution = IssueResolution.Open,
                Category = FindOrAddCategory(m.Category),
                ReporterId = MapUser(m.Reporter) ?? access.UserId,
                AssigneeId = MapUser(m.Assignee),
                ImportedExternalKey = key,
                CreatedAt = m.CreatedAt ?? now,
                UpdatedAt = m.CreatedAt ?? now,
            };
            issue.History.Add(new IssueHistory
            {
                UserId = access.UserId, FieldChanged = "Imported", OldValue = null,
                NewValue = m.ExternalId is { Length: > 0 } id ? $"{sourceLabel} {id}" : sourceLabel,
                ChangedAt = now,
            });

            foreach (var tagName in m.Tags)
            {
                var tag = await GetOrAddTagAsync(db, tagCache, tagName, ct);
                if (tag is null) continue;
                issue.IssueTags.Add(new IssueTag { Tag = tag });
                tagsLinked++;
            }

            db.Issues.Add(issue);
            imported++;
            if (key is not null) alreadyImported.Add(key);
        }

        await db.SaveChangesAsync(ct);
        if (imported == 0 && skipped == 0) warnings.Add("No issues were found in the file.");
        if (noId > 0)
            warnings.Add($"{noId} row(s) had no id/key column, so they can't be de-duplicated — re-importing this file will add them again.");
        return new IssueImportSummary(imported, skipped, tagsLinked, warnings);
    }

    private static async Task<Tag?> GetOrAddTagAsync(AppDbContext db, Dictionary<string, Tag> cache, string rawName, CancellationToken ct)
    {
        var name = rawName.Trim();
        if (name.Length == 0) return null;
        if (name.Length > FieldLimits.TagName) name = name[..FieldLimits.TagName];
        if (cache.TryGetValue(name, out var cached)) return cached;

        var lowered = name.ToLowerInvariant();
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
        if (tag is null) { tag = new Tag { Name = name }; db.Tags.Add(tag); }
        cache[name] = tag;
        return tag;
    }

    private static string BuildDescription(ImportedIssue m, string source, bool reporterMapped)
    {
        var parts = new List<string>();
        if (!reporterMapped && m.Reporter is not null)
            parts.Add($"*Imported from {source} — originally reported by \"{m.Reporter}\".*");
        if (m.Description is not null) parts.Add(m.Description);
        var text = string.Join("\n\n", parts);
        return text.Length == 0 ? $"(imported from {source})" : text;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
