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

using OpenTrack.Core.Querying;

namespace OpenTrack.Core.Entities;

/// <summary>A user's personal defaults. One row per user (keyed by <see cref="UserId"/>).</summary>
public class UserPreference
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Pre-selected project when quick-adding a new issue.</summary>
    public int? DefaultProjectId { get; set; }

    /// <summary>Default sort order applied to the issue list when the URL doesn't specify one.</summary>
    public IssueSort? DefaultSort { get; set; }
}
