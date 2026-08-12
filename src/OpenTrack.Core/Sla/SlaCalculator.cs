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

using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Sla;

/// <summary>An issue's standing against its service-level target.</summary>
public enum SlaStatus
{
    /// <summary>No SLA target applies (no policy for this priority, or the issue's clock has stopped).</summary>
    NotTracked = 0,
    /// <summary>Comfortably within target.</summary>
    OnTrack = 1,
    /// <summary>Past the at-risk threshold but not yet past the deadline.</summary>
    AtRisk = 2,
    /// <summary>Past the deadline and still open.</summary>
    Breached = 3,
}

/// <summary>The computed SLA standing of one issue: its status and (when tracked) its due-by instant.</summary>
public readonly record struct SlaAssessment(SlaStatus Status, DateTime? DueUtc)
{
    public static readonly SlaAssessment NotTracked = new(SlaStatus.NotTracked, null);
    public bool IsBreached => Status == SlaStatus.Breached;
    public bool IsAtRisk => Status == SlaStatus.AtRisk;
}

public static class SlaDefaults
{
    /// <summary>Fraction of the target elapsed at which an open issue is flagged "at risk" (0.8 = 80%).</summary>
    public const double AtRiskFraction = 0.8;
}

/// <summary>
/// Pure SLA evaluation: given an issue's priority, when it was created, whether it's still open, and the
/// resolution-time target (hours) configured for that priority, decide whether it is on-track, at-risk, or
/// breached. v1 tracks the clock for OPEN issues only — a resolved/closed issue reports NotTracked (its
/// clock has stopped). Kept in Core so the background scanner, the data layer, and the tests all agree.
/// </summary>
public static class SlaCalculator
{
    public static SlaAssessment Evaluate(
        IssuePriority priority, DateTime createdAtUtc, bool isOpen, DateTime nowUtc, int? targetHours)
    {
        // No target for this priority, or the clock has stopped → nothing to track.
        if (!isOpen || targetHours is not > 0) return SlaAssessment.NotTracked;

        var dueUtc = createdAtUtc.AddHours(targetHours.Value);
        var atRiskUtc = createdAtUtc.AddHours(targetHours.Value * SlaDefaults.AtRiskFraction);

        var status = nowUtc >= dueUtc ? SlaStatus.Breached
            : nowUtc >= atRiskUtc ? SlaStatus.AtRisk
            : SlaStatus.OnTrack;
        return new SlaAssessment(status, dueUtc);
    }

    /// <summary>True for statuses whose SLA clock is still running (below Resolved).</summary>
    public static bool IsOpen(IssueStatus status) => (int)status < (int)IssueStatus.Resolved;
}
