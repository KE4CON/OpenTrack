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

/// <summary>Email-to-ticket routing: derive the project key from the recipient address, via
/// sub-addressing ("tickets+WEB@…") or the whole local part ("web@…").</summary>
public class EmailRoutingTests
{
    [Theory]
    [InlineData("tickets+WEB@example.com", "WEB")]
    [InlineData("tickets+web@example.com", "WEB")]          // normalized to upper
    [InlineData("web@example.com", "WEB")]                  // whole local part
    [InlineData("\"Support\" <tickets+WEB@example.com>", "WEB")] // display-name form
    [InlineData("TICKETS+web-app@example.com", "WEBAPP")]     // punctuation stripped by NormalizeKey
    public void ProjectKeyFromRecipient_DerivesKey(string recipient, string expected)
    {
        Assert.Equal(expected, EmailRouting.ProjectKeyFromRecipient(recipient));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("tickets+@example.com")]   // empty sub-address token → no key
    [InlineData("+++@example.com")]        // nothing alphanumeric
    public void ProjectKeyFromRecipient_NullWhenNoKey(string? recipient)
    {
        Assert.Null(EmailRouting.ProjectKeyFromRecipient(recipient));
    }

    [Fact]
    public void SplitFromAddress_UnwrapsNameAndAddress()
    {
        Assert.Equal(("Alice Smith", "alice@example.com"), EmailRouting.SplitFromAddress("Alice Smith <alice@example.com>"));
        Assert.Equal(("Alice", "alice@example.com"), EmailRouting.SplitFromAddress("\"Alice\" <alice@example.com>"));
        Assert.Equal((null, "bob@example.com"), EmailRouting.SplitFromAddress("bob@example.com"));   // bare address
        Assert.Equal((null, "bob@example.com"), EmailRouting.SplitFromAddress("  bob@example.com "));  // trimmed
        Assert.Equal(("No Addr", (string?)null), EmailRouting.SplitFromAddress("No Addr <>")); // empty angle brackets
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SplitFromAddress_NullPairWhenBlank(string? from)
    {
        Assert.Equal(((string?)null, (string?)null), EmailRouting.SplitFromAddress(from));
    }
}
