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
using OpenTrack.Core.Import;

namespace OpenTrack.Core.Tests;

/// <summary>Parsing a MantisBT XML export. The coded fields map from their numeric id (Mantis and
/// OpenTrack share enum values), and notes/tags/dates/privacy come through.</summary>
public class MantisImportTests
{
    private const string Sample = """
        <?xml version="1.0" encoding="UTF-8"?>
        <mantis version="2.25.0">
          <issue>
            <id>42</id>
            <project id="1">Rocket</project>
            <reporter id="3">alice</reporter>
            <handler id="4">bob</handler>
            <priority id="40">high</priority>
            <severity id="70">crash</severity>
            <reproducibility id="10">always</reproducibility>
            <status id="50">assigned</status>
            <resolution id="10">open</resolution>
            <category id="2">Engine</category>
            <date_submitted>1609459200</date_submitted>
            <last_updated>1609545600</last_updated>
            <view_state id="10">public</view_state>
            <summary>Engine explodes on ignition</summary>
            <description>It goes boom.</description>
            <steps_to_reproduce>Press the red button.</steps_to_reproduce>
            <additional_information>Only on Tuesdays.</additional_information>
            <sticky>1</sticky>
            <notes>
              <note>
                <reporter id="3">alice</reporter>
                <view_state id="50">private</view_state>
                <date_submitted>1609460000</date_submitted>
                <note>Investigating.</note>
              </note>
            </notes>
            <tags>
              <tag id="1">urgent</tag>
              <tag id="2">propulsion</tag>
            </tags>
          </issue>
          <issue>
            <id>43</id>
            <project id="1">Rocket</project>
            <status id="90">closed</status>
            <summary>Typo in manual</summary>
          </issue>
        </mantis>
        """;

    [Fact]
    public void Parse_MapsCodedFields_FromNumericIds()
    {
        var issues = MantisImport.Parse(Sample);
        Assert.Equal(2, issues.Count);

        var i = issues[0];
        Assert.Equal(42, i.MantisId);
        Assert.Equal("Rocket", i.ProjectName);
        Assert.Equal("Engine explodes on ignition", i.Summary);
        Assert.Equal("It goes boom.", i.Description);
        Assert.Equal("Press the red button.", i.StepsToReproduce);
        Assert.Equal("Only on Tuesdays.", i.AdditionalInformation);
        Assert.Equal(IssueStatus.Assigned, i.Status);
        Assert.Equal(IssueSeverity.Crash, i.Severity);
        Assert.Equal(IssuePriority.High, i.Priority);
        Assert.Equal(IssueReproducibility.Always, i.Reproducibility);
        Assert.Equal("Engine", i.Category);
        Assert.Equal("alice", i.Reporter);
        Assert.Equal("bob", i.Handler);
        Assert.True(i.Sticky);
        Assert.False(i.Private);
        Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), i.DateSubmitted);
    }

    [Fact]
    public void Parse_ReadsTagsAndNotes_WithPrivacy()
    {
        var i = MantisImport.Parse(Sample)[0];
        Assert.Equal(new[] { "urgent", "propulsion" }, i.Tags);

        var note = Assert.Single(i.Notes);
        Assert.Equal("Investigating.", note.Text);
        Assert.True(note.Private);           // view_state id=50
        Assert.Equal("alice", note.Reporter);
    }

    [Fact]
    public void Parse_UsesDefaults_ForMissingFields()
    {
        var i = MantisImport.Parse(Sample)[1];
        Assert.Equal(IssueStatus.Closed, i.Status);
        Assert.Equal(IssueSeverity.Minor, i.Severity);      // missing → default
        Assert.Equal(IssuePriority.Normal, i.Priority);     // missing → default
        Assert.Null(i.Description);
        Assert.Empty(i.Notes);
        Assert.Empty(i.Tags);
    }

    [Fact]
    public void Parse_RejectsNonMantisXml()
    {
        Assert.Throws<FormatException>(() => MantisImport.Parse("<something><else/></something>"));
        Assert.Throws<FormatException>(() => MantisImport.Parse("not xml at all <"));
    }

    [Fact]
    public void Parse_EmptyOrBlank_ReturnsEmpty()
    {
        Assert.Empty(MantisImport.Parse(null));
        Assert.Empty(MantisImport.Parse("   "));
    }
}
