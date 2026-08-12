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
using OpenTrack.Core.Sla;

namespace OpenTrack.Core.Tests;

/// <summary>The pure SLA calculator: on-track before 80% of the target, at-risk from 80% to the deadline,
/// breached past it; and NotTracked when there's no target or the clock has stopped.</summary>
public class SlaCalculatorTests
{
    private static readonly DateTime Created = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static SlaAssessment Eval(int? targetHours, double hoursElapsed, bool isOpen = true) =>
        SlaCalculator.Evaluate(IssuePriority.High, Created, isOpen, Created.AddHours(hoursElapsed), targetHours);

    [Fact]
    public void NoTarget_IsNotTracked()
    {
        Assert.Equal(SlaStatus.NotTracked, Eval(null, 100).Status);
        Assert.Equal(SlaStatus.NotTracked, Eval(0, 100).Status);
        Assert.Null(Eval(null, 100).DueUtc);
    }

    [Fact]
    public void ClosedIssue_IsNotTracked_EvenIfPastDeadline()
    {
        // 24h target, 100h elapsed, but the issue is no longer open → clock stopped.
        Assert.Equal(SlaStatus.NotTracked, Eval(24, 100, isOpen: false).Status);
    }

    [Fact]
    public void OnTrack_BeforeAtRiskThreshold()
    {
        // 10h target → at-risk at 8h. At 5h we're comfortably on track.
        var a = Eval(10, 5);
        Assert.Equal(SlaStatus.OnTrack, a.Status);
        Assert.Equal(Created.AddHours(10), a.DueUtc);
    }

    [Fact]
    public void AtRisk_FromEightyPercentUntilDeadline()
    {
        // 10h target → at-risk window is [8h, 10h).
        Assert.Equal(SlaStatus.AtRisk, Eval(10, 8).Status);   // exactly at 80%
        Assert.Equal(SlaStatus.AtRisk, Eval(10, 9.5).Status);
    }

    [Fact]
    public void Breached_AtOrPastDeadline()
    {
        Assert.Equal(SlaStatus.Breached, Eval(10, 10).Status);   // exactly at the deadline
        Assert.Equal(SlaStatus.Breached, Eval(10, 50).Status);
    }

    [Fact]
    public void IsOpen_StopsAtResolved()
    {
        Assert.True(SlaCalculator.IsOpen(IssueStatus.New));
        Assert.True(SlaCalculator.IsOpen(IssueStatus.Assigned));
        Assert.False(SlaCalculator.IsOpen(IssueStatus.Resolved));
        Assert.False(SlaCalculator.IsOpen(IssueStatus.Closed));
    }
}
