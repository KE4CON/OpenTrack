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

namespace OpenTrack.Infrastructure.Ai;

/// <summary>
/// Routes each AI task to the right provider in a two-tier setup. The <paramref name="menial"/> provider
/// (typically a small local model, e.g. Ollama on the LAN) handles triage, search, and summaries; the
/// <paramref name="smart"/> provider (typically cloud Claude) handles the reasoning-heavy "Suggest a fix".
/// <para>
/// Both tiers degrade gracefully: if the preferred tier for a task is disabled, the other is used when it
/// is enabled. When <paramref name="smart"/> is null (no second provider configured) the menial provider
/// handles everything, so a single-provider configuration behaves exactly as before this router existed.
/// </para>
/// </summary>
public sealed class AiAssistantRouter(IAiAssistant menial, IAiAssistant? smart) : IAiAssistant
{
    /// <summary>The tier for menial tasks: the menial provider when enabled, otherwise the smart provider.</summary>
    private IAiAssistant Menial => menial.IsEnabled ? menial : SmartOrMenial;

    /// <summary>The tier for smart tasks: the smart provider when enabled, otherwise the menial provider.</summary>
    private IAiAssistant Smart => smart is { IsEnabled: true } ? smart : menial;

    private IAiAssistant SmartOrMenial => smart is { IsEnabled: true } ? smart : menial;

    /// <summary>Enabled when either tier can do work.</summary>
    public bool IsEnabled => menial.IsEnabled || (smart?.IsEnabled ?? false);

    public Task<TriageSuggestion?> SuggestTriageAsync(
        string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default) =>
        Menial.SuggestTriageAsync(title, description, categories, ct);

    public Task<SearchCriteria?> InterpretSearchAsync(
        string query, IReadOnlyList<string> projectNames, CancellationToken ct = default) =>
        Menial.InterpretSearchAsync(query, projectNames, ct);

    public Task<string?> SummarizeIssueAsync(
        string title, string? description, IReadOnlyList<string> notes, CancellationToken ct = default) =>
        Menial.SummarizeIssueAsync(title, description, notes, ct);

    public Task<ResolutionSuggestion?> SuggestResolutionAsync(ResolutionContext context, CancellationToken ct = default) =>
        Smart.SuggestResolutionAsync(context, ct);
}
