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

/// <summary>The CSV importer: header aliasing, quoted fields, fuzzy status/severity/priority mapping,
/// multi-column labels (Jira repeats "Labels"), and skipping title-less rows.</summary>
public class CsvIssueImportTests
{
    [Fact]
    public void MapsCommonColumns_AndQuotedFields()
    {
        var csv = "Summary,Description,Status,Priority,Severity,Assignee\n" +
                  "\"Login, broken\",\"line1\nline2\",In Progress,High,Critical,alice\n";
        var issues = CsvIssueImport.Parse(csv);
        var i = Assert.Single(issues);
        Assert.Equal("Login, broken", i.Title);          // comma inside quotes preserved
        Assert.Equal("line1\nline2", i.Description);      // newline inside quotes preserved
        Assert.Equal(IssueStatus.Confirmed, i.Status);    // "In Progress" → Confirmed
        Assert.Equal(IssuePriority.High, i.Priority);
        Assert.Equal(IssueSeverity.Crash, i.Severity);    // "Critical" → Crash
        Assert.Equal("alice", i.Assignee);
    }

    [Fact]
    public void JiraLabels_AcrossRepeatedColumns_AreCollected()
    {
        // Jira exports repeat the "Labels" header, one label per column.
        var csv = "Issue key,Summary,Labels,Labels,Component\n" +
                  "APP-1,Crash on boot,ui,startup,Engine\n";
        var i = Assert.Single(CsvIssueImport.Parse(csv));
        Assert.Equal("Crash on boot", i.Title);
        Assert.Equal("APP-1", i.ExternalId);
        Assert.Equal("Engine", i.Category);               // "Component" → category
        Assert.Equal(new[] { "ui", "startup" }, i.Tags);
    }

    [Fact]
    public void RowsWithoutTitle_AreSkipped()
    {
        var csv = "Title,Status\nReal issue,New\n,New\n\n";
        var i = Assert.Single(CsvIssueImport.Parse(csv));
        Assert.Equal("Real issue", i.Title);
    }

    [Fact]
    public void UnknownCodedValues_LeaveNullsForDefaults()
    {
        var i = Assert.Single(CsvIssueImport.Parse("Title,Priority\nX,Whatever\n"));
        Assert.Null(i.Priority);   // unmappable → null so the importer applies a default
        Assert.Null(i.Status);
    }

    [Fact]
    public void NoTitleColumn_OrEmpty_YieldsNothing()
    {
        Assert.Empty(CsvIssueImport.Parse("Foo,Bar\n1,2\n"));   // no title-like column
        Assert.Empty(CsvIssueImport.Parse("Title,Status\n"));   // header only
        Assert.Empty(CsvIssueImport.Parse(""));
    }
}
