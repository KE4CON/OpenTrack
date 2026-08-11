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

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTrack.UI.Services;

namespace OpenTrack.Desktop.Services;

/// <summary>
/// HTTP-backed implementation of <see cref="IOpenTrackDataService"/> for the desktop app.
/// Calls OpenTrack.API over HTTP (thin client). The API's DTOs are shaped to match the
/// UI's view models, so most calls are near-passthrough deserialization. The authenticated
/// bearer token is attached by <see cref="AuthTokenHandler"/> on the underlying HttpClient,
/// so this class stays identity-agnostic just like the DB-backed version.
/// </summary>
public class HttpOpenTrackDataService(HttpClient http) : IOpenTrackDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ProjectRow>>("/api/projects", JsonOptions, ct) ?? [];

    public async Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/projects/{id}", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ProjectDetail>(JsonOptions, ct);
    }

    public async Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/projects", input, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ProjectDetail>(JsonOptions, ct);
        return created?.Id ?? 0;
    }

    public async Task UpdateProjectAsync(int id, UpdateProjectInput input, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{id}", input, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IssueRow>> GetIssuesAsync(int? projectId = null, CancellationToken ct = default)
    {
        var url = projectId is null ? "/api/issues" : $"/api/issues?projectId={projectId}";
        return await http.GetFromJsonAsync<List<IssueRow>>(url, JsonOptions, ct) ?? [];
    }

    public async Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/issues/{id}", ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<IssueDetail>(JsonOptions, ct);
    }

    public async Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/issues", input, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<int>(JsonOptions, ct);
    }

    public async Task UpdateIssueAsync(int id, UpdateIssueInput input, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/issues/{id}", input, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task AddIssueNoteAsync(int issueId, string text, bool isPrivate = false, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/issues/{issueId}/notes", new { text, isPrivate }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IssueHistoryEntry>> GetIssueHistoryAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<IssueHistoryEntry>>($"/api/issues/{issueId}/history", JsonOptions, ct) ?? [];

    public async Task<IReadOnlyList<CategoryView>> GetProjectCategoriesAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<CategoryView>>($"/api/projects/{projectId}/categories", JsonOptions, ct) ?? [];

    public async Task<IReadOnlyList<ProjectMemberView>> GetProjectMembersAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ProjectMemberView>>($"/api/projects/{projectId}/members", JsonOptions, ct) ?? [];

    public async Task<IReadOnlyList<ProjectMemberDetail>> GetProjectMemberDetailsAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ProjectMemberDetail>>($"/api/projects/{projectId}/member-details", JsonOptions, ct) ?? [];

    public async Task<string?> AddProjectMemberAsync(int projectId, AddProjectMemberInput input, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/members", input, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            return await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task SetProjectMemberRoleAsync(int projectId, int userId, OpenTrack.Core.Enums.UserRole role, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/members/{userId}", new SetMemberRoleInput(role), JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task RemoveProjectMemberAsync(int projectId, int userId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/members/{userId}", ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<string?> CreateCategoryAsync(int projectId, string name, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/categories", new { name }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteCategoryAsync(int projectId, int categoryId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/categories/{categoryId}", ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<ProjectVersionView>> GetProjectVersionsAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ProjectVersionView>>($"/api/projects/{projectId}/versions", JsonOptions, ct) ?? [];

    public async Task<string?> CreateVersionAsync(int projectId, CreateVersionInput input, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/versions", input, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteVersionAsync(int projectId, int versionId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/versions/{versionId}", ct);
        resp.EnsureSuccessStatusCode();
    }
}
