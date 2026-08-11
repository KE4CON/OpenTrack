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

using OpenTrack.Core.Automation;
using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Tests;

/// <summary>The pure automation evaluator: condition matching (ANDed, "null = any"), action resolution
/// (last scalar wins, tags accumulate), and that non-matching rules are skipped.</summary>
public class AutomationEvaluatorTests
{
    private static AutomationRuleDef Rule(
        string name = "r",
        string? text = null, IssueSeverity? whenSev = null, IssuePriority? whenPri = null, int? whenCat = null,
        IssueSeverity? setSev = null, IssuePriority? setPri = null, IssueStatus? setStatus = null,
        int? assign = null, string? addTag = null) =>
        new(name, text, whenSev, whenPri, whenCat, setSev, setPri, setStatus, assign, addTag);

    private static AutomationInput Issue(
        string title = "Login page", string? desc = "It broke",
        IssueSeverity sev = IssueSeverity.Minor, IssuePriority pri = IssuePriority.Normal, int? cat = null) =>
        new(title, desc, sev, pri, cat);

    [Fact]
    public void RuleWithNoConditions_MatchesEveryIssue_AndAppliesActions()
    {
        var outcome = AutomationEvaluator.Evaluate(
            Issue(), [Rule(setPri: IssuePriority.High, addTag: "triage")]);

        Assert.Equal(IssuePriority.High, outcome.Priority);
        Assert.Equal(new[] { "triage" }, outcome.AddTags);
        Assert.Equal(new[] { "r" }, outcome.AppliedRuleNames);
        Assert.True(outcome.AnyEffect);
    }

    [Fact]
    public void Conditions_AreAnded()
    {
        var rule = Rule(text: "crash", whenSev: IssueSeverity.Crash, setPri: IssuePriority.Immediate);

        // Both conditions hold → applies.
        var hit = AutomationEvaluator.Evaluate(
            Issue(title: "App crash on startup", sev: IssueSeverity.Crash), [rule]);
        Assert.Equal(IssuePriority.Immediate, hit.Priority);

        // Text matches but severity doesn't → no match.
        var miss = AutomationEvaluator.Evaluate(
            Issue(title: "App crash on startup", sev: IssueSeverity.Minor), [rule]);
        Assert.Null(miss.Priority);
        Assert.Empty(miss.AppliedRuleNames);
    }

    [Fact]
    public void TextCondition_MatchesTitleOrDescription_CaseInsensitive()
    {
        var rule = Rule(text: "TIMEOUT", addTag: "net");
        Assert.Single(AutomationEvaluator.Evaluate(Issue(title: "Request timeout", desc: "x"), [rule]).AddTags);
        Assert.Single(AutomationEvaluator.Evaluate(Issue(title: "x", desc: "a Timeout occurred"), [rule]).AddTags);
        Assert.Empty(AutomationEvaluator.Evaluate(Issue(title: "x", desc: "y"), [rule]).AddTags);
    }

    [Fact]
    public void LaterScalarWins_TagsAccumulate_AndDeduplicate()
    {
        var rules = new[]
        {
            Rule("a", setPri: IssuePriority.Low, addTag: "auto"),
            Rule("b", setPri: IssuePriority.Urgent, addTag: "Auto"), // dup tag (case-insensitive), overrides priority
            Rule("c", addTag: "crash"),
        };
        var outcome = AutomationEvaluator.Evaluate(Issue(), rules);

        Assert.Equal(IssuePriority.Urgent, outcome.Priority);        // last matching scalar wins
        Assert.Equal(new[] { "auto", "crash" }, outcome.AddTags);    // "Auto" deduped against "auto"
        Assert.Equal(new[] { "a", "b", "c" }, outcome.AppliedRuleNames);
    }

    [Fact]
    public void NoMatchingRules_ProducesNoEffect()
    {
        var outcome = AutomationEvaluator.Evaluate(
            Issue(cat: 1), [Rule(whenCat: 2, setStatus: IssueStatus.Acknowledged)]);
        Assert.False(outcome.AnyEffect);
        Assert.Null(outcome.Status);
        Assert.Empty(outcome.AppliedRuleNames);
    }
}
