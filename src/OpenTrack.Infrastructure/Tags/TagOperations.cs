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
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Tags;

public readonly record struct TagItem(int Id, string Name);

/// <summary>
/// ACL-aware tag operations on issues, shared by both the Web API and the web/EF data service so the
/// authorization logic exists once. Viewing an issue's tags requires view access to that issue;
/// adding/removing a tag requires edit access (Updater+). Tags themselves are global — a new tag is
/// created on first use (create-on-assign, MantisBT-style), reusing an existing tag case-insensitively.
/// </summary>
public static class TagOperations
{
    public static async Task<IReadOnlyList<TagItem>> ListForIssueAsync(AppDbContext db, AccessSnapshot access, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return [];
        return await db.IssueTags.AsNoTracking()
            .Where(it => it.IssueId == issueId)
            .OrderBy(it => it.Tag.Name)
            .Select(it => new TagItem(it.TagId, it.Tag.Name))
            .ToListAsync(ct);
    }

    public static async Task<string?> AddAsync(AppDbContext db, AccessSnapshot access, int issueId, string tagName, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return "Issue not found.";
        if (!access.For(issue.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("Tagging an issue requires the Updater role on its project.");

        var name = tagName.Trim();
        if (name.Length == 0) return "Tag name is required.";
        if (name.Length > FieldLimits.TagName) return $"Tag name must be {FieldLimits.TagName} characters or fewer.";

        // Reuse an existing tag case-insensitively, else create it (create-on-assign).
        var lowered = name.ToLower();
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
        if (tag is null)
        {
            tag = new Tag { Name = name };
            db.Tags.Add(tag);
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException) // unique-index race: another request created the same tag
            {
                db.Entry(tag).State = EntityState.Detached;
                tag = await db.Tags.FirstAsync(t => t.Name.ToLower() == lowered, ct);
            }
        }

        if (await db.IssueTags.AnyAsync(it => it.IssueId == issueId && it.TagId == tag.Id, ct))
            return null; // already tagged — idempotent

        db.IssueTags.Add(new IssueTag { IssueId = issueId, TagId = tag.Id });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { /* already linked concurrently — fine */ }
        return null;
    }

    public static async Task<bool> RemoveAsync(AppDbContext db, AccessSnapshot access, int issueId, int tagId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return false;
        if (!access.For(issue.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("Untagging an issue requires the Updater role on its project.");

        var link = await db.IssueTags.FirstOrDefaultAsync(it => it.IssueId == issueId && it.TagId == tagId, ct);
        if (link is null) return false;
        db.IssueTags.Remove(link);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>All tag names, for the filter dropdown / autocomplete. Tag names aren't sensitive;
    /// what an issue reveals is still governed by that issue's ACL when it is viewed.</summary>
    public static async Task<IReadOnlyList<TagItem>> ListAllAsync(AppDbContext db, CancellationToken ct = default) =>
        await db.Tags.AsNoTracking().OrderBy(t => t.Name).Select(t => new TagItem(t.Id, t.Name)).ToListAsync(ct);

    private static bool CanView(AccessSnapshot access, Issue issue) =>
        access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId);
}
