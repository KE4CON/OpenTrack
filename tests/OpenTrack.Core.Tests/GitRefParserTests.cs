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

using OpenTrack.Core.Git;

namespace OpenTrack.Core.Tests;

/// <summary>The commit-message reference parser: plain #mentions vs closing keywords, de-duplication with
/// closing-wins, and rejection of glued/invalid numbers.</summary>
public class GitRefParserTests
{
    [Fact]
    public void PlainMention_IsNotClosing()
    {
        var refs = GitRefParser.Parse("Working toward #42, see also the notes");
        Assert.Equal(new[] { new GitIssueRef(42, false) }, refs);
    }

    [Theory]
    [InlineData("fixes #42")]
    [InlineData("Fixed #42")]
    [InlineData("close #42")]
    [InlineData("Closes: #42")]
    [InlineData("resolve #42")]
    [InlineData("RESOLVED #42")]
    public void ClosingKeywords_MarkClosing(string message)
    {
        var refs = GitRefParser.Parse(message);
        Assert.Equal(new[] { new GitIssueRef(42, true) }, refs);
    }

    [Fact]
    public void MultipleIssues_MixedIntent()
    {
        var refs = GitRefParser.Parse("Refactor for #10; fixes #11 and closes #12").OrderBy(r => r.IssueId).ToList();
        Assert.Equal(new[]
        {
            new GitIssueRef(10, false),
            new GitIssueRef(11, true),
            new GitIssueRef(12, true),
        }, refs);
    }

    [Fact]
    public void SameIssueBothWays_ClosingWins()
    {
        var refs = GitRefParser.Parse("mentions #5 and later fixes #5");
        Assert.Equal(new[] { new GitIssueRef(5, true) }, refs);
    }

    [Fact]
    public void GluedOrInvalid_AreIgnored()
    {
        Assert.Empty(GitRefParser.Parse("commit abc#12 in v1.2#3"));   // glued to a word char
        Assert.Empty(GitRefParser.Parse("no refs here"));
        Assert.Empty(GitRefParser.Parse(""));
        Assert.Empty(GitRefParser.Parse(null));
    }
}
