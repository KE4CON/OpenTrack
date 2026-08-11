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

public record IssueNoteView(int Id, string AuthorName, string Text, bool IsPrivate, DateTime CreatedAt);

public record IssueHistoryEntry(int Id, string UserName, string FieldChanged, string? OldValue, string? NewValue, DateTime ChangedAt);

public record AttachmentView(int Id, string FileName, long FileSize, string ContentType, string UploadedByName, DateTime UploadedAt);

/// <summary>A relationship as seen from the issue currently being viewed: the resolved reciprocal
/// label plus the OTHER issue (already filtered to ones the viewer may see).</summary>
public record IssueRelationshipView(int Id, int OtherIssueId, string OtherIssueTitle, string OtherProjectName, string Label);

public record TagView(int Id, string Name);

/// <summary>A user's saved issue filter — a name plus the issue-list query string to apply.</summary>
public record SavedFilterView(int Id, string Name, string Query);

/// <summary>A user's personal defaults.</summary>
public record PreferencesView(int? DefaultProjectId, IssueSort? DefaultSort);

/// <summary>A project's outgoing webhook (Manager-managed).</summary>
public record WebhookView(int Id, string Url, WebhookFormat Format, bool IsActive);

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
    IReadOnlyList<IssueNoteView> Notes);

public record CreateIssueInput(
    string Title, string Description, string? StepsToReproduce,
    string? ExpectedBehavior, string? ActualBehavior, int? CategoryId,
    IssueSeverity Severity, IssuePriority Priority, IssueReproducibility Reproducibility,
    DateTime? DueDate, int? AffectsVersionId, int? FixVersionId);

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
