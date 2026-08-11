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

using OpenTrack.Core.Checklists;

namespace OpenTrack.Core.Tests;

/// <summary>The pasted-checklist parser — headings become areas; bullets, task-checkboxes, numbers,
/// and plain lines all become items with the marker stripped.</summary>
public class ChecklistImportTests
{
    [Fact]
    public void Parse_HeadingsBecomeAreas_MarkersStripped()
    {
        var text = """
            # Concurrency
            - [ ] Message store is thread-safe
            - [x] Geofence service is locked
            ## RF identity
            * TX blocked on N0CALL
            1. Callsign validated on transmit
            plain line item
            """;
        var items = ChecklistImport.Parse(text);

        Assert.Equal(5, items.Count);
        Assert.Equal(("Concurrency", "Message store is thread-safe"), (items[0].Area, items[0].Title));
        Assert.Equal(("Concurrency", "Geofence service is locked"), (items[1].Area, items[1].Title));
        Assert.Equal(("RF identity", "TX blocked on N0CALL"), (items[2].Area, items[2].Title));
        Assert.Equal(("RF identity", "Callsign validated on transmit"), (items[3].Area, items[3].Title));
        Assert.Equal(("RF identity", "plain line item"), (items[4].Area, items[4].Title));
    }

    [Fact]
    public void Parse_ItemsBeforeAnyHeading_HaveNullArea()
    {
        var items = ChecklistImport.Parse("- first\n- second");
        Assert.All(items, i => Assert.Null(i.Area));
        Assert.Equal(2, items.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("# Only a heading\n\n## Another heading")]
    public void Parse_NoItems_ReturnsEmpty(string text)
    {
        Assert.Empty(ChecklistImport.Parse(text));
    }

    [Fact]
    public void Parse_HandlesCrlf_AndBlankLines()
    {
        var items = ChecklistImport.Parse("# A\r\n\r\n- one\r\n\r\n- two\r\n");
        Assert.Equal(2, items.Count);
        Assert.Equal("A", items[0].Area);
    }
}
