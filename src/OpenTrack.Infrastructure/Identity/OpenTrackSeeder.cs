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

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Identity;

/// <summary>
/// Safe, out-of-band administrator bootstrap (audit finding M8). Instead of "the first HTTP caller to
/// reach the register page becomes admin" (a race, and an attacker-beats-operator hole on a fresh
/// public deployment), an operator sets <c>OpenTrack:BootstrapAdmin:Email</c> and
/// <c>:Password</c> in configuration and this seeder creates/promotes exactly that account at
/// startup. Idempotent — safe to run on every launch.
/// </summary>
public static class OpenTrackSeeder
{
    public static async Task EnsureBootstrapAdminAsync(IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        var email = configuration["OpenTrack:BootstrapAdmin:Email"];
        var password = configuration["OpenTrack:BootstrapAdmin:Password"];
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("OpenTrackSeeder");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return; // no bootstrap configured — nothing to do

        var userManager = services.GetRequiredService<UserManager<User>>();
        var existing = await userManager.FindByEmailAsync(email);

        if (existing is null)
        {
            var admin = new User { UserName = email, Email = email, EmailConfirmed = true, IsActive = true, Role = UserRole.Administrator };
            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
                logger?.LogInformation("Bootstrap administrator {Email} created.", email);
            else
                logger?.LogWarning("Bootstrap administrator {Email} could not be created: {Errors}",
                    email, string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        else if (existing.Role != UserRole.Administrator || !existing.IsActive)
        {
            existing.Role = UserRole.Administrator;
            existing.IsActive = true;
            await userManager.UpdateAsync(existing);
            logger?.LogInformation("Bootstrap administrator {Email} promoted/reactivated.", email);
        }
    }

    /// <summary>True if at least one active administrator account exists.</summary>
    public static Task<bool> HasAdministratorAsync(AppDbContext db, CancellationToken ct = default) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Role == UserRole.Administrator && u.IsActive, ct);
}
