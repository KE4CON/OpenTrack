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

/// <summary>Where a bug-hunt checklist item stands as you work through it.</summary>
public enum ChecklistItemStatus
{
    /// <summary>Not checked yet.</summary>
    Pending = 0,
    /// <summary>Checked and fine.</summary>
    Pass = 1,
    /// <summary>Checked and found a problem — usually turned into a linked issue.</summary>
    Fail = 2,
    /// <summary>Doesn't apply to this project.</summary>
    NotApplicable = 3,
}
