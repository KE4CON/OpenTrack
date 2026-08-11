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

namespace OpenTrack.Core.Entities;

/// <summary>
/// A project's outgoing webhook: when an issue in the project is created, changed, or noted, OpenTrack
/// POSTs a message to <see cref="Url"/> (shaped by <see cref="Format"/> for Slack/Discord/generic).
/// Configured by a project Manager. Delivery is best-effort — a failing webhook never blocks or breaks
/// the underlying edit.
/// </summary>
public class ProjectWebhook
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public WebhookFormat Format { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
