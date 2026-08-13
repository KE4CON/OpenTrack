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

namespace OpenTrack.Infrastructure.Backup;

/// <summary>
/// Configuration for the scheduled database backup job, bound from the <c>OpenTrack:Backup</c>
/// configuration section. Backups are opt-in: nothing runs unless <see cref="Enabled"/> is true.
/// </summary>
public sealed class BackupOptions
{
    /// <summary>Configuration section these options bind from (e.g. in <c>appsettings.json</c>).</summary>
    public const string Section = "OpenTrack:Backup";

    /// <summary>Master switch. When false the scheduler does nothing. Defaults to false (opt-in).</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Folder to write backups into. When blank, a <c>backups</c> folder next to the live database
    /// file is used.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>How often to write a fresh snapshot. Defaults to once a day.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// How many of the newest snapshots to keep; older ones are pruned. Zero (or negative) means
    /// keep everything and never prune. Defaults to 7.
    /// </summary>
    public int Retention { get; set; } = 7;
}
