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
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Reporting;

public readonly record struct ReportBar(string Label, int Count);
public readonly record struct ReportResult(
    int TotalIssues, int OpenIssues, int ResolvedThisMonth,
    IReadOnlyList<ReportBar> CreatedByMonth,
    IReadOnlyList<ReportBar> OpenByStatus,
    IReadOnlyList<ReportBar> OpenBySeverity);

/// <summary>
/// Computes reporting figures over the issues a user may see (ACL first): headline counts, issues
/// created per month (last 6 months), and the current open breakdown by status and by severity. Shared
/// by the Web API and the web/EF data service. Kept simple — small in-memory aggregation over a lean
/// projection — which is plenty for a self-hosted tracker.
/// </summary>
public static class ReportQuery
{
    public static async Task<ReportResult> BuildAsync(
        AppDbContext db, AccessSnapshot access, int? projectId, DateTime nowUtc, CancellationToken ct = default)
    {
        var q = db.Issues.AsNoTracking().WhereVisibleTo(access);
        if (projectId is { } pid) q = q.Where(i => i.ProjectId == pid);

        var rows = await q.Select(i => new { i.CreatedAt, i.UpdatedAt, i.Status, i.Severity }).ToListAsync(ct);

        bool IsOpen(IssueStatus s) => (int)s < (int)IssueStatus.Resolved;
        var monthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var total = rows.Count;
        var open = rows.Count(r => IsOpen(r.Status));
        var resolvedThisMonth = rows.Count(r => !IsOpen(r.Status) && r.UpdatedAt >= monthStart);

        // Created per month for the last 6 calendar months (oldest → newest).
        var months = Enumerable.Range(0, 6)
            .Select(back => monthStart.AddMonths(-5 + back))
            .ToList();
        var createdByMonth = months
            .Select(m => new ReportBar(
                m.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                rows.Count(r => r.CreatedAt.Year == m.Year && r.CreatedAt.Month == m.Month)))
            .ToList();

        var openRows = rows.Where(r => IsOpen(r.Status)).ToList();
        var openByStatus = Enum.GetValues<IssueStatus>()
            .Where(s => IsOpen(s))
            .Select(s => new ReportBar(s.ToString(), openRows.Count(r => r.Status == s)))
            .Where(b => b.Count > 0)
            .ToList();
        var openBySeverity = Enum.GetValues<IssueSeverity>()
            .OrderByDescending(s => (int)s)
            .Select(s => new ReportBar(s.ToString(), openRows.Count(r => r.Severity == s)))
            .Where(b => b.Count > 0)
            .ToList();

        return new ReportResult(total, open, resolvedThisMonth, createdByMonth, openByStatus, openBySeverity);
    }
}
