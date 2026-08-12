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
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;

namespace OpenTrack.Infrastructure.Sla;

/// <summary>Manager-gated management of a project's SLA targets (one hour target per priority). Mirrors
/// the other per-project operations classes: reads return an empty list for a non-manager; writes throw.
/// A target of null/≤0 removes that priority's row (untracked).</summary>
public static class SlaPolicyOperations
{
    /// <summary>Sanity cap so a fat-fingered target can't overflow date math (100000 h ≈ 11 years).</summary>
    public const int MaxTargetHours = 100_000;

    public static async Task<IReadOnlyList<SlaPolicy>> ListForProjectAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject()) return [];
        return await db.SlaPolicies.AsNoTracking()
            .Where(p => p.ProjectId == projectId)
            .OrderBy(p => p.Priority)
            .ToListAsync(ct);
    }

    /// <summary>Set (or clear, when <paramref name="hours"/> is null/≤0) the target for one priority.</summary>
    public static async Task<string?> SetAsync(
        Data.AppDbContext db, AccessSnapshot access, int projectId, IssuePriority priority, int? hours, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing SLA targets requires the Manager role on this project.");
        if (hours > MaxTargetHours) return $"Target must be {MaxTargetHours} hours or fewer.";

        var existing = await db.SlaPolicies.FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Priority == priority, ct);
        if (hours is null or <= 0)
        {
            if (existing is not null) { db.SlaPolicies.Remove(existing); await db.SaveChangesAsync(ct); }
            return null;
        }
        if (existing is null)
            db.SlaPolicies.Add(new SlaPolicy { ProjectId = projectId, Priority = priority, TargetHours = hours.Value });
        else
            existing.TargetHours = hours.Value;
        await db.SaveChangesAsync(ct);
        return null;
    }
}
