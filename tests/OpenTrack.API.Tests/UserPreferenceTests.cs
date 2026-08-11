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
using OpenTrack.Core.Querying;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Preferences;

namespace OpenTrack.API.Tests;

/// <summary>Per-user preferences: default to empty, upsert in place (one row per user), and stay
/// isolated between users.</summary>
public sealed class UserPreferenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Alice = 1, Bob = 2;

    public UserPreferenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Alice, UserName = "a" }, new User { Id = Bob, UserName = "b" });
        db.SaveChanges();
    }

    [Fact]
    public async Task Defaults_AreEmpty()
    {
        await using var db = new AppDbContext(_options);
        var p = await UserPreferenceOperations.GetAsync(db, Alice);
        Assert.Null(p.DefaultProjectId);
        Assert.Null(p.DefaultSort);
    }

    [Fact]
    public async Task Save_Upserts_AndIsPerUser()
    {
        await using (var db = new AppDbContext(_options))
            await UserPreferenceOperations.SaveAsync(db, Alice, 7, IssueSort.PriorityDesc);
        await using (var db = new AppDbContext(_options))
            await UserPreferenceOperations.SaveAsync(db, Alice, 9, IssueSort.CreatedAsc); // update in place

        await using var check = new AppDbContext(_options);
        var alice = await UserPreferenceOperations.GetAsync(check, Alice);
        Assert.Equal(9, alice.DefaultProjectId);
        Assert.Equal(IssueSort.CreatedAsc, alice.DefaultSort);
        Assert.Equal(1, await check.UserPreferences.CountAsync(x => x.UserId == Alice)); // one row

        var bob = await UserPreferenceOperations.GetAsync(check, Bob);
        Assert.Null(bob.DefaultProjectId); // Bob unaffected
    }

    public void Dispose() => _connection.Dispose();
}
