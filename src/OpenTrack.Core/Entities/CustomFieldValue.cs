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

/// <summary>
/// One issue's value for one <see cref="CustomFieldDefinition"/>. Keyed by (IssueId, DefinitionId) so
/// an issue has at most one value per field. The stored form is always text; the definition's type
/// governs how it was validated and how it renders.
/// </summary>
public class CustomFieldValue
{
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition Definition { get; set; } = null!;

    public string? Value { get; set; }
}
