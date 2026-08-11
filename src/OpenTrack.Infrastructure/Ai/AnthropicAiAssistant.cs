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

        var body = new
        {
            model = options.Model,
            max_tokens = 512,
            tools = new object[]
            {
                new
                {
                    name = AiTriage.ToolName,
                    description = AiTriage.ToolDescription,
                    input_schema = AiTriage.BuildInputSchema(),
                },
            },
            tool_choice = new { type = "tool", name = AiTriage.ToolName },
            messages = new object[]
            {
                new { role = "user", content = AiTriage.BuildPrompt(title, description, categories) },
            },
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

    /// <summary>Parse an Anthropic response: find the tool_use content block and map its input.</summary>
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
                    return AiTriage.FromInput(input, categories);
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
