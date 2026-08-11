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

/// <summary>Roadmap/changelog groups issues by fix version, counts done (Resolved/Closed) for progress,
/// splits released vs unreleased, and never counts an issue the viewer can't see.</summary>
public sealed class RoadmapTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Viewer = 2, V10 = 10, V09 = 9;

    public RoadmapTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = Owner });
        db.Versions.AddRange(
            new ProjectVersion { Id = V10, ProjectId = 1, Name = "1.0", IsReleased = false },
            new ProjectVersion { Id = V09, ProjectId = 1, Name = "0.9", IsReleased = true, ReleaseDate = new DateTime(2026, 1, 1) });
        db.Issues.AddRange(
            new Issue { Id = 100, ProjectId = 1, Title = "a", Description = "d", ReporterId = Owner, FixVersionId = V10, Status = IssueStatus.New },
            new Issue { Id = 101, ProjectId = 1, Title = "b", Description = "d", ReporterId = Owner, FixVersionId = V10, Status = IssueStatus.Closed },
            new Issue { Id = 102, ProjectId = 1, Title = "c", Description = "d", ReporterId = Owner, FixVersionId = V09, Status = IssueStatus.Resolved },
            new Issue { Id = 103, ProjectId = 1, Title = "secret", Description = "d", IsPrivate = true, ReporterId = Owner, FixVersionId = V10, Status = IssueStatus.New });
        db.SaveChanges();
    }

    [Fact]
    public async Task Build_GroupsByFixVersion_CountsDone_AndRespectsAcl()
    {
        await using var db = new AppDbContext(_options);
        var access = await AccessSnapshot.LoadAsync(db, new AccessIdentity(Viewer, UserRole.Viewer));
        var rows = await RoadmapQuery.BuildAsync(db, access, 1);

        // Unreleased first.
        Assert.False(rows[0].IsReleased);
        Assert.Equal("1.0", rows[0].Name);
        Assert.Equal(2, rows[0].Total);   // #100 + #101 (NOT the private #103)
        Assert.Equal(1, rows[0].Done);    // #101 Closed

        var changelog = rows.Single(r => r.IsReleased);
        Assert.Equal("0.9", changelog.Name);
        Assert.Equal(1, changelog.Total);
        Assert.Equal(1, changelog.Done);  // #102 Resolved
    }

    public void Dispose() => _connection.Dispose();
}
