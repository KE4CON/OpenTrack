# Public trouble tickets (let people report problems without an account)

OpenTrack can accept problem reports from people who don't have an account —
useful if you run a helpdesk, a club, or take field reports. It's a page built
into OpenTrack (not a separate website), and it's **off by default** for every
project until you turn it on.

## Turn it on for a project

1. Open the project → **Settings** (you need the Manager role).
2. Under **Public trouble-ticket intake**, click **Turn on public intake**.
3. A public link appears, e.g. `http://your-opentrack-server/report/3`. Share
   that link with whoever should be able to submit tickets.

Anyone with the link can now open it, fill in a short form (their name and email
are optional), and submit. Each submission becomes a normal issue in that
project — you and your team triage it like any other bug, and it triggers the
usual notifications and any webhooks you've set up.

## What the submitter sees

- A simple **Report a problem** form — no login.
- After submitting, a **reference number** (e.g. #42). If they gave an email and
  you have email configured, they also get an acknowledgement message.
- A **Check your ticket** page (`/report/status`) where they enter their
  reference number and the email they used to see the current status. They can
  only see *their own* ticket — the reference alone isn't enough.

## Reaching it from outside your network

The report page lives on your OpenTrack server. People on your **local network**
can reach it right away. To take reports from the **public internet**, expose
your OpenTrack server through your existing hosting / reverse proxy (and turn on
HTTPS — see the network-access guide). There's no second app to deploy.

## Keeping out spam

Because it's a public form, it's a target for bots. OpenTrack includes built-in
guardrails:

- **Off by default**, per project — nothing is public until you enable it.
- **Rate limiting** — a single source can only submit a handful of times per few
  minutes.
- **A hidden honeypot field** — automated bots fill it and are silently ignored.
- **Length limits** on every field.

If you expect heavy public traffic, put a CAPTCHA or your reverse proxy's bot
protection in front of the `/report/...` routes as well.
