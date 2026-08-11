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

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Services;

public record AdminUserRow(int Id, string UserName, string? Email, UserRole Role, bool IsActive);

/// <summary>
/// Web-only administrative operations over the global user directory (set global role, activate/
/// deactivate). Deliberately NOT part of the shared IOpenTrackDataService seam: user administration
/// is a web-based admin surface (as in MantisBT), so it doesn't need a desktop/HTTP implementation.
/// Every method requires the caller to be a global Administrator; a caller cannot change their own
/// role or deactivate themselves (prevents an admin locking the last admin out).
/// </summary>
public sealed class AdminService(IDbContextFactory<AppDbContext> dbFactory, AuthenticationStateProvider authState)
{
    private async Task<(AppDbContext Db, int CallerId)> OpenAsAdminAsync(CancellationToken ct)
    {
        var state = await authState.GetAuthenticationStateAsync();
        var identity = state.User.GetAccessIdentity()
            ?? throw new UnauthorizedAccessException("Not signed in.");
        if ((int)identity.GlobalRole < (int)UserRole.Administrator)
            throw new UnauthorizedAccessException("Administrator role required.");
        return (await dbFactory.CreateDbContextAsync(ct), identity.UserId);
    }

    public async Task<IReadOnlyList<AdminUserRow>> GetUsersAsync(CancellationToken ct = default)
    {
        var (db, _) = await OpenAsAdminAsync(ct);
        await using var _d = db;
        return await db.Users.AsNoTracking().OrderBy(u => u.UserName)
            .Select(u => new AdminUserRow(u.Id, u.UserName ?? "unknown", u.Email, u.Role, u.IsActive))
            .ToListAsync(ct);
    }

    public async Task SetUserRoleAsync(int userId, UserRole role, CancellationToken ct = default)
    {
        var (db, callerId) = await OpenAsAdminAsync(ct);
        await using var _d = db;
        if (userId == callerId)
            throw new InvalidOperationException("You cannot change your own role.");
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.Role = role;
        await db.SaveChangesAsync(ct);
    }

    public async Task SetUserActiveAsync(int userId, bool active, CancellationToken ct = default)
    {
        var (db, callerId) = await OpenAsAdminAsync(ct);
        await using var _d = db;
        if (userId == callerId && !active)
            throw new InvalidOperationException("You cannot deactivate your own account.");
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.IsActive = active;
        await db.SaveChangesAsync(ct);
    }
}
