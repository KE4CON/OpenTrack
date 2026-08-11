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
using OpenTrack.Core.Querying;
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
        await ThrowIfConflictAsync(resp, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IssueRow>> GetIssuesAsync(IssueFilter filter, CancellationToken ct = default)
    {
        var q = new List<string>();
        if (filter.ProjectId is { } p) q.Add($"projectId={p}");
        if (filter.Status is { } s) q.Add($"status={s}");
        if (filter.Severity is { } sv) q.Add($"severity={sv}");
        if (filter.Priority is { } pr) q.Add($"priority={pr}");
        if (filter.AssigneeId is { } a) q.Add($"assigneeId={a}");
        if (filter.CategoryId is { } c) q.Add($"categoryId={c}");
        if (!string.IsNullOrWhiteSpace(filter.Text)) q.Add($"text={Uri.EscapeDataString(filter.Text)}");
        if (filter.TagId is { } tg) q.Add($"tagId={tg}");
        q.Add($"sort={filter.Sort}");
        var url = "/api/issues?" + string.Join("&", q);
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
        await ThrowIfConflictAsync(resp, ct);
        resp.EnsureSuccessStatusCode();
    }

    private static async Task ThrowIfConflictAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var msg = await resp.Content.ReadAsStringAsync(ct);
            throw new OpenTrack.Core.ConcurrencyConflictException(
                string.IsNullOrWhiteSpace(msg) ? OpenTrack.Core.ConcurrencyConflictException.DefaultMessage : msg);
        }
    }

    public async Task AddIssueNoteAsync(int issueId, string text, bool isPrivate = false, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/issues/{issueId}/notes", new { text, isPrivate }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<IssueHistoryEntry>> GetIssueHistoryAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<IssueHistoryEntry>>($"/api/issues/{issueId}/history", JsonOptions, ct) ?? [];

    public async Task<IReadOnlyList<IssueRelationshipView>> GetIssueRelationshipsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<IssueRelationshipView>>($"/api/issues/{issueId}/relationships", JsonOptions, ct) ?? [];

    public async Task<string?> AddIssueRelationshipAsync(int sourceIssueId, int targetIssueId, OpenTrack.Core.Enums.IssueRelationshipType type, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/issues/{sourceIssueId}/relationships", new { targetIssueId, type }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Adding a relationship requires the Updater role on this issue's project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task RemoveIssueRelationshipAsync(int relationshipId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/relationships/{relationshipId}", ct);
        ThrowIfForbidden(resp, "Removing a relationship requires the Updater role on one of the linked issues.");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<TagView>> GetAllTagsAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<TagView>>("/api/tags", JsonOptions, ct) ?? [];

    public async Task<IReadOnlyList<TagView>> GetIssueTagsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<TagView>>($"/api/issues/{issueId}/tags", JsonOptions, ct) ?? [];

    public async Task<string?> AddIssueTagAsync(int issueId, string tagName, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/issues/{issueId}/tags", new { name = tagName }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Tagging an issue requires the Updater role on its project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task RemoveIssueTagAsync(int issueId, int tagId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/issues/{issueId}/tags/{tagId}", ct);
        ThrowIfForbidden(resp, "Untagging an issue requires the Updater role on its project.");
        resp.EnsureSuccessStatusCode();
    }

    private sealed record MonitorState(bool Monitoring);

    public async Task<bool> IsMonitoringIssueAsync(int issueId, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/issues/{issueId}/monitor", ct);
        if (!resp.IsSuccessStatusCode) return false;
        var state = await resp.Content.ReadFromJsonAsync<MonitorState>(JsonOptions, ct);
        return state?.Monitoring ?? false;
    }

    public async Task SetIssueMonitorAsync(int issueId, bool monitor, CancellationToken ct = default)
    {
        var resp = monitor
            ? await http.PostAsync($"/api/issues/{issueId}/monitor", content: null, ct)
            : await http.DeleteAsync($"/api/issues/{issueId}/monitor", ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<int> GetUnreadNotificationCountAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<int>("/api/notifications/unread-count", JsonOptions, ct);

    public async Task<IReadOnlyList<NotificationView>> GetNotificationsAsync(bool unreadOnly = false, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<NotificationView>>($"/api/notifications?unreadOnly={unreadOnly}", JsonOptions, ct) ?? [];

    public async Task MarkNotificationReadAsync(int notificationId, CancellationToken ct = default)
    {
        var resp = await http.PostAsync($"/api/notifications/{notificationId}/read", content: null, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task MarkAllNotificationsReadAsync(CancellationToken ct = default)
    {
        var resp = await http.PostAsync("/api/notifications/read-all", content: null, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<OpenTrack.Core.Bulk.BulkResult> BulkUpdateIssuesAsync(IReadOnlyCollection<int> issueIds, OpenTrack.Core.Bulk.BulkAction action, CancellationToken ct = default)
    {
        var body = new { issueIds = issueIds.ToArray(), type = action.Type, status = action.Status, assigneeId = action.AssigneeId, tag = action.Tag };
        var resp = await http.PostAsJsonAsync("/api/issues/bulk", body, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<OpenTrack.Core.Bulk.BulkResult>(JsonOptions, ct) ?? new OpenTrack.Core.Bulk.BulkResult(0, 0);
    }

    // Translate a 403 into the same exception the web/EF path throws, so the shared razor pages
    // handle a denied action identically on both hosts.
    private static void ThrowIfForbidden(HttpResponseMessage resp, string message)
    {
        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new UnauthorizedAccessException(message);
    }

    public async Task<IReadOnlyList<AttachmentView>> GetIssueAttachmentsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<AttachmentView>>($"/api/issues/{issueId}/attachments", JsonOptions, ct) ?? [];

    public async Task DeleteAttachmentAsync(int attachmentId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/attachments/{attachmentId}", ct);
        resp.EnsureSuccessStatusCode();
    }

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
