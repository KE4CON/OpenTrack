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

/// <summary>A Git commit that referenced an issue (from a push webhook). Unique per (issue, commit) so a
/// re-delivered webhook doesn't duplicate the link.</summary>
public class IssueCommitLink
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    /// <summary>Full commit SHA.</summary>
    public string Sha { get; set; } = string.Empty;
    /// <summary>First line of the commit message.</summary>
    public string Message { get; set; } = string.Empty;
    public string? Author { get; set; }
    /// <summary>Link back to the commit at the host (from the webhook payload).</summary>
    public string? Url { get; set; }
    /// <summary>True when this commit used a closing keyword ("fixes #123") for this issue.</summary>
    public bool Closing { get; set; }

    public DateTime CommittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
