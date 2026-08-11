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
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Queries;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests the shared issue filter/sort (IssueQueries.ApplyFilter) against a real SQLite database, and
/// — the security-critical part — that filtering composes AFTER the ACL: a filter can only ever
/// narrow the rows a user is already allowed to see, never widen them.
/// </summary>
public sealed class IssueFilterTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int PublicProject = 1, PrivateProject = 2;
    private const int Owner = 1, Outsider = 99;

    public IssueFilterTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();

        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Outsider, UserName = "outsider" });
        db.Projects.AddRange(
            new Project { Id = PublicProject, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = PrivateProject, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = PrivateProject, UserId = Owner, Role = UserRole.Manager });

        db.Issues.AddRange(
            new Issue { Id = 1, ProjectId = PublicProject, Title = "Login crash", Description = "boom", ReporterId = Owner, Status = IssueStatus.New, Severity = IssueSeverity.Crash, Priority = IssuePriority.High },
            new Issue { Id = 2, ProjectId = PublicProject, Title = "Typo on page", Description = "spelling", ReporterId = Owner, Status = IssueStatus.Resolved, Severity = IssueSeverity.Trivial, Priority = IssuePriority.Low },
            new Issue { Id = 3, ProjectId = PublicProject, Title = "Slow query", Description = "login is slow", ReporterId = Owner, Status = IssueStatus.New, Severity = IssueSeverity.Major, Priority = IssuePriority.Normal },
            // A matching issue that an outsider must NOT see, in the private project.
            new Issue { Id = 4, ProjectId = PrivateProject, Title = "Login secret", Description = "hidden", ReporterId = Owner, Status = IssueStatus.New, Severity = IssueSeverity.Crash, Priority = IssuePriority.High });
        db.SaveChanges();
    }

    private async Task<int[]> Filter(int userId, UserRole role, IssueFilter filter)
    {
        await using var db = new AppDbContext(_options);
        var access = await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
        return await db.Issues.WhereVisibleTo(access).ApplyFilter(filter)
            .Select(i => i.Id).ToArrayAsync();
    }

    [Fact]
    public async Task Filter_ByStatus_ReturnsOnlyThatStatus()
    {
        var ids = await Filter(Owner, UserRole.Administrator, new IssueFilter(Status: IssueStatus.New, Sort: IssueSort.IdAsc));
        Assert.Equal(new[] { 1, 3, 4 }, ids);
    }

    [Fact]
    public async Task Filter_ByText_MatchesTitleOrDescription()
    {
        // "login" appears in issue 1's title and issue 3's description (admin sees all).
        var ids = await Filter(Owner, UserRole.Administrator, new IssueFilter(Text: "login", Sort: IssueSort.IdAsc));
        Assert.Contains(1, ids);
        Assert.Contains(3, ids);
        Assert.DoesNotContain(2, ids);
    }

    [Fact]
    public async Task Sort_PriorityDesc_OrdersHighToLow()
    {
        var ids = await Filter(Owner, UserRole.Administrator, new IssueFilter(ProjectId: PublicProject, Sort: IssueSort.PriorityDesc));
        Assert.Equal(new[] { 1, 3, 2 }, ids); // High, Normal, Low
    }

    [Fact]
    public async Task Filter_CannotWidenVisibility()
    {
        // Outsider searches for "login" — the matching PRIVATE issue (4) must never appear, only the
        // public matches. This is the key security property: ACL runs before the filter.
        var ids = await Filter(Outsider, UserRole.Reporter, new IssueFilter(Text: "login", Sort: IssueSort.IdAsc));
        Assert.DoesNotContain(4, ids);
        Assert.Contains(1, ids);
        Assert.Contains(3, ids);
    }

    public void Dispose() => _connection.Dispose();
}
