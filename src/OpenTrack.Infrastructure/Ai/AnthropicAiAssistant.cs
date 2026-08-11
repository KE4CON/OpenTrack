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

namespace OpenTrack.Infrastructure.Ai;

/// <summary>
/// <see cref="IAiAssistant"/> backed by the Anthropic Messages API (<c>/v1/messages</c>), using
/// tool-use so the model returns a strict JSON object we can map. All calls are best-effort: any error
/// (network, auth, malformed response) is logged and returns null so the caller is never blocked. The
/// API key comes from <see cref="AiOptions"/> and is sent only server-side.
/// </summary>
public sealed class AnthropicAiAssistant(HttpClient http, AiOptions options, ILogger<AnthropicAiAssistant> logger) : IAiAssistant
{
    public bool IsEnabled => options.Enabled && !string.IsNullOrWhiteSpace(options.ApiKey);

    public async Task<TriageSuggestion?> SuggestTriageAsync(
        string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default)
    {
        if (!IsEnabled) return null;
        var body = BuildBody(AiTriage.BuildPrompt(title, description, categories),
            AiTriage.ToolName, AiTriage.ToolDescription, AiTriage.BuildInputSchema());
        var raw = await PostAsync(body, ct);
        return raw is null ? null : ParseTriage(raw, categories);
    }

    public async Task<SearchCriteria?> InterpretSearchAsync(
        string query, IReadOnlyList<string> projectNames, CancellationToken ct = default)
    {
        if (!IsEnabled) return null;
        var body = BuildBody(AiSearch.BuildPrompt(query, projectNames),
            AiSearch.ToolName, AiSearch.ToolDescription, AiSearch.BuildInputSchema(projectNames));
        var raw = await PostAsync(body, ct);
        return raw is null ? null : ParseSearch(raw, projectNames);
    }

    public async Task<string?> SummarizeIssueAsync(
        string title, string? description, IReadOnlyList<string> notes, CancellationToken ct = default)
    {
        if (!IsEnabled) return null;
        var raw = await PostAsync(BuildTextBody(AiSummary.BuildPrompt(title, description, notes)), ct);
        return raw is null ? null : ExtractText(raw);
    }

    private object BuildBody(string prompt, string toolName, string toolDesc, object schema) => new
    {
        model = options.Model,
        max_tokens = 512,
        tools = new object[] { new { name = toolName, description = toolDesc, input_schema = schema } },
        tool_choice = new { type = "tool", name = toolName },
        messages = new object[] { new { role = "user", content = prompt } },
    };

    private object BuildTextBody(string prompt) => new
    {
        model = options.Model,
        max_tokens = 700,
        messages = new object[] { new { role = "user", content = prompt } },
    };

    private async Task<string?> PostAsync(object body, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", options.ApiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = JsonContent.Create(body);

            using var resp = await http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("AI call returned {Status}.", (int)resp.StatusCode);
                return null;
            }
            return await resp.Content.ReadAsStringAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI call failed.");
            return null;
        }
    }

    /// <summary>Extract the tool_use content block's input object (detached from its document so it stays
    /// valid after we dispose the parse). Null if there is no tool_use block.</summary>
    public static JsonElement? ExtractToolInput(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (!doc.RootElement.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return null;
            foreach (var block in content.EnumerateArray())
                if (block.TryGetProperty("type", out var t) && t.GetString() == "tool_use" &&
                    block.TryGetProperty("input", out var input))
                    return input.Clone();
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static TriageSuggestion? ParseTriage(string responseJson, IReadOnlyList<string> categories) =>
        ExtractToolInput(responseJson) is { } input ? AiTriage.FromInput(input, categories) : null;

    public static SearchCriteria? ParseSearch(string responseJson, IReadOnlyList<string> projectNames) =>
        ExtractToolInput(responseJson) is { } input ? AiSearch.FromInput(input, projectNames) : null;

    /// <summary>Concatenate the "text" content blocks of a (non-tool) Messages response.</summary>
    public static string? ExtractText(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (!doc.RootElement.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
                return null;
            var sb = new System.Text.StringBuilder();
            foreach (var block in content.EnumerateArray())
                if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                    block.TryGetProperty("text", out var txt) && txt.GetString() is { } s)
                    sb.Append(s);
            var result = sb.ToString().Trim();
            return result.Length == 0 ? null : result;
        }
        catch
        {
            return null;
        }
    }
}
