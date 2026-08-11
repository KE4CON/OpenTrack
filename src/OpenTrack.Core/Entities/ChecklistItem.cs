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
/// One item on a project's bug-hunt checklist — a thing to verify. You work through the list marking
/// each Pass / Fail / N-A with optional notes; a Fail is typically turned into a linked issue (see
/// <see cref="LinkedIssueId"/>). The list lives alongside — not inside — the normal issue list, so any
/// unlisted problem is still logged as an ordinary issue.
/// </summary>
public class ChecklistItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>The thing to check.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Optional longer guidance on how/what to check.</summary>
    public string? Details { get; set; }
    /// <summary>Optional grouping heading (e.g. "Concurrency", "RF identity"); items sort within it.</summary>
    public string? Area { get; set; }

    public ChecklistItemStatus Status { get; set; } = ChecklistItemStatus.Pending;
    /// <summary>What you found when checking (especially for a Fail).</summary>
    public string? Notes { get; set; }

    /// <summary>The issue spawned from this item's failure, if any. Set-null on that issue's delete.</summary>
    public int? LinkedIssueId { get; set; }
    public Issue? LinkedIssue { get; set; }

    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>When the status last moved off Pending.</summary>
    public DateTime? CheckedAt { get; set; }
}
