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

/// <summary>A past, already-resolved issue similar to the one we're suggesting a fix for. This is the
/// strongest grounding signal — "you solved something like this before, and here's how."</summary>
public readonly record struct ResolvedReference(int Number, string Title, string? Resolution);

/// <summary>Everything assembled to ground a fix suggestion for one issue. All text must already be
/// ACL-filtered by the caller (the AI only ever sees what the requesting user is allowed to see).</summary>
public sealed record ResolutionContext(
    string Title,
    string? Description,
    IReadOnlyList<string> Notes,
    IReadOnlyList<string> LogExcerpts,
    IReadOnlyList<ResolvedReference> SimilarResolved);

/// <summary>An AI-suggested resolution for an issue. Always a draft for a human to accept or discard —
/// never applied automatically, and never edits the issue of record on its own.</summary>
public readonly record struct ResolutionSuggestion(
    string Summary,
    IReadOnlyList<string> Causes,
    IReadOnlyList<string> Steps,
    string Confidence,
    IReadOnlyList<string> Sources);

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

    /// <summary>Turn a plain-English request ("crashes nobody has touched in a month") into structured
    /// filter fields. <paramref name="projectNames"/> constrains any project match to ones the caller can
    /// see. Returns null if AI is off or the call fails (the caller then leaves the search unchanged).</summary>
    Task<SearchCriteria?> InterpretSearchAsync(
        string query, IReadOnlyList<string> projectNames, CancellationToken ct = default);

    /// <summary>Summarize an issue thread (title + description + already-ACL-filtered comments) into a few
    /// plain-language sentences. Returns null if AI is off or the call fails.</summary>
    Task<string?> SummarizeIssueAsync(
        string title, string? description, IReadOnlyList<string> notes, CancellationToken ct = default);

    /// <summary>Suggest how to resolve an issue: likely cause(s) and concrete steps to try, grounded in the
    /// given <see cref="ResolutionContext"/> (the issue text, its notes, log excerpts, and similar past
    /// <em>resolved</em> issues). The result is always a draft suggestion for a human to accept or discard.
    /// Returns null if AI is off or the call fails. In a tiered setup this is the "smart" task and is routed
    /// to the smart provider (e.g. cloud Claude).</summary>
    Task<ResolutionSuggestion?> SuggestResolutionAsync(ResolutionContext context, CancellationToken ct = default);
}
