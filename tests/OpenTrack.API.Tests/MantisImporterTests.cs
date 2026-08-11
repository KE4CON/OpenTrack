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
using OpenTrack.Infrastructure.Import;

namespace OpenTrack.API.Tests;

/// <summary>The MantisBT importer: Manager-gated, creates projects/issues/notes/tags with fields mapped
/// by their shared enum ids, matches Mantis users to OpenTrack accounts where possible, preserves
/// dates and privacy, and matches (not duplicates) an existing project on re-import.</summary>
public sealed class MantisImporterTests : IDisposable
{
    private const string Xml = """
        <mantis version="2.25.0">
          <issue>
            <id>42</id>
            <project id="1">Rocket</project>
            <reporter id="9">alice</reporter>
            <handler id="2">bob</handler>
            <priority id="40">high</priority>
            <severity id="70">crash</severity>
            <status id="50">assigned</status>
            <category id="2">Engine</category>
            <date_submitted>1609459200</date_submitted>
            <last_updated>1609545600</last_updated>
            <view_state id="50">private</view_state>
            <summary>Engine explodes</summary>
            <description>Boom.</description>
            <sticky>1</sticky>
            <notes><note><reporter id="9">alice</reporter><date_submitted>1609460000</date_submitted><note>Looking into it.</note></note></notes>
            <tags><tag id="1">urgent</tag></tags>
          </issue>
        </mantis>
        """;

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Importer = 1, Bob = 2;

    public MantisImporterTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Importer, UserName = "admin" },
            new User { Id = Bob, UserName = "bob" });  // handler maps; "alice" does not exist
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task Import_IsManagerGated()
    {
        var updater = await Access(Importer, UserRole.Updater);
        await using var db = new AppDbContext(_options);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => MantisImporter.ImportAsync(db, updater, Xml));
    }

    [Fact]
    public async Task Import_CreatesMappedData()
    {
        var admin = await Access(Importer, UserRole.Administrator);
        MantisImportSummary summary;
        await using (var db = new AppDbContext(_options))
            summary = await MantisImporter.ImportAsync(db, admin, Xml);

        Assert.Equal(1, summary.ProjectsCreated);
        Assert.Equal(1, summary.IssuesImported);
        Assert.Equal(1, summary.NotesImported);
        Assert.Equal(1, summary.TagsLinked);

        await using var check = new AppDbContext(_options);
        var project = await check.Projects.FirstAsync(p => p.Name == "Rocket");
        Assert.False(project.IsPublic);                          // imported projects default private
        Assert.Equal(Importer, project.OwnerId);

        var issue = await check.Issues.Include(i => i.Notes).Include(i => i.IssueTags).ThenInclude(t => t.Tag)
            .Include(i => i.History).FirstAsync();
        Assert.Equal("Engine explodes", issue.Title);
        Assert.Equal(IssueStatus.Assigned, issue.Status);       // mapped by id
        Assert.Equal(IssueSeverity.Crash, issue.Severity);
        Assert.Equal(IssuePriority.High, issue.Priority);
        Assert.True(issue.IsPrivate);
        Assert.True(issue.IsSticky);
        Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), issue.CreatedAt); // date preserved
        Assert.Equal(Bob, issue.AssigneeId);                    // handler "bob" matched an account
        Assert.Equal(Importer, issue.ReporterId);               // "alice" unknown → importer
        Assert.Contains("originally reported by \"alice\"", issue.Description);
        Assert.Equal("urgent", Assert.Single(issue.IssueTags).Tag.Name);
        Assert.Single(issue.Notes);
        Assert.Single(issue.History);
    }

    [Fact]
    public async Task ReImport_MatchesProject_AndSkipsAlreadyImportedIssues()
    {
        var admin = await Access(Importer, UserRole.Administrator);
        await using (var db = new AppDbContext(_options))
            await MantisImporter.ImportAsync(db, admin, Xml);
        MantisImportSummary second;
        await using (var db = new AppDbContext(_options))
            second = await MantisImporter.ImportAsync(db, admin, Xml);

        Assert.Equal(0, second.ProjectsCreated);
        Assert.Equal(1, second.ProjectsMatched);
        Assert.Equal(0, second.IssuesImported);   // duplicate-safe: nothing re-imported
        Assert.Equal(1, second.IssuesSkipped);    // the already-imported issue is skipped

        await using var check = new AppDbContext(_options);
        Assert.Equal(1, await check.Projects.CountAsync(p => p.Name == "Rocket"));  // still one project
        Assert.Equal(1, await check.Issues.CountAsync());                          // and NOT duplicated
    }

    public void Dispose() => _connection.Dispose();
}
