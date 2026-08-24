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

using System.Text.RegularExpressions;

namespace OpenTrack.Core.Text;

/// <summary>
/// Human-friendly ticket numbers. When a project has a <c>Key</c> (e.g. "APRS"), an issue is shown as
/// "APRS-42"; without a key it falls back to "#42". The numeric issue id is always the real internal
/// key — the friendly number is a display/label wrapper — so one instance tracking several apps (each a
/// project with its own key) can tell tickets apart at a glance and quote them over the phone.
/// </summary>
public static partial class TicketNumber
{
    /// <summary>Maximum stored key length. Short enough to stay readable in "KEY-42".</summary>
    public const int MaxKeyLength = 10;

    /// <summary>Formats the friendly ticket number: "APRS-42" when the project has a key, else "#42".</summary>
    public static string Format(string? projectKey, int issueId)
    {
        var key = NormalizeKey(projectKey);
        return key is null ? $"#{issueId}" : $"{key}-{issueId}";
    }

    /// <summary>
    /// Parses a user-entered reference into the numeric issue id. Accepts the raw number ("42"), the
    /// "#42" form, and the friendly "APRS-42" form (any case) — anything ending in digits. Returns false
    /// when there is no trailing number.
    /// </summary>
    public static bool TryParseId(string? input, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var m = TrailingDigits().Match(input.Trim());
        return m.Success && int.TryParse(m.Groups[1].Value, out id);
    }

    /// <summary>
    /// Normalizes a user-entered key to its stored form: uppercase, letters and digits only, capped at
    /// <see cref="MaxKeyLength"/>. Returns null when the result is empty (so "no key" is a clean null).
    /// </summary>
    public static string? NormalizeKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var cleaned = new string(key.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (cleaned.Length == 0) return null;
        return cleaned.Length <= MaxKeyLength ? cleaned : cleaned[..MaxKeyLength];
    }

    [GeneratedRegex(@"(\d+)\s*$")]
    private static partial Regex TrailingDigits();
}
