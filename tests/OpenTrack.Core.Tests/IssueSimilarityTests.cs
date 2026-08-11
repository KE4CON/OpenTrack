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

using OpenTrack.Core.Text;

namespace OpenTrack.Core.Tests;

/// <summary>Title-similarity helpers used for duplicate detection.</summary>
public class IssueSimilarityTests
{
    [Fact]
    public void SignificantWords_DropsShortWordsStopwordsAndDuplicates()
    {
        var words = IssueSimilarity.SignificantWords("The app crashes when saving the CRASH log");
        Assert.Contains("crashes", words);
        Assert.Contains("saving", words);
        Assert.Contains("log", words);
        Assert.DoesNotContain("the", words);   // stopword
        Assert.DoesNotContain("app", words);   // stopword
        Assert.DoesNotContain("when", words);  // stopword
        Assert.Single(words.Where(w => w == "crash")); // de-duplicated, case-insensitive
    }

    [Theory]
    [InlineData("Login button does nothing", "Login button unresponsive on mobile", 2)] // login, button
    [InlineData("Export to CSV fails", "Import from CSV broken", 1)]                     // csv
    [InlineData("Totally unrelated title", "Nothing in common here", 0)]
    public void Overlap_CountsSharedSignificantWords(string a, string b, int expected)
    {
        Assert.Equal(expected, IssueSimilarity.Overlap(a, b));
    }

    [Fact]
    public void Overlap_IsZero_ForEmptyOrStopwordOnly()
    {
        Assert.Equal(0, IssueSimilarity.Overlap("", "anything"));
        Assert.Equal(0, IssueSimilarity.Overlap("the and for", "the and for"));
    }
}
