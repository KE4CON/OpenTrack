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
using OpenTrack.Infrastructure.Checklist;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Tests;

/// <summary>
/// The bug-hunt checklist: defining items is Manager-only, working them (status/convert) needs Updater,
/// converting a failure produces a normal issue linked back to the item, and a private project's
/// checklist never leaks to a non-member.
/// </summary>
public sealed class ChecklistTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int Owner = 1, Manager = 2, Updater = 3, Viewer = 4;
    private const int PubProject = 1, PrivProject = 2;

    public ChecklistTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Manager, UserName = "manager", Role = UserRole.Viewer },
            new User { Id = Updater, UserName = "updater", Role = UserRole.Updater },
            new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.AddRange(
            new Project { Id = PubProject, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = PrivProject, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = PubProject, UserId = Manager, Role = UserRole.Manager });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    private async Task<int> ImportOneItemAsManager()
    {
        var mgr = await Access(Manager, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var added = await ChecklistOperations.ImportAsync(db, mgr, PubProject, "# Concurrency\n- [ ] Message store is thread-safe", CancellationToken.None);
        Assert.Equal(1, added);
        return await db.ChecklistItems.Where(c => c.ProjectId == PubProject).Select(c => c.Id).SingleAsync();
    }

    [Fact]
    public async Task DefiningItems_IsManagerOnly()
    {
        var updater = await Access(Updater, UserRole.Updater);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                ChecklistOperations.AddItemAsync(db, updater, PubProject, "x", null, null));

        var id = await ImportOneItemAsManager();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task SettingStatus_NeedsUpdater_AndPreservesNotesOnNull()
    {
        var id = await ImportOneItemAsManager();

        // A viewer cannot work the checklist.
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                ChecklistOperations.SetStatusAsync(db, viewer, PubProject, id, ChecklistItemStatus.Pass, null));

        var updater = await Access(Updater, UserRole.Updater);
        // Write a note, then a quick Pass with null notes must keep it.
        await using (var db = new AppDbContext(_options))
            Assert.Null(await ChecklistOperations.SetStatusAsync(db, updater, PubProject, id, ChecklistItemStatus.Fail, "found a race"));
        await using (var db = new AppDbContext(_options))
            Assert.Null(await ChecklistOperations.SetStatusAsync(db, updater, PubProject, id, ChecklistItemStatus.Pass, null));

        await using var check = new AppDbContext(_options);
        var item = await check.ChecklistItems.FindAsync(id);
        Assert.Equal(ChecklistItemStatus.Pass, item!.Status);
        Assert.Equal("found a race", item.Notes);       // preserved
        Assert.NotNull(item.CheckedAt);
    }

    [Fact]
    public async Task Convert_CreatesLinkedIssue_AndIsIdempotent()
    {
        var id = await ImportOneItemAsManager();
        var updater = await Access(Updater, UserRole.Updater);

        int? issueId;
        await using (var db = new AppDbContext(_options))
            issueId = await ChecklistOperations.ConvertToIssueAsync(db, updater, PubProject, id, CancellationToken.None);
        Assert.NotNull(issueId);

        await using (var check = new AppDbContext(_options))
        {
            var item = await check.ChecklistItems.FindAsync(id);
            Assert.Equal(ChecklistItemStatus.Fail, item!.Status);   // convert marks it failed
            Assert.Equal(issueId, item.LinkedIssueId);

            var issue = await check.Issues.Include(i => i.History).FirstAsync(i => i.Id == issueId);
            Assert.Equal(Updater, issue.ReporterId);                // reporter is the converter
            Assert.Equal(IssueStatus.New, issue.Status);
            Assert.Equal(PubProject, issue.ProjectId);
            Assert.Single(issue.History);                           // constructed like a normal new issue
        }

        // Converting again returns the SAME issue and doesn't create a second one.
        await using (var db = new AppDbContext(_options))
        {
            var again = await ChecklistOperations.ConvertToIssueAsync(db, updater, PubProject, id, CancellationToken.None);
            Assert.Equal(issueId, again);
        }
        await using (var check = new AppDbContext(_options))
            Assert.Equal(1, await check.Issues.CountAsync());
    }

    [Fact]
    public async Task PrivateProjectChecklist_DoesNotLeakToNonMember()
    {
        // Seed an item in the private project (owner is a manager there via ownership path — use admin).
        var admin = await Access(Owner, UserRole.Administrator);
        await using (var db = new AppDbContext(_options))
            await ChecklistOperations.AddItemAsync(db, admin, PrivProject, "secret check", null, null);

        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var check = new AppDbContext(_options);
        Assert.Empty(await ChecklistOperations.ListForProjectAsync(check, viewer, PrivProject));
    }

    public void Dispose() => _connection.Dispose();
}
