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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Tests;

/// <summary>
/// Proves the shared row-level visibility filter (used by BOTH the Web API and the web/EF data
/// service) actually excludes projects/issues a user may not see, against a real relational (SQLite)
/// database. Regression tests for C1 (private issues) and C2 (project IDOR).
/// </summary>
public sealed class VisibilityQueryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    // Ids used by the seed.
    private const int PublicProject = 1;
    private const int PrivateProject = 2;
    private const int U_Owner1 = 1, U_Owner2 = 2, U_ReporterMember = 3, U_Outsider = 99;
    private const int I_PublicInPublic = 10;
    private const int I_PrivateInPublic = 11;
    private const int I_PublicInPrivate = 12;
    private const int I_PrivateInPrivate = 13;

    public VisibilityQueryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();

        db.Users.AddRange(
            new User { Id = U_Owner1, UserName = "owner1" },
            new User { Id = U_Owner2, UserName = "owner2" },
            new User { Id = U_ReporterMember, UserName = "reporter3" },
            new User { Id = U_Outsider, UserName = "outsider" });

        db.Projects.AddRange(
            new Project { Id = PublicProject, Name = "Public", IsPublic = true, OwnerId = U_Owner1 },
            new Project { Id = PrivateProject, Name = "Private", IsPublic = false, OwnerId = U_Owner2 });

        db.ProjectMemberships.AddRange(
            new ProjectMembership { ProjectId = PublicProject, UserId = U_Owner1, Role = UserRole.Manager },
            new ProjectMembership { ProjectId = PrivateProject, UserId = U_Owner2, Role = UserRole.Manager },
            new ProjectMembership { ProjectId = PrivateProject, UserId = U_ReporterMember, Role = UserRole.Reporter });

        db.Issues.AddRange(
            new Issue { Id = I_PublicInPublic, ProjectId = PublicProject, Title = "pub/pub", Description = "d", ReporterId = U_Owner1, IsPrivate = false },
            new Issue { Id = I_PrivateInPublic, ProjectId = PublicProject, Title = "priv/pub", Description = "d", ReporterId = U_Owner2, IsPrivate = true },
            new Issue { Id = I_PublicInPrivate, ProjectId = PrivateProject, Title = "pub/priv", Description = "d", ReporterId = U_Owner2, IsPrivate = false },
            new Issue { Id = I_PrivateInPrivate, ProjectId = PrivateProject, Title = "priv/priv", Description = "d", ReporterId = U_ReporterMember, IsPrivate = true });

        db.SaveChanges();
    }

    private async Task<(int[] projects, int[] issues)> VisibleTo(int userId, UserRole globalRole)
    {
        await using var db = new AppDbContext(_options);
        var access = await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, globalRole));
        var projects = await db.Projects.WhereVisibleTo(access).Select(p => p.Id).OrderBy(i => i).ToArrayAsync();
        var issues = await db.Issues.WhereVisibleTo(access).Select(i => i.Id).OrderBy(i => i).ToArrayAsync();
        return (projects, issues);
    }

    [Fact]
    public async Task Outsider_SeesOnlyPublicProjectAndItsPublicIssues()
    {
        var (projects, issues) = await VisibleTo(U_Outsider, UserRole.Reporter);
        Assert.Equal(new[] { PublicProject }, projects);
        // NOT the private issue in the public project, and nothing in the private project.
        Assert.Equal(new[] { I_PublicInPublic }, issues);
    }

    [Fact]
    public async Task ReporterMember_SeesPrivateProject_AndTheirOwnPrivateIssue_ButNotOthers()
    {
        var (projects, issues) = await VisibleTo(U_ReporterMember, UserRole.Reporter);
        Assert.Equal(new[] { PublicProject, PrivateProject }, projects);
        // Public-in-public + everything they can see in the private project (public issue + their own
        // private issue), but NOT the private issue in the public project (not reporter/assignee/dev).
        Assert.Equal(new[] { I_PublicInPublic, I_PublicInPrivate, I_PrivateInPrivate }, issues);
    }

    [Fact]
    public async Task GlobalAdministrator_SeesEverything()
    {
        var (projects, issues) = await VisibleTo(U_Outsider, UserRole.Administrator);
        Assert.Equal(new[] { PublicProject, PrivateProject }, projects);
        Assert.Equal(new[] { I_PublicInPublic, I_PrivateInPublic, I_PublicInPrivate, I_PrivateInPrivate }, issues);
    }

    public void Dispose() => _connection.Dispose();
}
