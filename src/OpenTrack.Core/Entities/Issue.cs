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

    /// <summary>Optional location where the problem is (captured in the browser for field/mobile reports).
    /// Null unless the reporter chose to attach their location.</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    /// <summary>When the SLA-breach escalation was sent for this issue, so the background scanner notifies
    /// once rather than every tick. Null = not yet escalated.</summary>
    public DateTime? SlaBreachNotifiedAt { get; set; }

    /// <summary>The MantisBT issue id this was imported from, if any. Lets a re-import of the same
    /// export skip issues already brought in, instead of duplicating them.</summary>
    public int? ImportedMantisId { get; set; }

    /// <summary>A stable source-scoped key for issues imported from CSV/GitHub/Jira (e.g. "github:123",
    /// "JIRA-45"), so a re-import of the same file skips rows already brought in. Null for issues not
    /// imported this way (MantisBT uses <see cref="ImportedMantisId"/>).</summary>
    public string? ImportedExternalKey { get; set; }

    /// <summary>For an issue submitted via the public trouble-ticket page: the submitter's name and
    /// email (both optional). The email also lets that person look up their ticket's status later.</summary>
    public string? IntakeName { get; set; }
    public string? IntakeEmail { get; set; }

    /// <summary>Optimistic-concurrency token. Reassigned on every update; if the value a client
    /// loaded no longer matches on save, EF raises DbUpdateConcurrencyException instead of silently
    /// overwriting a concurrent edit (lost update).</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // Navigation
    public ICollection<IssueNote> Notes { get; set; } = [];
    public ICollection<IssueAttachment> Attachments { get; set; } = [];
    public ICollection<IssueHistory> History { get; set; } = [];
    public ICollection<IssueTag> IssueTags { get; set; } = [];
    public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = [];
}
