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

namespace OpenTrack.Core.Enums;

/// <summary>How a project webhook's payload is shaped for its destination.</summary>
public enum WebhookFormat
{
    /// <summary>Full structured JSON (event, project, issue, actor, timestamp).</summary>
    Generic = 0,
    /// <summary>Slack incoming webhook — a { "text": "…" } message.</summary>
    Slack = 1,
    /// <summary>Discord webhook — a { "content": "…" } message.</summary>
    Discord = 2,
}
