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

/// <summary>Human-friendly ticket numbers: "WEB-42" when a project has a key, "#42" otherwise; and a
/// lenient reference parser that accepts the raw number, the "#42" form, and the "WEB-42" form.</summary>
public class TicketNumberTests
{
    [Fact]
    public void Format_UsesKeyWhenPresent_ElseHash()
    {
        Assert.Equal("WEB-42", TicketNumber.Format("WEB", 42));
        Assert.Equal("WEB-42", TicketNumber.Format("web", 42));   // normalized to upper
        Assert.Equal("#42", TicketNumber.Format(null, 42));
        Assert.Equal("#42", TicketNumber.Format("   ", 42));
        Assert.Equal("#42", TicketNumber.Format("!!", 42));         // no letters/digits → no key
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("#42", 42)]
    [InlineData("WEB-42", 42)]
    [InlineData("web-42", 42)]
    [InlineData("  WEB-7 ", 7)]
    public void TryParseId_AcceptsAllReferenceForms(string input, int expected)
    {
        Assert.True(TicketNumber.TryParseId(input, out var id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("WEB-")]
    [InlineData("no number here")]
    [InlineData(null)]
    public void TryParseId_FailsWithoutTrailingNumber(string? input)
    {
        Assert.False(TicketNumber.TryParseId(input, out var id));
        Assert.Equal(0, id);
    }

    [Fact]
    public void NormalizeKey_UppercasesStripsAndCaps()
    {
        Assert.Equal("WEB", TicketNumber.NormalizeKey("web"));
        Assert.Equal("WEBAPP", TicketNumber.NormalizeKey("web-app"));      // punctuation stripped
        Assert.Null(TicketNumber.NormalizeKey("  "));
        Assert.Null(TicketNumber.NormalizeKey("---"));
        Assert.Equal(TicketNumber.MaxKeyLength, TicketNumber.NormalizeKey(new string('A', 50))!.Length); // capped
    }
}
