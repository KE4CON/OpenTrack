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

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Enums;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>
/// <see cref="IAiAssistant"/> backed by the Anthropic Messages API (<c>/v1/messages</c>), using
/// tool-use so the model returns a strict JSON object we can map. All calls are best-effort: any error
/// (network, auth, malformed response) is logged and returns null so issue creation is never blocked.
/// The API key comes from <see cref="AiOptions"/> and is sent only server-side.
/// </summary>
public sealed class AnthropicAiAssistant(HttpClient http, AiOptions options, ILogger<AnthropicAiAssistant> logger) : IAiAssistant
{
    public bool IsEnabled => options.Enabled && !string.IsNullOrWhiteSpace(options.ApiKey);

    public async Task<TriageSuggestion?> SuggestTriageAsync(
        string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default)
    {
        if (!IsEnabled) return null;

        var severities = Enum.GetNames<IssueSeverity>();
        var priorities = Enum.GetNames<IssuePriority>();
        var prompt =
            "You are triaging a new software issue for a bug tracker. Given the summary and details, " +
            "suggest a severity, a priority, the single best-fitting category from the provided list " +
            "(or omit it if none fit), and up to 5 short lower-case tags. Respond ONLY via the tool.\n\n" +
            $"Summary: {title}\n" +
            $"Details: {(string.IsNullOrWhiteSpace(description) ? "(none)" : description)}\n" +
            $"Available categories: {(categories.Count == 0 ? "(none)" : string.Join(", ", categories))}";

        var body = new
        {
            model = options.Model,
            max_tokens = 512,
            tools = new object[]
            {
                new
                {
                    name = "suggest_triage",
                    description = "Return the suggested triage for the issue.",
                    input_schema = new
                    {
                        type = "object",
                        properties = new Dictionary<string, object>
                        {
                            ["severity"] = new { type = "string", @enum = severities },
                            ["priority"] = new { type = "string", @enum = priorities },
                            ["category"] = new { type = "string" },
                            ["tags"] = new { type = "array", items = new { type = "string" } },
                        },
                    },
                },
            },
            tool_choice = new { type = "tool", name = "suggest_triage" },
            messages = new object[] { new { role = "user", content = prompt } },
        };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", options.ApiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = JsonContent.Create(body);

            using var resp = await http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("AI triage call returned {Status}.", (int)resp.StatusCode);
                return null;
            }
            var json = await resp.Content.ReadAsStringAsync(ct);
            return ParseTriage(json, categories);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI triage call failed.");
            return null;
        }
    }

    /// <summary>Parse the Anthropic response: find the tool_use content block and read its input.</summary>
    public static TriageSuggestion? ParseTriage(string responseJson, IReadOnlyList<string> categories)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (!doc.RootElement.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var block in content.EnumerateArray())
            {
                if (block.TryGetProperty("type", out var t) && t.GetString() == "tool_use" &&
                    block.TryGetProperty("input", out var input))
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
            return null;
        }
        catch
        {
            return null;
        }
    }
}
