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

/// <summary>Access levels, ascending in privilege (Mantis-style numeric values).</summary>
public enum UserRole
{
    Viewer = 10,
    Reporter = 25,
    Updater = 40,
    Developer = 55,
    Manager = 70,
    Administrator = 90
}
