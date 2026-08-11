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

namespace OpenTrack.Core.Bulk;

public enum BulkActionType { SetStatus, Close, Assign, Unassign, AddTag }

/// <summary>A single action to apply to a set of selected issues. Only the field relevant to
/// <see cref="Type"/> is used.</summary>
public record BulkAction(BulkActionType Type, IssueStatus? Status = null, int? AssigneeId = null, string? Tag = null);

/// <summary>Outcome of a bulk operation: how many issues were changed vs skipped (skipped = the caller
/// lacked the required permission on that issue, or the action didn't apply).</summary>
public record BulkResult(int Updated, int Skipped);
