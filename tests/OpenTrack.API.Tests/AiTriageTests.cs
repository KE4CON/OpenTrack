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
}
