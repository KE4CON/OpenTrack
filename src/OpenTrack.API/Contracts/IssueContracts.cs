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
    string? StepsToReproduce, IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    IssueReproducibility Reproducibility, IssueResolution Resolution,
    int ReporterId, string ReporterName, int? AssigneeId, string? AssigneeName,
    int? CategoryId, string? CategoryName, bool IsSticky, bool IsPrivate,
    DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<IssueNoteDto> Notes);

public record IssueNoteDto(int Id, string AuthorName, string Text, DateTime CreatedAt);

public record CreateIssueRequest(
    string Title, string Description, string? StepsToReproduce, int? CategoryId,
    IssueSeverity Severity, IssuePriority Priority, IssueReproducibility Reproducibility);

public record UpdateIssueRequest(
    string Title, string Description, IssueStatus Status, IssueSeverity Severity,
    IssuePriority Priority, IssueResolution Resolution, int? AssigneeId, int? CategoryId,
    bool IsSticky, bool IsPrivate);

public record AddIssueNoteRequest(string Text);
