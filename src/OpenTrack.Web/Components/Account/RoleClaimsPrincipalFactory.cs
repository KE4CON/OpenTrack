// OpenTrack - an open-source bug and issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenTrack.Core.Entities;

namespace OpenTrack.Web.Components.Account;

/// <summary>
/// Adds the user's global <see cref="User.Role"/> as an "OpenTrack.Role" claim on the
/// signed-in principal, so [Authorize(Policy = "Require...")] and &lt;AuthorizeView&gt;
/// can check role level without a database round trip on every request. Per-project
/// roles (ProjectMembership) are checked separately, closer to the project/issue data.
/// </summary>
public class RoleClaimsPrincipalFactory(
    UserManager<User> userManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<User>(userManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var principal = await base.CreateAsync(user);
        if (principal.Identity is ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim("OpenTrack.Role", user.Role.ToString()));
        }
        return principal;
    }
}
