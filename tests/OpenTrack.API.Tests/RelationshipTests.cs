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
using OpenTrack.Infrastructure.Relationships;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests issue relationships (shared RelationshipOperations): reciprocal labels, and the security
/// properties — the related-list never leaks a private issue's existence, adding requires edit on the
/// source + a visible target, and self/duplicate links are rejected.
/// </summary>
public sealed class RelationshipTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int Owner = 1, Outsider = 99;
    private const int I1 = 1, I2 = 2, I3Private = 3;

    public RelationshipTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();

        db.Users.AddRange(new User { Id = Owner, UserName = "owner" }, new User { Id = Outsider, UserName = "outsider" });
        db.Projects.AddRange(
            new Project { Id = 1, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = 2, Name = "Private", IsPublic = false, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = 2, UserId = Owner, Role = UserRole.Manager });
        db.Issues.AddRange(
            new Issue { Id = I1, ProjectId = 1, Title = "One", Description = "d", ReporterId = Owner },
            new Issue { Id = I2, ProjectId = 1, Title = "Two", Description = "d", ReporterId = Owner },
            new Issue { Id = I3Private, ProjectId = 2, Title = "Secret", Description = "d", ReporterId = Owner });
        db.IssueRelationships.AddRange(
            new IssueRelationship { Id = 1, SourceIssueId = I1, TargetIssueId = I2, Type = IssueRelationshipType.RelatedTo, CreatedById = Owner },
            new IssueRelationship { Id = 2, SourceIssueId = I1, TargetIssueId = I3Private, Type = IssueRelationshipType.Blocks, CreatedById = Owner });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    [Fact]
    public async Task List_HidesRelatedPrivateIssueFromOutsider()
    {
        await using var db = new AppDbContext(_options);
        var items = await RelationshipOperations.ListAsync(db, await Access(Outsider, UserRole.Reporter), I1);
        // The public relationship shows; the "blocks #3 (private)" link must NOT — no existence leak.
        Assert.Contains(items, i => i.OtherIssueId == I2);
        Assert.DoesNotContain(items, i => i.OtherIssueId == I3Private);
    }

    [Fact]
    public async Task List_ShowsReciprocalLabelFromTargetSide()
    {
        await using var db = new AppDbContext(_options);
        var admin = await Access(Owner, UserRole.Administrator);
        var fromI3 = await RelationshipOperations.ListAsync(db, admin, I3Private);
        var link = Assert.Single(fromI3);
        Assert.Equal(I1, link.OtherIssueId);
        Assert.Equal("blocked by", link.Label); // reciprocal of "blocks"
    }

    [Fact]
    public async Task Add_RejectsSelfInvisibleTargetAndDuplicate()
    {
        await using var db = new AppDbContext(_options);
        var admin = await Access(Owner, UserRole.Administrator);

        Assert.NotNull(await RelationshipOperations.AddAsync(db, admin, I1, I1, IssueRelationshipType.RelatedTo));      // self
        Assert.NotNull(await RelationshipOperations.AddAsync(db, admin, I1, 999, IssueRelationshipType.RelatedTo));    // missing
        Assert.NotNull(await RelationshipOperations.AddAsync(db, admin, I2, I1, IssueRelationshipType.RelatedTo));     // duplicate (symmetric)
        Assert.Null(await RelationshipOperations.AddAsync(db, admin, I1, I2, IssueRelationshipType.Blocks));           // new type -> ok
    }

    [Fact]
    public async Task Add_InvisibleTargetReportedAsNotFound_NotAccessDenied()
    {
        await using var db = new AppDbContext(_options);
        // Grant the outsider Updater so the source-edit check passes, isolating the target-visibility rule.
        var outsider = await Access(Outsider, UserRole.Updater);
        var result = await RelationshipOperations.AddAsync(db, outsider, I1, I3Private, IssueRelationshipType.RelatedTo);
        Assert.Contains("not found", result); // never reveals that the private issue exists
    }

    [Fact]
    public async Task Remove_NoViewOfEitherIssue_ReturnsFalseNotThrow()
    {
        // A relationship between two PRIVATE issues the outsider can't see at all.
        await using (var seed = new AppDbContext(_options))
        {
            seed.Issues.Add(new Issue { Id = 4, ProjectId = 2, Title = "Secret2", Description = "d", ReporterId = Owner });
            seed.IssueRelationships.Add(new IssueRelationship { Id = 3, SourceIssueId = I3Private, TargetIssueId = 4, Type = IssueRelationshipType.RelatedTo, CreatedById = Owner });
            await seed.SaveChangesAsync();
        }

        await using var db = new AppDbContext(_options);
        // Updater globally, but NOT a member of the private project — can view neither endpoint.
        var removed = await RelationshipOperations.RemoveAsync(db, await Access(Outsider, UserRole.Updater), 3);
        Assert.False(removed); // treated as not-found; must not throw or reveal the link exists
    }

    [Fact]
    public async Task Add_RequiresEditOnSource()
    {
        await using var db = new AppDbContext(_options);
        var reporter = await Access(Outsider, UserRole.Reporter); // can view public issue, cannot edit
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            RelationshipOperations.AddAsync(db, reporter, I1, I2, IssueRelationshipType.RelatedTo));
    }

    public void Dispose() => _connection.Dispose();
}
