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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>
/// <see cref="IAiAssistant"/> backed by any OpenAI-<em>compatible</em> Chat Completions API
/// (<c>{baseUrl}/chat/completions</c>) using function-calling for strict JSON output. One implementation
/// covers OpenAI, Azure OpenAI, Groq, OpenRouter, and local engines like Ollama / LM Studio — the only
/// difference is <see cref="AiOptions.BaseUrl"/> (and whether a key is needed). Best-effort: any error is
/// logged and returns null. A local engine keeps all data on-machine.
/// </summary>
public sealed class OpenAiAssistant(HttpClient http, AiOptions options, ILogger<OpenAiAssistant> logger) : IAiAssistant
{
    public bool IsEnabled => options.Enabled && options.IsOpenAi && options.HasCredentials;

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

    private object BuildBody(string prompt, string toolName, string toolDesc, object schema) => new
    {
        model = options.Model,
        max_tokens = 512,
        messages = new object[] { new { role = "user", content = prompt } },
        tools = new object[]
        {
            new { type = "function", function = new { name = toolName, description = toolDesc, parameters = schema } },
        },
        tool_choice = new { type = "function", function = new { name = toolName } },
    };

    private async Task<string?> PostAsync(object body, CancellationToken ct)
    {
        try
        {
            var url = $"{options.ResolvedOpenAiBaseUrl}/chat/completions";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            // A key is optional for local engines (Ollama/LM Studio); send it when present.
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
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

    /// <summary>Extract the first tool call's arguments object (detached so it stays valid after dispose).
    /// The <c>function.arguments</c> is usually a JSON string; some local engines send an object.</summary>
    public static JsonElement? ExtractToolInput(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
                choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
                return null;

            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("tool_calls", out var calls) ||
                calls.ValueKind != JsonValueKind.Array || calls.GetArrayLength() == 0)
                return null;

            var args = calls[0].GetProperty("function").GetProperty("arguments");
            if (args.ValueKind == JsonValueKind.String)
            {
                using var inner = JsonDocument.Parse(args.GetString()!);
                return inner.RootElement.Clone();
            }
            if (args.ValueKind == JsonValueKind.Object)
                return args.Clone();
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
}
