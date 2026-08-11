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
using OpenTrack.Infrastructure.Activity;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Tests;

/// <summary>The smart-poll change token moves when a VISIBLE issue changes, but not when something the
/// user can't see changes — so a poller never even learns that hidden activity happened.</summary>
public sealed class ActivityTokenTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Viewer = 2;

    public ActivityTokenTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = Owner });
        db.Issues.Add(new Issue { Id = 10, ProjectId = 1, Title = "visible", Description = "d", ReporterId = Owner, UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Token_Changes_WhenAVisibleIssueChanges()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        string before, after;
        await using (var db = new AppDbContext(_options))
            before = await ActivityToken.ComputeAsync(db, viewer);

        await using (var db = new AppDbContext(_options))
        {
            var i = await db.Issues.FindAsync(10);
            i!.UpdatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            await db.SaveChangesAsync();
        }
        await using (var db = new AppDbContext(_options))
            after = await ActivityToken.ComputeAsync(db, viewer);

        Assert.NotEqual(before, after);
    }

    [Fact]
    public async Task Token_DoesNotChange_ForActivityTheUserCannotSee()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        string before, after;
        await using (var db = new AppDbContext(_options))
            before = await ActivityToken.ComputeAsync(db, viewer);

        // A new PRIVATE issue the viewer can't see is added.
        await using (var db = new AppDbContext(_options))
        {
            db.Issues.Add(new Issue { Id = 11, ProjectId = 1, Title = "secret", Description = "d", IsPrivate = true, ReporterId = Owner, UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        await using (var db = new AppDbContext(_options))
            after = await ActivityToken.ComputeAsync(db, viewer);

        Assert.Equal(before, after); // the viewer's token is unmoved — no leak of hidden activity
    }

    public void Dispose() => _connection.Dispose();
}
