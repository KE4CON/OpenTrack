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

namespace OpenTrack.Core.Enums;

/// <summary>
/// How one issue relates to another. Stored once as a directed row (source → target). Most types have
/// a reciprocal label shown from the target's side (see <see cref="RelationshipLabels"/>):
/// duplicate-of ↔ has-duplicate, parent-of ↔ child-of, blocks ↔ blocked-by. Related-to is symmetric.
/// </summary>
public enum IssueRelationshipType
{
    RelatedTo = 0,
    DuplicateOf = 1,
    ParentOf = 2,
    Blocks = 3,
}

/// <summary>Human-readable labels for a relationship, from the perspective of the issue being viewed.</summary>
public static class RelationshipLabels
{
    /// <summary>The label to show on the issue currently being viewed.
    /// <paramref name="viewerIsSource"/> is true when the viewed issue is the relationship's source.</summary>
    public static string Describe(IssueRelationshipType type, bool viewerIsSource) => type switch
    {
        IssueRelationshipType.RelatedTo => "related to",
        IssueRelationshipType.DuplicateOf => viewerIsSource ? "duplicate of" : "has duplicate",
        IssueRelationshipType.ParentOf => viewerIsSource ? "parent of" : "child of",
        IssueRelationshipType.Blocks => viewerIsSource ? "blocks" : "blocked by",
        _ => "related to",
    };
}
