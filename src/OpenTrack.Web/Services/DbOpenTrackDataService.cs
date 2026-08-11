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
using OpenTrack.Core;
using OpenTrack.Core.Authorization;
using OpenTrack.Core.Bulk;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Bulk;
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Attachments;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Checklist;
using OpenTrack.Infrastructure.CustomFields;
using OpenTrack.Infrastructure.Dashboard;
using OpenTrack.Infrastructure.Filters;
using OpenTrack.Infrastructure.Preferences;
using OpenTrack.Infrastructure.Reporting;
using OpenTrack.Infrastructure.TimeLogs;
using OpenTrack.Infrastructure.Webhooks;
using OpenTrack.Infrastructure.Workflow;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Notifications;
using OpenTrack.Infrastructure.Queries;
using OpenTrack.Infrastructure.Relationships;
using OpenTrack.Infrastructure.Tags;
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
public class DbOpenTrackDataService(
    IDbContextFactory<AppDbContext> dbFactory,
    AuthenticationStateProvider authState,
    IAttachmentStorage attachmentStorage,
    NotificationDispatch notifications)
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
        return new ProjectDetail(p.Id, p.Name, p.Description, p.IsPublic, p.OwnerId, p.CreatedAt, p.RowVersion, p.PublicIntakeEnabled);
    }

    public async Task SetPublicIntakeEnabledAsync(int projectId, bool enabled, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Changing public intake requires the Manager role on this project.");
        var p = await db.Projects.FirstOrDefaultAsync(x => x.Id == projectId, ct);
        if (p is null) return;
        p.PublicIntakeEnabled = enabled;
        await db.SaveChangesAsync(ct);
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

        db.Entry(project).Property(p => p.RowVersion).OriginalValue = input.RowVersion;
        project.Name = input.Name;
        project.Description = input.Description;
        project.IsPublic = input.IsPublic;
        project.RowVersion = Guid.NewGuid();
        await SaveDetectingConflictAsync(db, ct);
    }

    /// <summary>Saves, translating an EF optimistic-concurrency failure into our domain exception.</summary>
    private static async Task SaveDetectingConflictAsync(AppDbContext db, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException(ConcurrencyConflictException.DefaultMessage);
        }
    }

    // ---- Issues ----

    public async Task<IReadOnlyList<WorkflowTransitionView>> GetWorkflowAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await WorkflowOperations.ListForProjectAsync(db, access, projectId, ct);
        return items.Select(w => new WorkflowTransitionView(w.Id, w.FromStatus, w.ToStatus)).ToList();
    }

    public async Task<string?> AddWorkflowTransitionAsync(int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await WorkflowOperations.AddAsync(db, access, projectId, from, to, ct);
    }

    public async Task DeleteWorkflowTransitionAsync(int projectId, int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await WorkflowOperations.DeleteAsync(db, access, projectId, id, ct);
    }

    public async Task<IReadOnlyList<WebhookView>> GetWebhooksAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await WebhookOperations.ListForProjectAsync(db, access, projectId, ct);
        return items.Select(w => new WebhookView(w.Id, w.Url, w.Format, w.IsActive)).ToList();
    }

    public async Task<string?> AddWebhookAsync(int projectId, string url, WebhookFormat format, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await WebhookOperations.AddAsync(db, access, projectId, url, format, ct);
    }

    public async Task DeleteWebhookAsync(int projectId, int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await WebhookOperations.DeleteAsync(db, access, projectId, id, ct);
    }

    public async Task<IReadOnlyList<SavedFilterView>> GetSavedFiltersAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await SavedFilterOperations.ListForUserAsync(db, access.UserId, ct);
        return items.Select(f => new SavedFilterView(f.Id, f.Name, f.Query)).ToList();
    }

    public async Task<string?> SaveFilterAsync(string name, string query, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await SavedFilterOperations.SaveAsync(db, access.UserId, name, query, ct);
    }

    public async Task DeleteSavedFilterAsync(int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await SavedFilterOperations.DeleteAsync(db, access.UserId, id, ct);
    }

    public async Task<PreferencesView> GetPreferencesAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var p = await UserPreferenceOperations.GetAsync(db, access.UserId, ct);
        return new PreferencesView(p.DefaultProjectId, p.DefaultSort);
    }

    public async Task SavePreferencesAsync(int? defaultProjectId, IssueSort? defaultSort, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await UserPreferenceOperations.SaveAsync(db, access.UserId, defaultProjectId, defaultSort, ct);
    }

    public async Task<ReportView> GetReportAsync(int? projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var r = await ReportQuery.BuildAsync(db, access, projectId, DateTime.UtcNow, ct);
        static IReadOnlyList<ReportBarView> Map(IReadOnlyList<OpenTrack.Infrastructure.Reporting.ReportBar> b) =>
            b.Select(x => new ReportBarView(x.Label, x.Count)).ToList();
        return new ReportView(r.TotalIssues, r.OpenIssues, r.ResolvedThisMonth,
            Map(r.CreatedByMonth), Map(r.OpenByStatus), Map(r.OpenBySeverity));
    }

    public async Task<IReadOnlyList<RoadmapVersionView>> GetRoadmapAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var rows = await RoadmapQuery.BuildAsync(db, access, projectId, ct);
        return rows.Select(v => new RoadmapVersionView(v.VersionId, v.Name, v.IsReleased, v.ReleaseDate, v.Total, v.Done,
            v.Issues.Select(i => new RoadmapIssueView(i.Id, i.Title, i.Status, i.Severity, i.Done)).ToList())).ToList();
    }

    public async Task<IReadOnlyList<SimilarIssueView>> FindSimilarIssuesAsync(int? projectId, string title, int? excludeIssueId = null, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var rows = await SimilarIssueQuery.FindAsync(db, access, projectId, title, excludeIssueId, ct: ct);
        return rows.Select(r => new SimilarIssueView(r.Id, r.ProjectId, r.ProjectName, r.Title, r.Status)).ToList();
    }

    public async Task<IReadOnlyList<IssueRow>> GetIssuesAsync(IssueFilter filter, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await db.Issues.AsNoTracking()
            .WhereVisibleTo(access)   // ACL first — filtering can only narrow what the user may see
            .ApplyFilter(filter)
            .Select(i => new IssueRow(
                i.Id, i.ProjectId, i.Project.Name, i.Title, i.Status, i.Severity, i.Priority,
                i.Reporter.UserName ?? "unknown", i.Assignee != null ? i.Assignee.UserName : null, i.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<DashboardView> GetDashboardAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var r = await DashboardQuery.BuildAsync(db, access, DateTime.UtcNow, ct);
        return new DashboardView(
            r.TotalOpen,
            r.TotalOverdue,
            r.TotalStale,
            r.Projects.Select(p => new DashboardProjectSummary(p.ProjectId, p.ProjectName, p.OpenCount, p.OverdueCount)).ToList(),
            r.OpenBySeverity.Select(s => new DashboardSeverityCount(s.Severity, s.Count)).ToList(),
            r.Recent.Select(i => new IssueRow(
                i.Id, i.ProjectId, i.ProjectName, i.Title, i.Status, i.Severity, i.Priority,
                i.ReporterName, i.AssigneeName, i.UpdatedAt)).ToList());
    }

    public async Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var i = await db.Issues.AsNoTracking()
            .Include(x => x.Project).Include(x => x.Category)
            .Include(x => x.Reporter).Include(x => x.Assignee)
            .Include(x => x.AffectsVersion).Include(x => x.FixVersion)
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
            i.AffectsVersionId, i.AffectsVersion?.Name, i.FixVersionId, i.FixVersion?.Name, i.RowVersion,
            i.Notes.Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
                .OrderBy(n => n.CreatedAt)
                .Select(n => new IssueNoteView(n.Id, n.Author.UserName ?? "unknown", n.Text, n.IsPrivate, n.CreatedAt))
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
            DueDate = input.DueDate, AffectsVersionId = input.AffectsVersionId, FixVersionId = input.FixVersionId,
            ReporterId = access.UserId, Status = IssueStatus.New,
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
        // Notify the assignee/monitors and fire any project webhooks for the new issue (best-effort).
        await notifications.NotifyIssueChangedAsync(db, access.UserId, issue.Id, "a new issue was created", ct);
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

        db.Entry(issue).Property(i => i.RowVersion).OriginalValue = input.RowVersion;
        issue.RowVersion = Guid.NewGuid();

        var originalStatus = issue.Status;
        var originalAssigneeId = issue.AssigneeId;

        // Enforce the project's workflow (if it defines one) before accepting a status change.
        if (input.Status != originalStatus &&
            !await WorkflowOperations.IsAllowedAsync(db, issue.ProjectId, originalStatus, input.Status, ct))
            throw new InvalidOperationException($"Changing status from {originalStatus} to {input.Status} isn't allowed by this project's workflow.");

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
        issue.AffectsVersionId = input.AffectsVersionId;
        issue.FixVersionId = input.FixVersionId;

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

        await SaveDetectingConflictAsync(db, ct);
        var summary = UpdateSummary(originalStatus, issue.Status, originalAssigneeId, issue.AssigneeId);
        if (summary is not null) // only notify on a meaningful change, not every edit — avoids mail floods
            await notifications.NotifyIssueChangedAsync(db, access.UserId, id, summary, ct);
    }

    private static string? UpdateSummary(IssueStatus oldStatus, IssueStatus newStatus, int? oldAssignee, int? newAssignee)
    {
        var parts = new List<string>();
        if (newStatus != oldStatus) parts.Add($"status set to {newStatus}");
        if (newAssignee != oldAssignee) parts.Add(newAssignee is null ? "unassigned" : "reassigned");
        return parts.Count > 0 ? string.Join("; ", parts) : null;
    }

    /// <summary>An assignee must be null (unassign) or an actual member of the issue's project.</summary>
    private static async Task<bool> IsAssignableAsync(AppDbContext db, int projectId, int? assigneeId, CancellationToken ct)
    {
        if (assigneeId is null) return true;
        return await db.ProjectMemberships.AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId, ct);
    }

    public async Task AddIssueNoteAsync(int issueId, string text, bool isPrivate = false, CancellationToken ct = default)
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
        // A caller who cannot author private notes has the flag silently forced off.
        var effectivePrivate = isPrivate && ctx.CanAddPrivateNote();

        db.IssueNotes.Add(new IssueNote
        {
            IssueId = issueId, AuthorId = access.UserId, Text = text, IsPrivate = effectivePrivate, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        // Private notes only reach those who could see them anyway; NotifyIssueChangedAsync re-checks
        // view access per recipient, so no private-note text leaks to someone who can't view the issue.
        await notifications.NotifyIssueChangedAsync(db, access.UserId, issueId, "a note was added", ct);
    }

    public async Task<IReadOnlyList<IssueHistoryEntry>> GetIssueHistoryAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return [];
        var ctx = access.For(issue.ProjectId);
        if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return [];

        return await db.IssueHistories.AsNoTracking()
            .Where(h => h.IssueId == issueId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new IssueHistoryEntry(h.Id, h.User.UserName ?? "unknown", h.FieldChanged, h.OldValue, h.NewValue, h.ChangedAt))
            .ToListAsync(ct);
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

    // ---- Category & version management (Manager+ on the project) ----

    public async Task<string?> CreateCategoryAsync(int projectId, string name, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing categories requires the Manager role on this project.");
        name = name.Trim();
        if (string.IsNullOrEmpty(name)) return "Category name is required.";
        if (await db.Categories.AnyAsync(c => c.ProjectId == projectId && c.Name == name, ct))
            return "A category with that name already exists.";
        db.Categories.Add(new Category { ProjectId = projectId, Name = name });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task DeleteCategoryAsync(int projectId, int categoryId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing categories requires the Manager role on this project.");
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.ProjectId == projectId, ct);
        if (category is null) return;
        db.Categories.Remove(category); // issues referencing it have Category set null (SetNull FK)
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectVersionView>> GetProjectVersionsAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic))
            return [];
        return await db.Versions.AsNoTracking()
            .Where(v => v.ProjectId == projectId).OrderBy(v => v.Name)
            .Select(v => new ProjectVersionView(v.Id, v.Name, v.Description, v.ReleaseDate, v.IsReleased))
            .ToListAsync(ct);
    }

    public async Task<string?> CreateVersionAsync(int projectId, CreateVersionInput input, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing versions requires the Manager role on this project.");
        var name = input.Name.Trim();
        if (string.IsNullOrEmpty(name)) return "Version name is required.";
        if (await db.Versions.AnyAsync(v => v.ProjectId == projectId && v.Name == name, ct))
            return "A version with that name already exists.";
        db.Versions.Add(new ProjectVersion
        {
            ProjectId = projectId, Name = name, Description = input.Description,
            ReleaseDate = input.ReleaseDate, IsReleased = input.IsReleased
        });
        await db.SaveChangesAsync(ct);
        return null;
    }

    public async Task DeleteVersionAsync(int projectId, int versionId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing versions requires the Manager role on this project.");
        var version = await db.Versions.FirstOrDefaultAsync(v => v.Id == versionId && v.ProjectId == projectId, ct);
        if (version is null) return;
        db.Versions.Remove(version); // issues referencing it have the version set null (SetNull FK)
        await db.SaveChangesAsync(ct);
    }

    // ---- Attachments (metadata + delete; upload/download are host-specific stream endpoints) ----

    public async Task<IReadOnlyList<AttachmentView>> GetIssueAttachmentsAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null) return [];
        if (!access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return [];

        return await db.IssueAttachments.AsNoTracking()
            .Where(a => a.IssueId == issueId).OrderBy(a => a.UploadedAt)
            .Select(a => new AttachmentView(a.Id, a.FileName, a.FileSize, a.ContentType, a.UploadedBy.UserName ?? "unknown", a.UploadedAt))
            .ToListAsync(ct);
    }

    public async Task DeleteAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var attachment = await db.IssueAttachments.Include(a => a.Issue).ThenInclude(i => i.Project)
            .FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment is null) return;

        var issue = attachment.Issue;
        var ctx = access.For(issue.ProjectId);
        if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return;
        // The uploader may remove their own attachment; otherwise Updater+ on the project is required.
        if (attachment.UploadedById != access.UserId && !ctx.CanEditIssue())
            throw new UnauthorizedAccessException("You can only delete attachments you uploaded, unless you can edit the issue.");

        var storageKey = attachment.FilePath;
        db.IssueAttachments.Remove(attachment);
        await db.SaveChangesAsync(ct);
        await attachmentStorage.DeleteAsync(storageKey, ct);
    }

    // ---- Relationships (ACL logic shared with the API via RelationshipOperations) ----

    public async Task<IReadOnlyList<IssueRelationshipView>> GetIssueRelationshipsAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await RelationshipOperations.ListAsync(db, access, issueId, ct);
        return items.Select(i => new IssueRelationshipView(i.Id, i.OtherIssueId, i.OtherIssueTitle, i.OtherProjectName, i.Label)).ToList();
    }

    public async Task<string?> AddIssueRelationshipAsync(int sourceIssueId, int targetIssueId, IssueRelationshipType type, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await RelationshipOperations.AddAsync(db, access, sourceIssueId, targetIssueId, type, ct);
    }

    public async Task RemoveIssueRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await RelationshipOperations.RemoveAsync(db, access, relationshipId, ct);
    }

    // ---- Tags (ACL logic shared with the API via TagOperations) ----

    public async Task<IReadOnlyList<TagView>> GetAllTagsAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var items = await TagOperations.ListAllAsync(db, ct);
        return items.Select(t => new TagView(t.Id, t.Name)).ToList();
    }

    public async Task<IReadOnlyList<TagView>> GetIssueTagsAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await TagOperations.ListForIssueAsync(db, access, issueId, ct);
        return items.Select(t => new TagView(t.Id, t.Name)).ToList();
    }

    public async Task<string?> AddIssueTagAsync(int issueId, string tagName, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await TagOperations.AddAsync(db, access, issueId, tagName, ct);
    }

    public async Task RemoveIssueTagAsync(int issueId, int tagId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await TagOperations.RemoveAsync(db, access, issueId, tagId, ct);
    }

    // ---- Custom fields (ACL logic shared with the API via CustomFieldOperations) ----

    public async Task<IReadOnlyList<CustomFieldDefinitionView>> GetCustomFieldsAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await CustomFieldOperations.ListDefinitionsAsync(db, access, projectId, ct);
        return items.Select(d => new CustomFieldDefinitionView(d.Id, d.ProjectId, d.Name, d.Type, d.EnumOptions, d.Required, d.DisplayOrder)).ToList();
    }

    public async Task<string?> CreateCustomFieldAsync(int projectId, string name, CustomFieldType type, string? enumOptions, bool required, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await CustomFieldOperations.CreateDefinitionAsync(db, access, projectId, name, type, enumOptions, required, ct);
    }

    public async Task<string?> UpdateCustomFieldAsync(int projectId, int fieldId, string name, string? enumOptions, bool required, int displayOrder, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await CustomFieldOperations.UpdateDefinitionAsync(db, access, projectId, fieldId, name, enumOptions, required, displayOrder, ct);
    }

    public async Task DeleteCustomFieldAsync(int projectId, int fieldId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await CustomFieldOperations.DeleteDefinitionAsync(db, access, projectId, fieldId, ct);
    }

    public async Task<IReadOnlyList<CustomFieldValueView>> GetIssueCustomFieldsAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await CustomFieldOperations.ListValuesForIssueAsync(db, access, issueId, ct);
        return items.Select(v => new CustomFieldValueView(v.DefinitionId, v.Name, v.Type, v.EnumOptions, v.Required, v.DisplayOrder, v.Value)).ToList();
    }

    public async Task<string?> SetIssueCustomFieldAsync(int issueId, int fieldId, string? value, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await CustomFieldOperations.SetValueAsync(db, access, issueId, fieldId, value, ct);
    }

    // ---- Time logging (ACL logic shared with the API via TimeLogOperations) ----

    public async Task<IReadOnlyList<TimeLogView>> GetTimeLogsAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await TimeLogOperations.ListForIssueAsync(db, access, issueId, ct);
        return items.Select(t => new TimeLogView(t.Id, t.Minutes, t.Note, t.WorkedOn, t.AuthorName, t.AuthorId)).ToList();
    }

    public async Task<string?> AddTimeLogAsync(int issueId, int minutes, string? note, DateTime? workedOn, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await TimeLogOperations.AddAsync(db, access, issueId, minutes, note, workedOn, ct);
    }

    public async Task DeleteTimeLogAsync(int logId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await TimeLogOperations.DeleteAsync(db, access, logId, ct);
    }

    // ---- Bug-hunt checklist (ACL logic shared with the API via ChecklistOperations) ----

    public async Task<IReadOnlyList<ChecklistItemView>> GetChecklistAsync(int projectId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var items = await ChecklistOperations.ListForProjectAsync(db, access, projectId, ct);
        return items.Select(c => new ChecklistItemView(c.Id, c.ProjectId, c.Title, c.Details, c.Area, c.Status, c.Notes, c.LinkedIssueId, c.DisplayOrder)).ToList();
    }

    public async Task<string?> AddChecklistItemAsync(int projectId, string title, string? details, string? area, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await ChecklistOperations.AddItemAsync(db, access, projectId, title, details, area, ct);
    }

    public async Task<int> ImportChecklistAsync(int projectId, string text, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await ChecklistOperations.ImportAsync(db, access, projectId, text, ct);
    }

    public async Task<string?> UpdateChecklistItemAsync(int projectId, int itemId, string title, string? details, string? area, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await ChecklistOperations.UpdateItemAsync(db, access, projectId, itemId, title, details, area, ct);
    }

    public async Task DeleteChecklistItemAsync(int projectId, int itemId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await ChecklistOperations.DeleteItemAsync(db, access, projectId, itemId, ct);
    }

    public async Task<string?> SetChecklistItemStatusAsync(int projectId, int itemId, ChecklistItemStatus status, string? notes, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await ChecklistOperations.SetStatusAsync(db, access, projectId, itemId, status, notes, ct);
    }

    public async Task<int?> ConvertChecklistItemToIssueAsync(int projectId, int itemId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await ChecklistOperations.ConvertToIssueAsync(db, access, projectId, itemId, ct);
    }

    // ---- Monitoring & notifications ----

    public async Task<bool> IsMonitoringIssueAsync(int issueId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await db.IssueMonitors.AsNoTracking().AnyAsync(m => m.IssueId == issueId && m.UserId == access.UserId, ct);
    }

    public async Task SetIssueMonitorAsync(int issueId, bool monitor, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        // You can only monitor an issue you can currently see.
        if (issue is null || !access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
            return;

        var existing = await db.IssueMonitors.FirstOrDefaultAsync(m => m.IssueId == issueId && m.UserId == access.UserId, ct);
        if (monitor && existing is null)
            db.IssueMonitors.Add(new IssueMonitor { IssueId = issueId, UserId = access.UserId, CreatedAt = DateTime.UtcNow });
        else if (!monitor && existing is not null)
            db.IssueMonitors.Remove(existing);
        else return;
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetUnreadNotificationCountAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await db.Notifications.AsNoTracking().CountAsync(n => n.UserId == access.UserId && !n.IsRead, ct);
    }

    public async Task<IReadOnlyList<NotificationView>> GetNotificationsAsync(bool unreadOnly = false, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        var q = db.Notifications.AsNoTracking().Where(n => n.UserId == access.UserId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        return await q.OrderByDescending(n => n.CreatedAt).Take(200)
            .Select(n => new NotificationView(n.Id, n.IssueId, n.Text, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task MarkNotificationReadAsync(int notificationId, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        // Scoped to the caller's own notifications — can't touch anyone else's.
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == access.UserId, ct);
        if (n is null || n.IsRead) return;
        n.IsRead = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkAllNotificationsReadAsync(CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        await db.Notifications.Where(n => n.UserId == access.UserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task<BulkResult> BulkUpdateIssuesAsync(IReadOnlyCollection<int> issueIds, BulkAction action, CancellationToken ct = default)
    {
        var (db, access) = await OpenAsync(ct);
        await using var _ = db;
        return await BulkOperations.ApplyAsync(db, access, issueIds, action, notifications, ct);
    }
}
