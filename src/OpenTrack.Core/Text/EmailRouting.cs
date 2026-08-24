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

namespace OpenTrack.Core.Text;

/// <summary>
/// Routing helpers for the email-to-ticket intake. An inbound email is filed under the project whose
/// <c>Key</c> matches the recipient address: either sub-addressing (<c>tickets+APRS@domain</c> → APRS)
/// or the whole local part (<c>aprs@domain</c> → APRS). Hand-rolled and dependency-free (the project
/// rejects MailKit/MimeKit); this only needs the address string, which inbound-parse services already
/// hand over as plain fields.
/// </summary>
public static class EmailRouting
{
    /// <summary>
    /// Extracts the project key from a recipient address. Prefers the sub-address token after a '+'
    /// (<c>tickets+APRS@…</c> → "APRS"); otherwise uses the whole local part (<c>aprs@…</c> → "APRS").
    /// Tolerates a display-name form (<c>"Support" &lt;tickets+APRS@…&gt;</c>). Returns null when no key
    /// can be derived. The result is normalized the same way project keys are stored.
    /// </summary>
    public static string? ProjectKeyFromRecipient(string? recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient)) return null;

        var addr = ExtractAddress(recipient);
        var at = addr.IndexOf('@');
        var local = at >= 0 ? addr[..at] : addr;
        if (local.Length == 0) return null;

        // Sub-addressing: take the token after the first '+'.
        var plus = local.IndexOf('+');
        var token = plus >= 0 ? local[(plus + 1)..] : local;

        return TicketNumber.NormalizeKey(token);
    }

    /// <summary>Pulls the bare address out of a header value, unwrapping a "Name &lt;addr&gt;" form.</summary>
    private static string ExtractAddress(string value)
    {
        var s = value.Trim();
        var open = s.LastIndexOf('<');
        var close = s.LastIndexOf('>');
        if (open >= 0 && close > open)
            s = s[(open + 1)..close].Trim();
        return s;
    }
}
