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

namespace OpenTrack.UI.Services;

/// <summary>
/// Authorization policy name constants, shared so pages and both hosts refer to the same
/// strings. The actual policy REGISTRATION is done per-host (Web, Desktop, API) because the
/// DI extension for it lives in ASP.NET Core packages the thin-client desktop app shouldn't
/// all pull in the same way. The role-threshold logic each host registers is identical.
/// </summary>
public static class PolicyNames
{
    public const string RequireUpdater = "RequireUpdater";
    public const string RequireDeveloper = "RequireDeveloper";
    public const string RequireManager = "RequireManager";
    public const string RequireAdministrator = "RequireAdministrator";
}
