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
using OpenTrack.Infrastructure.Email;
using OpenTrack.Infrastructure.Notifications;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests the notification dispatcher's recipient rules and — the security-critical part — that a
/// monitor who cannot currently VIEW a (private) issue receives no notification, so the issue title
/// never leaks to them. Also verifies the actor is never notified of their own change.
/// </summary>
public sealed class NotificationTests : IDisposable
{
    private sealed class NoEmail : IEmailService
    {
        public bool IsConfigured => false;
        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default) => Task.CompletedTask;
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int Reporter = 1, Assignee = 2, DevMonitor = 3, OutsiderMonitor = 99, Admin = 5;
    private const int Issue1 = 1;

    public NotificationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();

        db.Users.AddRange(
            new User { Id = Reporter, UserName = "reporter" },
            new User { Id = Assignee, UserName = "assignee" },
            new User { Id = DevMonitor, UserName = "dev" },
            new User { Id = OutsiderMonitor, UserName = "outsider" },
            new User { Id = Admin, UserName = "admin", Role = UserRole.Administrator });
        db.Projects.Add(new Project { Id = 1, Name = "Private", IsPublic = false, OwnerId = Reporter });
        db.ProjectMemberships.AddRange(
            new ProjectMembership { ProjectId = 1, UserId = Reporter, Role = UserRole.Manager },
            new ProjectMembership { ProjectId = 1, UserId = Assignee, Role = UserRole.Developer },
            new ProjectMembership { ProjectId = 1, UserId = DevMonitor, Role = UserRole.Developer });
        // A PRIVATE issue: only reporter/assignee/Developer+ members can see it.
        db.Issues.Add(new Issue { Id = Issue1, ProjectId = 1, Title = "Secret bug", Description = "d", IsPrivate = true, ReporterId = Reporter, AssigneeId = Assignee });
        db.IssueMonitors.AddRange(
            new IssueMonitor { IssueId = Issue1, UserId = DevMonitor },          // can view (Developer member)
            new IssueMonitor { IssueId = Issue1, UserId = OutsiderMonitor });    // cannot view (not a member)
        db.SaveChanges();
    }

    private async Task<int[]> RecipientsAfterChangeBy(int actorId)
    {
        await using var db = new AppDbContext(_options);
        await new NotificationDispatch(new NoEmail()).NotifyIssueChangedAsync(db, actorId, Issue1, "status changed");
        await using var check = new AppDbContext(_options);
        return await check.Notifications.AsNoTracking().Where(n => n.IssueId == Issue1)
            .Select(n => n.UserId).Distinct().OrderBy(x => x).ToArrayAsync();
    }

    [Fact]
    public async Task Notify_GoesToReporterAssigneeAndViewableMonitors_NotUnviewableMonitor()
    {
        var recipients = await RecipientsAfterChangeBy(Admin);
        Assert.Equal(new[] { Reporter, Assignee, DevMonitor }, recipients);
        Assert.DoesNotContain(OutsiderMonitor, recipients); // cannot view the private issue -> no leak
    }

    [Fact]
    public async Task Notify_ExcludesTheActor()
    {
        // The assignee makes the change — they should not be notified of their own action.
        var recipients = await RecipientsAfterChangeBy(Assignee);
        Assert.DoesNotContain(Assignee, recipients);
        Assert.Contains(Reporter, recipients);
        Assert.Contains(DevMonitor, recipients);
    }

    public void Dispose() => _connection.Dispose();
}
