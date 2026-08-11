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
using OpenTrack.Infrastructure.Dashboard;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Tests;

/// <summary>
/// The dashboard aggregates over MANY issues at once, so its row-level ACL is the thing most likely to
/// leak: a tally that quietly includes an issue the user could never open directly. These tests pin
/// that a plain viewer's counts and "recent" list exclude private issues and private-project issues,
/// while an admin sees everything — and that "open" and "overdue" are counted correctly.
/// </summary>
public sealed class DashboardTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private static readonly DateTime Now = new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    private const int Owner = 1, Viewer = 2;
    private const int PubProject = 1, PrivProject = 2;

    public DashboardTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.AddRange(
            new Project { Id = PubProject, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = PrivProject, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.Issues.AddRange(
            // Visible & open (public, not private):
            new Issue { Id = 10, ProjectId = PubProject, Title = "open", Description = "d", Status = IssueStatus.New, ReporterId = Owner, UpdatedAt = Now.AddMinutes(-5) },
            // Visible, open, OVERDUE:
            new Issue { Id = 11, ProjectId = PubProject, Title = "overdue", Description = "d", Status = IssueStatus.Assigned, ReporterId = Owner, DueDate = Now.AddDays(-2), UpdatedAt = Now.AddMinutes(-4) },
            // Visible but CLOSED (not open):
            new Issue { Id = 12, ProjectId = PubProject, Title = "closed", Description = "d", Status = IssueStatus.Closed, ReporterId = Owner, UpdatedAt = Now.AddMinutes(-3) },
            // NOT visible to the viewer: a private issue in the public project (reporter is the owner):
            new Issue { Id = 13, ProjectId = PubProject, Title = "secret", Description = "d", Status = IssueStatus.New, IsPrivate = true, ReporterId = Owner, UpdatedAt = Now.AddMinutes(-1) },
            // NOT visible to the viewer: an issue in a private project:
            new Issue { Id = 14, ProjectId = PrivProject, Title = "hidden", Description = "d", Status = IssueStatus.New, ReporterId = Owner, DueDate = Now.AddDays(-9), UpdatedAt = Now.AddMinutes(-2) });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Viewer_SeesOnlyVisibleIssues_InEveryTally()
    {
        var access = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var r = await DashboardQuery.BuildAsync(db, access, Now);

        Assert.Equal(2, r.TotalOpen);       // #10 and #11 only — not the closed, private, or hidden ones
        Assert.Equal(1, r.TotalOverdue);    // #11 only — the private project's overdue #14 is invisible

        var pub = Assert.Single(r.Projects); // the private project never appears
        Assert.Equal(PubProject, pub.ProjectId);
        Assert.Equal(2, pub.OpenCount);
        Assert.Equal(1, pub.OverdueCount);

        // Recent = visible issues regardless of open/closed: #10, #11, #12 — never #13 or #14.
        Assert.Equal(new[] { 10, 11, 12 }, r.Recent.Select(i => i.Id).OrderBy(x => x).ToArray());
        Assert.DoesNotContain(r.Recent, i => i.Id == 13 || i.Id == 14);
    }

    [Fact]
    public async Task Admin_SeesEverything()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using var db = new AppDbContext(_options);
        var r = await DashboardQuery.BuildAsync(db, access: admin, Now);

        Assert.Equal(4, r.TotalOpen);       // #10, #11, #13, #14 are open; only #12 (Closed) is excluded
        Assert.Equal(2, r.TotalOverdue);    // #11 and #14
        Assert.Equal(2, r.Projects.Count);  // both projects
    }

    public void Dispose() => _connection.Dispose();
}
