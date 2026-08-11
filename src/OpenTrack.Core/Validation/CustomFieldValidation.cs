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

using System.Globalization;
using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Validation;

/// <summary>
/// Pure validation/normalization for custom-field values, shared by the definition CRUD (to validate
/// a field's enum option list) and the value write path (to validate an issue's value against its
/// field's type). Kept free of EF/HTTP so the whole matrix is unit-testable.
/// </summary>
public static class CustomFieldValidation
{
    /// <summary>The result of validating a raw value: either an <see cref="Error"/> message (and no
    /// normalized value) or a <see cref="Normalized"/> canonical form to persist.</summary>
    public readonly record struct ValueResult(string? Error, string? Normalized)
    {
        public bool Ok => Error is null;
    }

    /// <summary>Split an Enum field's stored option list (one per line) into trimmed, non-empty choices.</summary>
    public static IReadOnlyList<string> ParseEnumOptions(string? enumOptions) =>
        string.IsNullOrWhiteSpace(enumOptions)
            ? []
            : enumOptions.Split('\n')
                .Select(o => o.Trim())
                .Where(o => o.Length > 0)
                .ToList();

    /// <summary>
    /// Validate a raw value for a field of the given type. A blank value is always allowed here and
    /// normalizes to <c>null</c> ("no value"); the caller layers on any required-field rule. On success
    /// returns the canonical text to store (numbers trimmed, dates as ISO yyyy-MM-dd, enums matched to a
    /// defined option); on failure returns a human-readable error.
    /// </summary>
    public static ValueResult ValidateValue(CustomFieldType type, string? enumOptions, string? raw)
    {
        var value = raw?.Trim();
        if (string.IsNullOrEmpty(value)) return new ValueResult(null, null); // cleared / unset

        switch (type)
        {
            case CustomFieldType.Text:
                return new ValueResult(null, value);

            case CustomFieldType.Number:
                return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _)
                    ? new ValueResult(null, value)
                    : new ValueResult("Value must be a number.", null);

            case CustomFieldType.Date:
                return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                    ? new ValueResult(null, d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                    : new ValueResult("Value must be a date (YYYY-MM-DD).", null);

            case CustomFieldType.Enum:
                var options = ParseEnumOptions(enumOptions);
                var match = options.FirstOrDefault(o => string.Equals(o, value, StringComparison.OrdinalIgnoreCase));
                return match is not null
                    ? new ValueResult(null, match) // store the canonically-cased option
                    : new ValueResult("Value must be one of the defined options.", null);

            default:
                return new ValueResult("Unknown field type.", null);
        }
    }
}
