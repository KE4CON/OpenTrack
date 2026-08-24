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
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Ai;

namespace OpenTrack.API.Tests;

/// <summary>The "Suggest a fix" feature: both providers map their tool/function response to a
/// <see cref="ResolutionSuggestion"/>, the prompt is grounded in the assembled context, confidence is
/// normalized, a disabled provider does nothing, and the tiered router sends each task to the right
/// provider (menial → base, fix suggestion → smart) with graceful fallback.</summary>
public class AiResolutionTests
{
    // --- Anthropic tool-use parsing ---

    [Fact]
    public void Anthropic_ParseResolution_MapsToolUseInput()
    {
        var json = """
        {
          "content": [
            { "type": "text", "text": "thinking..." },
            { "type": "tool_use", "name": "suggest_resolution", "input": {
                "summary": "The service loses its token on reconnect.",
                "causes": ["token not refreshed", "race on reconnect"],
                "steps": ["Log the token lifetime", "Refresh before reconnecting"],
                "confidence": "medium",
                "sources": ["issue #123", "attached log app.log"] } }
          ]
        }
        """;
        var s = AnthropicAiAssistant.ParseResolution(json);
        Assert.NotNull(s);
        Assert.Equal("The service loses its token on reconnect.", s!.Value.Summary);
        Assert.Equal(new[] { "token not refreshed", "race on reconnect" }, s.Value.Causes);
        Assert.Equal(new[] { "Log the token lifetime", "Refresh before reconnecting" }, s.Value.Steps);
        Assert.Equal("medium", s.Value.Confidence);
        Assert.Equal(new[] { "issue #123", "attached log app.log" }, s.Value.Sources);
    }

    [Fact]
    public void OpenAi_ParseResolution_MapsFunctionCall_ArgumentsAsJsonString()
    {
        var json = """
        {
          "choices": [
            { "message": { "tool_calls": [
                { "type": "function", "function": {
                    "name": "suggest_resolution",
                    "arguments": "{\"summary\":\"Disk full.\",\"steps\":[\"Free space\"],\"confidence\":\"high\"}"
                } } ] } }
          ]
        }
        """;
        var s = OpenAiAssistant.ParseResolution(json);
        Assert.NotNull(s);
        Assert.Equal("Disk full.", s!.Value.Summary);
        Assert.Equal(new[] { "Free space" }, s.Value.Steps);
        Assert.Equal("high", s.Value.Confidence);
        Assert.Empty(s.Value.Causes);   // omitted → empty, not null
        Assert.Empty(s.Value.Sources);
    }

    [Fact]
    public void ParseResolution_NormalizesUnknownConfidence_ToLow()
    {
        var json = """
        { "content": [ { "type": "tool_use", "name": "suggest_resolution", "input": {
            "summary": "x", "steps": ["y"], "confidence": "banana" } } ] }
        """;
        var s = AnthropicAiAssistant.ParseResolution(json);
        Assert.NotNull(s);
        Assert.Equal("low", s!.Value.Confidence);   // unknown value → conservative "low"
    }

    [Fact]
    public void ParseResolution_ReturnsNull_WhenNoUsableContent()
    {
        // No summary and no steps → nothing worth showing.
        var empty = """{ "content": [ { "type": "tool_use", "name": "suggest_resolution", "input": { "causes": ["a"] } } ] }""";
        Assert.Null(AnthropicAiAssistant.ParseResolution(empty));
        // No tool call at all.
        Assert.Null(AnthropicAiAssistant.ParseResolution("""{ "content": [ { "type": "text", "text": "hi" } ] }"""));
        Assert.Null(OpenAiAssistant.ParseResolution("""{ "choices": [] }"""));
        Assert.Null(AnthropicAiAssistant.ParseResolution("not json"));
    }

    [Fact]
    public async Task SuggestResolutionAsync_ReturnsNull_WhenDisabled()
    {
        var off = new AnthropicAiAssistant(new HttpClient(), new AiOptions { Enabled = false }, NullLogger<AnthropicAiAssistant>.Instance);
        Assert.Null(await off.SuggestResolutionAsync(SampleContext()));
    }

    // --- Grounded prompt ---

