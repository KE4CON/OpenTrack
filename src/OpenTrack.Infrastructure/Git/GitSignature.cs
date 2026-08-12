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

namespace OpenTrack.Infrastructure.Git;

/// <summary>Verifies a GitHub webhook's <c>X-Hub-Signature-256</c> header: <c>sha256=</c> followed by the
/// hex HMAC-SHA256 of the raw request body keyed by the project's shared secret. Comparison is
/// constant-time to avoid leaking timing information.</summary>
public static class GitSignature
{
    public static bool IsValid(string? secret, byte[] body, string? signatureHeader)
    {
        if (string.IsNullOrEmpty(secret) || body is null || string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        const string prefix = "sha256=";
        if (!signatureHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;
        var provided = signatureHeader[prefix.Length..].Trim();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexStringLower(hmac.ComputeHash(body));

        // Constant-time compare of the two hex strings.
        var a = Encoding.ASCII.GetBytes(expected);
        var b = Encoding.ASCII.GetBytes(provided);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
