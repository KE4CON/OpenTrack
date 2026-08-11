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

/// <summary>A directed relationship from one issue (Source) to another (Target). Stored once; the
/// reciprocal is derived for display (see <see cref="RelationshipLabels"/>).</summary>
public class IssueRelationship
{
    public int Id { get; set; }

    public int SourceIssueId { get; set; }
    public Issue SourceIssue { get; set; } = null!;

    public int TargetIssueId { get; set; }
    public Issue TargetIssue { get; set; } = null!;

    public IssueRelationshipType Type { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
