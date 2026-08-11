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
using OpenTrack.Core.Authorization;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.UI.Services;

namespace OpenTrack.Web.Services;

/// <summary>
/// EF Core-backed implementation of <see cref="IOpenTrackDataService"/> for the web app.
/// Creates a short-lived <see cref="AppDbContext"/> per operation via the factory (a single scoped
/// context would live for the whole Blazor Server circuit and is not thread-safe), and enforces the
/// same per-project access rules as the Web API by calling the shared <see cref="AccessContext"/> /
/// <see cref="VisibilityQueries"/> from OpenTrack.Core/Infrastructure. Reads the current user from
/// the authenticated Blazor Server circuit.
///
/// Read methods return null/empty for content the user may not see (so a page renders "not found"
/// rather than leaking existence); write methods throw <see cref="UnauthorizedAccessException"/> for
/// forbidden actions (the UI hides those controls by role, so this is defense in depth).
/// </summary>
public class DbOpenTrackDataService(IDbContextFactory<AppDbContext> dbFactory, AuthenticationStateProvider authState)
    : IOpenTrackDataService
{
    private async Task<AccessIdentity> RequireIdentityAsync()
    {
        var state = await authState.GetAuthenticationStateAsync();
        return state.User.GetAccessIdentity()
            ?? throw new InvalidOperationException("Could not determine the signed-in user.");
    }

    private async Task<(AppDbContext Db, AccessSnapshot Access)> OpenAsync(CancellationToken ct)
    {
        var identity = await RequireIdentityAsync();
        var db = await dbFactory.CreateDbContextAsync(ct);
        var access = await AccessSnapshot.LoadAsync(db, identity, ct);
        return (db, access);
    }

    // ---- Projects ----

    public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await db.Projects.AsNoTracking()
            .WhereVisibleTo(access)
            .OrderBy(p => p.Name)
            .Select(p => new ProjectRow(
                p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId,
                p.Issues.Count(i => i.Status != IssueStatus.Closed)))
            .ToListAsync(ct);
    }

    public async Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var p = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !access.For(p.Id).CanViewProject(p.IsPublic))
            return null;
        return new ProjectDetail(p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId, p.CreatedAt);
    }

    public async Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        // Creating a project isn't scoped to an existing project: require global Manager+.
        if (!access.GlobalAtLeast(UserRole.Manager))
            throw new UnauthorizedAccessException("Creating a project requires the Manager role.");

        var project = new Project
        {
            Name = input.Name, Description = input.Description, IsPublic = input.IsPublic, OwnerId = access.UserId
        };
        project.Members.Add(new ProjectMembership { UserId = access.UserId, Role = UserRole.Manager });
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return project.Id;
    }

    public async Task UpdateProjectAsync(int id, UpdateProjectInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null || !access.For(project.Id).CanViewProject(project.IsPublic))
            return; // treat as not-found; don't leak existence
        if (!access.For(project.Id).CanEditProject())
            throw new UnauthorizedAccessException("Editing this project requires the Manager role on it.");

        project.Name = input.Name;
        project.Description = input.Description;
        project.IsPublic = input.IsPublic;
        await db.SaveChangesAsync(ct);
    }

    // ---- Issues ----

    public async Task<IReadOnlyList<IssueRow>> GetIssuesAsync(int? projectId = null, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var q = db.Issues.AsNoTracking().WhereVisibleTo(access);
        if (projectId is not null) q = q.Where(i => i.ProjectId == projectId);
        return await q
            .OrderByDescending(i => i.IsSticky).ThenByDescending(i => i.UpdatedAt)
            .Select(i => new IssueRow(
                i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status, i.Severity, i.Priority,
                i.Reporter.UserName ?? "unknown", i.Assignee != null ? i.Assignee.UserName : null, i.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var i = await db.Issues.AsNoTracking()
            .Include(x => x.Project).Include(x => x.Category)
            .Include(x => x.Reporter).Include(x => x.Assignee)
            .Include(x => x.Notes).ThenInclude(n => n.Author)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i is null) return null;

        var ctx = access.For(i.ProjectId);
        if (!ctx.CanViewIssue(i.Project.IsPublic, i.IsPrivate, i.ReporterId, i.AssigneeId))
            return null;

        return new IssueDetail(
            i.Id, i.ProjectId, i.Project.Name, i.Title, i.Description, i.StepsToReproduce,
            i.ExpectedBehavior, i.ActualBehavior,
            i.Status, i.Severity, i.Priority, i.Reproducibility, i.Resolution,
            i.ReporterId, i.Reporter.UserName ?? "unknown", i.AssigneeId, i.Assignee?.UserName,
            i.CategoryId, i.Category?.Name, i.IsSticky, i.IsPrivate, i.CreatedAt, i.UpdatedAt, i.DueDate,
            i.Notes.Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
                .OrderBy(n => n.CreatedAt)
                .Select(n => new IssueNoteView(n.Id, n.Author.UserName ?? "unknown", n.Text, n.CreatedAt))
                .ToList());
    }

    public async Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct)
            ?? throw new InvalidOperationException($"Project {projectId} does not exist.");

        var ctx = access.For(projectId);
        if (!ctx.CanViewProject(project.IsPublic))
            throw new UnauthorizedAccessException("You do not have access to this project.");
        if (!ctx.CanCreateIssue(project.IsPublic))
            throw new UnauthorizedAccessException("Reporting an issue requires the Reporter role on this project.");

        var issue = new Issue
        {
            ProjectId = projectId, Title = input.Title, Description = input.Description,
            StepsToReproduce = input.StepsToReproduce, ExpectedBehavior = input.ExpectedBehavior,
            ActualBehavior = input.ActualBehavior, CategoryId = input.CategoryId,
            Severity = input.Severity, Priority = input.Priority, Reproducibility = input.Reproducibility,
            DueDate = input.DueDate, ReporterId = access.UserId, Status = IssueStatus.New,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        // Add the creation-history row via the navigation collection so the issue and its history
        // persist in a SINGLE SaveChanges — atomic, no half-written issue with no history.
        issue.History.Add(new IssueHistory
        {
            UserId = access.UserId, FieldChanged = "Status",
            OldValue = null, NewValue = IssueStatus.New.ToString(), ChangedAt = DateTime.UtcNow
        });
        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct);
        return issue.Id;
    }

    public async Task UpdateIssueAsync(int id, UpdateIssueInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var issue = await db.Issues.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
        if (issue is null) return;

        var ctx = access.For(issue.ProjectId);
        if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return; // not-found to this user
        if (!ctx.CanEditIssue())
            throw new UnauthorizedAccessException("Editing this issue requires the Updater role on its project.");

        var originalStatus = issue.Status;
        var originalAssigneeId = issue.AssigneeId;

        // Fields any Updater may change.
        issue.Title = input.Title;
        issue.Description = input.Description;
        issue.StepsToReproduce = input.StepsToReproduce;
        issue.ExpectedBehavior = input.ExpectedBehavior;
        issue.ActualBehavior = input.ActualBehavior;
        issue.Status = input.Status;
        issue.Severity = input.Severity;
        issue.Priority = input.Priority;
        issue.Reproducibility = input.Reproducibility;
        issue.Resolution = input.Resolution;
        issue.CategoryId = input.CategoryId;
        issue.DueDate = input.DueDate;

        // Privileged fields: silently keep the existing value if the caller lacks the right, so a
        // crafted request can never escalate (the UI already hides these controls by role).
        if (ctx.CanAssignIssue() && await IsAssignableAsync(db, issue.ProjectId, input.AssigneeId, ct))
            issue.AssigneeId = input.AssigneeId;
        if (ctx.CanSetIssuePrivacy())
            issue.IsPrivate = input.IsPrivate;
        if (ctx.CanSetIssueSticky())
            issue.IsSticky = input.IsSticky;

        issue.UpdatedAt = DateTime.UtcNow;

        if (issue.Status != originalStatus)
            issue.History.Add(new IssueHistory
            {
                UserId = access.UserId, FieldChanged = "Status",
                OldValue = originalStatus.ToString(), NewValue = issue.Status.ToString(), ChangedAt = DateTime.UtcNow
            });
        if (issue.AssigneeId != originalAssigneeId)
            issue.History.Add(new IssueHistory
            {
                UserId = access.UserId, FieldChanged = "Assignee",
                OldValue = originalAssigneeId?.ToString(), NewValue = issue.AssigneeId?.ToString(), ChangedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync(ct);
    }

    /// <summary>An assignee must be null (unassign) or an actual member of the issue's project.</summary>
    private static async Task<bool> IsAssignableAsync(AppDbContext db, int projectId, int? assigneeId, CancellationToken ct)
    {
        if (assigneeId is null) return true;
        return await db.ProjectMemberships.AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId, ct);
    }

    public async Task AddIssueNoteAsync(int issueId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return;

        var ctx = access.For(issue.ProjectId);
        if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return;
        if (!ctx.CanAddNote())
            throw new UnauthorizedAccessException("Adding a note requires the Reporter role on this project.");

        db.IssueNotes.Add(new IssueNote
        {
            IssueId = issueId, AuthorId = access.UserId, Text = text, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    // ---- Lookups ----

    public async Task<IReadOnlyList<CategoryView>> GetProjectCategoriesAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic))
            return [];
        return await db.Categories.AsNoTracking()
            .Where(c => c.ProjectId == projectId).OrderBy(c => c.Name)
            .Select(c => new CategoryView(c.Id, c.Name))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectMemberView>> GetProjectMembersAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic))
            return [];
        var memberIds = await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.ProjectId == projectId).Select(m => m.UserId).ToListAsync(ct);
        return await db.Users.AsNoTracking()
            .Where(u => memberIds.Contains(u.Id)).OrderBy(u => u.UserName)
            .Select(u => new ProjectMemberView(u.Id, u.UserName ?? "unknown"))
            .ToListAsync(ct);
    }

    // ---- Member management (Manager+ on the project) ----

    public async Task<IReadOnlyList<ProjectMemberDetail>> GetProjectMemberDetailsAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanManageProject())
            return [];
        return await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.User.UserName)
            .Select(m => new ProjectMemberDetail(m.UserId, m.User.UserName ?? "unknown", m.Role, m.UserId == project.OwnerId))
            .ToListAsync(ct);
    }

    public async Task<string?> AddProjectMemberAsync(int projectId, AddProjectMemberInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return "Project not found.";
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing members requires the Manager role on this project.");
        if (!IsAssignableProjectRole(input.Role)) return "Invalid project role.";

        var normalized = input.Email.Trim().ToUpperInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is null) return $"No user with email '{input.Email}'. They must register first.";

        var already = await db.ProjectMemberships.AnyAsync(m => m.ProjectId == projectId && m.UserId == user.Id, ct);
        if (already) return "That user is already a member of this project.";

        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = projectId, UserId = user.Id, Role = input.Role });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task SetProjectMemberRoleAsync(int projectId, int userId, UserRole role, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing members requires the Manager role on this project.");
        if (!IsAssignableProjectRole(role)) throw new ArgumentException("Invalid project role.", nameof(role));
        // The owner always retains at least Manager so a project can't be left unmanageable.
        if (userId == project.OwnerId && (int)role < (int)UserRole.Manager)
            throw new InvalidOperationException("The project owner must remain a Manager.");

        var membership = await db.ProjectMemberships.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);
        if (membership is null) return;
        membership.Role = role;
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveProjectMemberAsync(int projectId, int userId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing members requires the Manager role on this project.");
        if (userId == project.OwnerId)
            throw new InvalidOperationException("The project owner cannot be removed.");

        var membership = await db.ProjectMemberships.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);
        if (membership is null) return;
        db.ProjectMemberships.Remove(membership);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Per-project roles range from Viewer to Manager; Administrator is global-only.</summary>
    private static bool IsAssignableProjectRole(UserRole role) =>
        (int)role >= (int)UserRole.Viewer && (int)role <= (int)UserRole.Manager;
}
