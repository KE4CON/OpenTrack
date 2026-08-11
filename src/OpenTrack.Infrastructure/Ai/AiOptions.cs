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
/// is true AND the chosen provider is usable.
/// <para>
/// Two providers are supported. <see cref="Provider"/> = "anthropic" (the default) talks to Anthropic's
/// Claude API. <see cref="Provider"/> = "openai" talks to any OpenAI-<em>compatible</em> Chat Completions
/// endpoint — that one setting covers OpenAI itself, Azure OpenAI, Groq, OpenRouter, and local/offline
/// engines such as Ollama and LM Studio (point <see cref="BaseUrl"/> at them; a local engine keeps all
/// data on your own machine). Any API key is billed to <em>that</em> provider's account and is separate
/// from any Claude subscription.
/// </para>
/// </summary>
public sealed class AiOptions
{
    public const string Section = "OpenTrack:Ai";

    public bool Enabled { get; set; }

    /// <summary>"anthropic" (Claude, default) or "openai" (any OpenAI-compatible Chat Completions API).</summary>
    public string Provider { get; set; } = "anthropic";

    public string? ApiKey { get; set; }

    /// <summary>Model id. Defaults to a fast, inexpensive Claude model; set this to match your provider
    /// (e.g. "gpt-4o-mini" for OpenAI, or a local model name like "llama3.1" for Ollama).</summary>
    public string Model { get; set; } = "claude-haiku-4-5-20251001";

    /// <summary>Only used by the "openai" provider: the base URL of the OpenAI-compatible API. Leave null
    /// for OpenAI's own cloud (https://api.openai.com/v1). Set it for alternatives, e.g.
    /// "http://localhost:11434/v1" (Ollama) or "http://localhost:1234/v1" (LM Studio).</summary>
    public string? BaseUrl { get; set; }

    /// <summary>True when this provider has what it needs to make a call. Cloud providers need a key; a
    /// local OpenAI-compatible engine (identified by a custom <see cref="BaseUrl"/>) does not.</summary>
    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(ApiKey) ||
        (IsOpenAi && !string.IsNullOrWhiteSpace(BaseUrl));

    public bool IsOpenAi => string.Equals(Provider, "openai", StringComparison.OrdinalIgnoreCase);

    /// <summary>Resolved base URL for the "openai" provider (default: OpenAI's own cloud).</summary>
    public string ResolvedOpenAiBaseUrl =>
        string.IsNullOrWhiteSpace(BaseUrl) ? "https://api.openai.com/v1" : BaseUrl.TrimEnd('/');
}
