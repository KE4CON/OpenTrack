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

using OpenTrack.Core.Authorization;
using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Tests;

/// <summary>
/// The per-project access-control matrix. These are the rules the Web API and the web/EF data service
/// BOTH call, so getting them right here means both surfaces enforce the same thing. Regression tests
/// for audit findings C1 (private issues), C2 (project IDOR), and H3 (privileged-field escalation).
/// </summary>
public class AccessControlTests
{
    private const int Me = 1;
    private const int Someone = 2;

    private static AccessContext Ctx(UserRole global, UserRole? project = null) => new(Me, global, project);

    // ---- EffectiveRole ----

    [Theory]
    [InlineData(UserRole.Reporter, null, UserRole.Reporter)]          // no membership -> global
    [InlineData(UserRole.Reporter, UserRole.Manager, UserRole.Manager)] // project role higher
    [InlineData(UserRole.Developer, UserRole.Reporter, UserRole.Developer)] // global higher
    [InlineData(UserRole.Administrator, UserRole.Reporter, UserRole.Administrator)]
    public void EffectiveRole_IsMaxOfGlobalAndProject(UserRole global, UserRole? project, UserRole expected)
        => Assert.Equal(expected, Ctx(global, project).EffectiveRole);

    // ---- Project visibility (C2) ----

    [Fact]
    public void PublicProject_IsViewable_ByAnyAuthenticatedUser()
        => Assert.True(Ctx(UserRole.Viewer).CanViewProject(projectIsPublic: true));

    [Fact]
    public void PrivateProject_IsHidden_FromNonMembers()
        => Assert.False(Ctx(UserRole.Updater).CanViewProject(projectIsPublic: false));

    [Fact]
    public void PrivateProject_IsViewable_ByMembers()
        => Assert.True(Ctx(UserRole.Reporter, UserRole.Reporter).CanViewProject(projectIsPublic: false));

    [Fact]
    public void PrivateProject_IsViewable_ByGlobalAdmin_WithoutMembership()
        => Assert.True(Ctx(UserRole.Administrator).CanViewProject(projectIsPublic: false));

    // ---- Issue visibility (C1) ----

    [Fact]
    public void PrivateIssue_IsHidden_FromOrdinaryProjectMembers()
    {
        // A Reporter member of a public project cannot see a private issue they neither reported nor own.
        var ctx = Ctx(UserRole.Reporter, UserRole.Reporter);
        Assert.False(ctx.CanViewIssue(projectIsPublic: true, issueIsPrivate: true, reporterId: Someone, assigneeId: Someone));
    }

    [Fact]
    public void PrivateIssue_IsVisible_ToItsReporter()
        => Assert.True(Ctx(UserRole.Reporter).CanViewIssue(true, true, reporterId: Me, assigneeId: Someone));

    [Fact]
    public void PrivateIssue_IsVisible_ToItsAssignee()
        => Assert.True(Ctx(UserRole.Reporter).CanViewIssue(true, true, reporterId: Someone, assigneeId: Me));

    [Fact]
    public void PrivateIssue_IsVisible_ToDevelopers()
        => Assert.True(Ctx(UserRole.Developer).CanViewIssue(true, true, reporterId: Someone, assigneeId: Someone));

    [Fact]
    public void PrivateIssue_IsVisible_ToProjectDeveloper_EvenWhenGlobalReporter()
        => Assert.True(Ctx(UserRole.Reporter, UserRole.Developer).CanViewIssue(true, true, Someone, Someone));

    [Fact]
    public void PrivateIssue_InPrivateProject_IsHidden_FromNonMember_EvenIfGlobalDeveloper()
    {
        // Developer privilege lets you see private issues, but only in projects you can view at all.
        var ctx = Ctx(UserRole.Developer); // no membership on this private project
        Assert.False(ctx.CanViewIssue(projectIsPublic: false, issueIsPrivate: false, reporterId: Someone, assigneeId: Someone));
    }

    [Fact]
    public void PublicIssue_IsVisible_ToAnyViewerOfTheProject()
        => Assert.True(Ctx(UserRole.Viewer).CanViewIssue(true, false, Someone, Someone));

    // ---- Write gates (H3) ----

    [Theory]
    [InlineData(UserRole.Reporter, false)]
    [InlineData(UserRole.Updater, true)]
    [InlineData(UserRole.Developer, true)]
    public void EditIssue_RequiresUpdater(UserRole effective, bool allowed)
        => Assert.Equal(allowed, Ctx(effective).CanEditIssue());

    [Theory]
    [InlineData(UserRole.Updater, false)]   // an Updater cannot assign or toggle privacy...
    [InlineData(UserRole.Developer, true)]  // ...a Developer can
    public void AssignAndPrivacy_RequireDeveloper(UserRole effective, bool allowed)
    {
        Assert.Equal(allowed, Ctx(effective).CanAssignIssue());
        Assert.Equal(allowed, Ctx(effective).CanSetIssuePrivacy());
    }

    [Theory]
    [InlineData(UserRole.Developer, false)] // pinning is a project-management action...
    [InlineData(UserRole.Manager, true)]    // ...Manager+ only
    public void Sticky_RequiresManager(UserRole effective, bool allowed)
        => Assert.Equal(allowed, Ctx(effective).CanSetIssueSticky());

    [Theory]
    [InlineData(UserRole.Viewer, false)]
    [InlineData(UserRole.Reporter, true)]
    public void CreateIssueAndAddNote_RequireReporter(UserRole effective, bool allowed)
    {
        Assert.Equal(allowed, Ctx(effective).CanCreateIssue(projectIsPublic: true));
        Assert.Equal(allowed, Ctx(effective).CanAddNote());
    }

    [Theory]
    [InlineData(UserRole.Developer, false)]
    [InlineData(UserRole.Manager, true)]
    public void EditProject_RequiresManager(UserRole effective, bool allowed)
        => Assert.Equal(allowed, Ctx(effective).CanEditProject());

    // ---- Private notes ----

    [Fact]
    public void PrivateNote_IsHidden_FromOrdinaryMembers_ButVisibleToAuthorAndDevelopers()
    {
        Assert.False(Ctx(UserRole.Reporter).CanViewNote(notePrivate: true, authorId: Someone));
        Assert.True(Ctx(UserRole.Reporter).CanViewNote(notePrivate: true, authorId: Me));         // author
        Assert.True(Ctx(UserRole.Developer).CanViewNote(notePrivate: true, authorId: Someone));   // developer
        Assert.True(Ctx(UserRole.Reporter).CanViewNote(notePrivate: false, authorId: Someone));   // public note
    }
}
