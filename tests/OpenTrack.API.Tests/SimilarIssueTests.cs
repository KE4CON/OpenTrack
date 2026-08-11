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
using OpenTrack.Infrastructure.Queries;

namespace OpenTrack.API.Tests;

/// <summary>Duplicate suggestions only ever include issues the searcher could open, and rank by title
/// overlap (excluding the issue being viewed).</summary>
public sealed class SimilarIssueTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Viewer = 2;

    public SimilarIssueTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.AddRange(
            new Project { Id = 1, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = 2, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.Issues.AddRange(
            new Issue { Id = 10, ProjectId = 1, Title = "Login button broken on mobile", Description = "d", ReporterId = Owner },
            new Issue { Id = 11, ProjectId = 1, Title = "Login button secret regression", Description = "d", IsPrivate = true, ReporterId = Owner },
            new Issue { Id = 12, ProjectId = 2, Title = "Login button hidden project", Description = "d", ReporterId = Owner });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Find_OnlyReturnsVisibleIssues()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var hits = await SimilarIssueQuery.FindAsync(db, viewer, projectId: null, title: "Login button not working", excludeIssueId: null);
        var ids = hits.Select(h => h.Id).ToList();

        Assert.Contains(10, ids);        // public, visible
        Assert.DoesNotContain(11, ids);  // private issue the viewer can't see
        Assert.DoesNotContain(12, ids);  // issue in a private project
    }

    [Fact]
    public async Task Find_ExcludesTheGivenIssue()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using var db = new AppDbContext(_options);
        var hits = await SimilarIssueQuery.FindAsync(db, admin, projectId: 1, title: "Login button broken", excludeIssueId: 10);
        Assert.DoesNotContain(10, hits.Select(h => h.Id)); // never suggests the issue you're on
    }

    public void Dispose() => _connection.Dispose();
}
