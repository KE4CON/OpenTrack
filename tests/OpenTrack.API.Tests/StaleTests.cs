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
using OpenTrack.Infrastructure.Dashboard;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Queries;

namespace OpenTrack.API.Tests;

/// <summary>"Stale" = an OPEN issue not touched in 30+ days. It must exclude recently-updated issues
/// and issues that are already resolved/closed, in both the issue filter and the dashboard count.</summary>
public sealed class StaleTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private static readonly DateTime Now = new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    public StaleTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.Add(new User { Id = 1, UserName = "owner" });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = 1 });
        db.Issues.AddRange(
            new Issue { Id = 10, ProjectId = 1, Title = "stale open", Description = "d", Status = IssueStatus.New, ReporterId = 1, UpdatedAt = Now.AddDays(-45) },
            new Issue { Id = 11, ProjectId = 1, Title = "fresh open", Description = "d", Status = IssueStatus.New, ReporterId = 1, UpdatedAt = Now.AddDays(-3) },
            new Issue { Id = 12, ProjectId = 1, Title = "old but closed", Description = "d", Status = IssueStatus.Closed, ReporterId = 1, UpdatedAt = Now.AddDays(-90) });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Admin()
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(1, UserRole.Administrator));
    }

    [Fact]
    public async Task Filter_KeepsOnlyStaleOpenIssues()
    {
        var access = await Admin();
        await using var db = new AppDbContext(_options);
        var cutoff = Now.AddDays(-IssueDefaults.StaleDays);
        var rows = await db.Issues.AsNoTracking()
            .WhereVisibleTo(access)
            .ApplyFilter(new IssueFilter(StaleBeforeUtc: cutoff))
            .Select(i => i.Id)
            .ToListAsync();

        Assert.Equal(new[] { 10 }, rows);   // not #11 (fresh), not #12 (closed)
    }

    [Fact]
    public async Task Dashboard_CountsStaleOpenOnly()
    {
        var access = await Admin();
        await using var db = new AppDbContext(_options);
        var r = await DashboardQuery.BuildAsync(db, access, Now);
        Assert.Equal(1, r.TotalStale);      // only #10
        Assert.Equal(2, r.TotalOpen);       // #10 + #11 open; #12 closed
    }

    public void Dispose() => _connection.Dispose();
}
