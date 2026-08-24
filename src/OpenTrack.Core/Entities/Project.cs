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

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; } = true;

    /// <summary>Optional short uppercase key (e.g. "APRS", "WEB") used to form human-friendly ticket
    /// numbers like "APRS-42" — nicer to quote over the phone and clearer across projects than a raw id.
    /// Null/blank falls back to "#42". The numeric issue id is always the real internal key.</summary>
    public string? Key { get; set; }

    /// <summary>When true, anyone (no account) can submit a trouble ticket for this project via the
    /// public "Report a problem" page. Off by default — a Manager turns it on in project settings.</summary>
    public bool PublicIntakeEnabled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    /// <summary>Optimistic-concurrency token (reassigned on every update).</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // Navigation
    public ICollection<ProjectMembership> Members { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<ProjectVersion> Versions { get; set; } = [];
    public ICollection<Issue> Issues { get; set; } = [];
    public ICollection<CustomFieldDefinition> CustomFields { get; set; } = [];
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
}
