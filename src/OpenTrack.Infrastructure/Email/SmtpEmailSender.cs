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

using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Entities;

namespace OpenTrack.Infrastructure.Email;

/// <summary>
/// Sends Identity emails (account confirmation, password reset) over SMTP using the framework's
/// System.Net.Mail client — deliberately NOT MailKit, whose current releases pull in a MimeKit with an
/// unpatched advisory, which we won't add to a security-audited project. This covers basic
/// transactional email, which is all OpenTrack needs.
///
/// If email isn't configured (the common trusted-LAN case), it does NOT fail — it logs the message,
/// including any confirmation/reset link, so a self-hosted install works without an SMTP server and an
/// operator can still complete flows from the log. Configure it to actually send:
///   OpenTrack:Email:{ Enabled, Host, Port, User, Password, From, UseSsl }
/// </summary>
public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    : IEmailSender<User>
{
    private readonly string? _host = configuration["OpenTrack:Email:Host"];
    private readonly int _port = int.TryParse(configuration["OpenTrack:Email:Port"], out var p) ? p : 587;
    private readonly string? _user = configuration["OpenTrack:Email:User"];
    private readonly string? _password = configuration["OpenTrack:Email:Password"];
    private readonly string _from = configuration["OpenTrack:Email:From"] is { Length: > 0 } f ? f : "no-reply@opentrack.local";
    private readonly bool _useSsl = configuration.GetValue("OpenTrack:Email:UseSsl", true);
    private readonly bool _enabled = configuration.GetValue("OpenTrack:Email:Enabled", false);

    /// <summary>True only when an operator has actually configured a mail server. When false, emails
    /// are logged rather than sent (and the register-confirmation page shows the link on screen).</summary>
    public bool IsConfigured => _enabled && !string.IsNullOrWhiteSpace(_host);

    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your OpenTrack email",
            $"Confirm your account by <a href=\"{confirmationLink}\">clicking here</a>.");

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink) =>
        SendAsync(email, "Reset your OpenTrack password",
            $"Reset your password by <a href=\"{resetLink}\">clicking here</a>.");

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode) =>
        SendAsync(email, "Reset your OpenTrack password",
            $"Your password reset code is: <strong>{resetCode}</strong>");

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!IsConfigured)
        {
            // No mail server configured — log so a self-hosted install still works (links included).
            logger.LogInformation("Email not configured; would send to {Email}. Subject: {Subject}. Body: {Body}",
                toEmail, subject, htmlBody);
            return;
        }

        try
        {
            using var message = new MailMessage(_from, toEmail, subject, htmlBody) { IsBodyHtml = true };
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = _useSsl,
                Credentials = string.IsNullOrEmpty(_user) ? null : new NetworkCredential(_user, _password),
            };
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Don't let a mail failure break registration/reset; surface it in the log.
            logger.LogError(ex, "Failed to send email to {Email} (subject: {Subject}).", toEmail, subject);
        }
    }
}
