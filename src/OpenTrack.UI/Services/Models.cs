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

using OpenTrack.Core.Enums;
using OpenTrack.Core.Querying;
using OpenTrack.Core.Sla;

namespace OpenTrack.UI.Services;

// View models shared by the UI layer, independent of how the data is fetched
// (direct EF Core in the web app, HTTP in the desktop app). These deliberately
// mirror the API contracts so the HTTP-backed implementation is a near-passthrough.

public record ProjectRow(int Id, string Name, string? Description, bool IsPublic, int OwnerId, int OpenIssueCount);

public record ProjectDetail(int Id, string Name, string? Description, bool IsPublic, int OwnerId, DateTime CreatedAt, Guid RowVersion, bool PublicIntakeEnabled = false);

public record CreateProjectInput(string Name, string? Description, bool IsPublic);

public record UpdateProjectInput(string Name, string? Description, bool IsPublic, Guid RowVersion);

public record IssueRow(
    int Id, int ProjectId, string ProjectName, string Title,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    string ReporterName, string? AssigneeName, DateTime UpdatedAt);

/// <summary>Per-project open/overdue tallies for the cross-project dashboard.</summary>
public record DashboardProjectSummary(int ProjectId, string ProjectName, int OpenCount, int OverdueCount);

/// <summary>Count of open issues at one severity, for the dashboard breakdown.</summary>
public record DashboardSeverityCount(IssueSeverity Severity, int Count);

/// <summary>The cross-project "where should I look" overview. Every number and row is already filtered
/// to what the signed-in user may see.</summary>
public record DashboardView(
    int TotalOpen,
    int TotalOverdue,
    int TotalStale,
    IReadOnlyList<DashboardProjectSummary> Projects,
    IReadOnlyList<DashboardSeverityCount> OpenBySeverity,
    IReadOnlyList<IssueRow> Recent);

/// <summary>A bug-hunt checklist item for a project (display + working state).</summary>
public record ChecklistItemView(
    int Id, int ProjectId, string Title, string? Details, string? Area,
    ChecklistItemStatus Status, string? Notes, int? LinkedIssueId, int DisplayOrder);

/// <summary>A possible-duplicate match for a proposed issue title.</summary>
public record SimilarIssueView(int Id, int ProjectId, string ProjectName, string Title, IssueStatus Status);

/// <summary>One time/work-log entry on an issue.</summary>
public record TimeLogView(int Id, int Minutes, string? Note, DateTime WorkedOn, string AuthorName, int AuthorId);

public record IssueNoteView(int Id, string AuthorName, string Text, bool IsPrivate, DateTime CreatedAt);

public record IssueHistoryEntry(int Id, string UserName, string FieldChanged, string? OldValue, string? NewValue, DateTime ChangedAt);

public record AttachmentView(int Id, string FileName, long FileSize, string ContentType, string UploadedByName, DateTime UploadedAt);

/// <summary>A relationship as seen from the issue currently being viewed: the resolved reciprocal
/// label plus the OTHER issue (already filtered to ones the viewer may see).</summary>
public record IssueRelationshipView(int Id, int OtherIssueId, string OtherIssueTitle, string OtherProjectName, string Label);

public record TagView(int Id, string Name);

/// <summary>A user's saved issue filter — a name plus the issue-list query string to apply.</summary>
public record SavedFilterView(int Id, string Name, string Query);

/// <summary>Per-project Git integration settings (Manager view; includes the webhook secret).</summary>
public record GitIntegrationView(bool Enabled, string? WebhookSecret, bool AutoResolve);

/// <summary>A commit that referenced an issue, for the issue's "Linked commits" list.</summary>
public record IssueCommitView(
    string Sha, string ShortSha, string Message, string? Author, string? Url, bool Closing, DateTime CommittedAtUtc);

/// <summary>A project's SLA target for one priority: resolve within this many hours of creation.</summary>
public record SlaTargetView(IssuePriority Priority, int TargetHours);

/// <summary>One open issue on the SLA board (at-risk or breached), with its due-by time.</summary>
public record SlaBoardItemView(
    int Id, int ProjectId, string ProjectName, string Title, IssuePriority Priority,
    string? AssigneeName, DateTime CreatedAtUtc, DateTime DueUtc, SlaStatus Status);

/// <summary>The SLA board: breached issues then at-risk issues, each most-overdue-first.</summary>
public record SlaBoardView(IReadOnlyList<SlaBoardItemView> Breached, IReadOnlyList<SlaBoardItemView> AtRisk);

/// <summary>A single issue's SLA standing (for the detail page).</summary>
public record SlaIssueView(SlaStatus Status, DateTime? DueUtc);

/// <summary>A project automation rule ("when a new issue matches → do"). Used for both read (Id set) and
/// write (Id = 0 means create). Null conditions/actions mean "don't care" / "no action".</summary>
public record AutomationRuleView(
    int Id, string Name, bool IsEnabled, int SortOrder,
    string? WhenTextContains, IssueSeverity? WhenSeverity, IssuePriority? WhenPriority, int? WhenCategoryId,
    IssueSeverity? SetSeverity, IssuePriority? SetPriority, IssueStatus? SetStatus,
    int? AssignToUserId, string? AddTag);

