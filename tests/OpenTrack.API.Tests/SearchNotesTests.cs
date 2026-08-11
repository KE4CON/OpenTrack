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
using OpenTrack.Infrastructure.Queries;

namespace OpenTrack.API.Tests;

/// <summary>Text search also matches PUBLIC note text, but never private-note text (so search can't
/// reveal that a note the searcher can't read contains their term).</summary>
public sealed class SearchNotesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public SearchNotesTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.Add(new User { Id = 1, UserName = "u" });
        db.Projects.Add(new Project { Id = 1, Name = "P", IsPublic = true, OwnerId = 1 });
        db.Issues.AddRange(
            new Issue { Id = 10, ProjectId = 1, Title = "Alpha", Description = "d", ReporterId = 1 },
            new Issue { Id = 11, ProjectId = 1, Title = "Beta", Description = "d", ReporterId = 1 });
        db.IssueNotes.AddRange(
            new IssueNote { IssueId = 10, AuthorId = 1, Text = "the widget frobnicates", IsPrivate = false, CreatedAt = DateTime.UtcNow },
            new IssueNote { IssueId = 11, AuthorId = 1, Text = "secret frobnicates", IsPrivate = true, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }

    [Fact]
    public async Task Search_MatchesPublicNoteText_NotPrivate()
    {
        await using var db = new AppDbContext(_options);
        var ids = await db.Issues.AsNoTracking()
            .ApplyFilter(new IssueFilter(Text: "frobnicates"))
            .Select(i => i.Id)
            .ToListAsync();

        Assert.Contains(10, ids);        // matched via its public note
        Assert.DoesNotContain(11, ids);  // the term is only in a private note → not matched
    }

    public void Dispose() => _connection.Dispose();
}
