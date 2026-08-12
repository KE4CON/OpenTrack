# OpenTrack — Deployment Notes

These are plain-language notes for the person who runs OpenTrack (the "operator"). This is not a
full step-by-step install guide. It covers the handful of settings that people ask about first: how
to turn on encryption, how to point the desktop app at the right server, how to (optionally) send
email, how to set the first administrator, and how to schedule automatic backups of your data.

## In a nutshell

Out of the box, OpenTrack runs on your own trusted local network and does not encrypt traffic, does
not send email, and makes the **first person who registers** the administrator. If your server can
be reached from the wider internet, turn on encryption by setting `OpenTrack:RequireHttps` to
`true`. Point the desktop app at your server's address in its **Settings** screen. Everything else on
this page — email, choosing the administrator ahead of time, and automatic backups — is optional and
turned off until you switch it on.

## Encrypted (HTTPS) vs. plain (HTTP) connections — the `RequireHttps` setting

First, two terms you'll see over and over:

- **Hypertext Transfer Protocol (HTTP)** is the ordinary, unencrypted way that a web browser or app
  talks to a server. Anyone who can watch the network can read what goes back and forth.
- **Hypertext Transfer Protocol Secure (HTTPS)** is the same thing, but scrambled (encrypted) so
  onlookers can't read it.

OpenTrack is designed to run on a **trusted local network** — for example, a small server box (like
a Beelink mini-computer) sitting on your home or office **Local Area Network (LAN)**, the private
network inside your building. On that kind of network, OpenTrack runs over plain **HTTP** by default.

**What this means for your security.** With plain HTTP, the traffic between a browser (or the desktop
app) and the server is **not encrypted**. Anyone who can watch that network could read login
passwords and the sign-in tokens (the little digital passes that keep you logged in). On a trusted
LAN that only you control, that is usually an acceptable trade-off. But if the server can ever be
reached from **outside** that trusted network — say, from the open internet — you should turn HTTPS
on.

### How to turn HTTPS on

You need to set a value named `OpenTrack:RequireHttps` to `true`. There are two ways to do it. Pick
one; you do not need both.

**Option 1 — edit the settings file.** Open the file named `appsettings.json` and add the following.
If an `"OpenTrack"` section already exists in that file, just add the `"RequireHttps"` line inside
it rather than creating a second `"OpenTrack"` section:

```json
"OpenTrack": {
  "RequireHttps": true
}
```

**Option 2 — use an environment variable.** An environment variable is a setting you give the
program from outside, without editing any file or rebuilding anything. Set this one:
`OpenTrack__RequireHttps=true`. (Note the **two** underscores in the middle — that is not a typo;
environment variables use `__` where the settings file uses a colon `:`.)

**What happens when it's `true`.** The web app and the **Application Programming Interface (API)** —
the behind-the-scenes service the desktop app talks to — will require HTTPS. They automatically send
anyone who arrives over plain HTTP to the encrypted HTTPS address instead, and they send **HTTP
Strict Transport Security (HSTS)**, a signal that tells browsers to always use the encrypted address
from then on.

Two more things you must do when HTTPS is on:

1. You must actually have an HTTPS endpoint set up — that means installing a security certificate on
   the server. That is a standard part of **ASP.NET Core** hosting (the web framework OpenTrack is
   built on) and is beyond what this document covers.
2. You must point the **desktop app's server address** at the `https://…` web address. See the next
   section for exactly how.

**What happens when it's `false` (the default).** The app runs over plain HTTP and does **not**
redirect anyone to HTTPS. That is exactly what you want on a plain-HTTP LAN. If it tried to force a
redirect to HTTPS while no HTTPS endpoint existed, it would simply break access for everyone.

## Desktop app: which server it talks to

The desktop app needs to know the server's web address — specifically the address of the OpenTrack
API. There are two ways to set or change it. You can change it later at any time, so don't worry
about getting it perfect the first time.

**Way 1 — inside the app (easiest).**

1. Open the app.
2. From the menu, click **Settings**.
3. In the server-address box, type the address of your server. For example, type
   `http://192.168.1.50:5003` — but replace `192.168.1.50` with your own server's actual address
   and `5003` with the port it actually uses. If you turned HTTPS on (see the previous section), the
   address will start with `https://` instead of `http://`.
4. Save.

The change takes effect on your next action in the app — you do **not** need to reinstall anything.

**Way 2 — before you run the app for the first time.**

Open the file `wwwroot/appsettings.json` inside the app's folder and set the value named
`ApiBaseUrl` to your server's address (the same kind of address as in the example above). This is
just the starting default the app uses **until** you change it in the in-app Settings screen.

