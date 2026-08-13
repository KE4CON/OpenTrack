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

using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenTrack.Infrastructure.Backup;

/// <summary>
/// Naming and retention rules for database backup files. Backups are named
/// <c>opentrack-yyyyMMdd-HHmmss.db</c> so a plain lexical sort is also a chronological sort, which
/// keeps the prune logic simple and dependency-free.
/// </summary>
public static partial class BackupRetention
{
    /// <summary>Builds the backup file name for a given (UTC) timestamp.</summary>
    public static string FileName(DateTime timestampUtc) =>
        string.Format(CultureInfo.InvariantCulture, "opentrack-{0:yyyyMMdd-HHmmss}.db", timestampUtc);

    /// <summary>True if <paramref name="name"/> is one of our timestamped backup files.</summary>
    public static bool IsBackup(string name) => BackupNamePattern().IsMatch(name);

    /// <summary>
    /// Given a set of file names, returns the backups that should be deleted to keep only the newest
    /// <paramref name="retention"/>. Non-backup files are ignored (never returned). A retention of
    /// zero or less keeps everything. Returned oldest-first.
    /// </summary>
    public static IReadOnlyList<string> SelectForDeletion(IEnumerable<string> names, int retention)
    {
        // Keep everything: nothing to prune.
        if (retention <= 0) return [];

        var backups = names
            .Where(IsBackup)
            .OrderByDescending(n => n, StringComparer.Ordinal) // newest first (lexical == chronological)
            .ToList();

        if (backups.Count <= retention) return [];

        return backups
            .Skip(retention)                              // drop the newest N we're keeping
            .OrderBy(n => n, StringComparer.Ordinal)      // return the doomed ones oldest-first
            .ToList();
    }

    [GeneratedRegex(@"^opentrack-\d{8}-\d{6}\.db$", RegexOptions.CultureInvariant)]
    private static partial Regex BackupNamePattern();
}
