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
using OpenTrack.Infrastructure.Intake;

namespace OpenTrack.API.Tests;

/// <summary>The public trouble-ticket intake: submissions are only accepted when the project has intake
/// enabled; an accepted one becomes an issue attributed to the owner with the submitter captured; and a
/// status lookup only returns a ticket when the reference AND the submitter's email both match.</summary>
public sealed class PublicIntakeTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private const int Owner = 1, Off = 1, On = 2;

    public PublicIntakeTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.Add(new User { Id = Owner, UserName = "owner" });
        db.Projects.AddRange(
            new Project { Id = Off, Name = "Closed", IsPublic = true, OwnerId = Owner, PublicIntakeEnabled = false },
            new Project { Id = On, Name = "Open", IsPublic = true, OwnerId = Owner, PublicIntakeEnabled = true });
        // A normal (non-intake) issue must never be lookup-able by the public.
        db.Issues.Add(new Issue { Id = 500, ProjectId = On, Title = "internal", Description = "d", ReporterId = Owner });
        db.SaveChanges();
    }

    [Fact]
    public async Task Submit_RejectedWhenIntakeDisabled()
    {
        await using var db = new AppDbContext(_options);
        var r = await PublicIntakeOperations.SubmitAsync(db, Off, "Jane", "jane@example.com", "It broke", "details");
        Assert.Null(r.IssueId);
        Assert.NotNull(r.Error);
        Assert.Equal(0, await db.Issues.CountAsync(i => i.ProjectId == Off));
    }

    [Fact]
    public async Task Submit_CreatesIssue_AttributedToOwner_WithSubmitterCaptured()
    {
        int id;
        await using (var db = new AppDbContext(_options))
        {
            var r = await PublicIntakeOperations.SubmitAsync(db, On, "Jane Doe", "jane@example.com", "Login fails", "on my phone");
            Assert.Null(r.Error);
            id = r.IssueId!.Value;
        }
        await using var check = new AppDbContext(_options);
        var issue = await check.Issues.FirstAsync(i => i.Id == id);
        Assert.Equal(Owner, issue.ReporterId);          // a valid reporter
        Assert.Equal(IssueStatus.New, issue.Status);
        Assert.Equal("Login fails", issue.Title);
        Assert.Equal("jane@example.com", issue.IntakeEmail);
        Assert.Equal("Jane Doe", issue.IntakeName);
        Assert.Contains("public", issue.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Submit_RequiresASummary()
    {
        await using var db = new AppDbContext(_options);
        Assert.NotNull((await PublicIntakeOperations.SubmitAsync(db, On, null, null, "   ", "x")).Error);
    }

    [Fact]
    public async Task Lookup_RequiresMatchingReferenceAndEmail()
    {
        int id;
        await using (var db = new AppDbContext(_options))
            id = (await PublicIntakeOperations.SubmitAsync(db, On, null, "jane@example.com", "Login fails", null)).IssueId!.Value;

        await using var check = new AppDbContext(_options);
        Assert.NotNull(await PublicIntakeOperations.LookupAsync(check, id, "JANE@example.com")); // case-insensitive match
        Assert.Null(await PublicIntakeOperations.LookupAsync(check, id, "mallory@example.com")); // wrong email
        Assert.Null(await PublicIntakeOperations.LookupAsync(check, 500, "owner"));              // non-intake issue not lookup-able
    }

    public void Dispose() => _connection.Dispose();
}
