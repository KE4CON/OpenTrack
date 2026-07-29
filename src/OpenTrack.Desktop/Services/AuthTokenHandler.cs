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

using System.Net.Http.Headers;

namespace OpenTrack.Desktop.Services;

/// <summary>
/// Attaches the signed-in user's bearer token to every outgoing API request. The token
/// is held by <see cref="DesktopAuthState"/>, which the login page populates after a
/// successful /api/auth/login call.
/// </summary>
public class AuthTokenHandler(DesktopAuthState auth) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(auth.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return base.SendAsync(request, cancellationToken);
    }
}
