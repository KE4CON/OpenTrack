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

using OpenTrack.Core.Querying;

namespace OpenTrack.UI.Services;

/// <summary>
/// The single data-access seam for OpenTrack's shared Blazor UI. The web app implements
/// this with direct EF Core access (DbOpenTrackDataService); the desktop app implements it
/// by calling OpenTrack.API over HTTP (HttpOpenTrackDataService). The CRUD pages depend
/// only on this interface, so the exact same components run in both hosts.
///
/// The current-user identity is NOT passed in per-call: each implementation resolves it
/// from its own context (the web app from the authenticated ClaimsPrincipal; the desktop
/// app from the signed-in API session). This keeps the pages identity-agnostic.
/// </summary>
public interface IOpenTrackDataService
{
    // Projects
    Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default);
    Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default);
    Task UpdateProjectAsync(int id, UpdateProjectInput input, CancellationToken ct = default);

    // Issues
    Task<IReadOnlyList<IssueRow>> GetIssuesAsync(IssueFilter filter, CancellationToken ct = default);
    Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default);
    Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default);
    Task UpdateIssueAsync(int id, UpdateIssueInput input, CancellationToken ct = default);
    Task AddIssueNoteAsync(int issueId, string text, bool isPrivate = false, CancellationToken ct = default);
    Task<IReadOnlyList<IssueHistoryEntry>> GetIssueHistoryAsync(int issueId, CancellationToken ct = default);

    // Attachment metadata + delete go through the seam (JSON). Upload/download are stream operations
    // handled by host-specific endpoints (web multipart form / desktop HttpClient), not here.
    Task<IReadOnlyList<AttachmentView>> GetIssueAttachmentsAsync(int issueId, CancellationToken ct = default);
    Task DeleteAttachmentAsync(int attachmentId, CancellationToken ct = default);

    // Lookups used by the issue create/edit forms
    Task<IReadOnlyList<CategoryView>> GetProjectCategoriesAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectMemberView>> GetProjectMembersAsync(int projectId, CancellationToken ct = default);

    // Member management (Manager+ on the project). Returns include the per-project role.
    Task<IReadOnlyList<ProjectMemberDetail>> GetProjectMemberDetailsAsync(int projectId, CancellationToken ct = default);
    /// <summary>Adds an existing user (looked up by email) to the project. Returns null on success,
    /// or a human-readable reason on failure (unknown email, already a member, etc.).</summary>
    Task<string?> AddProjectMemberAsync(int projectId, AddProjectMemberInput input, CancellationToken ct = default);
    Task SetProjectMemberRoleAsync(int projectId, int userId, OpenTrack.Core.Enums.UserRole role, CancellationToken ct = default);
    Task RemoveProjectMemberAsync(int projectId, int userId, CancellationToken ct = default);

    // Category management (Manager+ on the project). Returns null on success or a reason on failure.
    Task<string?> CreateCategoryAsync(int projectId, string name, CancellationToken ct = default);
    Task DeleteCategoryAsync(int projectId, int categoryId, CancellationToken ct = default);

    // Version management (Manager+ on the project).
    Task<IReadOnlyList<ProjectVersionView>> GetProjectVersionsAsync(int projectId, CancellationToken ct = default);
    Task<string?> CreateVersionAsync(int projectId, CreateVersionInput input, CancellationToken ct = default);
    Task DeleteVersionAsync(int projectId, int versionId, CancellationToken ct = default);
}
