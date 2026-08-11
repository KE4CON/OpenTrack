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

using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Automation;

/// <summary>The condition/action shape of one rule, decoupled from the EF entity so the evaluator is a
/// pure function the infrastructure and tests can both drive.</summary>
public sealed record AutomationRuleDef(
    string Name,
    string? WhenTextContains,
    IssueSeverity? WhenSeverity,
    IssuePriority? WhenPriority,
    int? WhenCategoryId,
    IssueSeverity? SetSeverity,
    IssuePriority? SetPriority,
    IssueStatus? SetStatus,
    int? AssignToUserId,
    string? AddTag);

/// <summary>The as-created state of an issue that rules match against.</summary>
public sealed record AutomationInput(
    string Title, string? Description, IssueSeverity Severity, IssuePriority Priority, int? CategoryId);

/// <summary>The net effect of all matching rules: which scalar fields to change (null = leave as-is),
/// which tags to add, and the names of the rules that fired (for logging/telemetry).</summary>
public sealed record AutomationOutcome(
    IssueSeverity? Severity, IssuePriority? Priority, IssueStatus? Status,
    int? AssignToUserId, IReadOnlyList<string> AddTags, IReadOnlyList<string> AppliedRuleNames)
{
    public static readonly AutomationOutcome None = new(null, null, null, null, [], []);
    public bool AnyEffect =>
        Severity is not null || Priority is not null || Status is not null ||
        AssignToUserId is not null || AddTags.Count > 0;
}

/// <summary>
/// Pure evaluation of automation rules against a newly-created issue. Rules are considered in the order
/// given; every rule's conditions are tested against the ORIGINAL issue state (not the running result),
/// so the outcome doesn't depend on subtle action/condition interplay. For scalar actions the last
/// matching rule wins; tags accumulate (de-duplicated, case-insensitive).
/// </summary>
public static class AutomationEvaluator
{
    public static AutomationOutcome Evaluate(AutomationInput input, IEnumerable<AutomationRuleDef> rules)
    {
        IssueSeverity? severity = null;
        IssuePriority? priority = null;
        IssueStatus? status = null;
        int? assignee = null;
        var tags = new List<string>();
        var applied = new List<string>();

        foreach (var r in rules)
        {
            if (!Matches(input, r)) continue;
            applied.Add(r.Name);

            if (r.SetSeverity is { } s) severity = s;
            if (r.SetPriority is { } p) priority = p;
            if (r.SetStatus is { } st) status = st;
            if (r.AssignToUserId is { } a) assignee = a;
            if (!string.IsNullOrWhiteSpace(r.AddTag))
            {
                var tag = r.AddTag.Trim();
                if (!tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
                    tags.Add(tag);
            }
        }

        return new AutomationOutcome(severity, priority, status, assignee, tags, applied);
    }

    private static bool Matches(AutomationInput i, AutomationRuleDef r)
    {
        if (!string.IsNullOrWhiteSpace(r.WhenTextContains))
        {
            var needle = r.WhenTextContains.Trim();
            var inTitle = i.Title?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true;
            var inDesc = i.Description?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true;
            if (!inTitle && !inDesc) return false;
        }
        if (r.WhenSeverity is { } sev && i.Severity != sev) return false;
        if (r.WhenPriority is { } pri && i.Priority != pri) return false;
        if (r.WhenCategoryId is { } cat && i.CategoryId != cat) return false;
        return true;
    }
}
