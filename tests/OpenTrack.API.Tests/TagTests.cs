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
using OpenTrack.Infrastructure.Tags;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests tags (shared TagOperations + tag filter): create-on-assign with case-insensitive reuse,
/// idempotent add, and the security properties — tagging requires edit, an issue's tags require view
/// access, and filtering by a tag cannot reveal a private issue the caller can't see.
/// </summary>
public sealed class TagTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Outsider = 99, I1Public = 1, I2Private = 2;

    public TagTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Outsider, UserName = "outsider" });
        db.Projects.AddRange(
            new Project { Id = 1, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = 2, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = 2, UserId = Owner, Role = UserRole.Manager });
        db.Issues.AddRange(
            new Issue { Id = I1Public, ProjectId = 1, Title = "Pub", Description = "d", ReporterId = Owner },
            new Issue { Id = I2Private, ProjectId = 2, Title = "Priv", Description = "d", ReporterId = Owner });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Add_CreatesTagOnAssign_AndReusesCaseInsensitively()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using (var db = new AppDbContext(_options))
        {
            Assert.Null(await TagOperations.AddAsync(db, admin, I1Public, "Regression"));
            Assert.Null(await TagOperations.AddAsync(db, admin, I2Private, "regression")); // same tag, different case
        }
        await using var check = new AppDbContext(_options);
        Assert.Equal(1, await check.Tags.CountAsync()); // one tag, reused
    }

    [Fact]
    public async Task Add_IsIdempotent()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using var db = new AppDbContext(_options);
        await TagOperations.AddAsync(db, admin, I1Public, "ui");
        await TagOperations.AddAsync(db, admin, I1Public, "ui"); // again
        var tags = await TagOperations.ListForIssueAsync(db, admin, I1Public);
        Assert.Single(tags);
    }

    [Fact]
    public async Task Add_RequiresEdit()
    {
        var reporter = await Access(Outsider, UserRole.Reporter); // can view public issue, cannot edit
        await using var db = new AppDbContext(_options);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => TagOperations.AddAsync(db, reporter, I1Public, "x"));
    }

    [Fact]
    public async Task List_HidesTagsOfIssuesTheUserCannotSee()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using (var db = new AppDbContext(_options))
            await TagOperations.AddAsync(db, admin, I2Private, "confidential");

        await using var db2 = new AppDbContext(_options);
        var outsider = await Access(Outsider, UserRole.Reporter);
        Assert.Empty(await TagOperations.ListForIssueAsync(db2, outsider, I2Private)); // can't view the issue at all
    }

    [Fact]
    public async Task FilterByTag_CannotRevealPrivateIssue()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        int tagId;
        await using (var db = new AppDbContext(_options))
        {
            await TagOperations.AddAsync(db, admin, I2Private, "hush");
            tagId = (await db.Tags.FirstAsync(t => t.Name == "hush")).Id;
        }

        await using var db2 = new AppDbContext(_options);
        var outsider = await Access(Outsider, UserRole.Reporter);
        var ids = await db2.Issues.WhereVisibleTo(outsider).ApplyFilter(new IssueFilter(TagId: tagId))
            .Select(i => i.Id).ToArrayAsync();
        Assert.DoesNotContain(I2Private, ids); // tag filter can't surface an issue the ACL hides
    }

    public void Dispose() => _connection.Dispose();
}
