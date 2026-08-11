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

/// <summary>A free-form label that can be attached to issues. Tags are global (shared across
/// projects, MantisBT-style); a tag name is not sensitive. Which issues a tag reveals is still
/// governed by each issue's access control.</summary>
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<IssueTag> IssueTags { get; set; } = [];
}

/// <summary>Join row linking a <see cref="Tag"/> to an <see cref="Issue"/>.</summary>
public class IssueTag
{
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
