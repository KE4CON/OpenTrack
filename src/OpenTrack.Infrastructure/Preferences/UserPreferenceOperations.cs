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

using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Preferences;

public readonly record struct UserPreferenceItem(int? DefaultProjectId, IssueSort? DefaultSort);

/// <summary>A user's personal defaults, scoped strictly to the owning user id (no cross-user access).</summary>
public static class UserPreferenceOperations
{
    public static async Task<UserPreferenceItem> GetAsync(AppDbContext db, int userId, CancellationToken ct = default)
    {
        var p = await db.UserPreferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        return p is null ? new UserPreferenceItem(null, null) : new UserPreferenceItem(p.DefaultProjectId, p.DefaultSort);
    }

    public static async Task SaveAsync(AppDbContext db, int userId, int? defaultProjectId, IssueSort? defaultSort, CancellationToken ct = default)
    {
        var p = await db.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (p is null)
        {
            db.UserPreferences.Add(new UserPreference { UserId = userId, DefaultProjectId = defaultProjectId, DefaultSort = defaultSort });
        }
        else
        {
            p.DefaultProjectId = defaultProjectId;
            p.DefaultSort = defaultSort;
        }
        await db.SaveChangesAsync(ct);
    }
}
