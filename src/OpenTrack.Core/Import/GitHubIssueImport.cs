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

using System.Text.Json;
using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Import;

/// <summary>
/// Parses GitHub Issues JSON — the array returned by <c>GET /repos/{owner}/{repo}/issues?state=all</c>
/// (also accepts an object wrapping the array under "issues"). Pull requests (items carrying a
/// <c>pull_request</c> field) are skipped, since GitHub's issues endpoint includes them. GitHub has no
/// severity/priority, so those are left to OpenTrack defaults; labels become tags. Pure — no persistence.
/// </summary>
public static class GitHubIssueImport
{
    /// <summary>Throws <see cref="FormatException"/> if the text isn't JSON containing an issues array.</summary>
    public static IReadOnlyList<ImportedIssue> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];

        JsonDocument doc;
        try { doc = JsonDocument.Parse(json); }
        catch (JsonException ex) { throw new FormatException("The file isn't valid JSON.", ex); }

        using (doc)
        {
            var array = doc.RootElement;
            if (array.ValueKind == JsonValueKind.Object)
            {
                if (!array.TryGetProperty("issues", out array) || array.ValueKind != JsonValueKind.Array)
                    throw new FormatException("Expected a JSON array of GitHub issues (or an object with an \"issues\" array).");
            }
            if (array.ValueKind != JsonValueKind.Array)
                throw new FormatException("Expected a JSON array of GitHub issues.");

            var issues = new List<ImportedIssue>();
            foreach (var el in array.EnumerateArray())
            {
                if (el.ValueKind != JsonValueKind.Object) continue;
                if (el.TryGetProperty("pull_request", out _)) continue; // it's a PR, not an issue

                var title = Str(el, "title");
                if (string.IsNullOrWhiteSpace(title)) continue;

                var state = Str(el, "state");
                var status = string.Equals(state, "closed", StringComparison.OrdinalIgnoreCase)
                    ? IssueStatus.Closed : IssueStatus.New;

                issues.Add(new ImportedIssue(
                    Title: title!.Trim(),
                    Description: Blank(Str(el, "body")),
                    Status: status,
                    Severity: null,
                    Priority: null,
                    Category: null,
                    Reporter: Login(el, "user"),
                    Assignee: Login(el, "assignee") ?? FirstAssignee(el),
                    Tags: Labels(el),
                    CreatedAt: Date(el, "created_at"),
                    ExternalId: el.TryGetProperty("number", out var num) && num.ValueKind == JsonValueKind.Number
                        ? num.GetInt64().ToString(System.Globalization.CultureInfo.InvariantCulture) : null));
            }
            return issues;
        }
    }

    private static string? Str(JsonElement e, string prop) =>
        e.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? Login(JsonElement e, string prop) =>
        e.TryGetProperty(prop, out var o) && o.ValueKind == JsonValueKind.Object ? Blank(Str(o, "login")) : null;

    private static string? FirstAssignee(JsonElement e)
    {
        if (e.TryGetProperty("assignees", out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var a in arr.EnumerateArray())
                if (Blank(Str(a, "login")) is { } login) return login;
        return null;
    }

    private static IReadOnlyList<string> Labels(JsonElement e)
    {
        var tags = new List<string>();
        if (e.TryGetProperty("labels", out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var l in arr.EnumerateArray())
            {
                // Labels can be objects ({name}) or plain strings.
                var name = l.ValueKind == JsonValueKind.String ? l.GetString() : Str(l, "name");
                if (Blank(name) is { } n && !tags.Contains(n, StringComparer.OrdinalIgnoreCase)) tags.Add(n);
            }
        return tags;
    }

    private static DateTime? Date(JsonElement e, string prop) =>
        Str(e, prop) is { } s && DateTimeOffset.TryParse(s, out var dto) ? dto.UtcDateTime : null;
}
