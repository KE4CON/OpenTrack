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
using OpenTrack.Infrastructure;
using OpenTrack.Infrastructure.Backup;

namespace OpenTrack.Web.Services;

/// <summary>
/// Periodically writes a consistent snapshot of the SQLite database to the backup directory and prunes to
/// the configured retention. Uses SQLite's <c>VACUUM INTO</c>, which produces a clean, self-contained copy
/// safely while the app keeps running. Disabled unless OpenTrack:Backup:Enabled is true. Best-effort: any
/// failure is logged and the loop continues.
/// </summary>
public sealed class BackupScheduler(IConfiguration config, BackupOptions options, ILogger<BackupScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled) return; // opt-in; nothing to do

        // Small startup delay so the DB/migration is ready before the first snapshot.
        if (!await DelaySafe(TimeSpan.FromMinutes(1), stoppingToken)) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try { BackupOnce(stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogWarning(ex, "Scheduled backup failed; will retry next interval."); }

            if (!await DelaySafe(options.Interval, stoppingToken)) break;
        }
    }

    private void BackupOnce(CancellationToken ct)
    {
        var connString = config.ResolveOpenTrackConnectionString();
        var dbPath = new SqliteConnectionStringBuilder(connString).DataSource;

        var dir = string.IsNullOrWhiteSpace(options.Directory)
            ? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(dbPath)) ?? ".", "backups")
            : options.Directory;
        Directory.CreateDirectory(dir);

        var dest = Path.Combine(dir, BackupRetention.FileName(DateTime.UtcNow));

        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            // VACUUM INTO takes a string literal, not a parameter; escape single quotes in the path.
            cmd.CommandText = $"VACUUM INTO '{dest.Replace("'", "''")}'";
            cmd.ExecuteNonQuery();
        }
        ct.ThrowIfCancellationRequested();

        // Prune older backups beyond the retention count.
        var names = Directory.GetFiles(dir).Select(Path.GetFileName).Where(n => n is not null).Cast<string>();
        foreach (var name in BackupRetention.SelectForDeletion(names, options.Retention))
        {
            try { File.Delete(Path.Combine(dir, name)); }
            catch (Exception ex) { logger.LogWarning(ex, "Couldn't prune old backup {File}.", name); }
        }

        logger.LogInformation("Wrote database backup {File}.", dest);
    }

    private static async Task<bool> DelaySafe(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); return true; }
        catch (OperationCanceledException) { return false; }
    }
}
