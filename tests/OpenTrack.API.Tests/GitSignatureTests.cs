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

using System.Security.Cryptography;
using System.Text;
using OpenTrack.Infrastructure.Git;

namespace OpenTrack.API.Tests;

/// <summary>The GitHub webhook signature check: accepts a correct sha256 HMAC of the exact body, rejects a
/// wrong secret, tampered body, missing/garbled header, and empty secret.</summary>
public class GitSignatureTests
{
    private static string Sign(string secret, byte[] body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return "sha256=" + Convert.ToHexStringLower(hmac.ComputeHash(body));
    }

    [Fact]
    public void ValidSignature_IsAccepted()
    {
        var body = Encoding.UTF8.GetBytes("""{"commits":[]}""");
        Assert.True(GitSignature.IsValid("s3cret", body, Sign("s3cret", body)));
    }

    [Fact]
    public void WrongSecret_IsRejected()
    {
        var body = Encoding.UTF8.GetBytes("payload");
        Assert.False(GitSignature.IsValid("s3cret", body, Sign("different", body)));
    }

    [Fact]
    public void TamperedBody_IsRejected()
    {
        var sig = Sign("s3cret", Encoding.UTF8.GetBytes("original"));
        Assert.False(GitSignature.IsValid("s3cret", Encoding.UTF8.GetBytes("tampered"), sig));
    }

    [Fact]
    public void MissingOrMalformedHeader_IsRejected()
    {
        var body = Encoding.UTF8.GetBytes("x");
        Assert.False(GitSignature.IsValid("s3cret", body, null));
        Assert.False(GitSignature.IsValid("s3cret", body, ""));
        Assert.False(GitSignature.IsValid("s3cret", body, "deadbeef"));            // no sha256= prefix
        Assert.False(GitSignature.IsValid("s3cret", body, "sha256=nothex"));
    }

    [Fact]
    public void EmptySecret_IsRejected()
    {
        var body = Encoding.UTF8.GetBytes("x");
        Assert.False(GitSignature.IsValid(null, body, Sign("s3cret", body)));
        Assert.False(GitSignature.IsValid("", body, Sign("s3cret", body)));
    }
}
