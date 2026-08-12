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

namespace OpenTrack.Core.Entities;

/// <summary>
/// Per-project Git integration settings. When <see cref="Enabled"/>, a GitHub push webhook posted to
/// <c>/git/webhook/{projectId}</c> is accepted if its <c>X-Hub-Signature-256</c> HMAC matches
/// <see cref="WebhookSecret"/>; commit messages are then scanned for issue references (e.g. "fixes #123")
/// to link commits to issues and, when <see cref="AutoResolve"/> is on and a closing keyword is used, move
/// the issue to Resolved (only if the project's workflow permits it).
/// </summary>
public class GitIntegration
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public bool Enabled { get; set; }

    /// <summary>Shared secret used to verify the GitHub webhook signature. Stored server-side only.</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>Move an issue to Resolved when a pushed commit uses a closing keyword ("fixes #123").</summary>
    public bool AutoResolve { get; set; } = true;
}
