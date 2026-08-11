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

using OpenTrack.Core.Enums;

namespace OpenTrack.Infrastructure.Ai;

/// <summary>An AI-suggested triage for a proposed issue. Every field is a suggestion for a human to
/// accept or change — never applied automatically.</summary>
public readonly record struct TriageSuggestion(
    IssueSeverity? Severity, IssuePriority? Priority, string? Category, IReadOnlyList<string> Tags);

/// <summary>
/// Optional, opt-in AI assistance backed by a configurable provider — Anthropic Claude, or any
/// OpenAI-compatible endpoint (including local Ollama / LM Studio). When disabled (the default) or
/// unconfigured, <see cref="IsEnabled"/> is false and the methods return null, so callers degrade
/// gracefully. Every call sends the given text to the AI provider, so it is only ever invoked behind an
/// explicit opt-in.
/// </summary>
public interface IAiAssistant
{
    bool IsEnabled { get; }

    /// <summary>Suggest a severity/priority/category/tags for a proposed issue. Returns null if AI is
    /// off or the call fails (best-effort — a failure never blocks issue creation).</summary>
    Task<TriageSuggestion?> SuggestTriageAsync(
        string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default);
}
