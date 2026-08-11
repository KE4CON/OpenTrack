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

namespace OpenTrack.UI.Services;

// View models shared by the UI layer, independent of how the data is fetched
// (direct EF Core in the web app, HTTP in the desktop app). These deliberately
// mirror the API contracts so the HTTP-backed implementation is a near-passthrough.

public record ProjectRow(int Id, string Name, string? Description, bool IsPublic, int OwnerId, int OpenIssueCount);

public record ProjectDetail(int Id, string Name, string? Description, bool IsPublic, int OwnerId, DateTime CreatedAt, Guid RowVersion);

public record CreateProjectInput(string Name, string? Description, bool IsPublic);

public record UpdateProjectInput(string Name, string? Description, bool IsPublic, Guid RowVersion);

public record IssueRow(
    int Id, int ProjectId, string ProjectName, string Title,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    string ReporterName, string? AssigneeName, DateTime UpdatedAt);

public record IssueNoteView(int Id, string AuthorName, string Text, bool IsPrivate, DateTime CreatedAt);

public record IssueHistoryEntry(int Id, string UserName, string FieldChanged, string? OldValue, string? NewValue, DateTime ChangedAt);

public record AttachmentView(int Id, string FileName, long FileSize, string ContentType, string UploadedByName, DateTime UploadedAt);

/// <summary>A relationship as seen from the issue currently being viewed: the resolved reciprocal
/// label plus the OTHER issue (already filtered to ones the viewer may see).</summary>
public record IssueRelationshipView(int Id, int OtherIssueId, string OtherIssueTitle, string OtherProjectName, string Label);

public record TagView(int Id, string Name);

public record NotificationView(int Id, int IssueId, string Text, bool IsRead, DateTime CreatedAt);

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
