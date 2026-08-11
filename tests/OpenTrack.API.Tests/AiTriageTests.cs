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

using Microsoft.Extensions.Logging.Abstractions;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Ai;

namespace OpenTrack.API.Tests;

/// <summary>The AI assistant is off unless configured, and it maps the Anthropic tool-use response to a
/// triage suggestion — validating the category against the project's real categories and dropping
/// unknown enum values.</summary>
public class AiTriageTests
{
    [Fact]
    public void IsEnabled_RequiresBothTheToggleAndAKey()
    {
        AnthropicAiAssistant Make(bool enabled, string? key) =>
            new(new HttpClient(), new AiOptions { Enabled = enabled, ApiKey = key }, NullLogger<AnthropicAiAssistant>.Instance);

        Assert.False(Make(false, "sk-x").IsEnabled);
        Assert.False(Make(true, null).IsEnabled);
        Assert.False(Make(true, "  ").IsEnabled);
        Assert.True(Make(true, "sk-x").IsEnabled);
    }

    [Fact]
    public void ParseTriage_MapsToolUseInput_AndValidatesCategory()
    {
        var json = """
        {
          "content": [
            { "type": "text", "text": "here you go" },
            { "type": "tool_use", "name": "suggest_triage", "input": {
                "severity": "Crash", "priority": "High", "category": "Engine", "tags": ["startup","crash"] } }
          ]
        }
        """;
        var s = AnthropicAiAssistant.ParseTriage(json, new[] { "Engine", "UI" });
        Assert.NotNull(s);
        Assert.Equal(IssueSeverity.Crash, s!.Value.Severity);
        Assert.Equal(IssuePriority.High, s.Value.Priority);
        Assert.Equal("Engine", s.Value.Category);          // matched a real category
        Assert.Equal(new[] { "startup", "crash" }, s.Value.Tags);
    }

    [Fact]
    public void ParseTriage_DropsUnknownCategoryAndBadEnum()
    {
        var json = """
        { "content": [ { "type": "tool_use", "name": "suggest_triage", "input": {
            "severity": "Nonsense", "priority": "Normal", "category": "DoesNotExist", "tags": [] } } ] }
        """;
        var s = AnthropicAiAssistant.ParseTriage(json, new[] { "Engine" });
        Assert.NotNull(s);
        Assert.Null(s!.Value.Severity);                    // "Nonsense" isn't a real severity
        Assert.Equal(IssuePriority.Normal, s.Value.Priority);
        Assert.Null(s.Value.Category);                     // not one of the project's categories
    }

    [Fact]
    public void ParseTriage_ReturnsNull_WhenNoToolUse()
    {
        Assert.Null(AnthropicAiAssistant.ParseTriage("""{ "content": [ { "type": "text", "text": "hi" } ] }""", []));
        Assert.Null(AnthropicAiAssistant.ParseTriage("not json", []));
    }

    // --- OpenAI-compatible provider (OpenAI / Azure / Groq / OpenRouter / Ollama / LM Studio) ---

    [Fact]
    public void OpenAi_ParseTriage_MapsFunctionCall_ArgumentsAsJsonString()
    {
        // OpenAI's standard shape: function.arguments is a JSON *string*.
        var json = """
        {
          "choices": [
            { "message": { "tool_calls": [
                { "type": "function", "function": {
                    "name": "suggest_triage",
                    "arguments": "{\"severity\":\"Major\",\"priority\":\"High\",\"category\":\"UI\",\"tags\":[\"layout\"]}"
                } } ] } }
          ]
        }
        """;
        var s = OpenAiAssistant.ParseTriage(json, new[] { "Engine", "UI" });
        Assert.NotNull(s);
        Assert.Equal(IssueSeverity.Major, s!.Value.Severity);
        Assert.Equal(IssuePriority.High, s.Value.Priority);
        Assert.Equal("UI", s.Value.Category);
        Assert.Equal(new[] { "layout" }, s.Value.Tags);
    }

    [Fact]
    public void OpenAi_ParseTriage_MapsFunctionCall_ArgumentsAsObject_AndValidatesCategory()
    {
        // Lenient: some local engines return arguments as an already-parsed object; unknown category dropped.
        var json = """
        { "choices": [ { "message": { "tool_calls": [
            { "function": { "name": "suggest_triage", "arguments":
                { "severity": "Crash", "priority": "Urgent", "category": "Nope", "tags": [] } } } ] } } ] }
        """;
        var s = OpenAiAssistant.ParseTriage(json, new[] { "Engine" });
        Assert.NotNull(s);
        Assert.Equal(IssueSeverity.Crash, s!.Value.Severity);
        Assert.Equal(IssuePriority.Urgent, s.Value.Priority);
        Assert.Null(s.Value.Category);            // "Nope" isn't one of the project's categories
    }

    [Fact]
    public void OpenAi_ParseTriage_ReturnsNull_WhenNoToolCall()
    {
        Assert.Null(OpenAiAssistant.ParseTriage("""{ "choices": [ { "message": { "content": "hi" } } ] }""", []));
        Assert.Null(OpenAiAssistant.ParseTriage("""{ "choices": [] }""", []));
        Assert.Null(OpenAiAssistant.ParseTriage("not json", []));
    }

    [Fact]
    public void HasCredentials_LocalOpenAiEngineNeedsNoKey_ButCloudDoes()
    {
        // A local OpenAI-compatible engine (identified by a custom BaseUrl) is usable with no key.
        Assert.True(new AiOptions { Provider = "openai", BaseUrl = "http://localhost:11434/v1" }.HasCredentials);
        // Cloud OpenAI (no BaseUrl) still needs a key.
        Assert.False(new AiOptions { Provider = "openai" }.HasCredentials);
        Assert.True(new AiOptions { Provider = "openai", ApiKey = "sk-x" }.HasCredentials);
        // Anthropic always needs a key.
        Assert.False(new AiOptions { Provider = "anthropic", BaseUrl = "http://localhost" }.HasCredentials);
        Assert.True(new AiOptions { Provider = "anthropic", ApiKey = "sk-x" }.HasCredentials);
    }
}
