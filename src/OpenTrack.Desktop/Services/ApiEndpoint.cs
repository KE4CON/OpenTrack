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

namespace OpenTrack.Desktop.Services;

/// <summary>
/// Holds the current OpenTrack.API base address for the desktop client. Every named HttpClient reads
/// its base address from here at creation time, so changing it (via the in-app Settings page) takes
/// effect on the next request without an app restart. The value is seeded at startup from the saved
/// user preference, falling back to the bundled appsettings default.
/// </summary>
public sealed class ApiEndpoint(string baseUrl)
{
    /// <summary>The preference key under which the chosen server address is persisted per machine.</summary>
    public const string PreferenceKey = "ApiBaseUrl";

    public string BaseUrl { get; set; } = baseUrl;
}
