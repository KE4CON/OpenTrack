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
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Webhooks;

namespace OpenTrack.API.Tests;

/// <summary>Project webhooks: Manager-gated CRUD with URL validation, and a dispatcher that shapes the
/// payload per destination and fires only a project's active hooks.</summary>
public sealed class WebhookTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Manager = 2, Updater = 3;

    public WebhookTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Manager, UserName = "manager", Role = UserRole.Viewer },
            new User { Id = Updater, UserName = "updater", Role = UserRole.Updater });
        db.Projects.Add(new Project { Id = 1, Name = "Rocket", IsPublic = true, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = 1, UserId = Manager, Role = UserRole.Manager });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Add_IsManagerGated_AndValidatesUrl()
    {
        var updater = await Access(Updater, UserRole.Updater);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                WebhookOperations.AddAsync(db, updater, 1, "https://example.com/hook", WebhookFormat.Slack));

        var mgr = await Access(Manager, UserRole.Viewer);
        await using (var db = new AppDbContext(_options))
        {
            Assert.NotNull(await WebhookOperations.AddAsync(db, mgr, 1, "ftp://nope", WebhookFormat.Generic));   // bad scheme
            Assert.NotNull(await WebhookOperations.AddAsync(db, mgr, 1, "   ", WebhookFormat.Generic));           // empty
            Assert.Null(await WebhookOperations.AddAsync(db, mgr, 1, "https://hooks.slack.com/x", WebhookFormat.Slack)); // ok
        }
        await using var check = new AppDbContext(_options);
        Assert.Single(await check.ProjectWebhooks.ToListAsync());
    }

    private sealed class Capturing : HttpMessageHandler
    {
        public readonly TaskCompletionSource<(string Url, string Body)> Done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct);
            Done.TrySetResult((request.RequestUri!.ToString(), body));
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }

    [Theory]
    [InlineData(WebhookFormat.Slack, "\"text\"")]
    [InlineData(WebhookFormat.Discord, "\"content\"")]
    [InlineData(WebhookFormat.Generic, "\"event\"")]
    public async Task Dispatch_ShapesPayload_PerFormat(WebhookFormat format, string mustContain)
    {
        await using (var db = new AppDbContext(_options))
        {
            db.ProjectWebhooks.Add(new ProjectWebhook { ProjectId = 1, Url = "https://example.com/hook", Format = format, IsActive = true });
            db.ProjectWebhooks.Add(new ProjectWebhook { ProjectId = 1, Url = "https://example.com/off", Format = format, IsActive = false }); // must be ignored
            await db.SaveChangesAsync();
        }

        var handler = new Capturing();
        var dispatch = new WebhookDispatch(new HttpClient(handler), NullLogger<WebhookDispatch>.Instance);
        await using (var db = new AppDbContext(_options))
            await dispatch.DispatchAsync(db, 1, "Rocket", 42, "Engine explodes", "Assigned", "status changed");

        var completed = await Task.WhenAny(handler.Done.Task, Task.Delay(5000));
        Assert.Same(handler.Done.Task, completed); // the (active) hook fired
        var (url, body) = handler.Done.Task.Result;
        Assert.Equal("https://example.com/hook", url); // the ACTIVE hook, not the inactive one
        Assert.Contains(mustContain, body);
        Assert.Contains("Engine explodes", body);
    }

    public void Dispose() => _connection.Dispose();
}
