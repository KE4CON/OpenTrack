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

namespace OpenTrack.API.Tests;

/// <summary>
/// Regression test for audit finding M2 (silent lost updates). Proves the RowVersion concurrency
/// token actually rejects a stale write: two editors load the same issue, the first save wins, and
/// the second — carrying the now-outdated token it loaded — is rejected with
/// DbUpdateConcurrencyException instead of silently overwriting the first editor's change.
/// (The data services translate this into ConcurrencyConflictException / HTTP 409.)
/// </summary>
public sealed class ConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public ConcurrencyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.Add(new User { Id = 1, UserName = "u" });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = 1 });
        db.Issues.Add(new Issue { Id = 1, ProjectId = 1, Title = "t", Description = "d", ReporterId = 1 });
        db.SaveChanges();
    }

    private AppDbContext New() => new(_options);

    [Fact]
    public async Task StaleUpdate_IsRejected()
    {
        // Both editors load the same issue and its token.
        Guid tokenSeenByBoth;
        await using (var read = New())
            tokenSeenByBoth = (await read.Issues.AsNoTracking().FirstAsync(i => i.Id == 1)).RowVersion;

        // Editor A saves first — succeeds, token rotates.
        await using (var editorA = New())
        {
            var issue = await editorA.Issues.FirstAsync(i => i.Id == 1);
            editorA.Entry(issue).Property(i => i.RowVersion).OriginalValue = tokenSeenByBoth;
            issue.Title = "changed by A";
            issue.RowVersion = Guid.NewGuid();
            await editorA.SaveChangesAsync();
        }

        // Editor B submits with the STALE token it loaded earlier — must be rejected.
        await using var editorB = New();
        var issueB = await editorB.Issues.FirstAsync(i => i.Id == 1);
        editorB.Entry(issueB).Property(i => i.RowVersion).OriginalValue = tokenSeenByBoth;
        issueB.Title = "changed by B";
        issueB.RowVersion = Guid.NewGuid();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => editorB.SaveChangesAsync());

        // A's change is the one that stuck.
        await using var verify = New();
        Assert.Equal("changed by A", (await verify.Issues.FirstAsync(i => i.Id == 1)).Title);
    }

    [Fact]
    public async Task NonConflictingUpdate_Succeeds()
    {
        await using var db = New();
        var issue = await db.Issues.FirstAsync(i => i.Id == 1);
        db.Entry(issue).Property(i => i.RowVersion).OriginalValue = issue.RowVersion;
        issue.Title = "fresh edit";
        issue.RowVersion = Guid.NewGuid();
        await db.SaveChangesAsync(); // no concurrent change -> succeeds
        Assert.Equal("fresh edit", (await New().Issues.FirstAsync(i => i.Id == 1)).Title);
    }

    public void Dispose() => _connection.Dispose();
}
