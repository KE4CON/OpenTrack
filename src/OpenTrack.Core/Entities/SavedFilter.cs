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

namespace OpenTrack.Core.Entities;

/// <summary>
/// A user's named, saved issue filter. <see cref="Query"/> is the issue-list query string (without the
/// leading '?'), so applying a saved filter is just navigating to <c>/issues?{Query}</c> — and because
/// the issue list always re-applies row-level ACL, a saved (or shared) filter only ever shows the
/// viewer what they're allowed to see.
/// </summary>
public class SavedFilter
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
