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

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace OpenTrack.Desktop.Services;

/// <summary>
/// Bridges the desktop app's bearer-token session (<see cref="DesktopAuthState"/>) to Blazor's
/// authorization system so the shared OpenTrack.UI pages' [Authorize] and &lt;AuthorizeView&gt;
/// work unchanged.
///
/// Note: the tokens issued by ASP.NET Core Identity's /login endpoint are OPAQUE (encrypted),
/// not decodable JWTs, so we can't read claims out of the token itself. Instead we mark the
/// user as authenticated once a token is present, and carry the role/name that DesktopAuthState
/// captured from the API's user-info call. This keeps the shared pages working without trying
/// to decode a token that isn't a JWT.
/// </summary>
public class DesktopAuthenticationStateProvider(DesktopAuthState auth) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsIdentity identity;
        if (auth.IsSignedIn)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, auth.UserName ?? "user"),
                new(ClaimTypes.NameIdentifier, auth.UserId?.ToString() ?? "0"),
            };
            if (!string.IsNullOrEmpty(auth.Role))
                claims.Add(new Claim("OpenTrack.Role", auth.Role));

            identity = new ClaimsIdentity(claims, authenticationType: "opentrack-desktop");
        }
        else
        {
            identity = new ClaimsIdentity();
        }
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    /// <summary>Called by DesktopAuthState after sign-in/out so the UI re-evaluates auth.</summary>
    public void NotifyChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
