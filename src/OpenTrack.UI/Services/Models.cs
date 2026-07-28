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

public record ProjectDetail(int Id, string Name, string? Description, bool IsPublic, int OwnerId, DateTime CreatedAt);

public record CreateProjectInput(string Name, string? Description, bool IsPublic);

public record UpdateProjectInput(string Name, string? Description, bool IsPublic);

public record IssueRow(
    int Id, int ProjectId, string ProjectName, string Title,
    IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    string ReporterName, string? AssigneeName, DateTime UpdatedAt);

public record IssueNoteView(int Id, string AuthorName, string Text, DateTime CreatedAt);

public record IssueDetail(
    int Id, int ProjectId, string ProjectName, string Title, string Description,
    string? StepsToReproduce, IssueStatus Status, IssueSeverity Severity, IssuePriority Priority,
    IssueReproducibility Reproducibility, IssueResolution Resolution,
    int ReporterId, string ReporterName, int? AssigneeId, string? AssigneeName,
    int? CategoryId, string? CategoryName, bool IsSticky, bool IsPrivate,
    DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<IssueNoteView> Notes);

public record CreateIssueInput(
    string Title, string Description, string? StepsToReproduce, int? CategoryId,
    IssueSeverity Severity, IssuePriority Priority, IssueReproducibility Reproducibility);

public record UpdateIssueInput(
    string Title, string Description, IssueStatus Status, IssueSeverity Severity,
    IssuePriority Priority, IssueResolution Resolution, int? AssigneeId, int? CategoryId,
    bool IsSticky, bool IsPrivate);

public record CategoryView(int Id, string Name);

public record ProjectMemberView(int Id, string UserName);
