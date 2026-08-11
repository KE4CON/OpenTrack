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

/// <summary>One block of time logged against an issue by a user.</summary>
public class TimeLog
{
    public int Id { get; set; }

    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Minutes worked (always &gt; 0).</summary>
    public int Minutes { get; set; }
    public string? Note { get; set; }
    /// <summary>The date the work was done (defaults to when it was logged).</summary>
    public DateTime WorkedOn { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
