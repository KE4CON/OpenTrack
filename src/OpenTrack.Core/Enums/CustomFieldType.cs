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
/// The data type of a project custom field. Values are always stored as text; the type governs how a
/// value is validated on input and how it is rendered/edited in the UI.
/// </summary>
public enum CustomFieldType
{
    /// <summary>Free text (single line).</summary>
    Text = 0,
    /// <summary>A number (stored as text; validated to parse as a decimal).</summary>
    Number = 1,
    /// <summary>A calendar date (stored ISO yyyy-MM-dd; validated to parse as a date).</summary>
    Date = 2,
    /// <summary>One value chosen from a fixed list defined on the field (see <c>EnumOptions</c>).</summary>
    Enum = 3,
}
