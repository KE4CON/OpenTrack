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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTrack.Core.Entities;
using OpenTrack.Infrastructure.Email;

namespace OpenTrack.API.Tests;

/// <summary>
/// Tests the SMTP email sender's configuration gate: it only counts as configured when explicitly
/// enabled AND given a host, and an UNconfigured sender must not throw (it logs instead) so a
/// self-hosted install with no mail server still works.
/// </summary>
public sealed class EmailSenderTests
{
    private static SmtpEmailSender Sender(bool enabled, string host)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTrack:Email:Enabled"] = enabled.ToString(),
                ["OpenTrack:Email:Host"] = host,
            })
            .Build();
        return new SmtpEmailSender(config, NullLogger<SmtpEmailSender>.Instance);
    }

    [Theory]
    [InlineData(false, "smtp.example.com", false)] // disabled -> not configured
    [InlineData(true, "", false)]                  // no host  -> not configured
    [InlineData(true, "smtp.example.com", true)]   // both     -> configured
    public void IsConfigured_RequiresEnabledAndHost(bool enabled, string host, bool expected)
        => Assert.Equal(expected, Sender(enabled, host).IsConfigured);

    [Fact]
    public async Task Unconfigured_SendDoesNotThrow()
    {
        var sender = Sender(enabled: false, host: "");
        var user = new User { Email = "person@example.com", UserName = "person@example.com" };
        // Logs instead of sending; must complete without throwing so registration isn't blocked.
        await sender.SendConfirmationLinkAsync(user, "person@example.com", "http://localhost/confirm?x=1");
    }
}
