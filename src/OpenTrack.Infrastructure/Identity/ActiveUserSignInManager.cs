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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTrack.Core.Entities;

namespace OpenTrack.Infrastructure.Identity;

/// <summary>
/// Enforces <see cref="User.IsActive"/> at the sign-in gate: a deactivated account cannot sign in
/// on either surface (cookie/web or bearer/API), since both go through <c>SignInManager</c>. Closes
/// audit finding M7 (IsActive was stored but never checked). Overriding <see cref="CanSignInAsync"/>
/// also blocks token refresh for a user deactivated after login.
/// </summary>
public sealed class ActiveUserSignInManager(
    UserManager<User> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<User> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<User>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<User> confirmation)
    : SignInManager<User>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
{
    public override async Task<bool> CanSignInAsync(User user)
    {
        if (!user.IsActive)
        {
            Logger.LogInformation("Sign-in blocked for deactivated user {UserId}.", user.Id);
            return false;
        }
        return await base.CanSignInAsync(user);
    }
}
