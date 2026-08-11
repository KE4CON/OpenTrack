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
using OpenTrack.Core.Checklists;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Checklist;

/// <summary>A checklist item as read for display.</summary>
public readonly record struct ChecklistItemDto(
    int Id, int ProjectId, string Title, string? Details, string? Area,
    ChecklistItemStatus Status, string? Notes, int? LinkedIssueId, int DisplayOrder);

/// <summary>
/// ACL-aware bug-hunt-checklist operations, shared by the Web API and the web/EF data service so the
/// authorization lives once. Defining the checklist (add/import/edit/delete items) is a project-
/// management action (Manager+, like categories/custom fields); reading it needs project view; working
/// through it (setting Pass/Fail/N-A + notes, and converting a failure into an issue) follows the same
/// edit authority as changing any issue on that project (Updater+).
/// </summary>
public static class ChecklistOperations
{
    // ---- Read (project view) ----

    public static async Task<IReadOnlyList<ChecklistItemDto>> ListForProjectAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return [];
        return await db.ChecklistItems.AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id)
            .Select(c => new ChecklistItemDto(
                c.Id, c.ProjectId, c.Title, c.Details, c.Area, c.Status, c.Notes, c.LinkedIssueId, c.DisplayOrder))
            .ToListAsync(ct);
    }

    // ---- Define the checklist (Manager+) ----

    public static async Task<string?> AddItemAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string title, string? details, string? area,
        CancellationToken ct = default)
    {
        RequireManage(access, projectId);
        if (!await db.Projects.AnyAsync(p => p.Id == projectId, ct)) return "Project not found.";

        var error = Normalize(ref title, ref details, ref area);
        if (error is not null) return error;

        var order = (await db.ChecklistItems.Where(c => c.ProjectId == projectId)
            .Select(c => (int?)c.DisplayOrder).MaxAsync(ct) ?? 0) + 1;
        db.ChecklistItems.Add(new ChecklistItem { ProjectId = projectId, Title = title, Details = details, Area = area, DisplayOrder = order });
        await db.SaveChangesAsync(ct);
        return null;
    }

    /// <summary>Bulk-add items parsed from pasted text (Markdown headings become areas). Returns how
    /// many were added.</summary>
    public static async Task<int> ImportAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string? text, CancellationToken ct = default)
    {
        RequireManage(access, projectId);
        if (!await db.Projects.AnyAsync(p => p.Id == projectId, ct)) return 0;

        var parsed = ChecklistImport.Parse(text);
        if (parsed.Count == 0) return 0;

        var order = await db.ChecklistItems.Where(c => c.ProjectId == projectId)
            .Select(c => (int?)c.DisplayOrder).MaxAsync(ct) ?? 0;
        var added = 0;
        foreach (var p in parsed)
        {
            var title = Truncate(p.Title.Trim(), FieldLimits.ChecklistTitle);
            if (title.Length == 0) continue;
            var area = string.IsNullOrWhiteSpace(p.Area) ? null : Truncate(p.Area.Trim(), FieldLimits.ChecklistArea);
            db.ChecklistItems.Add(new ChecklistItem { ProjectId = projectId, Title = title, Area = area, DisplayOrder = ++order });
            added++;
        }
        if (added > 0) await db.SaveChangesAsync(ct);
        return added;
    }

    public static async Task<string?> UpdateItemAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int itemId, string title, string? details, string? area,
        CancellationToken ct = default)
    {
        RequireManage(access, projectId);
        var item = await db.ChecklistItems.FirstOrDefaultAsync(c => c.Id == itemId && c.ProjectId == projectId, ct);
        if (item is null) return "Checklist item not found.";

        var error = Normalize(ref title, ref details, ref area);
        if (error is not null) return error;

        item.Title = title; item.Details = details; item.Area = area;
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteItemAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int itemId, CancellationToken ct = default)
    {
        RequireManage(access, projectId);
        var item = await db.ChecklistItems.FirstOrDefaultAsync(c => c.Id == itemId && c.ProjectId == projectId, ct);
        if (item is null) return false;
        db.ChecklistItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ---- Work through the checklist (Updater+ on the project) ----

    /// <summary>Set an item's status and (optionally) its notes. A null <paramref name="notes"/> leaves
    /// the existing notes untouched — so a quick Pass/Fail tap on a tablet never wipes what you already
    /// wrote; pass an empty string to clear notes explicitly.</summary>
    public static async Task<string?> SetStatusAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int itemId, ChecklistItemStatus status, string? notes,
        CancellationToken ct = default)
    {
        var item = await LoadWorkableAsync(db, access, projectId, itemId, ct);
        if (item is null) return "Checklist item not found.";

        item.Status = status;
        if (notes is not null)
            item.Notes = notes.Trim().Length == 0 ? null : Truncate(notes.Trim(), FieldLimits.ChecklistText);
        item.CheckedAt = status == ChecklistItemStatus.Pending ? null : DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return null;
    }

    /// <summary>Turn a checklist item into a normal issue (constructed exactly like a hand-created one),
    /// mark the item Failed, and link it. Idempotent: if already linked, returns the existing issue id.</summary>
    public static async Task<int?> ConvertToIssueAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int itemId, CancellationToken ct = default)
    {
        var item = await LoadWorkableAsync(db, access, projectId, itemId, ct);
        if (item is null) return null;
        if (item.LinkedIssueId is { } existing) return existing;

        var now = DateTime.UtcNow;
        var issue = new Issue
        {
            ProjectId = item.ProjectId,
            Title = Truncate(item.Title, FieldLimits.IssueTitle),
            Description = BuildIssueDescription(item),
            ReporterId = access.UserId,
            Status = IssueStatus.New,
            CreatedAt = now,
            UpdatedAt = now,
        };
        issue.History.Add(new IssueHistory
        {
            UserId = access.UserId,
            FieldChanged = "Status",
            OldValue = null,
            NewValue = IssueStatus.New.ToString(),
            ChangedAt = now,
        });
        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct); // assigns issue.Id

        item.LinkedIssueId = issue.Id;
        item.Status = ChecklistItemStatus.Fail;
        item.CheckedAt = now;
        await db.SaveChangesAsync(ct);
        return issue.Id;
    }

    // ---- helpers ----

    private static void RequireManage(AccessSnapshot access, int projectId)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing the checklist requires the Manager role on this project.");
    }

    /// <summary>Load an item for a "work" action: the project must be viewable AND the caller must have
    /// edit authority (Updater+), matching who may change an issue on that project.</summary>
    private static async Task<ChecklistItem?> LoadWorkableAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int itemId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        var ctx = access.For(projectId);
        if (project is null || !ctx.CanViewProject(project.IsPublic))
            throw new UnauthorizedAccessException("You don't have access to this project's checklist.");
        if (!ctx.CanEditIssue())
            throw new UnauthorizedAccessException("Working the checklist requires the Updater role on this project.");
        return await db.ChecklistItems.FirstOrDefaultAsync(c => c.Id == itemId && c.ProjectId == projectId, ct);
    }

    private static string? Normalize(ref string title, ref string? details, ref string? area)
    {
        title = title.Trim();
        if (title.Length == 0) return "Item title is required.";
        if (title.Length > FieldLimits.ChecklistTitle) return $"Item title must be {FieldLimits.ChecklistTitle} characters or fewer.";
        details = string.IsNullOrWhiteSpace(details) ? null : details.Trim();
        if (details is { Length: > FieldLimits.ChecklistText }) return $"Details must be {FieldLimits.ChecklistText} characters or fewer.";
        area = string.IsNullOrWhiteSpace(area) ? null : area.Trim();
        if (area is { Length: > FieldLimits.ChecklistArea }) return $"Area must be {FieldLimits.ChecklistArea} characters or fewer.";
        return null;
    }

    private static string BuildIssueDescription(ChecklistItem item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(item.Area)) parts.Add($"Checklist area: {item.Area}");
        if (!string.IsNullOrWhiteSpace(item.Details)) parts.Add(item.Details!);
        if (!string.IsNullOrWhiteSpace(item.Notes)) parts.Add($"What was found: {item.Notes}");
        parts.Add($"(Created from bug-hunt checklist item: “{item.Title}”.)");
        return string.Join("\n\n", parts);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
