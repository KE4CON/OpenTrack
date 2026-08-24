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

using System.Text;
using System.Text.Json;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>Provider-agnostic pieces of the "Suggest a fix" feature: the grounded prompt, the JSON schema
/// of the tool the model must call, and the mapping from the model's returned arguments to a
/// <see cref="ResolutionSuggestion"/>. Shared by the Anthropic and OpenAI-compatible assistants so both
/// speak the same contract. The prompt is deliberately conservative — "use ONLY what is provided, be honest
/// about confidence, do not invent" — because a problem tracker is a system of record and a confidently
/// wrong fix is worse than a hedged one.</summary>
public static class AiResolution
{
    public const string ToolName = "suggest_resolution";
    public const string ToolDescription = "Return a suggested resolution (likely causes and concrete steps) for the issue.";

    // Caps to bound token cost per call. Level 2 grounding (notes, logs, similar issues) is trimmed to these.
    public const int MaxTitleChars = 500;
    public const int MaxDescriptionChars = 4000;
    public const int MaxNoteChars = 800;
    public const int MaxNotes = 12;
    public const int MaxLogChars = 2000;
    public const int MaxLogExcerpts = 5;
    public const int MaxResolutionChars = 800;
    public const int MaxSimilar = 5;

    public static string BuildPrompt(ResolutionContext ctx)
    {
        var sb = new StringBuilder();
        sb.Append(
            "You are helping a maintainer resolve an issue in a problem/bug tracker. Using ONLY the " +
            "information provided below, give the most likely cause(s) and concrete steps to try, most " +
            "promising first. When a similar RESOLVED issue below applies, prefer the step that worked " +
            "there and cite it. Be honest: if the information is thin, say so and set confidence to \"low\". " +
            "Do NOT invent file names, versions, or facts that are not present. Respond ONLY via the tool.\n\n");

        sb.Append("Issue title: ").Append(AiText.Cap(ctx.Title, MaxTitleChars)).Append('\n');
        sb.Append("Details: ")
          .Append(string.IsNullOrWhiteSpace(ctx.Description) ? "(none)" : AiText.Cap(ctx.Description, MaxDescriptionChars))
          .Append('\n');

        if (ctx.Notes.Count > 0)
        {
            sb.Append("\nDiscussion notes:\n");
            foreach (var note in ctx.Notes.Take(MaxNotes))
                sb.Append("- ").Append(AiText.Cap(note, MaxNoteChars)).Append('\n');
        }

        if (ctx.LogExcerpts.Count > 0)
        {
            sb.Append("\nAttached log / error excerpts:\n");
            foreach (var log in ctx.LogExcerpts.Take(MaxLogExcerpts))
                sb.Append("```\n").Append(AiText.Cap(log, MaxLogChars)).Append("\n```\n");
        }

        if (ctx.SimilarResolved.Count > 0)
        {
            sb.Append("\nSimilar issues already RESOLVED (and how they were fixed):\n");
            foreach (var r in ctx.SimilarResolved.Take(MaxSimilar))
            {
                sb.Append("- #").Append(r.Number).Append(" \"").Append(AiText.Cap(r.Title, MaxTitleChars)).Append('"');
                sb.Append(string.IsNullOrWhiteSpace(r.Resolution)
                    ? " — (resolution not recorded)\n"
                    : $" — Resolution: {AiText.Cap(r.Resolution, MaxResolutionChars)}\n");
            }
        }

        return sb.ToString();
    }

    /// <summary>The JSON-schema object describing the tool's input. Identical shape for both providers.</summary>
    public static object BuildInputSchema() => new
    {
        type = "object",
        properties = new Dictionary<string, object>
        {
            ["summary"] = new { type = "string", description = "One-paragraph plain-language root-cause hypothesis." },
            ["causes"] = new { type = "array", items = new { type = "string" }, description = "Ranked likely causes, most likely first." },
            ["steps"] = new { type = "array", items = new { type = "string" }, description = "Concrete things to try, in order." },
            ["confidence"] = new { type = "string", @enum = new[] { "low", "medium", "high" } },
            ["sources"] = new { type = "array", items = new { type = "string" }, description = "Which provided items were used, e.g. \"issue #123\" or \"attached log\"." },
        },
        required = new[] { "summary", "steps", "confidence" },
    };

    /// <summary>Map the tool's returned input object to a suggestion — tolerant of missing values.
    /// Returns null only when there is no usable content at all (no summary and no steps).</summary>
    public static ResolutionSuggestion? FromInput(JsonElement input)
    {
        var summary = GetString(input, "summary") ?? "";
        var causes = GetStringList(input, "causes");
        var steps = GetStringList(input, "steps");
        var confidence = NormalizeConfidence(GetString(input, "confidence"));
        var sources = GetStringList(input, "sources");

        if (summary.Length == 0 && steps.Count == 0) return null;
        return new ResolutionSuggestion(summary, causes, steps, confidence, sources);
    }

    private static string NormalizeConfidence(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        "high" => "high",
        "medium" or "med" => "medium",
        "low" => "low",
        _ => "low", // unknown/absent → be conservative
    };

    private static string? GetString(JsonElement input, string name) =>
        input.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()?.Trim()
            : null;

    private static List<string> GetStringList(JsonElement input, string name)
    {
        var list = new List<string>();
        if (input.TryGetProperty(name, out var arr) && arr.ValueKind == JsonValueKind.Array)
            foreach (var item in arr.EnumerateArray())
                if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } s)
                    list.Add(s.Trim());
        return list;
    }
}
