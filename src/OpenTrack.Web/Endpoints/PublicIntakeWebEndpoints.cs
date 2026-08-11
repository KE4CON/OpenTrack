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

using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Email;
using OpenTrack.Infrastructure.Intake;
using OpenTrack.Infrastructure.Notifications;

namespace OpenTrack.Web.Endpoints;

/// <summary>
/// The public, UN-authenticated trouble-ticket endpoints. Access is gated by the project's
/// PublicIntakeEnabled flag (checked in PublicIntakeOperations), and abuse is bounded by the "intake"
/// rate-limiter policy + a hidden honeypot field. A submission creates an issue and notifies the team
/// (actor id 0 so the owner/reporter is not excluded) and fires webhooks; a best-effort acknowledgement
/// email goes to the submitter if they gave an address and a mail server is configured.
/// </summary>
public static class PublicIntakeWebEndpoints
{
    public static void MapPublicIntakeWebEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/report/{projectId:int}/submit",
            async (int projectId, HttpContext http, AppDbContext db, NotificationDispatch notifications, IEmailService email, CancellationToken ct) =>
        {
            var form = await http.Request.ReadFormAsync(ct);

            // Honeypot: bots fill the hidden "website" field. Pretend success and drop it silently.
            if (!string.IsNullOrWhiteSpace(form["website"]))
                return Results.Redirect($"/report/{projectId}?ref=0");

            var result = await PublicIntakeOperations.SubmitAsync(
                db, projectId, form["name"], form["email"], form["title"], form["description"], ct);

            if (result.Error is not null)
                return Results.Redirect($"/report/{projectId}?err={Uri.EscapeDataString(result.Error)}");

            var issueId = result.IssueId!.Value;
            // Notify the team + fire webhooks (actor 0 => the owner/reporter isn't filtered out).
            await notifications.NotifyIssueChangedAsync(db, 0, issueId, "a new ticket was submitted via the public form", ct);

            var submitter = form["email"].ToString();
            if (email.IsConfigured && !string.IsNullOrWhiteSpace(submitter))
            {
                try
                {
                    await email.SendAsync(submitter,
                        $"[OpenTrack] We received your report (ref #{issueId})",
                        $"Thanks for your report. Your reference is #{issueId}. You can check its status any time on the ticket-status page.", ct);
                }
                catch { /* best-effort acknowledgement */ }
            }
            return Results.Redirect($"/report/{projectId}?ref={issueId}");
        }).RequireRateLimiting("intake");

        app.MapPost("/report-status/lookup",
            async (HttpContext http, AppDbContext db, CancellationToken ct) =>
        {
            var form = await http.Request.ReadFormAsync(ct);
            if (!int.TryParse(form["reference"], out var reference))
                return Results.Redirect("/report/status?nf=1");
            var status = await PublicIntakeOperations.LookupAsync(db, reference, form["email"], ct);
            return status is { } s
                ? Results.Redirect($"/report/status?ref={s.IssueId}&st={Uri.EscapeDataString(s.Status.ToString())}&ti={Uri.EscapeDataString(s.Title)}")
                : Results.Redirect("/report/status?nf=1");
        }).RequireRateLimiting("intake");
    }
}
