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

namespace OpenTrack.API.Contracts;

public record IssueDto(
    int Id, int ProjectId, string ProjectName, string Title,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    string ReporterName, string? AssigneeName, DateTime UpdatedAt);

public record IssueDetailDto(
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
    IReadOnlyList<IssueNoteDto> Notes);

public record IssueNoteDto(int Id, string AuthorName, string Text, bool IsPrivate, DateTime CreatedAt);

public record IssueHistoryDto(int Id, string UserName, string FieldChanged, string? OldValue, string? NewValue, DateTime ChangedAt);

public record AttachmentDto(int Id, string FileName, long FileSize, string ContentType, string UploadedByName, DateTime UploadedAt);

public record IssueRelationshipDto(int Id, int OtherIssueId, string OtherIssueTitle, string OtherProjectName, string Label);

public record AddRelationshipRequest(int TargetIssueId, IssueRelationshipType Type);

public record TagDto(int Id, string Name);

public record AddTagRequest(string Name);

public record NotificationDto(int Id, int IssueId, string Text, bool IsRead, DateTime CreatedAt);

public record BulkUpdateRequest(int[] IssueIds, OpenTrack.Core.Bulk.BulkActionType Type, IssueStatus? Status, int? AssigneeId, string? Tag);

public record CreateIssueRequest(
    string Title, string Description, string? StepsToReproduce,
    string? ExpectedBehavior, string? ActualBehavior, int? CategoryId,
    IssueSeverity Severity, IssuePriority Priority, IssueReproducibility Reproducibility,
    DateTime? DueDate, int? AffectsVersionId, int? FixVersionId);

public record UpdateIssueRequest(
    string Title, string Description, string? StepsToReproduce,
    string? ExpectedBehavior, string? ActualBehavior,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    IssueReproducibility Reproducibility, IssueResolution Resolution,
    int? AssigneeId, int? CategoryId, bool IsSticky, bool IsPrivate, DateTime? DueDate,
    int? AffectsVersionId, int? FixVersionId, Guid RowVersion);

public record AddIssueNoteRequest(string Text, bool IsPrivate = false);

public record CustomFieldValueDto(int DefinitionId, string Name, CustomFieldType Type, string? EnumOptions, bool Required, int DisplayOrder, string? Value);

public record SetCustomFieldValueRequest(string? Value);
