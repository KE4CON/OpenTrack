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
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;

namespace OpenTrack.Infrastructure.Automation;

/// <summary>All fields of an automation rule as supplied by the editor (create or update).</summary>
public sealed record AutomationRuleInput(
    string Name, bool IsEnabled, int SortOrder,
    string? WhenTextContains, IssueSeverity? WhenSeverity, IssuePriority? WhenPriority, int? WhenCategoryId,
    IssueSeverity? SetSeverity, IssuePriority? SetPriority, IssueStatus? SetStatus,
    int? AssignToUserId, string? AddTag);

/// <summary>
/// Shared, Manager-gated CRUD for a project's automation rules — called by both hosts so the rules and
/// their authorization can't drift (mirrors <c>WorkflowOperations</c>). Reads return an empty list for a
/// non-manager; writes throw <see cref="UnauthorizedAccessException"/>. Returns a non-null string to mean
/// "rejected with this reason"; null means success.
/// </summary>
public static class AutomationRuleOperations
{
    public static async Task<IReadOnlyList<AutomationRule>> ListForProjectAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        // Rules are project configuration — only a manager of the project may see/edit them.
        if (!access.For(projectId).CanManageProject()) return [];
        return await db.AutomationRules.AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .ToListAsync(ct);
    }

    public static async Task<string?> CreateAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId, AutomationRuleInput input, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing automation rules requires the Manager role on this project.");
        if (await Validate(db, projectId, input) is { } error) return error;

        var rule = new AutomationRule { ProjectId = projectId };
        Apply(rule, input);
        db.AutomationRules.Add(rule);
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<string?> UpdateAsync(
        Data.AppDbContext db, AccessSnapshot access, int ruleId, AutomationRuleInput input, CancellationToken ct = default)
    {
        var rule = await db.AutomationRules.FirstOrDefaultAsync(r => r.Id == ruleId, ct);
        if (rule is null) return "Rule not found.";
        if (!access.For(rule.ProjectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing automation rules requires the Manager role on this project.");
        if (await Validate(db, rule.ProjectId, input) is { } error) return error;

        Apply(rule, input);
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteAsync(
        Data.AppDbContext db, AccessSnapshot access, int ruleId, CancellationToken ct = default)
    {
        var rule = await db.AutomationRules.FirstOrDefaultAsync(r => r.Id == ruleId, ct);
        if (rule is null) return false;
        if (!access.For(rule.ProjectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing automation rules requires the Manager role on this project.");
        db.AutomationRules.Remove(rule);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static async Task<string?> Validate(Data.AppDbContext db, int projectId, AutomationRuleInput input)
    {
        var name = input.Name?.Trim() ?? "";
        if (name.Length == 0) return "Rule name is required.";
        if (name.Length > FieldLimits.ProjectName) return $"Rule name must be {FieldLimits.ProjectName} characters or fewer.";
        if (!string.IsNullOrWhiteSpace(input.AddTag) && input.AddTag.Trim().Length > FieldLimits.TagName)
            return $"The tag to add must be {FieldLimits.TagName} characters or fewer.";
        // A category condition must belong to this project (defense in depth against a spoofed id).
        if (input.WhenCategoryId is { } cid &&
            !await db.Categories.AnyAsync(c => c.Id == cid && c.ProjectId == projectId))
            return "The selected category isn't part of this project.";
        return null;
    }

    private static void Apply(AutomationRule rule, AutomationRuleInput input)
    {
        rule.Name = input.Name.Trim();
        rule.IsEnabled = input.IsEnabled;
        rule.SortOrder = input.SortOrder;
        rule.WhenTextContains = Blank(input.WhenTextContains);
        rule.WhenSeverity = input.WhenSeverity;
        rule.WhenPriority = input.WhenPriority;
        rule.WhenCategoryId = input.WhenCategoryId;
        rule.SetSeverity = input.SetSeverity;
        rule.SetPriority = input.SetPriority;
        rule.SetStatus = input.SetStatus;
        rule.AssignToUserId = input.AssignToUserId;
        rule.AddTag = Blank(input.AddTag);
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
