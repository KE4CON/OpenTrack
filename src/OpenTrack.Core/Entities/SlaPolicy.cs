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
/// A per-project service-level target: how many hours a new issue of a given <see cref="Priority"/> has to
/// be resolved before it breaches. One row per (project, priority); a priority with no row is simply not
/// tracked. Higher-priority issues get shorter targets.
/// </summary>
public class SlaPolicy
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public IssuePriority Priority { get; set; }

    /// <summary>Resolution target in hours from issue creation. Always &gt; 0 (a zero/blank target means the
    /// row is deleted, i.e. that priority is untracked).</summary>
    public int TargetHours { get; set; }
}
