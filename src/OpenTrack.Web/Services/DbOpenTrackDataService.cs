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

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Data;
using OpenTrack.UI.Services;

namespace OpenTrack.Web.Services;

/// <summary>
/// EF Core-backed implementation of <see cref="IOpenTrackDataService"/> for the web app.
/// Talks directly to <see cref="AppDbContext"/> and resolves the current user from the
/// authenticated Blazor Server circuit. This is the exact logic that previously lived
/// inline in the CRUD pages, lifted behind the shared interface so the desktop app can
/// provide an HTTP-backed equivalent without the pages changing.
/// </summary>
public class DbOpenTrackDataService(AppDbContext db, AuthenticationStateProvider authState)
    : IOpenTrackDataService
{
    private async Task<int> RequireUserIdAsync()
    {
        var state = await authState.GetAuthenticationStateAsync();
        var claim = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim is null || !int.TryParse(claim, out var id))
            throw new InvalidOperationException("Could not determine the signed-in user.");
        return id;
    }

    public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default) =>
        await db.Projects.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProjectRow(
                p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId,
                p.Issues.Count(i => i.Status != IssueStatus.Closed)))
            .ToListAsync(ct);

    public async Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default)
    {
        var p = await db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return p is null ? null : new ProjectDetail(p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId, p.CreatedAt);
    }

    public async Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default)
    {
        var userId = await RequireUserIdAsync();
        var project = new Project
        {
            Name = input.Name, Description = input.Description, IsPublic = input.IsPublic, OwnerId = userId
        };
        project.Members.Add(new ProjectMembership { UserId = userId, Role = UserRole.Manager });
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        return project.Id;
    }

    public async Task UpdateProjectAsync(int id, UpdateProjectInput input, CancellationToken ct = default)
    {
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return;
        project.Name = input.Name;
        project.Description = input.Description;
        project.IsPublic = input.IsPublic;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<IssueRow>> GetIssuesAsync(int? projectId = null, CancellationToken ct = default)
    {
        var q = db.Issues.AsNoTracking().AsQueryable();
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
        var i = await db.Issues.AsNoTracking()
            .Include(x => x.Project).Include(x => x.Category)
            .Include(x => x.Reporter).Include(x => x.Assignee)
            .Include(x => x.Notes).ThenInclude(n => n.Author)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (i is null) return null;

        return new IssueDetail(
            i.Id, i.ProjectId, i.Project.Name, i.Title, i.Description, i.StepsToReproduce,
            i.Status, i.Severity, i.Priority, i.Reproducibility, i.Resolution,
            i.ReporterId, i.Reporter.UserName ?? "unknown", i.AssigneeId, i.Assignee?.UserName,
            i.CategoryId, i.Category?.Name, i.IsSticky, i.IsPrivate, i.CreatedAt, i.UpdatedAt,
            i.Notes.OrderBy(n => n.CreatedAt)
                .Select(n => new IssueNoteView(n.Id, n.Author.UserName ?? "unknown", n.Text, n.CreatedAt))
                .ToList());
    }

    public async Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default)
    {
        var userId = await RequireUserIdAsync();
        var issue = new Issue
        {
            ProjectId = projectId, Title = input.Title, Description = input.Description,
            StepsToReproduce = input.StepsToReproduce, CategoryId = input.CategoryId,
            Severity = input.Severity, Priority = input.Priority, Reproducibility = input.Reproducibility,
            ReporterId = userId, Status = IssueStatus.New,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct);

        db.IssueHistories.Add(new IssueHistory
        {
            IssueId = issue.Id, UserId = userId, FieldChanged = "Status",
            OldValue = null, NewValue = IssueStatus.New.ToString(), ChangedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return issue.Id;
    }

    public async Task UpdateIssueAsync(int id, UpdateIssueInput input, CancellationToken ct = default)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (issue is null) return;
        var userId = await RequireUserIdAsync();

        var originalStatus = issue.Status;
        var originalAssigneeId = issue.AssigneeId;

        issue.Title = input.Title;
        issue.Description = input.Description;
        issue.Status = input.Status;
        issue.Severity = input.Severity;
        issue.Priority = input.Priority;
        issue.Resolution = input.Resolution;
        issue.AssigneeId = input.AssigneeId;
        issue.CategoryId = input.CategoryId;
        issue.IsSticky = input.IsSticky;
        issue.IsPrivate = input.IsPrivate;
        issue.UpdatedAt = DateTime.UtcNow;

        if (issue.Status != originalStatus)
            db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id, UserId = userId, FieldChanged = "Status",
                OldValue = originalStatus.ToString(), NewValue = issue.Status.ToString(), ChangedAt = DateTime.UtcNow
            });
        if (issue.AssigneeId != originalAssigneeId)
            db.IssueHistories.Add(new IssueHistory
            {
                IssueId = issue.Id, UserId = userId, FieldChanged = "Assignee",
                OldValue = originalAssigneeId?.ToString(), NewValue = issue.AssigneeId?.ToString(), ChangedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync(ct);
    }

    public async Task AddIssueNoteAsync(int issueId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var userId = await RequireUserIdAsync();
        db.IssueNotes.Add(new IssueNote
        {
            IssueId = issueId, AuthorId = userId, Text = text, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CategoryView>> GetProjectCategoriesAsync(int projectId, CancellationToken ct = default) =>
        await db.Categories.AsNoTracking()
            .Where(c => c.ProjectId == projectId).OrderBy(c => c.Name)
            .Select(c => new CategoryView(c.Id, c.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectMemberView>> GetProjectMembersAsync(int projectId, CancellationToken ct = default)
    {
        var memberIds = await db.ProjectMemberships.AsNoTracking()
            .Where(m => m.ProjectId == projectId).Select(m => m.UserId).ToListAsync(ct);
        return await db.Users.AsNoTracking()
            .Where(u => memberIds.Contains(u.Id)).OrderBy(u => u.UserName)
            .Select(u => new ProjectMemberView(u.Id, u.UserName ?? "unknown"))
            .ToListAsync(ct);
    }
}
