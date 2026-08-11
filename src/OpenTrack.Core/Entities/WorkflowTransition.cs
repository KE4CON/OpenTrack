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
/// One allowed status change on a project. If a project defines ANY transitions, its workflow becomes
/// restricted: only the listed From→To changes (plus leaving a status unchanged) are permitted. A
/// project with no transitions keeps the default open workflow (any editor may set any status).
/// </summary>
public class WorkflowTransition
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public IssueStatus FromStatus { get; set; }
    public IssueStatus ToStatus { get; set; }
}
