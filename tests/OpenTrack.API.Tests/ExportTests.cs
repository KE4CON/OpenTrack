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
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Export;

namespace OpenTrack.API.Tests;

/// <summary>
/// Export must never widen visibility: the CSV and JSON contain only issues the requester could open
/// directly, private notes never appear for someone who couldn't see them, and a project the requester
/// can't view exports nothing.
/// </summary>
public sealed class ExportTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int Owner = 1, Viewer = 2;
    private const int PubProject = 1, PrivProject = 2;

    public ExportTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.AddRange(
            new Project { Id = PubProject, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = PrivProject, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.Issues.AddRange(
            new Issue { Id = 10, ProjectId = PubProject, Title = "Visible bug", Description = "d", ReporterId = Owner },
            new Issue { Id = 11, ProjectId = PubProject, Title = "Secret bug", Description = "d", IsPrivate = true, ReporterId = Owner },
            new Issue { Id = 20, ProjectId = PrivProject, Title = "Hidden bug", Description = "d", ReporterId = Owner });
        db.IssueNotes.AddRange(
            new IssueNote { IssueId = 10, AuthorId = Owner, Text = "public note", IsPrivate = false, CreatedAt = DateTime.UtcNow },
            new IssueNote { IssueId = 10, AuthorId = Owner, Text = "hush hush", IsPrivate = true, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Csv_ExcludesIssuesTheViewerCannotSee()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var csv = await ExportBuilder.BuildIssuesCsvAsync(db, viewer, projectId: null);

        Assert.Contains("Visible bug", csv);
        Assert.DoesNotContain("Secret bug", csv);   // private issue in a public project
        Assert.DoesNotContain("Hidden bug", csv);   // issue in a private project
    }

    [Fact]
    public async Task ProjectJson_OmitsPrivateNotes_ForANonPrivilegedViewer()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var json = await ExportBuilder.BuildProjectJsonAsync(db, viewer, PubProject);

        Assert.NotNull(json);
        Assert.Contains("public note", json);
        Assert.DoesNotContain("hush hush", json);    // private note must not leak
        Assert.DoesNotContain("Secret bug", json!);  // private issue must not leak
    }

    [Fact]
    public async Task ProjectJson_IsNullForAProjectTheViewerCannotSee()
    {
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        Assert.Null(await ExportBuilder.BuildProjectJsonAsync(db, viewer, PrivProject));
    }

    [Fact]
    public async Task Admin_JsonIncludesEverything()
    {
        var admin = await Access(Owner, UserRole.Administrator);
        await using var db = new AppDbContext(_options);
        var json = await ExportBuilder.BuildProjectJsonAsync(db, admin, PubProject);
        Assert.Contains("Secret bug", json!);
        Assert.Contains("hush hush", json!);
    }

    public void Dispose() => _connection.Dispose();
}
