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

namespace OpenTrack.Infrastructure.Email;

/// <summary>General transactional email, used by notifications (and internally by the Identity email
/// sender). Falls back to logging when no mail server is configured; never throws on a send failure.</summary>
public interface IEmailService
{
    /// <summary>True only when an operator has configured a mail server.</summary>
    bool IsConfigured { get; }

    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
