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

using OpenTrack.Infrastructure.Backup;

namespace OpenTrack.API.Tests;

/// <summary>Backup retention: keep the newest N (by the timestamped name), prune the rest, ignore
/// non-backup files, and never delete when retention is 0 (keep everything).</summary>
public class BackupRetentionTests
{
    private static readonly string[] Backups =
    [
        "opentrack-20260810-000000.db", // oldest
        "opentrack-20260811-000000.db",
        "opentrack-20260812-000000.db", // newest
    ];

    [Fact]
    public void KeepsNewest_PrunesOlder()
    {
        var toDelete = BackupRetention.SelectForDeletion(Backups, retention: 2);
        Assert.Equal(new[] { "opentrack-20260810-000000.db" }, toDelete); // only the oldest is dropped
    }

    [Fact]
    public void RetentionZero_KeepsEverything()
    {
        Assert.Empty(BackupRetention.SelectForDeletion(Backups, retention: 0));
        Assert.Empty(BackupRetention.SelectForDeletion(Backups, retention: -5));
    }

    [Fact]
    public void RetentionAtOrAboveCount_DeletesNothing()
    {
        Assert.Empty(BackupRetention.SelectForDeletion(Backups, retention: 3));
        Assert.Empty(BackupRetention.SelectForDeletion(Backups, retention: 99));
    }

    [Fact]
    public void IgnoresNonBackupFiles()
    {
        var files = new[] { "opentrack.db", "notes.txt", "opentrack-20260812-000000.db", "opentrack-20260811-000000.db" };
        var toDelete = BackupRetention.SelectForDeletion(files, retention: 1);
        Assert.Equal(new[] { "opentrack-20260811-000000.db" }, toDelete); // live DB + txt untouched
    }

    [Fact]
    public void FileName_IsSortableTimestamp()
    {
        var name = BackupRetention.FileName(new DateTime(2026, 8, 12, 1, 30, 0, DateTimeKind.Utc));
        Assert.Equal("opentrack-20260812-013000.db", name);
        Assert.True(BackupRetention.IsBackup(name));
        Assert.False(BackupRetention.IsBackup("opentrack.db"));
    }
}
