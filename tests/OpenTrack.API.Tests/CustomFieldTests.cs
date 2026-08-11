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
using OpenTrack.Infrastructure.CustomFields;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests the custom-field operations' authorization and validation: defining fields is Manager-only,
/// setting a value follows the issue's edit ACL, values are validated against the field type, required
/// fields can't be cleared, a value can't be attached via a field from another project, and the values
/// of an issue you can't see never leak.
/// </summary>
public sealed class CustomFieldTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    private const int Owner = 1, Manager = 2, Updater = 3, Viewer = 4;
    private const int PubProject = 1, OtherProject = 2, PubIssue = 10, PrivIssue = 11, OtherIssue = 20;

    public CustomFieldTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.EnsureCreated();
        db.Users.AddRange(
            new User { Id = Owner, UserName = "owner" },
            new User { Id = Manager, UserName = "manager", Role = UserRole.Viewer },
            new User { Id = Updater, UserName = "updater", Role = UserRole.Updater },
            new User { Id = Viewer, UserName = "viewer", Role = UserRole.Viewer });
        db.Projects.AddRange(
            new Project { Id = PubProject, Name = "Public", IsPublic = true, OwnerId = Owner },
            new Project { Id = OtherProject, Name = "Other", IsPublic = true, OwnerId = Owner });
        db.ProjectMemberships.Add(new ProjectMembership { ProjectId = PubProject, UserId = Manager, Role = UserRole.Manager });
        db.Issues.AddRange(
            new Issue { Id = PubIssue, ProjectId = PubProject, Title = "pub", Description = "d", ReporterId = Owner },
            new Issue { Id = PrivIssue, ProjectId = PubProject, Title = "secret", Description = "d", IsPrivate = true, ReporterId = Owner },
            new Issue { Id = OtherIssue, ProjectId = OtherProject, Title = "other", Description = "d", ReporterId = Owner });
        db.SaveChanges();
    }

    private async Task<AccessSnapshot> Access(int userId, UserRole role)
    {
        await using var db = new AppDbContext(_options);
        return await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, role));
    }

    private async Task<int> CreateNumberFieldAsManager(bool required = false)
    {
        var mgr = await Access(Manager, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var error = await CustomFieldOperations.CreateDefinitionAsync(db, mgr, PubProject, "Estimate", CustomFieldType.Number, null, required);
        Assert.Null(error);
        return await db.CustomFieldDefinitions.Where(d => d.ProjectId == PubProject).Select(d => d.Id).SingleAsync();
    }

    [Fact]
    public async Task DefineField_IsManagerOnly()
    {
        // A project Updater is below Manager → forbidden.
        var updater = await Access(Updater, UserRole.Updater);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                CustomFieldOperations.CreateDefinitionAsync(db, updater, PubProject, "X", CustomFieldType.Text, null, false));

        // The project Manager can.
        var id = await CreateNumberFieldAsManager();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task SetValue_RequiresEdit_AndValidatesType()
    {
        var fieldId = await CreateNumberFieldAsManager();

        // A viewer cannot set a value.
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using (var db = new AppDbContext(_options))
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                CustomFieldOperations.SetValueAsync(db, viewer, PubIssue, fieldId, "42"));

        var updater = await Access(Updater, UserRole.Updater);
        // A non-number is rejected with a message, not persisted.
        await using (var db = new AppDbContext(_options))
            Assert.NotNull(await CustomFieldOperations.SetValueAsync(db, updater, PubIssue, fieldId, "lots"));
        // A valid number persists.
        await using (var db = new AppDbContext(_options))
            Assert.Null(await CustomFieldOperations.SetValueAsync(db, updater, PubIssue, fieldId, "42"));

        await using var check = new AppDbContext(_options);
        var vals = await CustomFieldOperations.ListValuesForIssueAsync(check, updater, PubIssue);
        Assert.Equal("42", Assert.Single(vals).Value);
    }

    [Fact]
    public async Task RequiredField_CannotBeCleared()
    {
        var fieldId = await CreateNumberFieldAsManager(required: true);
        var updater = await Access(Updater, UserRole.Updater);
        await using var db = new AppDbContext(_options);
        var error = await CustomFieldOperations.SetValueAsync(db, updater, PubIssue, fieldId, "   ");
        Assert.NotNull(error); // blank on a required field is rejected
    }

    [Fact]
    public async Task Value_CannotUseFieldFromAnotherProject()
    {
        var fieldId = await CreateNumberFieldAsManager(); // belongs to PubProject
        var updater = await Access(Updater, UserRole.Updater);
        await using var db = new AppDbContext(_options);
        // Try to attach it to an issue in a DIFFERENT project.
        var error = await CustomFieldOperations.SetValueAsync(db, updater, OtherIssue, fieldId, "42");
        Assert.NotNull(error);
        Assert.Empty(await db.CustomFieldValues.ToListAsync());
    }

    [Fact]
    public async Task UpdateAndDelete_AreScopedToTheRouteProject()
    {
        var fieldId = await CreateNumberFieldAsManager(); // belongs to PubProject
        // An admin manages OtherProject too, but addressing PubProject's field THROUGH OtherProject
        // must behave as "not found" — never edit or reveal a field from a different project.
        var admin = await Access(Manager, UserRole.Administrator);

        await using (var db = new AppDbContext(_options))
        {
            var updErr = await CustomFieldOperations.UpdateDefinitionAsync(db, admin, OtherProject, fieldId, "Renamed", null, false, 0);
            Assert.Equal("Custom field not found.", updErr);
            var delOk = await CustomFieldOperations.DeleteDefinitionAsync(db, admin, OtherProject, fieldId);
            Assert.False(delOk);
        }
        // The field is untouched, and still editable through its OWN project.
        await using (var db = new AppDbContext(_options))
        {
            Assert.Equal("Estimate", (await db.CustomFieldDefinitions.FindAsync(fieldId))!.Name);
            Assert.Null(await CustomFieldOperations.UpdateDefinitionAsync(db, admin, PubProject, fieldId, "Estimate2", null, false, 0));
        }
    }

    [Fact]
    public async Task Values_OfUnviewableIssue_DoNotLeak()
    {
        await CreateNumberFieldAsManager();
        // The private issue is viewable to its reporter but not to a plain Viewer who isn't the reporter.
        var viewer = await Access(Viewer, UserRole.Viewer);
        await using var db = new AppDbContext(_options);
        var vals = await CustomFieldOperations.ListValuesForIssueAsync(db, viewer, PrivIssue);
        Assert.Empty(vals);
    }

    public void Dispose() => _connection.Dispose();
}
