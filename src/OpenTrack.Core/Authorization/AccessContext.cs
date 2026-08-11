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

namespace OpenTrack.Core.Authorization;

/// <summary>
/// The single source of truth for every per-project access decision in OpenTrack.
/// It is deliberately pure (no EF, no HTTP) so both the web/EF data service and the
/// Web API endpoints call the exact same rules — they cannot drift — and so the whole
/// matrix is unit-testable.
///
/// A user's authority on a given project is the <em>effective role</em>: the higher of
/// their global <see cref="User.Role"/> and their per-project <see cref="ProjectMembership.Role"/>
/// (Mantis-style — a global Developer is a Developer everywhere; a project Manager is a
/// Manager on that project even if their global role is only Reporter).
/// </summary>
public readonly record struct AccessContext(int UserId, UserRole GlobalRole, UserRole? ProjectRole)
{
    /// <summary>The higher of the global role and the (optional) per-project role.</summary>
    public UserRole EffectiveRole =>
        ProjectRole is { } pr && (int)pr > (int)GlobalRole ? pr : GlobalRole;

    /// <summary>Global administrators bypass per-project scoping entirely.</summary>
    public bool IsGlobalAdmin => (int)GlobalRole >= (int)UserRole.Administrator;

    /// <summary>True when the user is an explicit member of the project in question.</summary>
    public bool HasProjectMembership => ProjectRole is not null;

    private bool AtLeast(UserRole role) => (int)EffectiveRole >= (int)role;

    // ---- Project ----

    /// <summary>A project is viewable if it is public, the user is a member, or the user is a global admin.</summary>
    public bool CanViewProject(bool projectIsPublic) =>
        IsGlobalAdmin || projectIsPublic || HasProjectMembership;

    /// <summary>Editing project settings (name/description/visibility) requires Manager on the project.</summary>
    public bool CanEditProject() => AtLeast(UserRole.Manager);

    /// <summary>Managing categories, versions, and members requires Manager on the project.</summary>
    public bool CanManageProject() => AtLeast(UserRole.Manager);

    // ---- Issue ----

    /// <summary>
    /// A private issue is visible only to its reporter, its assignee, and users with Developer+
    /// authority on the project (in addition to the project itself being viewable).
    /// </summary>
    public bool CanViewIssue(bool projectIsPublic, bool issueIsPrivate, int reporterId, int? assigneeId) =>
        CanViewProject(projectIsPublic)
        && (!issueIsPrivate
            || reporterId == UserId
            || assigneeId == UserId
            || AtLeast(UserRole.Developer));

    /// <summary>Reporting a new issue requires Reporter+ on a viewable project.</summary>
    public bool CanCreateIssue(bool projectIsPublic) =>
        CanViewProject(projectIsPublic) && AtLeast(UserRole.Reporter);

    /// <summary>Editing an existing issue's core fields requires Updater+.</summary>
    public bool CanEditIssue() => AtLeast(UserRole.Updater);

    /// <summary>Assigning a handler requires Developer+.</summary>
    public bool CanAssignIssue() => AtLeast(UserRole.Developer);

    /// <summary>Flipping an issue's private flag requires Developer+ (matches who can see private issues).</summary>
    public bool CanSetIssuePrivacy() => AtLeast(UserRole.Developer);

    /// <summary>Pinning/unpinning (sticky) is a project-management action — Manager+.</summary>
    public bool CanSetIssueSticky() => AtLeast(UserRole.Manager);

    // ---- Notes ----

    /// <summary>Adding a note requires Reporter+ on a viewable issue.</summary>
    public bool CanAddNote() => AtLeast(UserRole.Reporter);

    /// <summary>Marking a note private requires Developer+.</summary>
    public bool CanAddPrivateNote() => AtLeast(UserRole.Developer);

    /// <summary>A private note is visible only to its author and to Developer+ users.</summary>
    public bool CanViewNote(bool notePrivate, int authorId) =>
        !notePrivate || authorId == UserId || AtLeast(UserRole.Developer);
}
