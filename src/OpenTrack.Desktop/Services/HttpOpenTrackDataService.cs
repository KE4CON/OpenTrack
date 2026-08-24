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

    public async Task<IReadOnlyList<WorkflowTransitionView>> GetWorkflowAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<WorkflowTransitionView>>($"/api/projects/{projectId}/workflow", JsonOptions, ct) ?? [];

    public async Task<string?> AddWorkflowTransitionAsync(int projectId, OpenTrack.Core.Enums.IssueStatus from, OpenTrack.Core.Enums.IssueStatus to, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/workflow", new { from, to }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing the workflow requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteWorkflowTransitionAsync(int projectId, int id, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/workflow/{id}", ct);
        ThrowIfForbidden(resp, "Managing the workflow requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
    }

    private const string AutomationManagerMsg = "Managing automation rules requires the Manager role on this project.";
    private sealed record SaveErrorDto(string? Error);

    public async Task<IReadOnlyList<AutomationRuleView>> GetAutomationRulesAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<AutomationRuleView>>($"/api/projects/{projectId}/automation-rules", JsonOptions, ct) ?? [];

    public async Task<string?> SaveAutomationRuleAsync(int projectId, AutomationRuleView rule, CancellationToken ct = default)
    {
        var resp = rule.Id == 0
            ? await http.PostAsJsonAsync($"/api/projects/{projectId}/automation-rules", rule, JsonOptions, ct)
            : await http.PutAsJsonAsync($"/api/automation-rules/{rule.Id}", rule, JsonOptions, ct);
        ThrowIfForbidden(resp, AutomationManagerMsg);
        resp.EnsureSuccessStatusCode();
        var d = await resp.Content.ReadFromJsonAsync<SaveErrorDto>(JsonOptions, ct);
        return d?.Error;
    }

    public async Task DeleteAutomationRuleAsync(int ruleId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/automation-rules/{ruleId}", ct);
        ThrowIfForbidden(resp, AutomationManagerMsg);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<SlaTargetView>> GetSlaPoliciesAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<SlaTargetView>>($"/api/projects/{projectId}/sla-policies", JsonOptions, ct) ?? [];

    public async Task<string?> SaveSlaTargetAsync(int projectId, OpenTrack.Core.Enums.IssuePriority priority, int? hours, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/sla-policies/{priority}", new { hours }, JsonOptions, ct);
        ThrowIfForbidden(resp, "Managing SLA targets requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        var d = await resp.Content.ReadFromJsonAsync<SaveErrorDto>(JsonOptions, ct);
        return d?.Error;
    }

    public async Task<SlaBoardView> GetSlaBoardAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<SlaBoardView>("/api/sla-board", JsonOptions, ct) ?? new SlaBoardView([], []);

    public async Task<SlaIssueView?> GetIssueSlaAsync(int issueId, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/issues/{issueId}/sla", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<SlaIssueView>(JsonOptions, ct);
    }

    public async Task<GitIntegrationView?> GetGitIntegrationAsync(int projectId, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/projects/{projectId}/git-integration", ct);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<GitIntegrationView>(JsonOptions, ct);
    }

    public async Task<string?> SaveGitIntegrationAsync(int projectId, bool enabled, string? webhookSecret, bool autoResolve, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/git-integration",
            new { enabled, webhookSecret, autoResolve }, JsonOptions, ct);
        ThrowIfForbidden(resp, "Managing Git integration requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        var d = await resp.Content.ReadFromJsonAsync<SaveErrorDto>(JsonOptions, ct);
        return d?.Error;
    }

    public async Task<IReadOnlyList<IssueCommitView>> GetIssueCommitsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<IssueCommitView>>($"/api/issues/{issueId}/commits", JsonOptions, ct) ?? [];

    public async Task SetPublicIntakeEnabledAsync(int projectId, bool enabled, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/public-intake", new { enabled }, JsonOptions, ct);
        ThrowIfForbidden(resp, "Changing public intake requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<RoadmapVersionView>> GetRoadmapAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<RoadmapVersionView>>($"/api/projects/{projectId}/roadmap", JsonOptions, ct) ?? [];

    public async Task<ReportView> GetReportAsync(int? projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<ReportView>("/api/reports" + (projectId is { } p ? $"?projectId={p}" : ""), JsonOptions, ct)
            ?? new ReportView(0, 0, 0, [], [], []);

    public async Task<IReadOnlyList<SimilarIssueView>> FindSimilarIssuesAsync(int? projectId, string title, int? excludeIssueId = null, CancellationToken ct = default)
    {
        var q = new List<string> { $"title={Uri.EscapeDataString(title ?? "")}" };
        if (projectId is { } p) q.Add($"projectId={p}");
        if (excludeIssueId is { } e) q.Add($"exclude={e}");
        return await http.GetFromJsonAsync<List<SimilarIssueView>>("/api/issues/similar?" + string.Join("&", q), JsonOptions, ct) ?? [];
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
        if (filter.StaleBeforeUtc is not null) q.Add("stale=true");
        q.Add($"sort={filter.Sort}");
        var url = "/api/issues?" + string.Join("&", q);
        return await http.GetFromJsonAsync<List<IssueRow>>(url, JsonOptions, ct) ?? [];
    }

    public async Task<bool> IsAiEnabledAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<bool>("/api/ai/enabled", JsonOptions, ct);

    private sealed record AiTriageDto(OpenTrack.Core.Enums.IssueSeverity? Severity, OpenTrack.Core.Enums.IssuePriority? Priority, string? Category, List<string>? Tags);
    public async Task<AiTriageView?> SuggestTriageAsync(int projectId, string title, string? description, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/ai/triage", new { projectId, title, description }, JsonOptions, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var d = await resp.Content.ReadFromJsonAsync<AiTriageDto>(JsonOptions, ct);
        return d is null ? null : new AiTriageView(d.Severity, d.Priority, d.Category, d.Tags ?? []);
    }

    private sealed record AiSearchDto(
        OpenTrack.Core.Enums.IssueStatus? Status, OpenTrack.Core.Enums.IssueSeverity? Severity,
        OpenTrack.Core.Enums.IssuePriority? Priority, string? Text, bool Stale,
        OpenTrack.Core.Querying.IssueSort? Sort, string? ProjectName);
    public async Task<AiSearchView?> InterpretIssueSearchAsync(string query, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/ai/search", new { query }, JsonOptions, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var d = await resp.Content.ReadFromJsonAsync<AiSearchDto>(JsonOptions, ct);
        return d is null ? null : new AiSearchView(d.Status, d.Severity, d.Priority, d.Text, d.Stale, d.Sort, d.ProjectName);
    }

    private sealed record AiSummaryDto(string? Summary);
    public async Task<string?> SummarizeIssueAsync(int issueId, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/ai/summarize", new { issueId }, JsonOptions, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var d = await resp.Content.ReadFromJsonAsync<AiSummaryDto>(JsonOptions, ct);
        return d?.Summary;
    }

    private sealed record AiResolutionDto(string? Summary, List<string>? Causes, List<string>? Steps, string? Confidence, List<string>? Sources);
    public async Task<AiResolutionView?> SuggestResolutionAsync(int issueId, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/ai/resolution", new { issueId }, JsonOptions, ct);
        if (!resp.IsSuccessStatusCode) return null;
        var d = await resp.Content.ReadFromJsonAsync<AiResolutionDto>(JsonOptions, ct);
        return d?.Summary is null ? null
            : new AiResolutionView(d.Summary, d.Causes ?? [], d.Steps ?? [], d.Confidence ?? "low", d.Sources ?? []);
    }

    public async Task<DashboardView> GetDashboardAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<DashboardView>("/api/dashboard", JsonOptions, ct)
            ?? new DashboardView(0, 0, 0, [], [], []);

    public async Task<IReadOnlyList<WebhookView>> GetWebhooksAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<WebhookView>>($"/api/projects/{projectId}/webhooks", JsonOptions, ct) ?? [];

    public async Task<string?> AddWebhookAsync(int projectId, string url, OpenTrack.Core.Enums.WebhookFormat format, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/webhooks", new { url, format }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing webhooks requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteWebhookAsync(int projectId, int id, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/webhooks/{id}", ct);
        ThrowIfForbidden(resp, "Managing webhooks requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<SavedFilterView>> GetSavedFiltersAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<SavedFilterView>>("/api/saved-filters", JsonOptions, ct) ?? [];

    public async Task<string?> SaveFilterAsync(string name, string query, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/saved-filters", new { name, query }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteSavedFilterAsync(int id, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/saved-filters/{id}", ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<PreferencesView> GetPreferencesAsync(CancellationToken ct = default) =>
        await http.GetFromJsonAsync<PreferencesView>("/api/preferences", JsonOptions, ct)
            ?? new PreferencesView(null, null);

    public async Task SavePreferencesAsync(int? defaultProjectId, OpenTrack.Core.Querying.IssueSort? defaultSort, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync("/api/preferences", new { defaultProjectId, defaultSort }, JsonOptions, ct);
        resp.EnsureSuccessStatusCode();
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
        // A workflow-disallowed status change comes back as 400 — surface it like the web host does.
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            throw new InvalidOperationException(await resp.Content.ReadAsStringAsync(ct));
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

    // ---- Custom fields (server enforces the same ACL as the shared operations) ----

    public async Task<IReadOnlyList<CustomFieldDefinitionView>> GetCustomFieldsAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<CustomFieldDefinitionView>>($"/api/projects/{projectId}/custom-fields", JsonOptions, ct) ?? [];

    public async Task<string?> CreateCustomFieldAsync(int projectId, string name, OpenTrack.Core.Enums.CustomFieldType type, string? enumOptions, bool required, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/custom-fields", new { name, type, enumOptions, required }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing custom fields requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<string?> UpdateCustomFieldAsync(int projectId, int fieldId, string name, string? enumOptions, bool required, int displayOrder, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/custom-fields/{fieldId}", new { name, enumOptions, required, displayOrder }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing custom fields requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteCustomFieldAsync(int projectId, int fieldId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/custom-fields/{fieldId}", ct);
        ThrowIfForbidden(resp, "Managing custom fields requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<CustomFieldValueView>> GetIssueCustomFieldsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<CustomFieldValueView>>($"/api/issues/{issueId}/custom-fields", JsonOptions, ct) ?? [];

    public async Task<string?> SetIssueCustomFieldAsync(int issueId, int fieldId, string? value, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/issues/{issueId}/custom-fields/{fieldId}", new { value }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Editing custom fields requires the Updater role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    // ---- Time logging (server enforces the same ACL as the shared operations) ----

    public async Task<IReadOnlyList<TimeLogView>> GetTimeLogsAsync(int issueId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<TimeLogView>>($"/api/issues/{issueId}/time", JsonOptions, ct) ?? [];

    public async Task<string?> AddTimeLogAsync(int issueId, int minutes, string? note, DateTime? workedOn, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/issues/{issueId}/time", new { minutes, note, workedOn }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Logging time requires the Updater role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteTimeLogAsync(int logId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/time/{logId}", ct);
        ThrowIfForbidden(resp, "You can only remove your own time entries.");
        resp.EnsureSuccessStatusCode();
    }

    // ---- Bug-hunt checklist (server enforces the same ACL as the shared operations) ----

    public async Task<IReadOnlyList<ChecklistItemView>> GetChecklistAsync(int projectId, CancellationToken ct = default) =>
        await http.GetFromJsonAsync<List<ChecklistItemView>>($"/api/projects/{projectId}/checklist", JsonOptions, ct) ?? [];

    public async Task<string?> AddChecklistItemAsync(int projectId, string title, string? details, string? area, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/checklist", new { title, details, area }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing the checklist requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    private sealed record ImportResult(int Added);
    public async Task<int> ImportChecklistAsync(int projectId, string text, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"/api/projects/{projectId}/checklist/import", new { text }, JsonOptions, ct);
        ThrowIfForbidden(resp, "Managing the checklist requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<ImportResult>(JsonOptions, ct);
        return result?.Added ?? 0;
    }

    public async Task<string?> UpdateChecklistItemAsync(int projectId, int itemId, string title, string? details, string? area, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/checklist/{itemId}", new { title, details, area }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Managing the checklist requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    public async Task DeleteChecklistItemAsync(int projectId, int itemId, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/projects/{projectId}/checklist/{itemId}", ct);
        ThrowIfForbidden(resp, "Managing the checklist requires the Manager role on this project.");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<string?> SetChecklistItemStatusAsync(int projectId, int itemId, OpenTrack.Core.Enums.ChecklistItemStatus status, string? notes, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/projects/{projectId}/checklist/{itemId}/status", new { status, notes }, JsonOptions, ct);
        if (resp.IsSuccessStatusCode) return null;
        if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest) return await resp.Content.ReadAsStringAsync(ct);
        ThrowIfForbidden(resp, "Working the checklist requires the Updater role on this project.");
        resp.EnsureSuccessStatusCode();
        return null;
    }

    private sealed record ConvertResult(int? IssueId);
    public async Task<int?> ConvertChecklistItemToIssueAsync(int projectId, int itemId, CancellationToken ct = default)
    {
        var resp = await http.PostAsync($"/api/projects/{projectId}/checklist/{itemId}/convert", content: null, ct);
        ThrowIfForbidden(resp, "Working the checklist requires the Updater role on this project.");
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<ConvertResult>(JsonOptions, ct);
        return result?.IssueId;
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
