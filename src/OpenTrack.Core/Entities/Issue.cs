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

public class Issue
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? StepsToReproduce { get; set; }
    public string? ExpectedBehavior { get; set; }
    public string? ActualBehavior { get; set; }

    public IssueStatus Status { get; set; } = IssueStatus.New;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;
    public IssuePriority Priority { get; set; } = IssuePriority.Normal;
    public IssueReproducibility Reproducibility { get; set; } = IssueReproducibility.HaveNotTried;
    public IssueResolution Resolution { get; set; } = IssueResolution.Open;

    public bool IsPrivate { get; set; }
    public bool IsSticky { get; set; }

    // Project / categorisation
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // People
    public int ReporterId { get; set; }
    public User Reporter { get; set; } = null!;
    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    // Versions
    public int? AffectsVersionId { get; set; }
    public ProjectVersion? AffectsVersion { get; set; }
    public int? FixVersionId { get; set; }
    public ProjectVersion? FixVersion { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Navigation
    public ICollection<IssueNote> Notes { get; set; } = [];
    public ICollection<IssueAttachment> Attachments { get; set; } = [];
    public ICollection<IssueHistory> History { get; set; } = [];
}
