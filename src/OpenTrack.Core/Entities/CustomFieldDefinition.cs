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

/// <summary>
/// A custom field defined by a Manager on a single project (MantisBT-style). Every issue in that
/// project can carry a value for it. The definition is owned by the project (cascade-deleted with it);
/// its <see cref="CustomFieldValue"/>s are owned by the definition and the issue.
/// </summary>
public class CustomFieldDefinition
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public CustomFieldType Type { get; set; }

    /// <summary>For <see cref="CustomFieldType.Enum"/> only: the allowed choices, one per line.
    /// Null/empty for every other type.</summary>
    public string? EnumOptions { get; set; }

    /// <summary>When true, a value is required before an issue in this project can be saved.</summary>
    public bool Required { get; set; }

    /// <summary>Ascending sort order on the manage screen and issue forms.</summary>
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CustomFieldValue> Values { get; set; } = [];
}
