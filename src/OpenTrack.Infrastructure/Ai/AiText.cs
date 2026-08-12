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

namespace OpenTrack.Infrastructure.Ai;

/// <summary>Small text helpers for the AI prompt builders — mainly capping user-supplied text so a caller
/// can't drive an arbitrarily large (billable) request to the AI provider.</summary>
public static class AiText
{
    public static string Cap(string? value, int maxChars)
    {
        var s = value ?? "";
        return s.Length <= maxChars ? s : s[..maxChars];
    }
}
