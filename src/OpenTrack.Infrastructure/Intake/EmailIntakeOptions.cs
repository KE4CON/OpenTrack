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

namespace OpenTrack.Infrastructure.Intake;

/// <summary>
/// Configuration for the email-to-ticket intake, bound from <c>OpenTrack:EmailIntake</c>. The feature is
/// OFF unless a <see cref="Secret"/> is set: the inbound-email poster (an inbound-parse mail service, or a
/// small forwarder that reads a mailbox) must present that shared secret, so only your own mail plumbing
/// can create tickets this way. A submission still also requires the target project to have public intake
/// enabled, so turning this on can't open a project that isn't already accepting public tickets.
/// </summary>
public sealed class EmailIntakeOptions
{
    public const string Section = "OpenTrack:EmailIntake";

    /// <summary>Shared secret the poster must present (header <c>X-OpenTrack-Secret</c> or a <c>secret</c>
    /// form field). Email intake is disabled while this is blank.</summary>
    public string? Secret { get; set; }

    public bool Enabled => !string.IsNullOrWhiteSpace(Secret);

    /// <summary>Constant-time check that a presented secret matches the configured one. Always false when
    /// the feature is disabled or the presented value is empty — so a blank secret can never authorize.</summary>
    public bool Matches(string? presented)
    {
        if (!Enabled || string.IsNullOrEmpty(presented)) return false;
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(Secret!), System.Text.Encoding.UTF8.GetBytes(presented));
    }
}
