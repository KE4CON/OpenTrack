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

namespace OpenTrack.Core.Git;

/// <summary>A reference to an OpenTrack issue found in a commit message.</summary>
/// <param name="IssueId">The issue number (from <c>#123</c>).</param>
/// <param name="Closing">True when a closing keyword (fix/close/resolve…) preceded the reference, meaning
/// the commit intends to resolve the issue.</param>
public readonly record struct GitIssueRef(int IssueId, bool Closing);

/// <summary>
/// Pure parser that pulls issue references out of a Git commit message — plain mentions (<c>#123</c>) and
/// closing references (<c>fixes #123</c>, <c>Closes: #45</c>, <c>resolved #7</c>). GitHub's closing-keyword
/// vocabulary is used. If the same issue is mentioned both ways, the closing intent wins. Kept in Core so
/// the webhook receiver and the tests share one definition.
/// </summary>
public static partial class GitRefParser
{
    // A #number not glued to a preceding word char (so "abc#12" / "v1.2#3" don't match), capturing an
    // optional immediately-preceding closing keyword.
    [GeneratedRegex(@"(?<![\w])(?:(?<kw>fix(?:e[sd])?|close[sd]?|resolve[sd]?)\s*:?\s+)?#(?<num>\d+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RefRegex();

    public static IReadOnlyList<GitIssueRef> Parse(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return [];

        // Accumulate closing intent per issue: any closing reference makes the issue "closing".
        var byIssue = new Dictionary<int, bool>();
        foreach (Match m in RefRegex().Matches(message))
        {
            if (!int.TryParse(m.Groups["num"].Value, out var id) || id <= 0) continue;
            var closing = m.Groups["kw"].Success;
            byIssue[id] = byIssue.TryGetValue(id, out var existing) ? existing || closing : closing;
        }

        return byIssue.Select(kv => new GitIssueRef(kv.Key, kv.Value)).ToList();
    }
}
