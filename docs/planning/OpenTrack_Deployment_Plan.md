# OpenTrack Deployment Plan — Beelink Mini PC as the Shared Server

*Written after confirming the Mac desktop app works end-to-end. This is the reminder for how
to get OpenTrack running as a shared tracker every device can use.*

## The Model (how it all fits together)

The **Beelink EQi12 is the single source of truth.** It runs the **API + database** — one
`opentrack.db` on the D: drive. Every other device is just a *client* that talks to that one
API over the home network. There is only ever ONE database; everybody reads and writes to it,
so a project created on the Mac instantly shows up on Windows, the Pi's browser, everywhere.

**Which device uses what:**
- **Windows laptop** → native desktop app (MAUI), pointed at the Beelink's address
- **MacBook** → native desktop app (MAUI), pointed at the Beelink's address
- **Linux Mint laptop** → NO native app (MAUI doesn't support Linux desktop) → uses the **web app** in a browser
- **Raspberry Pi** → same, uses the **web app** in a browser
- **Anything with a browser** → the web app works everywhere as a universal fallback

**Who can see/edit/delete** is governed by the user's ROLE (Viewer / Reporter / Updater /
Developer / Manager / Administrator), NOT by which device they're on. You log in with your
account from any machine and get exactly the rights your role grants. Log in as admin from the
Pi's browser → admin rights. Log in as a Reporter from the Mac → Reporter rights.

## What Has To Be Done To Make This Real

These are the pieces still ahead of us (most already rehearsed during Mac testing):

1. **Run the API + web app as persistent SERVICES on the Beelink.**
   Not `dotnet run` in a terminal you have to keep open — they need to auto-start and keep
   running (Windows Service, Task Scheduler at boot, or similar). This is the biggest new piece.

2. **Bind the API to the network, not just localhost.**
   The API must listen on `http://0.0.0.0:5003` (all interfaces) so other machines can reach it.
   We proved this works during Mac testing with: `dotnet run --urls "http://0.0.0.0:5003"`.
   For a service, this goes in configuration instead of a command-line flag.

3. **Open the Beelink's firewall for the API port** (and the web app's port).
   On Windows that was: `netsh advfirewall firewall add rule name="OpenTrack API 5003"
   dir=in action=allow protocol=TCP localport=5003 profile=any` (run as Administrator).

4. **Make the desktop apps' API URL CONFIGURABLE** (currently hardcoded to `http://localhost:5003`).
   This is the long-standing cleanup item — it matters SPECIFICALLY for this deployment, because
   the Windows and Mac apps will need to point at the Beelink's LAN address (e.g.
   `http://192.168.1.xxx:5003`), not localhost. Best done as a small settings file the app reads
   at startup, so each machine can point at the Beelink without recompiling.

5. **Point the database at the D: drive** via a `ConnectionStrings:Default` entry in the
   Beelink's API appsettings (the `ResolveOpenTrackConnectionString` logic already supports this —
   set the config and it uses D: instead of the default LocalAppData location).

6. **Norton: allow local network traffic** on every device that will connect.
   Norton VPN/Smart Firewall blocked device-to-device traffic repeatedly during testing. Either
   mark the home network "Trusted" in Norton, enable its "allow local/LAN traffic" option, or
   turn Norton off on the LAN. This bit us hard — don't forget it.

## Quick Sanity-Check Sequence (once deployed)

1. From another machine: `curl http://<beelink-ip>:5003/api/projects` → expect `401` (reachable, needs auth).
2. If it hangs/times out → firewall or Norton blocking (see #3, #6).
3. Desktop app or web app on another device → log in → see the same projects everyone sees.

## Reminder of the Admin-Account Gotcha (Item #23)

The "first user becomes Administrator" rule only fires when registering through the WEB APP.
Accounts registered via the API or desktop app default to Reporter. So: **create the admin
account via the web app**, OR promote an account directly in the database with:
`sqlite3 "<path>/opentrack.db" "UPDATE AspNetUsers SET Role = 90 WHERE Email = 'you@example.com';"`
(Role 90 = Administrator. That's how the Mac admin account was made during testing.)
