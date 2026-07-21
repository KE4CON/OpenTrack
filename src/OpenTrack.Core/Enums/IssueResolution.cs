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

public enum IssueResolution
{
    Open = 10,
    Fixed = 20,
    Reopened = 30,
    UnableToReproduce = 40,
    NotFixable = 50,
    Duplicate = 60,
    NotABug = 70,
    Suspended = 80,
    WontFix = 90
}
