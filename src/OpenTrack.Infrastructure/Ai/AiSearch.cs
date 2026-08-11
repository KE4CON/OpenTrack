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
using OpenTrack.Core.Querying;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>The structured filter an AI turned a plain-English search into. Every field is optional
/// ("any"); these map one-to-one onto the issue list's existing query-string filter, so natural-language
/// search can only ever produce a filter the user could have built by hand — no new query power, no ACL
/// bypass.</summary>
public readonly record struct SearchCriteria(
    IssueStatus? Status, IssueSeverity? Severity, IssuePriority? Priority,
    string? Text, bool Stale, IssueSort? Sort, string? ProjectName);

/// <summary>Provider-agnostic pieces of natural-language issue search: the prompt, the tool JSON-schema,
/// and the mapping from the model's arguments to a <see cref="SearchCriteria"/>. Shared by both AI
/// providers so they interpret searches identically.</summary>
public static class AiSearch
{
    public const string ToolName = "build_issue_filter";
    public const string ToolDescription = "Turn the user's plain-English request into issue-filter fields.";

    public static string BuildPrompt(string query, IReadOnlyList<string> projectNames) =>
        "Convert this plain-English request into issue-search filter fields. Only set a field the user " +
        "clearly implies and leave the rest empty. Words like 'stale', 'idle', 'untouched', 'stalled', or " +
        "'nobody has touched' mean stale=true (open issues no one has updated recently). Put free-text " +
        "keywords (words that should appear in the title/description) into 'text'. Match 'project' to one " +
        "of the provided project names exactly, or omit it. Respond ONLY via the tool.\n\n" +
        $"Request: {query}\n" +
        $"Projects: {(projectNames.Count == 0 ? "(any)" : string.Join(", ", projectNames))}";

    public static object BuildInputSchema(IReadOnlyList<string> projectNames)
    {
        var props = new Dictionary<string, object>
        {
            ["status"] = new { type = "string", @enum = Enum.GetNames<IssueStatus>() },
            ["severity"] = new { type = "string", @enum = Enum.GetNames<IssueSeverity>() },
            ["priority"] = new { type = "string", @enum = Enum.GetNames<IssuePriority>() },
            ["text"] = new { type = "string" },
            ["stale"] = new { type = "boolean" },
            ["sort"] = new { type = "string", @enum = Enum.GetNames<IssueSort>() },
        };
        // Constrain the project to the ones the user can actually see, when we know them.
        props["project"] = projectNames.Count == 0
            ? new { type = "string" }
            : new { type = "string", @enum = projectNames.ToArray() };
        return new { type = "object", properties = props };
    }

    public static SearchCriteria FromInput(JsonElement input, IReadOnlyList<string> projectNames)
    {
        IssueStatus? status = input.TryGetProperty("status", out var st) && Enum.TryParse<IssueStatus>(st.GetString(), out var sv) ? sv : null;
        IssueSeverity? sev = input.TryGetProperty("severity", out var se) && Enum.TryParse<IssueSeverity>(se.GetString(), out var sev2) ? sev2 : null;
        IssuePriority? pri = input.TryGetProperty("priority", out var pr) && Enum.TryParse<IssuePriority>(pr.GetString(), out var pv) ? pv : null;
        IssueSort? sort = input.TryGetProperty("sort", out var so) && Enum.TryParse<IssueSort>(so.GetString(), out var sov) ? sov : null;

        string? text = input.TryGetProperty("text", out var tx) && tx.ValueKind == JsonValueKind.String
            ? tx.GetString()?.Trim() is { Length: > 0 } s ? s : null
            : null;

        bool stale = input.TryGetProperty("stale", out var stl) &&
                     (stl.ValueKind == JsonValueKind.True ||
                      (stl.ValueKind == JsonValueKind.String && bool.TryParse(stl.GetString(), out var b) && b));

        string? project = null;
        if (input.TryGetProperty("project", out var pj) && pj.ValueKind == JsonValueKind.String)
            project = projectNames.FirstOrDefault(x => string.Equals(x, pj.GetString(), StringComparison.OrdinalIgnoreCase));

        return new SearchCriteria(status, sev, pri, text, stale, sort, project);
    }
}