    [Fact]
    public void BuildPrompt_IncludesGrounding_AndHonestyInstruction()
    {
        var ctx = new ResolutionContext(
            "App crashes on startup",
            "Happens every time on cold boot.",
            Notes: new[] { "alice: only after the last update" },
            LogExcerpts: new[] { "app.log:\nNullReferenceException at Startup" },
            SimilarResolved: new[] { new ResolvedReference(123, "Crash on cold boot", "resolved as Fixed; last note: added null guard") });

        var prompt = AiResolution.BuildPrompt(ctx);

        Assert.Contains("App crashes on startup", prompt);
        Assert.Contains("only after the last update", prompt);          // note
        Assert.Contains("NullReferenceException", prompt);              // log excerpt
        Assert.Contains("#123", prompt);                               // similar resolved issue
        Assert.Contains("added null guard", prompt);                   // its resolution
        Assert.Contains("ONLY", prompt);                               // "use ONLY the information provided"
        Assert.Contains("Respond ONLY via the tool", prompt);
    }

    [Fact]
    public void BuildPrompt_CapsLongText()
    {
        var hugeDescription = new string('x', AiResolution.MaxDescriptionChars + 5000);
        var ctx = new ResolutionContext("t", hugeDescription, [], [], []);
        var prompt = AiResolution.BuildPrompt(ctx);
        // The description is capped, so the prompt can't contain the full oversized blob.
        Assert.DoesNotContain(new string('x', AiResolution.MaxDescriptionChars + 1), prompt);
    }

    // --- Tiered router ---

    private sealed class FakeAssistant(bool enabled) : IAiAssistant
    {
        public bool IsEnabled => enabled;
        public string? LastCall { get; private set; }

        public Task<TriageSuggestion?> SuggestTriageAsync(string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default)
        { LastCall = "triage"; return Task.FromResult<TriageSuggestion?>(null); }

        public Task<SearchCriteria?> InterpretSearchAsync(string query, IReadOnlyList<string> projectNames, CancellationToken ct = default)
        { LastCall = "search"; return Task.FromResult<SearchCriteria?>(null); }

        public Task<string?> SummarizeIssueAsync(string title, string? description, IReadOnlyList<string> notes, CancellationToken ct = default)
        { LastCall = "summarize"; return Task.FromResult<string?>(null); }

        public Task<ResolutionSuggestion?> SuggestResolutionAsync(ResolutionContext context, CancellationToken ct = default)
        { LastCall = "resolution"; return Task.FromResult<ResolutionSuggestion?>(null); }
    }

    [Fact]
    public async Task Router_MenialGoesToBase_FixGoesToSmart()
    {
        var menial = new FakeAssistant(true);
        var smart = new FakeAssistant(true);
        var router = new AiAssistantRouter(menial, smart);

        await router.SuggestTriageAsync("t", null, []);
        await router.SummarizeIssueAsync("t", null, []);
        await router.SuggestResolutionAsync(SampleContext());

        Assert.Equal("summarize", menial.LastCall);   // menial handled triage + summarize (last was summarize)
        Assert.Equal("resolution", smart.LastCall);    // smart handled the fix suggestion
    }

    [Fact]
    public async Task Router_FallsBackToBase_WhenNoSmartConfigured()
    {
        var menial = new FakeAssistant(true);
        var router = new AiAssistantRouter(menial, smart: null);

        await router.SuggestResolutionAsync(SampleContext());
        Assert.Equal("resolution", menial.LastCall);   // no smart tier → base handles everything
    }

    [Fact]
    public async Task Router_MenialFallsBackToSmart_WhenBaseDisabled()
    {
        var menial = new FakeAssistant(false);         // e.g. local model not configured
        var smart = new FakeAssistant(true);
        var router = new AiAssistantRouter(menial, smart);

        await router.SuggestTriageAsync("t", null, []);
        Assert.Equal("triage", smart.LastCall);        // base off → smart takes the menial task
    }

    [Fact]
    public void Router_IsEnabled_WhenEitherTierEnabled()
    {
        Assert.True(new AiAssistantRouter(new FakeAssistant(true), null).IsEnabled);
        Assert.True(new AiAssistantRouter(new FakeAssistant(false), new FakeAssistant(true)).IsEnabled);
        Assert.False(new AiAssistantRouter(new FakeAssistant(false), new FakeAssistant(false)).IsEnabled);
        Assert.False(new AiAssistantRouter(new FakeAssistant(false), null).IsEnabled);
    }

    private static ResolutionContext SampleContext() =>
        new("title", "desc", [], [], []);
}
