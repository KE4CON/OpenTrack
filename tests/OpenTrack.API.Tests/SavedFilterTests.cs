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
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Filters;

namespace OpenTrack.API.Tests;

/// <summary>Saved filters are strictly per-user: one user can't see or delete another's, re-saving a
/// name overwrites (not duplicates), and an empty name is rejected.</summary>
public sealed class SavedFilterTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Alice = 1, Bob = 2;

    public SavedFilterTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Alice, UserName = "alice" }, new User { Id = Bob, UserName = "bob" });
        db.SaveChanges();
    }

    [Fact]
    public async Task Save_Then_List_IsPerUser()
    {
        await using (var db = new AppDbContext(_options))
        {
            Assert.Null(await SavedFilterOperations.SaveAsync(db, Alice, "My open crashes", "?Severity=Crash&Stale=true"));
        }
        await using (var db = new AppDbContext(_options))
        {
            var mine = await SavedFilterOperations.ListForUserAsync(db, Alice);
            var item = Assert.Single(mine);
            Assert.Equal("My open crashes", item.Name);
            Assert.Equal("Severity=Crash&Stale=true", item.Query);   // leading '?' trimmed

            Assert.Empty(await SavedFilterOperations.ListForUserAsync(db, Bob)); // Bob sees nothing
        }
    }

    [Fact]
    public async Task Save_SameName_OverwritesQuery_DoesNotDuplicate()
    {
        await using (var db = new AppDbContext(_options))
            await SavedFilterOperations.SaveAsync(db, Alice, "Filter", "Status=New");
        await using (var db = new AppDbContext(_options))
            await SavedFilterOperations.SaveAsync(db, Alice, "Filter", "Status=Closed");

        await using var check = new AppDbContext(_options);
        var item = Assert.Single(await SavedFilterOperations.ListForUserAsync(check, Alice));
        Assert.Equal("Status=Closed", item.Query);
    }

    [Fact]
    public async Task Delete_IsScopedToOwner()
    {
        int id;
        await using (var db = new AppDbContext(_options))
        {
            await SavedFilterOperations.SaveAsync(db, Alice, "Filter", "Status=New");
            id = (await SavedFilterOperations.ListForUserAsync(db, Alice)).Single().Id;
        }
        await using (var db = new AppDbContext(_options))
            Assert.False(await SavedFilterOperations.DeleteAsync(db, Bob, id)); // Bob can't delete Alice's
        await using (var db = new AppDbContext(_options))
            Assert.True(await SavedFilterOperations.DeleteAsync(db, Alice, id));
    }

    [Fact]
    public async Task Save_RejectsEmptyName()
    {
        await using var db = new AppDbContext(_options);
        Assert.NotNull(await SavedFilterOperations.SaveAsync(db, Alice, "   ", "Status=New"));
    }

    public void Dispose() => _connection.Dispose();
}
