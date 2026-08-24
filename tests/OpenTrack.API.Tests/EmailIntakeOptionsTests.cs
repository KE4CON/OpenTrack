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

using OpenTrack.Infrastructure.Intake;

namespace OpenTrack.API.Tests;

/// <summary>The email-to-ticket feature is off unless a secret is set, and the shared-secret check is a
/// constant-time compare that a blank/empty/wrong value never satisfies.</summary>
public class EmailIntakeOptionsTests
{
    [Fact]
    public void Enabled_OnlyWhenSecretSet()
    {
        Assert.False(new EmailIntakeOptions().Enabled);
        Assert.False(new EmailIntakeOptions { Secret = "   " }.Enabled);
        Assert.True(new EmailIntakeOptions { Secret = "s3cret" }.Enabled);
    }

    [Fact]
    public void Matches_AcceptsOnlyTheExactSecret()
    {
        var opts = new EmailIntakeOptions { Secret = "s3cret" };
        Assert.True(opts.Matches("s3cret"));
        Assert.False(opts.Matches("wrong"));
        Assert.False(opts.Matches("s3cre"));    // different length
        Assert.False(opts.Matches(""));
        Assert.False(opts.Matches(null));
    }

    [Fact]
    public void Matches_AlwaysFalse_WhenDisabled()
    {
        // No secret configured → nothing can authorize, even an empty presented value.
        var off = new EmailIntakeOptions();
        Assert.False(off.Matches(""));
        Assert.False(off.Matches("anything"));
    }
}
