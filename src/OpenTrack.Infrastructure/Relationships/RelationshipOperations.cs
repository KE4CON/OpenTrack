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
using OpenTrack.Core.Authorization;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Relationships;

/// <summary>One relationship as seen from the viewed issue: the resolved label and the OTHER issue.</summary>
public readonly record struct RelationshipItem(int Id, int OtherIssueId, string OtherIssueTitle, string OtherProjectName, string Label);

/// <summary>
/// The ACL-aware read/add/remove operations for issue relationships. Shared by both the Web API and
/// the web/EF data service so this security-sensitive logic exists once and can't drift. Key rules:
/// the related-issue list only ever includes issues the viewer may see (so a private issue's existence
/// isn't leaked), adding requires edit on the source + view on the target, and removing requires edit
/// on either linked issue. Adds throw <see cref="UnauthorizedAccessException"/> for a forbidden edit
/// and return a message for a validation problem (unknown/private target, self, duplicate).
/// </summary>
public static class RelationshipOperations
{
    public static async Task<IReadOnlyList<RelationshipItem>> ListAsync(
        AppDbContext db, AccessSnapshot access, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return [];

        var rels = await db.IssueRelationships.AsNoTracking()
            .Where(r => r.SourceIssueId == issueId || r.TargetIssueId == issueId)
            .Include(r => r.SourceIssue).ThenInclude(i => i.Project)
            .Include(r => r.TargetIssue).ThenInclude(i => i.Project)
            .ToListAsync(ct);

        var items = new List<RelationshipItem>();
        foreach (var r in rels)
        {
            var viewerIsSource = r.SourceIssueId == issueId;
            var other = viewerIsSource ? r.TargetIssue : r.SourceIssue;
            if (!CanView(access, other)) continue; // never leak a private issue's existence
            items.Add(new RelationshipItem(r.Id, other.Id, other.Title, other.Project.Name,
                RelationshipLabels.Describe(r.Type, viewerIsSource)));
        }
        return items.OrderBy(i => i.Label).ThenBy(i => i.OtherIssueId).ToList();
    }

    public static async Task<string?> AddAsync(
        AppDbContext db, AccessSnapshot access, int sourceIssueId, int targetIssueId, IssueRelationshipType type, CancellationToken ct = default)
    {
        if (sourceIssueId == targetIssueId) return "An issue can't be related to itself.";

        var source = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == sourceIssueId, ct);
        if (source is null || !CanView(access, source)) return "Issue not found.";
        if (!access.For(source.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("Adding a relationship requires the Updater role on this issue's project.");

        var target = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == targetIssueId, ct);
        // Report "not found" (not "no access") so a private issue's existence is never revealed.
        if (target is null || !CanView(access, target)) return $"Issue #{targetIssueId} not found.";

        // Store symmetric RelatedTo in a canonical direction (min→max) so the unique index blocks
        // duplicates atomically — A↔B and B↔A collapse to one row regardless of who added it.
        var (storeSource, storeTarget) = type == IssueRelationshipType.RelatedTo && sourceIssueId > targetIssueId
            ? (targetIssueId, sourceIssueId)
            : (sourceIssueId, targetIssueId);

        if (await db.IssueRelationships.AnyAsync(r => r.SourceIssueId == storeSource && r.TargetIssueId == storeTarget && r.Type == type, ct))
            return "That relationship already exists.";

        db.IssueRelationships.Add(new IssueRelationship
        {
            SourceIssueId = storeSource, TargetIssueId = storeTarget, Type = type,
            CreatedById = access.UserId, CreatedAt = DateTime.UtcNow
        });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return "That relationship already exists."; } // unique index (concurrent add)
        return null;
    }

    /// <summary>Removes the relationship. Returns false if it doesn't exist; throws if the caller may
    /// not edit either linked issue.</summary>
    public static async Task<bool> RemoveAsync(AppDbContext db, AccessSnapshot access, int relationshipId, CancellationToken ct = default)
    {
        var r = await db.IssueRelationships
            .Include(x => x.SourceIssue).ThenInclude(i => i.Project)
            .Include(x => x.TargetIssue).ThenInclude(i => i.Project)
            .FirstOrDefaultAsync(x => x.Id == relationshipId, ct);
        if (r is null) return false;

        // If the caller can't view EITHER endpoint, treat as not-found so a denied delete is
        // indistinguishable from a nonexistent id (no existence signal over relationship ids).
        if (!CanView(access, r.SourceIssue) && !CanView(access, r.TargetIssue))
            return false;
        if (!CanEdit(access, r.SourceIssue) && !CanEdit(access, r.TargetIssue))
            throw new UnauthorizedAccessException("Removing a relationship requires the Updater role on one of the linked issues.");

        db.IssueRelationships.Remove(r);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static bool CanView(AccessSnapshot access, Issue issue) =>
        access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId);

    private static bool CanEdit(AccessSnapshot access, Issue issue) =>
        CanView(access, issue) && access.For(issue.ProjectId).CanEditIssue();
}
