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
using OpenTrack.Infrastructure.Reporting;

namespace OpenTrack.API.Tests;

/// <summary>Reporting figures are computed only over the issues the user can see, split open vs done,
/// and bucketed by month.</summary>
public sealed class ReportTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private static readonly DateTime Now = new(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    private const int Owner = 1, Viewer = 2;

    public ReportTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = Owner });
        db.Issues.AddRange(
            new Issue { Id = 10, ProjectId = 1, Title = "open now", Description = "d", ReporterId = Owner, Status = IssueStatus.New, CreatedAt = Now.AddDays(-2), UpdatedAt = Now.AddDays(-2) },
            new Issue { Id = 11, ProjectId = 1, Title = "resolved now", Description = "d", ReporterId = Owner, Status = IssueStatus.Resolved, CreatedAt = Now.AddMonths(-1), UpdatedAt = Now.AddDays(-1) },
            new Issue { Id = 12, ProjectId = 1, Title = "private open", Description = "d", IsPrivate = true, ReporterId = Owner, Status = IssueStatus.New, CreatedAt = Now.AddDays(-3), UpdatedAt = Now.AddDays(-3) });
        db.SaveChanges();
    }

    [Fact]
    public async Task Build_CountsOnlyVisible_SplitsOpenAndResolvedThisMonth()
    {
        await using var db = new AppDbContext(_options);
        var access = await AccessSnapshot.LoadAsync(db, new AccessIdentity(Viewer, UserRole.Viewer));
        var r = await ReportQuery.BuildAsync(db, access, projectId: null, Now);

        Assert.Equal(2, r.TotalIssues);        // #10 + #11; not the private #12
        Assert.Equal(1, r.OpenIssues);         // #10
        Assert.Equal(1, r.ResolvedThisMonth);  // #11 resolved yesterday
        Assert.Equal(6, r.CreatedByMonth.Count);                   // six month buckets
        Assert.Equal(2, r.CreatedByMonth.Sum(b => b.Count));       // #10 (this month) + #11 (last month), not the private one
        Assert.Equal(1, r.CreatedByMonth[^1].Count);              // the current-month bucket holds #10

        Assert.Equal("New", Assert.Single(r.OpenByStatus).Label);  // only #10 is open
    }

    public void Dispose() => _connection.Dispose();
}
