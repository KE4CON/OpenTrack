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
using OpenTrack.Infrastructure.TimeLogs;

namespace OpenTrack.API.Tests;

/// <summary>Time logging: viewing follows the issue ACL, logging needs Updater, and a viewer can only
/// delete their own entry (an Updater+ can delete anyone's).</summary>
public sealed class TimeLogTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Updater = 2, Viewer = 3, Issue1 = 10;

    public TimeLogTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Updater, UserName = "updater", Role = UserRole.Updater },
            new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = Owner });
        db.Issues.Add(new Issue { Id = Issue1, ProjectId = 1, Title = "t", Description = "d", ReporterId = Owner });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Add_RequiresUpdater_AndListsWithTotal()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => TimeLogOperations.AddAsync(db, viewer, Issue1, 30, "peek", null));

        var updater = await Access(Updater, UserRole.Updater);
        await using (var db = new AppDbContext(_options))
        {
            Assert.Null(await TimeLogOperations.AddAsync(db, updater, Issue1, 90, "fixing", null));
            Assert.NotNull(await TimeLogOperations.AddAsync(db, updater, Issue1, 0, "nope", null)); // zero rejected
        }
        await using var check = new AppDbContext(_options);
        var items = await TimeLogOperations.ListForIssueAsync(check, updater, Issue1);
        Assert.Equal(90, Assert.Single(items).Minutes);
    }

    [Fact]
    public async Task Delete_OnlyOwnEntry_ForANonUpdater()
    {
        // Owner (as a plain member) logs an entry; a different plain viewer can't delete it.
        var owner = await Access(Owner, UserRole.Updater); // owner logs (needs Updater)
        int logId;
        await using (var db = new AppDbContext(_options))
        {
            await TimeLogOperations.AddAsync(db, owner, Issue1, 45, "mine", null);
            logId = (await TimeLogOperations.ListForIssueAsync(db, owner, Issue1)).Single().Id;
        }

        var viewer = await Access(Viewer, UserRole.Viewer);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => TimeLogOperations.DeleteAsync(db, viewer, logId));

        // An Updater on the project can remove anyone's entry.
        var updater = await Access(Updater, UserRole.Updater);
        await using (var db = new AppDbContext(_options))
            Assert.True(await TimeLogOperations.DeleteAsync(db, updater, logId));
    }

    public void Dispose() => _connection.Dispose();
}
