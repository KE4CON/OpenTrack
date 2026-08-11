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
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Workflow;

public readonly record struct WorkflowTransitionItem(int Id, IssueStatus FromStatus, IssueStatus ToStatus);

/// <summary>
/// Per-project workflow rules: the set of allowed status transitions. Defining any turns the project's
/// workflow "restricted"; defining none leaves it open (the default). Managing rules is Manager-only;
/// the allow-check is used by every issue-status write path so the API, web, board, and bulk actions all
/// enforce the same workflow.
/// </summary>
public static class WorkflowOperations
{
    public static async Task<IReadOnlyList<WorkflowTransitionItem>> ListForProjectAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject()) return [];
        return await db.WorkflowTransitions.AsNoTracking()
            .Where(w => w.ProjectId == projectId)
            .OrderBy(w => w.FromStatus).ThenBy(w => w.ToStatus)
            .Select(w => new WorkflowTransitionItem(w.Id, w.FromStatus, w.ToStatus))
            .ToListAsync(ct);
    }

    public static async Task<string?> AddAsync(
        AppDbContext db, AccessSnapshot access, int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing the workflow requires the Manager role on this project.");
        if (from == to) return "Pick two different statuses.";
        if (await db.WorkflowTransitions.AnyAsync(w => w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct))
            return "That transition is already allowed.";
        db.WorkflowTransitions.Add(new WorkflowTransition { ProjectId = projectId, FromStatus = from, ToStatus = to });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int id, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing the workflow requires the Manager role on this project.");
        var w = await db.WorkflowTransitions.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId, ct);
        if (w is null) return false;
        db.WorkflowTransitions.Remove(w);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Is moving an issue in <paramref name="projectId"/> from <paramref name="from"/> to
    /// <paramref name="to"/> allowed? Always yes when the status is unchanged or the project defines no
    /// workflow; otherwise only the listed transitions are allowed.</summary>
    public static async Task<bool> IsAllowedAsync(
        AppDbContext db, int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
    {
        if (from == to) return true;
        var any = await db.WorkflowTransitions.AsNoTracking().AnyAsync(w => w.ProjectId == projectId, ct);
        if (!any) return true; // no workflow defined => open
        return await db.WorkflowTransitions.AsNoTracking()
            .AnyAsync(w => w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct);
    }
}
