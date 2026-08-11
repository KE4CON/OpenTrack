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
/// Pure helpers for "possible duplicate" detection: reduce an issue title to its significant words and
/// score how many two titles share. Deliberately simple and dependency-free — enough to surface likely
/// duplicates when someone files a new issue, not a full text-similarity engine. The persistence layer
/// uses <see cref="SignificantWords"/> to build the candidate query and <see cref="Overlap"/> to rank.
/// </summary>
public static partial class IssueSimilarity
{
    [GeneratedRegex(@"[^a-z0-9]+")] private static partial Regex NonWord();

    // Common words that carry no signal for matching bug titles.
    private static readonly HashSet<string> Stop = new(StringComparer.Ordinal)
    {
        "the", "and", "for", "with", "when", "does", "not", "this", "that", "from", "into", "has",
        "have", "was", "are", "but", "you", "your", "can", "will", "its", "get", "got", "app",
        "issue", "bug", "error", "problem", "fix", "should", "would", "could", "some", "any", "all",
    };

    /// <summary>The distinct, lower-cased words of a title worth matching on (length ≥ 3, stopwords removed).</summary>
    public static IReadOnlyList<string> SignificantWords(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return [];
        return NonWord().Split(title.ToLowerInvariant())
            .Where(w => w.Length >= 3 && !Stop.Contains(w))
            .Distinct()
            .ToList();
    }

    /// <summary>How many significant words two titles share (higher = more likely a duplicate).</summary>
    public static int Overlap(string? a, string? b)
    {
        var wa = SignificantWords(a);
        if (wa.Count == 0) return 0;
        var wb = new HashSet<string>(SignificantWords(b), StringComparer.Ordinal);
        return wa.Count(wb.Contains);
    }
}
