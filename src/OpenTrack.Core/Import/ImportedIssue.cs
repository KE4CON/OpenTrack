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

namespace OpenTrack.Core.Import;

/// <summary>
/// A source-agnostic issue produced by any importer (CSV, GitHub, Jira export). Coded fields are nullable
/// so the importer can fall back to OpenTrack defaults when a source didn't provide (or we couldn't map)
/// them. <see cref="ExternalId"/> is a stable per-source key ("github:123", "JIRA-45") used to skip
/// duplicates on a re-import.
/// </summary>
public sealed record ImportedIssue(
    string Title,
    string? Description,
    IssueStatus? Status,
    IssueSeverity? Severity,
    IssuePriority? Priority,
    string? Category,
    string? Reporter,
    string? Assignee,
    IReadOnlyList<string> Tags,
    DateTime? CreatedAt,
    string? ExternalId);

/// <summary>
/// Best-effort mapping of free-text status/severity/priority values (from CSV headers, Jira, GitHub, …) to
/// OpenTrack's enums. Tries the enum name first, then a set of common synonyms; returns null when nothing
/// fits, so the importer applies a sensible default rather than guessing wrong.
/// </summary>
public static class ImportMapping
{
    public static IssueStatus? Status(string? value) => Normalize(value) switch
    {
        null => null,
        "new" or "open" or "todo" or "to do" or "backlog" or "reopened" => IssueStatus.New,
        "feedback" or "needs info" or "waiting" => IssueStatus.Feedback,
        "acknowledged" or "triaged" => IssueStatus.Acknowledged,
        "confirmed" or "in progress" or "in-progress" or "started" or "doing" => IssueStatus.Confirmed,
        "assigned" or "in review" or "review" => IssueStatus.Assigned,
        "resolved" or "fixed" or "done" or "complete" or "completed" => IssueStatus.Resolved,
        "closed" or "cancelled" or "canceled" or "wontfix" or "won't fix" => IssueStatus.Closed,
        var s => Enum.TryParse<IssueStatus>(s, ignoreCase: true, out var e) ? e : null,
    };

    public static IssueSeverity? Severity(string? value) => Normalize(value) switch
    {
        null => null,
        "block" or "blocker" or "blocking" => IssueSeverity.Block,
        "crash" or "critical" or "fatal" => IssueSeverity.Crash,
        "major" or "high" or "serious" => IssueSeverity.Major,
        "minor" or "medium" or "moderate" or "normal" => IssueSeverity.Minor,
        "tweak" => IssueSeverity.Tweak,
        "text" or "typo" => IssueSeverity.Text,
        "trivial" or "low" => IssueSeverity.Trivial,
        "feature" or "enhancement" or "improvement" => IssueSeverity.Feature,
        var s => Enum.TryParse<IssueSeverity>(s, ignoreCase: true, out var e) ? e : null,
    };

    public static IssuePriority? Priority(string? value) => Normalize(value) switch
    {
        null => null,
        "immediate" or "blocker" or "p0" => IssuePriority.Immediate,
        "urgent" or "highest" or "critical" or "p1" => IssuePriority.Urgent,
        "high" or "p2" => IssuePriority.High,
        "normal" or "medium" or "moderate" or "p3" => IssuePriority.Normal,
        "low" or "minor" or "p4" => IssuePriority.Low,
        "none" or "lowest" or "trivial" or "p5" => IssuePriority.None,
        var s => Enum.TryParse<IssuePriority>(s, ignoreCase: true, out var e) ? e : null,
    };

    private static string? Normalize(string? value)
    {
        var v = value?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}
