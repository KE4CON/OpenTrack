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

/// <summary>What an import did.</summary>
public sealed record MantisImportSummary(
    int ProjectsCreated, int ProjectsMatched, int IssuesImported, int IssuesSkipped, int NotesImported, int TagsLinked,
    IReadOnlyList<string> Warnings);

/// <summary>
/// Imports a MantisBT XML export into OpenTrack. Migration is a privileged action — it creates
/// projects and back-dates issues — so it requires global Manager+ (the same authority needed to
/// create a project). Coded fields map 1:1 by their shared enum ids (see <see cref="MantisImport"/>);
/// Mantis users are matched to OpenTrack accounts by username where possible, otherwise the issue/note
/// is attributed to the importer with the original author preserved in the text. Original submit/update
/// dates and public/private flags are kept.
/// </summary>
public static class MantisImporter
{
    public static async Task<MantisImportSummary> ImportAsync(
        AppDbContext db, AccessSnapshot access, string xml, CancellationToken ct = default)
    {
        if (!access.GlobalAtLeast(UserRole.Manager))
            throw new UnauthorizedAccessException("Importing from MantisBT requires the Manager role.");

        var issues = MantisImport.Parse(xml); // may throw FormatException — surfaced to the caller
        var warnings = new List<string>();
        int projectsCreated = 0, projectsMatched = 0, issuesImported = 0, issuesSkipped = 0, notesImported = 0, tagsLinked = 0;

        // Match Mantis usernames to existing OpenTrack accounts (case-insensitive).
        var users = await db.Users.AsNoTracking()
            .Where(u => u.UserName != null)
            .ToDictionaryAsync(u => u.UserName!.ToLower(), u => u.Id, ct);
        // MantisBT import is a global-Manager+ migration (a trusted admin action); mapping historical
        // Mantis authors to matching OpenTrack accounts is its intended behavior, so it maps by username
        // (unlike the per-project generic importer, which restricts to members).
        int? MapUser(string? name) =>
            name is not null && users.TryGetValue(name.ToLowerInvariant(), out var id) ? id : null;

        var tagCache = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in issues.GroupBy(i => i.ProjectName, StringComparer.OrdinalIgnoreCase))
        {
            var name = group.Key.Trim();
            if (name.Length > FieldLimits.ProjectName) name = name[..FieldLimits.ProjectName];

            var lowered = name.ToLowerInvariant();
            var project = await db.Projects.FirstOrDefaultAsync(p => p.Name.ToLower() == lowered, ct);
            if (project is null)
            {
                project = new Project { Name = name, IsPublic = false, OwnerId = access.UserId, CreatedAt = DateTime.UtcNow };
                db.Projects.Add(project);
                await db.SaveChangesAsync(ct); // assign project.Id
                projectsCreated++;
            }
            else
            {
                projectsMatched++;
            }

            var categories = await db.Categories.Where(c => c.ProjectId == project.Id).ToListAsync(ct);
            Category? FindOrAddCategory(string? catName)
            {
                if (string.IsNullOrWhiteSpace(catName)) return null;
                var trimmed = catName.Trim();
                if (trimmed.Length > FieldLimits.CategoryName) trimmed = trimmed[..FieldLimits.CategoryName];
                var existing = categories.FirstOrDefault(c => string.Equals(c.Name, trimmed, StringComparison.OrdinalIgnoreCase));
                if (existing is not null) return existing;
                var created = new Category { ProjectId = project.Id, Name = trimmed };
                db.Categories.Add(created);
                categories.Add(created);
                return created;
            }

            // Which Mantis ids are already imported into this project, so a re-run skips them.
            var alreadyImported = await db.Issues
                .Where(i => i.ProjectId == project.Id && i.ImportedMantisId != null)
                .Select(i => i.ImportedMantisId!.Value)
                .ToHashSetAsync(ct);

            foreach (var m in group)
            {
                if (m.MantisId is { } existingId && alreadyImported.Contains(existingId)) { issuesSkipped++; continue; }

                var reporterId = MapUser(m.Reporter) ?? access.UserId;
                var reporterMapped = MapUser(m.Reporter) is not null;
                var now = DateTime.UtcNow;

                var issue = new Issue
                {
                    ProjectId = project.Id,
                    Title = Truncate(m.Summary, FieldLimits.IssueTitle),
                    Description = BuildDescription(m, reporterMapped),
                    StepsToReproduce = m.StepsToReproduce,
                    Status = m.Status,
                    Severity = m.Severity,
                    Priority = m.Priority,
                    Reproducibility = m.Reproducibility,
                    Resolution = m.Resolution,
                    Category = FindOrAddCategory(m.Category),
                    ReporterId = reporterId,
                    AssigneeId = MapUser(m.Handler),
                    IsPrivate = m.Private,
                    IsSticky = m.Sticky,
                    ImportedMantisId = m.MantisId,
                    CreatedAt = m.DateSubmitted ?? now,
                    UpdatedAt = m.LastUpdated ?? m.DateSubmitted ?? now,
                };
                if (m.MantisId is { } newId) alreadyImported.Add(newId); // guard against dupes within one file
                issue.History.Add(new IssueHistory
                {
                    UserId = access.UserId,
                    FieldChanged = "Imported",
                    OldValue = null,
                    NewValue = m.MantisId is { } mid ? $"MantisBT #{mid}" : "MantisBT",
                    ChangedAt = now,
                });

                foreach (var n in m.Notes)
                {
                    issue.Notes.Add(new IssueNote
                    {
                        AuthorId = MapUser(n.Reporter) ?? access.UserId,
                        Text = MapUser(n.Reporter) is null && n.Reporter is not null
                            ? $"*(originally by {n.Reporter} in MantisBT)*\n\n{n.Text}"
                            : n.Text,
                        IsPrivate = n.Private,
                        CreatedAt = n.Date ?? now,
                    });
                    notesImported++;
                }

                foreach (var tagName in m.Tags)
                {
                    var tag = await GetOrAddTagAsync(db, tagCache, tagName, ct);
                    if (tag is null) continue;
                    issue.IssueTags.Add(new IssueTag { Tag = tag });
                    tagsLinked++;
                }

                db.Issues.Add(issue);
                issuesImported++;
            }

            await db.SaveChangesAsync(ct);
        }

        if (issuesImported == 0 && issuesSkipped == 0) warnings.Add("No issues were found in the file.");
        return new MantisImportSummary(projectsCreated, projectsMatched, issuesImported, issuesSkipped, notesImported, tagsLinked, warnings);
    }

    private static async Task<Tag?> GetOrAddTagAsync(AppDbContext db, Dictionary<string, Tag> cache, string rawName, CancellationToken ct)
    {
        var name = rawName.Trim();
        if (name.Length == 0) return null;
        if (name.Length > FieldLimits.TagName) name = name[..FieldLimits.TagName];
        if (cache.TryGetValue(name, out var cached)) return cached;

        var lowered = name.ToLowerInvariant();
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
        if (tag is null)
        {
            tag = new Tag { Name = name };
            db.Tags.Add(tag);
        }
        cache[name] = tag;
        return tag;
    }

    private static string BuildDescription(MantisIssue m, bool reporterMapped)
    {
        var parts = new List<string>();
        if (!reporterMapped && m.Reporter is not null)
            parts.Add($"*Imported from MantisBT — originally reported by \"{m.Reporter}\".*");
        if (m.Description is not null) parts.Add(m.Description);
        if (m.AdditionalInformation is not null) parts.Add("**Additional information**\n\n" + m.AdditionalInformation);
        var text = string.Join("\n\n", parts);
        return text.Length == 0 ? "(imported from MantisBT)" : text;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
