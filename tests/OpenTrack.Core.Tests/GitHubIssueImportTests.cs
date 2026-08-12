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

/// <summary>The GitHub Issues JSON importer: field mapping, state→status, labels→tags, PR skipping, the
/// object-wrapped variant, and rejection of non-JSON.</summary>
public class GitHubIssueImportTests
{
    [Fact]
    public void MapsIssueFields_AndLabels()
    {
        var json = """
        [
          { "number": 42, "title": "Bug", "body": "broke", "state": "open",
            "user": { "login": "octocat" }, "assignee": { "login": "hubot" },
            "labels": [ { "name": "bug" }, { "name": "ui" } ], "created_at": "2026-01-02T03:04:05Z" }
        ]
        """;
        var i = Assert.Single(GitHubIssueImport.Parse(json));
        Assert.Equal("Bug", i.Title);
        Assert.Equal("broke", i.Description);
        Assert.Equal(IssueStatus.New, i.Status);        // open → New
        Assert.Equal("octocat", i.Reporter);
        Assert.Equal("hubot", i.Assignee);
        Assert.Equal(new[] { "bug", "ui" }, i.Tags);
        Assert.Equal("42", i.ExternalId);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), i.CreatedAt);
    }

    [Fact]
    public void ClosedState_MapsToClosed_AndAssigneesFallback()
    {
        var json = """
        [ { "number": 7, "title": "Done", "state": "closed", "assignees": [ { "login": "dev1" } ] } ]
        """;
        var i = Assert.Single(GitHubIssueImport.Parse(json));
        Assert.Equal(IssueStatus.Closed, i.Status);
        Assert.Equal("dev1", i.Assignee);   // falls back to assignees[0] when assignee is absent
    }

    [Fact]
    public void PullRequests_AreSkipped()
    {
        var json = """
        [
          { "number": 1, "title": "Real issue", "state": "open" },
          { "number": 2, "title": "A PR", "state": "open", "pull_request": { "url": "x" } }
        ]
        """;
        var i = Assert.Single(GitHubIssueImport.Parse(json));
        Assert.Equal("Real issue", i.Title);
    }

    [Fact]
    public void ObjectWrappedArray_IsAccepted()
    {
        var json = """{ "issues": [ { "number": 3, "title": "Wrapped", "state": "open" } ] }""";
        Assert.Equal("Wrapped", Assert.Single(GitHubIssueImport.Parse(json)).Title);
    }

    [Fact]
    public void InvalidJson_Throws()
    {
        Assert.Throws<FormatException>(() => GitHubIssueImport.Parse("not json"));
        Assert.Throws<FormatException>(() => GitHubIssueImport.Parse("""{ "no": "issues" }"""));
    }
}
