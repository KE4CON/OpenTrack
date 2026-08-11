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
/// AI-assist configuration, bound from the "OpenTrack:Ai" configuration section (appsettings /
/// environment / user-secrets — never the database or the browser). Off unless <see cref="Enabled"/>
/// is true AND an <see cref="ApiKey"/> is present. The key is an Anthropic API key, billed to your
/// Anthropic API account — which is separate from any Claude subscription.
/// </summary>
public sealed class AiOptions
{
    public const string Section = "OpenTrack:Ai";

    public bool Enabled { get; set; }
    public string? ApiKey { get; set; }
    /// <summary>Model id. Defaults to a fast, inexpensive model suitable for triage/summaries.</summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
}
