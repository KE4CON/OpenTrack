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
    Task<IReadOnlyList<IssueRow>> GetIssuesAsync(int? projectId = null, CancellationToken ct = default);
    Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default);
    Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default);
    Task UpdateIssueAsync(int id, UpdateIssueInput input, CancellationToken ct = default);
    Task AddIssueNoteAsync(int issueId, string text, CancellationToken ct = default);

    // Lookups used by the issue create/edit forms
    Task<IReadOnlyList<CategoryView>> GetProjectCategoriesAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectMemberView>> GetProjectMembersAsync(int projectId, CancellationToken ct = default);
}
