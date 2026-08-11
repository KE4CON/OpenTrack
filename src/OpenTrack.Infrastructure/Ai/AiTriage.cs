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

namespace OpenTrack.Infrastructure.Ai;

/// <summary>Provider-agnostic pieces of the "smart triage" feature: the prompt, the JSON schema of the
/// tool the model must call, and the mapping from the model's returned arguments to a
/// <see cref="TriageSuggestion"/>. Shared by the Anthropic and OpenAI-compatible assistants so both speak
/// the same triage contract and stay in lock-step.</summary>
public static class AiTriage
{
    public const string ToolName = "suggest_triage";
    public const string ToolDescription = "Return the suggested triage for the issue.";

    /// <summary>The instruction sent to the model.</summary>
    public static string BuildPrompt(string title, string? description, IReadOnlyList<string> categories) =>
        "You are triaging a new software issue for a bug tracker. Given the summary and details, " +
        "suggest a severity, a priority, the single best-fitting category from the provided list " +
        "(or omit it if none fit), and up to 5 short lower-case tags. Respond ONLY via the tool.\n\n" +
        $"Summary: {title}\n" +
        $"Details: {(string.IsNullOrWhiteSpace(description) ? "(none)" : description)}\n" +
        $"Available categories: {(categories.Count == 0 ? "(none)" : string.Join(", ", categories))}";

    /// <summary>The JSON-schema object describing the tool's input. Identical for both providers
    /// (Anthropic "input_schema" and OpenAI function "parameters" share JSON-schema shape).</summary>
    public static object BuildInputSchema() => new
    {
        type = "object",
        properties = new Dictionary<string, object>
        {
            ["severity"] = new { type = "string", @enum = Enum.GetNames<IssueSeverity>() },
            ["priority"] = new { type = "string", @enum = Enum.GetNames<IssuePriority>() },
            ["category"] = new { type = "string" },
            ["tags"] = new { type = "array", items = new { type = "string" } },
        },
    };

    /// <summary>Map the tool's returned input object to a suggestion — tolerant of missing/unknown values:
    /// an unrecognized enum becomes null, and a category is accepted only if it exists on the project.</summary>
    public static TriageSuggestion FromInput(JsonElement input, IReadOnlyList<string> categories)
    {
        IssueSeverity? sev = input.TryGetProperty("severity", out var s) && Enum.TryParse<IssueSeverity>(s.GetString(), out var sv) ? sv : null;
        IssuePriority? pri = input.TryGetProperty("priority", out var p) && Enum.TryParse<IssuePriority>(p.GetString(), out var pv) ? pv : null;

        string? cat = null;
        if (input.TryGetProperty("category", out var c) && c.ValueKind == JsonValueKind.String)
        {
            var raw = c.GetString();
            // Only accept a category that actually exists on the project.
            cat = categories.FirstOrDefault(x => string.Equals(x, raw, StringComparison.OrdinalIgnoreCase));
        }

        var tags = new List<string>();
        if (input.TryGetProperty("tags", out var tg) && tg.ValueKind == JsonValueKind.Array)
            foreach (var tag in tg.EnumerateArray())
                if (tag.ValueKind == JsonValueKind.String && tag.GetString() is { Length: > 0 } str)
                    tags.Add(str.Trim());

        return new TriageSuggestion(sev, pri, cat, tags);
    }
}
