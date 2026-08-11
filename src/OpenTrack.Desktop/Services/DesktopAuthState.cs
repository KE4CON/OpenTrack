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

using System.Net.Http.Json;

namespace OpenTrack.Desktop.Services;

/// <summary>
/// Holds the desktop client's current API session (bearer + refresh tokens, plus the
/// signed-in user's id/name/role) and drives login/logout. After a successful login it
/// calls /api/auth/me to fetch the user's OpenTrack role, since the Identity login token
/// is opaque and can't be decoded client-side. Tokens are in-memory only.
/// </summary>
public class DesktopAuthState(IHttpClientFactory httpFactory)
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public int? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? Role { get; private set; }
    public bool IsSignedIn => !string.IsNullOrEmpty(AccessToken);

    public event Action? Changed;

    private sealed record LoginResponse(string tokenType, string accessToken, int expiresIn, string refreshToken);
    private sealed record MeResponse(string? id, string? name, string? role);

    public async Task<string?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        // Create the anonymous client per attempt so it always uses the CURRENT server address
        // (the user may have just changed it on the Settings page before signing in).
        var http = httpFactory.CreateClient("OpenTrackApiAnon");
        try
        {
            var resp = await http.PostAsJsonAsync("/api/auth/login", new { email, password }, ct);
            if (!resp.IsSuccessStatusCode)
                return resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Invalid email or password."
                    : $"Login failed ({(int)resp.StatusCode}).";

            var tokens = await resp.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            if (tokens is null) return "Login response could not be read.";

            AccessToken = tokens.accessToken;
            RefreshToken = tokens.refreshToken;

            try
            {
                using var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
                meReq.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
                var meResp = await http.SendAsync(meReq, ct);
                if (meResp.IsSuccessStatusCode)
                {
                    var me = await meResp.Content.ReadFromJsonAsync<MeResponse>(cancellationToken: ct);
                    if (me is not null)
                    {
                        UserName = me.name;
                        Role = me.role;
                        UserId = int.TryParse(me.id, out var uid) ? uid : null;
                    }
                }
            }
            catch
            {
                // If /me fails, we're still logged in; role-gated features just won't light up.
            }

            Changed?.Invoke();
            return null;
        }
        catch (Exception ex)
        {
            return $"Could not reach the server: {ex.Message}";
        }
    }

    public void SignOut()
    {
        AccessToken = null;
        RefreshToken = null;
        UserId = null;
        UserName = null;
        Role = null;
        Changed?.Invoke();
    }
}