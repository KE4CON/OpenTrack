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
/// sub-addressing ("tickets+APRS@…") or the whole local part ("aprs@…").</summary>
public class EmailRoutingTests
{
    [Theory]
    [InlineData("tickets+APRS@example.com", "APRS")]
    [InlineData("tickets+aprs@example.com", "APRS")]          // normalized to upper
    [InlineData("aprs@example.com", "APRS")]                  // whole local part
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
}
