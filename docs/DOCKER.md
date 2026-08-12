# Running OpenTrack in Docker

OpenTrack ships a `Dockerfile` and a `docker-compose.yml` so you can run the whole
thing with one command — handy for a home server, a NAS, or a small VPS.

## Quick start (Docker Compose)

From the repository folder:

```bash
docker compose up -d
```

Then open **http://localhost:8080**. That's it — the first account you register
becomes the administrator.

To stop it: `docker compose down` (your data stays). To update after pulling new
code: `docker compose up -d --build`.

## Where your data lives

Everything is stored in a Docker **named volume** called `opentrack-data`, mounted
at `/data` inside the container:

- `/data/opentrack.db` — the SQLite database (all your projects and issues).
- `/data/backups/` — automatic database snapshots (see below).

Because it's a named volume, your data **survives** container rebuilds and updates.
To back it up yourself, copy the volume's contents, or just grab the newest file
from `/data/backups`.

## Automatic backups

The compose file turns on **scheduled backups** by default: once every 24 hours the
server writes a consistent snapshot of the database (using SQLite's
`VACUUM INTO`, which is safe while the app is running) into `/data/backups`, and
keeps the newest 14. Tune it with these environment variables:

| Variable | Default | Meaning |
|---|---|---|
| `OpenTrack__Backup__Enabled` | `true` (in compose) | Turn scheduled backups on/off |
| `OpenTrack__Backup__IntervalHours` | `24` | Hours between backups |
| `OpenTrack__Backup__Directory` | `/data/backups` | Where snapshots are written |
| `OpenTrack__Backup__Retention` | `14` | How many recent snapshots to keep |

To **restore**, stop the container, copy a chosen `opentrack-YYYYMMDD-HHMMSS.db`
over `/data/opentrack.db`, and start it again.

## Ports, HTTPS, and reverse proxies

The container listens on plain **HTTP on port 8080**. For a trusted LAN that's
fine. If OpenTrack will be reachable from the internet, put it behind a reverse
proxy (Caddy, Nginx, Traefik) that terminates HTTPS, or set
`OpenTrack__RequireHttps=true` and provide certificates. Change the published port
by editing the `ports:` mapping (e.g. `"9000:8080"`).

## Configuration

Any setting from `appsettings.json` can be supplied as an environment variable by
replacing `:` with `__`. For example, to enable AI assist:

```
OpenTrack__Ai__Enabled=true
OpenTrack__Ai__ApiKey=sk-ant-...
```

Add these under `environment:` in `docker-compose.yml` (see `docs/AI_ASSIST.md`).

## Plain Docker (no Compose)

```bash
docker build -t opentrack .
docker volume create opentrack-data
docker run -d --name opentrack -p 8080:8080 \
  -v opentrack-data:/data \
  -e "ConnectionStrings__Default=Data Source=/data/opentrack.db;Cache=Shared" \
  -e OpenTrack__Backup__Enabled=true -e OpenTrack__Backup__Directory=/data/backups \
  opentrack
```