/// <summary>An AI-suggested triage for a proposed issue (each field a suggestion to accept or edit).</summary>
public record AiTriageView(IssueSeverity? Severity, IssuePriority? Priority, string? Category, IReadOnlyList<string> Tags);

/// <summary>A plain-English search the AI turned into issue-list filter fields. The UI maps these onto the
/// existing issue-list query string (project matched by name against the user's visible projects) — so it
/// can only ever produce a filter the user could have built by hand.</summary>
public record AiSearchView(
    IssueStatus? Status, IssueSeverity? Severity, IssuePriority? Priority,
    string? Text, bool Stale, IssueSort? Sort, string? ProjectName);

public record ReportBarView(string Label, int Count);

/// <summary>Reporting figures over the issues a user may see.</summary>
public record ReportView(
    int TotalIssues, int OpenIssues, int ResolvedThisMonth,
    IReadOnlyList<ReportBarView> CreatedByMonth,
    IReadOnlyList<ReportBarView> OpenByStatus,
    IReadOnlyList<ReportBarView> OpenBySeverity);

/// <summary>A user's personal defaults.</summary>
public record PreferencesView(int? DefaultProjectId, IssueSort? DefaultSort);

/// <summary>A project's outgoing webhook (Manager-managed).</summary>
public record WebhookView(int Id, string Url, WebhookFormat Format, bool IsActive);

/// <summary>An allowed status transition in a project's workflow.</summary>
public record WorkflowTransitionView(int Id, IssueStatus FromStatus, IssueStatus ToStatus);

/// <summary>An issue targeted to a version, for the roadmap/changelog.</summary>
public record RoadmapIssueView(int Id, string Title, IssueStatus Status, IssueSeverity Severity, bool Done);

/// <summary>A version with its targeted issues and progress (roadmap if unreleased, changelog if released).</summary>
public record RoadmapVersionView(
    int VersionId, string Name, bool IsReleased, DateTime? ReleaseDate, int Total, int Done, IReadOnlyList<RoadmapIssueView> Issues);

public record NotificationView(int Id, int IssueId, string Text, bool IsRead, DateTime CreatedAt);

/// <summary>A custom-field definition on a project (for the manage screen and for rendering inputs).</summary>
public record CustomFieldDefinitionView(int Id, int ProjectId, string Name, CustomFieldType Type, string? EnumOptions, bool Required, int DisplayOrder);

/// <summary>A custom-field definition paired with an issue's current value for it (null when unset).</summary>
public record CustomFieldValueView(int DefinitionId, string Name, CustomFieldType Type, string? EnumOptions, bool Required, int DisplayOrder, string? Value);

public record IssueDetail(
    int Id, int ProjectId, string ProjectName, string Title, string Description,
    string? StepsToReproduce, string? ExpectedBehavior, string? ActualBehavior,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    IssueReproducibility Reproducibility, IssueResolution Resolution,
    int ReporterId, string ReporterName, int? AssigneeId, string? AssigneeName,
    int? CategoryId, string? CategoryName, bool IsSticky, bool IsPrivate,
    DateTime CreatedAt, DateTime UpdatedAt, DateTime? DueDate,
    int? AffectsVersionId, string? AffectsVersionName,
    int? FixVersionId, string? FixVersionName,
    Guid RowVersion,
    IReadOnlyList<IssueNoteView> Notes,
    double? Latitude = null, double? Longitude = null);

public record CreateIssueInput(
    string Title, string Description, string? StepsToReproduce,
    string? ExpectedBehavior, string? ActualBehavior, int? CategoryId,
    IssueSeverity Severity, IssuePriority Priority, IssueReproducibility Reproducibility,
    DateTime? DueDate, int? AffectsVersionId, int? FixVersionId,
    double? Latitude = null, double? Longitude = null);

public record UpdateIssueInput(
    string Title, string Description, string? StepsToReproduce,
    string? ExpectedBehavior, string? ActualBehavior,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    IssueReproducibility Reproducibility, IssueResolution Resolution,
    int? AssigneeId, int? CategoryId, bool IsSticky, bool IsPrivate, DateTime? DueDate,
    int? AffectsVersionId, int? FixVersionId, Guid RowVersion);

public record CategoryView(int Id, string Name);

public record ProjectVersionView(int Id, string Name, string? Description, DateTime? ReleaseDate, bool IsReleased);

public record CreateVersionInput(string Name, string? Description, DateTime? ReleaseDate, bool IsReleased);

public record ProjectMemberView(int Id, string UserName);

/// <summary>A project member together with their per-project role, for the member-management UI.</summary>
public record ProjectMemberDetail(int UserId, string UserName, UserRole Role, bool IsOwner);

public record AddProjectMemberInput(string Email, UserRole Role);

public record SetMemberRoleInput(UserRole Role);
