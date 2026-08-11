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
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Identity;

namespace OpenTrack.API.Tests;

/// <summary>Regression tests for the admin-bootstrap check that de-races the first-registrant
/// promotion (audit finding M8): only promote the first user when NO active administrator exists.</summary>
public sealed class BootstrapSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public BootstrapSeederTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
    }

    private AppDbContext New() => new(_options);

    [Fact]
    public async Task NoUsers_HasNoAdministrator()
    {
        await using var db = New();
        Assert.False(await OpenTrackSeeder.HasAdministratorAsync(db));
    }

    [Fact]
    public async Task OnlyReporters_HasNoAdministrator()
    {
        await using (var db = New())
        {
            db.Users.Add(new User { Id = 1, UserName = "a", Role = UserRole.Reporter, IsActive = true });
            await db.SaveChangesAsync();
        }
        await using var check = New();
        Assert.False(await OpenTrackSeeder.HasAdministratorAsync(check));
    }

    [Fact]
    public async Task DeactivatedAdministrator_DoesNotCount()
    {
        await using (var db = New())
        {
            db.Users.Add(new User { Id = 1, UserName = "a", Role = UserRole.Administrator, IsActive = false });
            await db.SaveChangesAsync();
        }
        await using var check = New();
        Assert.False(await OpenTrackSeeder.HasAdministratorAsync(check));
    }

    [Fact]
    public async Task ActiveAdministrator_Counts()
    {
        await using (var db = New())
        {
            db.Users.Add(new User { Id = 1, UserName = "a", Role = UserRole.Administrator, IsActive = true });
            await db.SaveChangesAsync();
        }
        await using var check = New();
        Assert.True(await OpenTrackSeeder.HasAdministratorAsync(check));
    }

    public void Dispose() => _connection.Dispose();
}
