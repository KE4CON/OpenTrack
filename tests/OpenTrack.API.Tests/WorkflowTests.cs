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
using OpenTrack.Infrastructure.Workflow;

namespace OpenTrack.API.Tests;

/// <summary>Per-project workflow: no rules means any transition is allowed; once rules exist only they
/// (plus a no-op) are allowed, and the rule is enforced by the bulk status path (disallowed = skipped).</summary>
public sealed class WorkflowTests : IDisposable
{
    private sealed class NoEmail : IEmailService
    {
        public bool IsConfigured => false;
        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default) => Task.CompletedTask;
    }

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Issue1 = 10;

    public WorkflowTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.Add(new User { Id = Owner, UserName = "owner" });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = Owner });
        db.Issues.Add(new Issue { Id = Issue1, ProjectId = 1, Title = "t", Description = "d", ReporterId = Owner, Status = IssueStatus.New });
        db.SaveChanges();
    }

    [Fact]
    public async Task IsAllowed_OpenByDefault_ThenRestrictedOnceRulesExist()
    {
        await using var db = new AppDbContext(_options);
        // No rules => anything goes.
        Assert.True(await WorkflowOperations.IsAllowedAsync(db, 1, IssueStatus.New, IssueStatus.Closed));
        Assert.True(await WorkflowOperations.IsAllowedAsync(db, 1, IssueStatus.New, IssueStatus.New)); // no-op always allowed

        db.WorkflowTransitions.Add(new WorkflowTransition { ProjectId = 1, FromStatus = IssueStatus.New, ToStatus = IssueStatus.Acknowledged });
        await db.SaveChangesAsync();

        Assert.True(await WorkflowOperations.IsAllowedAsync(db, 1, IssueStatus.New, IssueStatus.Acknowledged)); // listed
        Assert.False(await WorkflowOperations.IsAllowedAsync(db, 1, IssueStatus.New, IssueStatus.Closed));      // not listed
        Assert.True(await WorkflowOperations.IsAllowedAsync(db, 1, IssueStatus.New, IssueStatus.New));          // still a no-op
    }

    [Fact]
    public async Task Bulk_SkipsADisallowedTransition()
    {
        await using (var db = new AppDbContext(_options))
        {
            db.WorkflowTransitions.Add(new WorkflowTransition { ProjectId = 1, FromStatus = IssueStatus.New, ToStatus = IssueStatus.Acknowledged });
            await db.SaveChangesAsync();
        }

        var admin = await AccessAsAdmin();
        await using (var db = new AppDbContext(_options))
        {
            // New -> Closed is not allowed by the workflow, so the bulk op skips it.
            var r = await BulkOperations.ApplyAsync(db, admin, new[] { Issue1 },
                new BulkAction(BulkActionType.SetStatus, Status: IssueStatus.Closed),
                new NotificationDispatch(new NoEmail(), NullLogger<NotificationDispatch>.Instance));
            Assert.Equal(0, r.Updated);
            Assert.Equal(1, r.Skipped);
        }
        await using var check = new AppDbContext(_options);
        Assert.Equal(IssueStatus.New, (await check.Issues.FindAsync(Issue1))!.Status); // unchanged
    }

    private async Task<AccessSnapshot> AccessAsAdmin()
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(Owner, UserRole.Administrator));
    }

    public void Dispose() => _connection.Dispose();
}
