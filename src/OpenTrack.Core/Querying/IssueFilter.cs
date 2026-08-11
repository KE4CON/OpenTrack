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

namespace OpenTrack.Core.Querying;

public enum IssueSort { UpdatedDesc, UpdatedAsc, CreatedDesc, CreatedAsc, PriorityDesc, SeverityDesc, StatusAsc, IdDesc, IdAsc }

/// <summary>
/// Criteria for filtering/sorting the issue list. Every field is optional — a null field means "any".
/// <see cref="Text"/> matches the title or description (case-insensitive contains). Lives in Core so
/// the same filter is applied by both the Web API and the web/EF data service (via the shared
/// IssueQueries.ApplyFilter extension) and can't drift. The ACL is always applied FIRST, so filtering
/// can only ever narrow what a user is already allowed to see.
/// </summary>
public record IssueFilter(
    int? ProjectId = null,
    IssueStatus? Status = null,
    IssueSeverity? Severity = null,
    IssuePriority? Priority = null,
    int? AssigneeId = null,
    int? CategoryId = null,
    string? Text = null,
    int? TagId = null,
    IssueSort Sort = IssueSort.UpdatedDesc,
    // When set, keep only "stale" issues: still open (below Resolved) and not updated since this instant.
    DateTime? StaleBeforeUtc = null);

/// <summary>Shared defaults for issue querying.</summary>
public static class IssueDefaults
{
    /// <summary>How long an open issue can sit untouched before it's considered stale.</summary>
    public const int StaleDays = 30;
}