Remember: whatever you type in the in-app **Settings** is saved on that computer and wins over the
bundled default from `appsettings.json`.

## Email — account confirmation and password reset (optional)

By default, OpenTrack does **not** send any email. Instead, the confirmation and password-reset
messages (including the link a user would click) are written into the application log — the running
record of what the program is doing. This means a self-hosted install works fine with no mail server
at all: if someone needs a link, the operator can copy it out of the log by hand. Because signing in
does **not** require confirming your account first, this is perfectly fine for most trusted-LAN
setups.

If you do want OpenTrack to actually send email, you'll need an **Simple Mail Transfer Protocol
(SMTP)** server — the standard kind of server that sends outgoing email. Set
`OpenTrack:Email:Enabled` to `true` and fill in your SMTP server's details:

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

Replace every value on the right with the real details your email provider gave you. For example,
`Host` might become `smtp.gmail.com`, `User` your full email login, `Password` the password (or app
password) for that account, and `From` the address you want messages to appear to come from, such as
`no-reply@yourdomain.com` with `yourdomain.com` swapped for your own domain. Leave `Port` and
`UseSsl` as shown unless your provider tells you otherwise.

If sending an email ever fails, it will **never** block someone from registering or resetting their
password — the failure is simply written to the log and the person continues on their way.

(A technical note for the curious: OpenTrack uses the mail client built into the framework rather
than a popular library called MailKit, because MailKit's current releases pull in another component
that has a known, unpatched security advisory.)

## First administrator (the "bootstrap" account)

By default, the **first account that registers becomes the administrator** — the person with full
control. That's simple, but it depends on who happens to sign up first.

If you'd rather decide the administrator ahead of time (so it never depends on who registers first),
set the two values below **before** you run the app for the first time. When the web host starts up,
it will promote that account to Administrator, or create it if it doesn't exist yet:

```
OpenTrack__BootstrapAdmin__Email=admin@example.com
OpenTrack__BootstrapAdmin__Password=<a strong password>
```

Replace `admin@example.com` with the real email address you want the administrator to have, and
replace `<a strong password>` (including the angle brackets) with an actual strong password of your
choosing — for example, a long, hard-to-guess passphrase. Do **not** type the words
`<a strong password>` literally.

A safety tip: prefer to set these as environment variables or "user-secrets" (a developer feature for
keeping secrets out of files) rather than saving the password into a file you might commit to source
control. This setting is honored by the web host, and because the API host shares the same database,
the account works there too.

## Scheduled backups (optional)

The server can automatically save periodic, complete snapshots of its database — the single file
where all of OpenTrack's data lives. It does this using a safe method called `VACUUM INTO`, which can
copy the **SQLite** database (the lightweight database engine OpenTrack uses) even while the app is
running, without corrupting anything.

This feature is **off by default**. To turn it on, set these values:

```
OpenTrack__Backup__Enabled=true
OpenTrack__Backup__IntervalHours=24        # how often
OpenTrack__Backup__Directory=              # blank = a "backups" folder next to the DB
OpenTrack__Backup__Retention=14            # keep the newest N snapshots
```

Here's what each line means:

- `OpenTrack__Backup__Enabled=true` turns the feature on.
- `OpenTrack__Backup__IntervalHours=24` sets how often a snapshot is taken, in hours. `24` means once
  a day. Put a smaller number for more frequent backups (for example, `6` for every six hours).
- `OpenTrack__Backup__Directory=` sets the folder where snapshots are saved. If you leave it blank
  (as shown), snapshots go into a folder named `backups` right next to the database file. To use a
  different folder, put its full path after the equals sign.
- `OpenTrack__Backup__Retention=14` sets how many snapshots to keep. `14` means keep the newest 14
  and automatically delete older ones, so backups don't pile up forever.

Each snapshot file is named in the pattern `opentrack-YYYYMMDD-HHMMSS.db`, where the middle part is
the date and time it was taken — for example, `opentrack-20260812-143005.db` for a snapshot taken on
August 12, 2026 at 2:30:05 in the afternoon.

**How to restore from a backup.** If you ever need to roll back to a saved snapshot:

1. Stop the server.
2. Copy the snapshot file you want and paste it over the live database file, `opentrack.db`,
   replacing it.
3. Start the server again.

(If you run OpenTrack with Docker Compose — a tool for running the app in a container — this backup
feature is already turned on by default. See `docs/guides/DOCKER.md` for details.)
