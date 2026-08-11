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
using Microsoft.Extensions.Logging.Abstractions;
using OpenTrack.Core.Bulk;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Bulk;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Email;
using OpenTrack.Infrastructure.Notifications;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests bulk actions (shared BulkOperations): the operation applies only to issues the caller may
/// act on and SKIPS the rest — an issue the caller can't see is never changed (and can't be probed),
/// and a bulk assign validates the assignee is a member of each issue's project.
/// </summary>
public sealed class BulkTests : IDisposable
{
    private sealed class NoEmail : IEmailService
    {
        public bool IsConfigured => false;
        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default) => Task.CompletedTask;
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private static NotificationDispatch Dispatch() => new(new NoEmail(), NullLogger<NotificationDispatch>.Instance);

    private const int Owner = 1, Admin = 5, Stranger = 7;
    private const int I1 = 1, I2 = 2, I3Private = 3;

    public BulkTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Admin, UserName = "admin", Role = UserRole.Administrator },
            new User { Id = Stranger, UserName = "stranger" });
        db.Projects.AddRange(
            new Project { Id = 1, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = 2, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = 2, UserId = Owner, Role = UserRole.Manager });
        db.Issues.AddRange(
            new Issue { Id = I1, ProjectId = 1, Title = "one", Description = "d", ReporterId = Owner, Status = IssueStatus.New },
            new Issue { Id = I2, ProjectId = 1, Title = "two", Description = "d", ReporterId = Owner, Status = IssueStatus.New },
            new Issue { Id = I3Private, ProjectId = 2, Title = "secret", Description = "d", ReporterId = Owner, Status = IssueStatus.New });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task SetStatus_AppliesToEditable_SkipsUnviewable()
    {
        // A global Updater can edit the two public issues but cannot even see the private one.
        var updater = await Access(Stranger, UserRole.Updater);
        BulkResult result;
        await using (var db = new AppDbContext(_options))
            result = await BulkOperations.ApplyAsync(db, updater, new[] { I1, I2, I3Private },
                new BulkAction(BulkActionType.SetStatus, Status: IssueStatus.Acknowledged), Dispatch());

        Assert.Equal(2, result.Updated);
        Assert.Equal(1, result.Skipped); // the private issue is skipped, never touched

        await using var check = new AppDbContext(_options);
        Assert.Equal(IssueStatus.Acknowledged, (await check.Issues.FindAsync(I1))!.Status);
        Assert.Equal(IssueStatus.Acknowledged, (await check.Issues.FindAsync(I2))!.Status);
        Assert.Equal(IssueStatus.New, (await check.Issues.FindAsync(I3Private))!.Status); // untouched
    }

    [Fact]
    public async Task Assign_SkipsWhenAssigneeNotAMember_ThenSucceedsOnceMember()
    {
        var admin = await Access(Admin, UserRole.Administrator);

        await using (var db = new AppDbContext(_options))
        {
            var r = await BulkOperations.ApplyAsync(db, admin, new[] { I1 },
                new BulkAction(BulkActionType.Assign, AssigneeId: Stranger), Dispatch());
            Assert.Equal(0, r.Updated);
            Assert.Equal(1, r.Skipped); // stranger isn't a member of project 1
        }

        await using (var db = new AppDbContext(_options))
        {
            db.ProjectMemberships.Add(new ProjectMembership { ProjectId = 1, UserId = Stranger, Role = UserRole.Developer });
            await db.SaveChangesAsync();
        }

        await using (var db = new AppDbContext(_options))
        {
            var r = await BulkOperations.ApplyAsync(db, admin, new[] { I1 },
                new BulkAction(BulkActionType.Assign, AssigneeId: Stranger), Dispatch());
            Assert.Equal(1, r.Updated);
        }
        await using var check = new AppDbContext(_options);
        Assert.Equal(Stranger, (await check.Issues.FindAsync(I1))!.AssigneeId);
    }

    public void Dispose() => _connection.Dispose();
}
