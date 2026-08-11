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
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Activity;

/// <summary>
/// A cheap per-user "has anything I can see changed?" token, used by the smart-poll auto-refresh: the
/// client fetches it every ~30s and reloads only when it changes. Computed over the issues the user may
/// see (ACL first), so it never reveals activity on issues they can't view. It is the count of visible
/// issues plus the newest UpdatedAt — which moves on any create, edit, status/assignee change, or
/// delete of a visible issue.
/// </summary>
public static class ActivityToken
{
    public static async Task<string> ComputeAsync(AppDbContext db, AccessSnapshot access, CancellationToken ct = default)
    {
        var visible = db.Issues.AsNoTracking().WhereVisibleTo(access);
        var count = await visible.CountAsync(ct);
        var maxTicks = await visible.MaxAsync(i => (long?)i.UpdatedAt.Ticks, ct) ?? 0L;
        return $"{count}:{maxTicks}";
    }
}
