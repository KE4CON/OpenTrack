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

namespace OpenTrack.Core.Entities;

/// <summary>
/// A per-project automation rule: "when a new issue matches these conditions, apply these actions."
/// Conditions are ANDed; a null condition means "don't care", so a rule with no conditions matches every
/// new issue. Actions are applied only when the rule matches; a null action does nothing. All conditions
/// and actions live on the one row (no child tables) to keep the schema and the editor simple.
/// <para>v1 runs on issue <b>creation only</b> — which is why there's no risk of a rule's own action
/// re-triggering the engine.</para>
/// </summary>
public class AutomationRule
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Operator-facing name, e.g. "Auto-tag crashes".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Disabled rules are skipped by the engine but kept for editing.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Rules run in ascending order; later matching rules override earlier scalar actions.</summary>
    public int SortOrder { get; set; }

    // ---- Conditions (all must match; null = ignore) ----

    /// <summary>Case-insensitive substring that must appear in the new issue's title OR description.</summary>
    public string? WhenTextContains { get; set; }
    public IssueSeverity? WhenSeverity { get; set; }
    public IssuePriority? WhenPriority { get; set; }
    public int? WhenCategoryId { get; set; }

    // ---- Actions (applied on match; null = no-op) ----

    public IssueSeverity? SetSeverity { get; set; }
    public IssuePriority? SetPriority { get; set; }
    public IssueStatus? SetStatus { get; set; }

    /// <summary>Assign the new issue to this user (applied only if they're a member of the project).</summary>
    public int? AssignToUserId { get; set; }

    /// <summary>Add this tag (by name, created if missing) to the new issue.</summary>
    public string? AddTag { get; set; }
}
