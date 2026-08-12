# OpenTrack — Deployment Notes

Plain-language notes for running OpenTrack. This is not a full install guide; it covers the two
settings people ask about first.

## HTTPS vs. plain HTTP (the `RequireHttps` setting)

OpenTrack is designed to run on a **trusted local network** (for example, a small server like the
Beelink box on your home/office LAN). On that kind of network it runs over plain **HTTP** by default.

**What this means for security:** with plain HTTP, the traffic between a browser (or the desktop app)
and the server is **not encrypted**. Anyone who can watch that network could read login passwords and
the sign-in tokens. On a trusted LAN that only you control, that is usually an acceptable trade-off.
If the server is ever reachable from **outside** that trusted network, you should turn HTTPS on.

**How to turn HTTPS on:** set `OpenTrack:RequireHttps` to `true`. You can do this in
`appsettings.json`:

```json
"OpenTrack": {
  "RequireHttps": true
}
```

…or with an environment variable (no rebuild needed): `OpenTrack__RequireHttps=true`.

When it is `true`, the web app and API require HTTPS (they redirect HTTP to HTTPS and send HSTS).
You must also have an HTTPS endpoint configured (a certificate) — that part is standard ASP.NET Core
hosting and is outside this document. And point the **desktop app's server address** at the
`https://…` URL (see below).

When it is `false` (the default), the app runs over plain HTTP and does **not** redirect — which is
what you want on a plain-HTTP LAN, because forcing a redirect with no HTTPS endpoint would just break
access.

## Desktop app: which server it talks to

The desktop app needs to know the server's address (the OpenTrack.API URL). You can change it two
ways:

1. **In the app:** open **Settings** from the menu, type the server address (for example
   `http://192.168.1.50:5003`, or `https://…` if HTTPS is on), and save. The change takes effect on
   your next action — no reinstall needed.
2. **Before first run:** edit `wwwroot/appsettings.json` in the app folder and set `ApiBaseUrl`. This
   is the default the app starts with until you change it in Settings.

The in-app Settings value is remembered on that machine and overrides the bundled default.

## Email (account confirmation & password reset)

By default OpenTrack does **not** send email. The confirmation/reset messages are written to the
application log instead (with the link included), so a self-hosted install works without a mail
server — an operator can copy the link from the log if needed. Account confirmation is not required
to sign in, so this is fine for most trusted-LAN setups.

To actually send email, set `OpenTrack:Email:Enabled` to `true` and fill in your SMTP server:

```json
"OpenTrack": {
  "Email": {
    "Enabled": true,
    "Host": "smtp.yourprovider.com",
    "Port": 587,
    "User": "your-smtp-username",
    "Password": "your-smtp-password",
    "From": "no-reply@yourdomain.com",
    "UseSsl": true
  }
}
```

A mail failure never blocks registration or password reset — it is logged and the flow continues.
(OpenTrack uses the framework's built-in SMTP client rather than MailKit, whose current releases pull
in a dependency with an unpatched security advisory.)

## First administrator (bootstrap)

By default the **first account you register becomes the administrator**. If you'd
rather set the administrator out-of-band (so it never depends on who registers
first), set these before the first run — the web host promotes or creates that
account to Administrator at startup:

```
OpenTrack__BootstrapAdmin__Email=admin@example.com
OpenTrack__BootstrapAdmin__Password=<a strong password>
```

Prefer environment variables or user-secrets over committing the password. This is
honored by the web host (the API host shares the same database, so the account
works there too).

## Scheduled backups

The server can write periodic, consistent snapshots of the SQLite database (using
`VACUUM INTO`, which is safe while the app is running). It's **off by default**;
turn it on with:

```
OpenTrack__Backup__Enabled=true
OpenTrack__Backup__IntervalHours=24        # how often
OpenTrack__Backup__Directory=              # blank = a "backups" folder next to the DB
OpenTrack__Backup__Retention=14            # keep the newest N snapshots
```

Snapshots are named `opentrack-YYYYMMDD-HHMMSS.db`. To **restore**, stop the
server, copy a chosen snapshot over the live `opentrack.db`, and start again. (The
Docker Compose file enables this by default — see `docs/DOCKER.md`.)
