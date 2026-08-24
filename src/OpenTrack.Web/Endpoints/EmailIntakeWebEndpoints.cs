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
using OpenTrack.Core.Text;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Intake;
using OpenTrack.Infrastructure.Notifications;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// The email-to-ticket intake. A mail inbound-parse service (Mailgun, SendGrid Inbound Parse, …) or a
/// small forwarder that reads a mailbox POSTs a received email here, and it becomes a public trouble
/// ticket. OFF unless <c>OpenTrack:EmailIntake:Secret</c> is set; the poster must present that shared
/// secret (header <c>X-OpenTrack-Secret</c> or a <c>secret</c> form field). The target project comes from
/// the recipient address key (<c>tickets+WEB@…</c> → the "WEB" project), and that project must already
/// have public intake enabled. Abuse is bounded by the same "intake" rate-limiter as the web form.
/// </summary>
public static class EmailIntakeWebEndpoints
{
    public static void MapEmailIntakeWebEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/intake/email",
            async (HttpContext http, EmailIntakeOptions options, AppDbContext db, NotificationDispatch notifications, CancellationToken ct) =>
        {
            if (!options.Enabled) return Results.NotFound(); // feature off — don't advertise it

            if (!http.Request.HasFormContentType) return Results.BadRequest(new { error = "Expected a form post." });
            var form = await http.Request.ReadFormAsync(ct);

            // Shared-secret check: header first, then a form field. Constant-time compare.
            var presented = http.Request.Headers["X-OpenTrack-Secret"].ToString();
            if (string.IsNullOrEmpty(presented)) presented = form["secret"].ToString();
            if (!SecretMatches(options.Secret!, presented)) return Results.Unauthorized();

            // Tolerate the common field names used by inbound-parse providers.
            var recipient = First(form, "recipient", "to", "To");
            var from = First(form, "from", "sender", "From");
            var subject = First(form, "subject", "Subject");
            var body = First(form, "body-plain", "stripped-text", "text", "body", "message");

            var (name, email) = SplitFrom(from);
            var projectKey = EmailRouting.ProjectKeyFromRecipient(recipient);

            var result = await PublicIntakeOperations.SubmitByProjectKeyAsync(db, projectKey, name, email, subject, body, ct);
            if (result.Error is not null) return Results.BadRequest(new { error = result.Error });

            var issueId = result.IssueId!.Value;
            // Notify the team + fire webhooks (actor 0 => the owner/reporter isn't filtered out).
            await notifications.NotifyIssueChangedAsync(db, 0, issueId, "a new ticket was submitted by email", ct);
            return Results.Ok(new { reference = issueId });
        }).RequireRateLimiting("intake");
    }

    private static string? First(IFormCollection form, params string[] keys)
    {
        foreach (var k in keys)
            if (form.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                return v.ToString();
        return null;
    }

    /// <summary>Split a From header into a display name and bare address ("Alice &lt;a@x&gt;" → ("Alice","a@x")).</summary>
    private static (string? Name, string? Email) SplitFrom(string? from)
    {
        if (string.IsNullOrWhiteSpace(from)) return (null, null);
        var s = from.Trim();
        var open = s.LastIndexOf('<');
        var close = s.LastIndexOf('>');
        if (open >= 0 && close > open)
        {
            var name = s[..open].Trim().Trim('"');
            var addr = s[(open + 1)..close].Trim();
            return (name.Length == 0 ? null : name, addr.Length == 0 ? null : addr);
        }
        return (null, s);
    }

    private static bool SecretMatches(string expected, string? presented)
    {
        if (string.IsNullOrEmpty(presented)) return false;
        // FixedTimeEquals returns false for different-length inputs without throwing.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(presented));
    }
}
