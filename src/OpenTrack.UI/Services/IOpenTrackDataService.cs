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

using OpenTrack.Core.Bulk;
using OpenTrack.Core.Enums;
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
    // Turn the public "Report a problem" intake on/off for a project (Manager only).
    Task SetPublicIntakeEnabledAsync(int projectId, bool enabled, CancellationToken ct = default);

    // Dashboard: a cross-project overview (open/overdue tallies, severity breakdown, recent activity),
    // filtered to what the signed-in user may see.
    Task<DashboardView> GetDashboardAsync(CancellationToken ct = default);

    // Optional AI assist (opt-in). IsAiEnabled reports whether it's configured; SuggestTriage returns a
    // suggested severity/priority/category/tags for a proposed issue, or null if AI is off or unavailable.
    Task<bool> IsAiEnabledAsync(CancellationToken ct = default);
    Task<AiTriageView?> SuggestTriageAsync(int projectId, string title, string? description, CancellationToken ct = default);
    // Turn a plain-English request into structured issue-list filter fields (project match limited to the
    // caller's visible projects). Null if AI is off/unavailable — the caller then leaves the search as-is.
    Task<AiSearchView?> InterpretIssueSearchAsync(string query, CancellationToken ct = default);
    // Plain-language summary of an issue thread (only notes the caller may see). Null if AI is off/unavailable.
    Task<string?> SummarizeIssueAsync(int issueId, CancellationToken ct = default);

    // Saved filters (the signed-in user's own). Saving with an existing name overwrites its query.
    Task<IReadOnlyList<SavedFilterView>> GetSavedFiltersAsync(CancellationToken ct = default);
    Task<string?> SaveFilterAsync(string name, string query, CancellationToken ct = default);
    Task DeleteSavedFilterAsync(int id, CancellationToken ct = default);

    // Per-user preferences (the signed-in user's own).
    Task<PreferencesView> GetPreferencesAsync(CancellationToken ct = default);
    Task SavePreferencesAsync(int? defaultProjectId, IssueSort? defaultSort, CancellationToken ct = default);

    // Per-project workflow: the allowed status transitions (Manager only). Empty = all transitions
    // allowed (the default). Add returns null on success or a reason.
    Task<IReadOnlyList<WorkflowTransitionView>> GetWorkflowAsync(int projectId, CancellationToken ct = default);
    Task<string?> AddWorkflowTransitionAsync(int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default);
    Task DeleteWorkflowTransitionAsync(int projectId, int id, CancellationToken ct = default);

    // Per-project automation rules (Manager only): "when a new issue matches → do". Save creates when the
    // rule's Id is 0, else updates; returns null on success or a validation reason.
    Task<IReadOnlyList<AutomationRuleView>> GetAutomationRulesAsync(int projectId, CancellationToken ct = default);
    Task<string?> SaveAutomationRuleAsync(int projectId, AutomationRuleView rule, CancellationToken ct = default);
    Task DeleteAutomationRuleAsync(int ruleId, CancellationToken ct = default);

    // Per-project SLA targets (Manager only): resolve-within-hours per priority. Saving hours null/≤0
    // clears that priority. The board lists at-risk/breached OPEN issues the caller can see; the single
    // issue lookup drives the detail-page badge.
    Task<IReadOnlyList<SlaTargetView>> GetSlaPoliciesAsync(int projectId, CancellationToken ct = default);
    Task<string?> SaveSlaTargetAsync(int projectId, IssuePriority priority, int? hours, CancellationToken ct = default);
    Task<SlaBoardView> GetSlaBoardAsync(CancellationToken ct = default);
    Task<SlaIssueView?> GetIssueSlaAsync(int issueId, CancellationToken ct = default);

    // Project webhooks (Manager only — a URL can carry a secret token). Returns null/reason on add.
    Task<IReadOnlyList<WebhookView>> GetWebhooksAsync(int projectId, CancellationToken ct = default);
    Task<string?> AddWebhookAsync(int projectId, string url, WebhookFormat format, CancellationToken ct = default);
    Task DeleteWebhookAsync(int projectId, int id, CancellationToken ct = default);

    // Roadmap & changelog for a project (versions with their fix-targeted issues), filtered to what
    // the user may see.
    Task<IReadOnlyList<RoadmapVersionView>> GetRoadmapAsync(int projectId, CancellationToken ct = default);

    // Reporting figures (headline counts, created-per-month, open by status/severity), optionally scoped
    // to one project; always filtered to what the user may see.
    Task<ReportView> GetReportAsync(int? projectId, CancellationToken ct = default);

    // Possible-duplicate suggestions for a proposed title (ACL-filtered). Optionally scoped to a project
    // and excluding a specific issue (when checking from an existing one).
    Task<IReadOnlyList<SimilarIssueView>> FindSimilarIssuesAsync(int? projectId, string title, int? excludeIssueId = null, CancellationToken ct = default);

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

    // Relationships. Adding requires edit on the source issue + view on the target; the returned list
    // is filtered to related issues the viewer may see. Returns null on success or a reason on failure.
    Task<IReadOnlyList<IssueRelationshipView>> GetIssueRelationshipsAsync(int issueId, CancellationToken ct = default);
    Task<string?> AddIssueRelationshipAsync(int sourceIssueId, int targetIssueId, IssueRelationshipType type, CancellationToken ct = default);
    Task RemoveIssueRelationshipAsync(int relationshipId, CancellationToken ct = default);

    // Tags. All tag names (for the filter/autocomplete) are visible to any signed-in user; an issue's
    // tags require view access to that issue, and adding/removing a tag requires edit access.
    Task<IReadOnlyList<TagView>> GetAllTagsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TagView>> GetIssueTagsAsync(int issueId, CancellationToken ct = default);
    Task<string?> AddIssueTagAsync(int issueId, string tagName, CancellationToken ct = default);
    Task RemoveIssueTagAsync(int issueId, int tagId, CancellationToken ct = default);

    // Monitoring: a user can monitor (subscribe to) any issue they can view; monitors are notified of
    // changes alongside the reporter and assignee.
    Task<bool> IsMonitoringIssueAsync(int issueId, CancellationToken ct = default);
    Task SetIssueMonitorAsync(int issueId, bool monitor, CancellationToken ct = default);

    // Notifications (the signed-in user's own).
    Task<int> GetUnreadNotificationCountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NotificationView>> GetNotificationsAsync(bool unreadOnly = false, CancellationToken ct = default);
    Task MarkNotificationReadAsync(int notificationId, CancellationToken ct = default);
    Task MarkAllNotificationsReadAsync(CancellationToken ct = default);

    // Bulk actions: applied only to the issues the caller may act on; others are skipped (the result
    // reports how many were updated vs skipped).
    Task<BulkResult> BulkUpdateIssuesAsync(IReadOnlyCollection<int> issueIds, BulkAction action, CancellationToken ct = default);

    // Custom field definitions (per project). Reading requires view access to the project; creating,
    // editing, and deleting a definition require the Manager role on that project. The mutating calls
    // return null on success or a validation/authorization reason on failure.
    Task<IReadOnlyList<CustomFieldDefinitionView>> GetCustomFieldsAsync(int projectId, CancellationToken ct = default);
    Task<string?> CreateCustomFieldAsync(int projectId, string name, CustomFieldType type, string? enumOptions, bool required, CancellationToken ct = default);
    Task<string?> UpdateCustomFieldAsync(int projectId, int fieldId, string name, string? enumOptions, bool required, int displayOrder, CancellationToken ct = default);
    Task DeleteCustomFieldAsync(int projectId, int fieldId, CancellationToken ct = default);

    // Time logging on an issue. Viewing follows the issue's view ACL; logging needs edit access;
    // deleting an entry is allowed to its author or an Updater+. Add returns null or a reason.
    Task<IReadOnlyList<TimeLogView>> GetTimeLogsAsync(int issueId, CancellationToken ct = default);
    Task<string?> AddTimeLogAsync(int issueId, int minutes, string? note, DateTime? workedOn, CancellationToken ct = default);
    Task DeleteTimeLogAsync(int logId, CancellationToken ct = default);

    // Custom field values on an issue. Reading follows the issue's view ACL; setting a value (blank
    // clears it) requires edit access. Returns null on success or a reason on failure.
    Task<IReadOnlyList<CustomFieldValueView>> GetIssueCustomFieldsAsync(int issueId, CancellationToken ct = default);
    Task<string?> SetIssueCustomFieldAsync(int issueId, int fieldId, string? value, CancellationToken ct = default);

    // Bug-hunt checklist (per project). Reading needs project view; defining items (add/import/edit/
    // delete) needs Manager; working through them (status/notes, convert-to-issue) needs Updater.
    // Mutations return null on success or a reason on failure; import returns the count added; convert
    // returns the new (or already-linked) issue id, or null if it couldn't.
    Task<IReadOnlyList<ChecklistItemView>> GetChecklistAsync(int projectId, CancellationToken ct = default);
    Task<string?> AddChecklistItemAsync(int projectId, string title, string? details, string? area, CancellationToken ct = default);
    Task<int> ImportChecklistAsync(int projectId, string text, CancellationToken ct = default);
    Task<string?> UpdateChecklistItemAsync(int projectId, int itemId, string title, string? details, string? area, CancellationToken ct = default);
    Task DeleteChecklistItemAsync(int projectId, int itemId, CancellationToken ct = default);
    Task<string?> SetChecklistItemStatusAsync(int projectId, int itemId, ChecklistItemStatus status, string? notes, CancellationToken ct = default);
    Task<int?> ConvertChecklistItemToIssueAsync(int projectId, int itemId, CancellationToken ct = default);
}
