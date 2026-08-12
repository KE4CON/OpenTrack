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

using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Core.Sla;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Services;

/// <summary>
/// Periodically escalates SLA breaches. On each tick it finds OPEN issues whose SLA target has passed and
/// that haven't been escalated yet (<see cref="Issue.SlaBreachNotifiedAt"/> is null), notifies the assignee
/// and the project's managers once, and stamps the issue so it isn't re-notified every tick.
/// <para>It's a trusted system actor with no signed-in user, so it queries issues directly (no per-user
/// ACL) and writes notifications straight to the store. Best-effort: any scan error is logged and the loop
/// continues. Uses <see cref="IDbContextFactory{TContext}"/> for a fresh short-lived context per tick.</para>
/// </summary>
public sealed class SlaScanner(IDbContextFactory<AppDbContext> dbFactory, ILogger<SlaScanner> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let startup (incl. the migration that creates the SLA table) settle before the first scan.
        if (!await DelaySafe(StartupDelay, stoppingToken)) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ScanOnceAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogWarning(ex, "SLA breach scan failed; will retry next interval."); }

            if (!await DelaySafe(Interval, stoppingToken)) break;
        }
    }

    private async Task ScanOnceAsync(CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var now = DateTime.UtcNow;

        var policies = await db.SlaPolicies.AsNoTracking().ToListAsync(ct);
        if (policies.Count == 0) return; // no project has SLA targets — nothing to do
        var targets = policies.ToDictionary(p => (p.ProjectId, p.Priority), p => p.TargetHours);
        var projectIds = policies.Select(p => p.ProjectId).Distinct().ToList();

        // Candidate open issues in SLA-enabled projects that haven't been escalated yet.
        var candidates = await db.Issues.AsNoTracking()
            .Where(i => projectIds.Contains(i.ProjectId)
                        && (int)i.Status < (int)IssueStatus.Resolved
                        && i.SlaBreachNotifiedAt == null)
            .Select(i => new { i.Id, i.ProjectId, i.Priority, i.CreatedAt, i.AssigneeId })
            .ToListAsync(ct);

        var breachedIds = new List<int>();
        foreach (var i in candidates)
        {
            targets.TryGetValue((i.ProjectId, i.Priority), out var hours);
            if (SlaCalculator.Evaluate(i.Priority, i.CreatedAt, isOpen: true, now, hours == 0 ? null : hours).IsBreached)
                breachedIds.Add(i.Id);
        }
        if (breachedIds.Count == 0) return;

        // Re-load the breached issues as tracked entities so we can stamp them, and notify recipients.
        var breached = await db.Issues.Where(i => breachedIds.Contains(i.Id)).ToListAsync(ct);
        foreach (var issue in breached)
        {
            var recipients = new HashSet<int>();
            if (issue.AssigneeId is { } aid) recipients.Add(aid);
            var managerIds = await db.ProjectMemberships.AsNoTracking()
                .Where(m => m.ProjectId == issue.ProjectId && (int)m.Role >= (int)UserRole.Manager)
                .Select(m => m.UserId).ToListAsync(ct);
            foreach (var mid in managerIds) recipients.Add(mid);

            // If there's nobody to escalate to yet (no assignee and no managers), leave the issue
            // un-stamped so it's re-evaluated next tick — otherwise a later-added assignee/manager would
            // never be notified about an already-breached issue.
            if (recipients.Count == 0) continue;

            foreach (var uid in recipients)
                db.Notifications.Add(new Notification
                {
                    UserId = uid,
                    IssueId = issue.Id,
                    Text = "SLA breached — this issue has passed its resolution target.",
                    CreatedAt = now,
                });
            issue.SlaBreachNotifiedAt = now;
        }
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SLA scan escalated {Count} newly-breached issue(s).", breached.Count);
    }

    /// <summary>Task.Delay that swallows cancellation; returns false when the host is stopping.</summary>
    private static async Task<bool> DelaySafe(TimeSpan delay, CancellationToken ct)
    {
        try { await Task.Delay(delay, ct); return true; }
        catch (OperationCanceledException) { return false; }
    }
}
