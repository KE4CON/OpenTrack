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
using OpenTrack.Core.Automation;
using OpenTrack.Core.Entities;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Automation;

/// <summary>
/// Runs a project's automation rules against a newly-created issue. Called from BOTH hosts' create paths
/// (right after the issue is first saved, so it has an id) with the same tracking <see cref="AppDbContext"/>
/// the create used. Rule matching itself is the pure <see cref="AutomationEvaluator"/>; this class only
/// loads the rules, applies the resulting mutations to the tracked issue, and persists.
/// <para>Runs on creation only, so a rule's own action can never re-trigger the engine. The engine acts
/// with system authority (the rules were authored by a project Manager) — it does not re-check the acting
/// user's edit rights — but it will not assign to a user who isn't a member of the project.</para>
/// </summary>
public static class AutomationEngine
{
    /// <summary>Apply matching create-time rules to <paramref name="issue"/> (already added &amp; saved on
    /// <paramref name="db"/>). Returns the names of the rules that fired (empty if none / feature unused).</summary>
    public static async Task<IReadOnlyList<string>> RunOnCreateAsync(AppDbContext db, Issue issue, CancellationToken ct = default)
    {
        var rules = await db.AutomationRules.AsNoTracking()
            .Where(r => r.ProjectId == issue.ProjectId && r.IsEnabled)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .ToListAsync(ct);
        if (rules.Count == 0) return [];

        var outcome = AutomationEvaluator.Evaluate(
            new AutomationInput(issue.Title, issue.Description, issue.Severity, issue.Priority, issue.CategoryId),
            rules.Select(ToDef));
        if (!outcome.AnyEffect) return outcome.AppliedRuleNames;

        var changed = false;
        if (outcome.Severity is { } sev && issue.Severity != sev) { issue.Severity = sev; changed = true; }
        if (outcome.Priority is { } pri && issue.Priority != pri) { issue.Priority = pri; changed = true; }
        if (outcome.Status is { } st && issue.Status != st) { issue.Status = st; changed = true; }
        if (outcome.AssignToUserId is { } uid && issue.AssigneeId != uid)
        {
            // Only auto-assign to an actual member of the project (a member may have been removed since
            // the rule was written — don't assign to someone who can no longer see the project).
            if (await db.ProjectMemberships.AnyAsync(m => m.ProjectId == issue.ProjectId && m.UserId == uid, ct))
            {
                issue.AssigneeId = uid;
                changed = true;
            }
        }

        foreach (var tagName in outcome.AddTags)
            changed |= await AddTagAsync(db, issue.Id, tagName, ct);

        if (changed)
        {
            issue.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return outcome.AppliedRuleNames;
    }

    private static AutomationRuleDef ToDef(AutomationRule r) => new(
        r.Name, r.WhenTextContains, r.WhenSeverity, r.WhenPriority, r.WhenCategoryId,
        r.SetSeverity, r.SetPriority, r.SetStatus, r.AssignToUserId, r.AddTag);

    /// <summary>Get-or-create a tag by name (case-insensitive) and link it to the issue, idempotently.
    /// Unlike TagOperations this does not gate on the acting user's edit rights — automation acts with
    /// system authority. Returns true if a new link was added.</summary>
    private static async Task<bool> AddTagAsync(AppDbContext db, int issueId, string tagName, CancellationToken ct)
    {
        var name = tagName.Trim();
        if (name.Length == 0) return false;

        var lowered = name.ToLowerInvariant();
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
        if (tag is null)
        {
            tag = new Tag { Name = name };
            db.Tags.Add(tag);
            try { await db.SaveChangesAsync(ct); }
            catch (DbUpdateException) // unique-index race: another writer created it
            {
                db.Entry(tag).State = EntityState.Detached;
                tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
                if (tag is null) throw;
            }
        }

        if (await db.IssueTags.AnyAsync(it => it.IssueId == issueId && it.TagId == tag.Id, ct))
            return false; // already tagged
        db.IssueTags.Add(new IssueTag { IssueId = issueId, TagId = tag.Id });
        return true;
    }
}
