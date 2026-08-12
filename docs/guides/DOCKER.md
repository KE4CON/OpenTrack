# Running OpenTrack in Docker

## In a nutshell

Open a terminal (a text window where you type commands) in the OpenTrack folder, type `docker compose up -d`, and press Enter. Then open your web browser and go to **http://localhost:8080**. The very first account you create becomes the boss (the administrator). Your data is saved automatically and backed up every day, so you can safely stop, restart, or update OpenTrack without losing anything.

## What is Docker, and what is a "container"?

**Docker** is a free program that runs other programs inside a sealed, self-contained bubble so they work the same way on any computer. That bubble is called a **container** — think of it as a tidy, pre-packed box that already has everything OpenTrack needs to run, so you don't have to install and configure a bunch of pieces yourself.

OpenTrack comes with two ready-made setup files:

- A **`Dockerfile`** — the recipe Docker uses to build the OpenTrack box.
- A **`docker-compose.yml`** — a settings file that tells Docker how to start OpenTrack with a single command.

**Docker Compose** is the part of Docker that reads that `docker-compose.yml` file and starts everything for you. Together, these files let you run the whole thing with one command — handy for a home server, a NAS (Network Attached Storage, a small always-on storage box on your home network), or a small VPS (Virtual Private Server, a rented computer in the cloud).

Before you start, you need Docker installed on your computer. If you don't have it yet, download and install **Docker Desktop** from the official Docker website, then continue below.

## Quick start (using Docker Compose)

**Step 1.** Open a terminal. A terminal is a plain text window where you type commands.
- On Windows, open **PowerShell** (click the Start menu, type "PowerShell", and click it).
- On macOS, open **Terminal** (press Command + Spacebar, type "Terminal", and press Enter).
- On Linux, open your **Terminal** app.

**Step 2.** In that window, move into the OpenTrack folder — the folder that contains the `docker-compose.yml` file. You do this with the `cd` command (short for "change directory"), followed by the path to the folder. For example, if OpenTrack is in a folder called `OpenTrack` inside your Documents, you would type:

```bash
cd Documents/OpenTrack
```

Replace `Documents/OpenTrack` with the actual location of your OpenTrack folder on your computer.

**Step 3.** Type this exact command and press Enter:

```bash
docker compose up -d
```

The `-d` means "run in the background" so it keeps running after you close the terminal. The first time you run it, Docker may spend a few minutes downloading and building things. That's normal — just wait until it finishes.

**Step 4.** Open your web browser (Chrome, Firefox, Edge, Safari — any of them) and go to this address:

**http://localhost:8080**

("localhost" simply means "this same computer," and "8080" is the door number the app is listening on.)

**Step 5.** That's it — the first account you register becomes the administrator (the main account that can manage everything).

### Stopping OpenTrack

To stop OpenTrack, type this in the terminal (from the same OpenTrack folder):

```bash
docker compose down
```

Your data stays safe — this only stops the program, it does not delete anything.

### Updating OpenTrack

After you download newer OpenTrack code, rebuild and restart with this command:

```bash
docker compose up -d --build
```

The `--build` part tells Docker to rebuild the box using the newer code.

## Where your data lives

All of your information is stored in something Docker calls a **named volume** — a labeled, permanent storage area that Docker manages for you outside the container. OpenTrack's named volume is called `opentrack-data`, and inside the container it appears as a folder called `/data`.

Inside `/data` you'll find:

- `/data/opentrack.db` — the **SQLite database**. SQLite is a simple, self-contained database (a single file that holds all your information). This file holds all your projects and issues.
- `/data/backups/` — automatic backup copies of that database (explained in the next section).

Because it's a named volume, your data **survives** when the container is rebuilt or updated — it is not stored inside the disposable box, so it doesn't get thrown away. To make your own backup, copy the contents of the volume, or simply grab the newest file from `/data/backups`.

## Automatic backups

The `docker-compose.yml` file turns on **scheduled backups** by default. This means: once every 24 hours, the server automatically writes a clean snapshot (a complete copy) of the database into the `/data/backups` folder, and keeps the 14 newest snapshots (deleting older ones so they don't pile up forever).

It does this using a safe SQLite feature called `VACUUM INTO`, which can copy the database correctly even while the app is running, so you don't have to shut anything down.

You can adjust this behavior using **environment variables** — named settings you can hand to the program when it starts. The table below lists each one, its default value, and what it does:

| Variable | Default | Meaning |
|---|---|---|
| `OpenTrack__Backup__Enabled` | `true` (in compose) | Turn scheduled backups on or off |
| `OpenTrack__Backup__IntervalHours` | `24` | Hours between backups |
| `OpenTrack__Backup__Directory` | `/data/backups` | Where snapshots are written |
| `OpenTrack__Backup__Retention` | `14` | How many recent snapshots to keep |

### How to restore from a backup

If you ever need to go back to an earlier copy of your data:

1. Stop the container with `docker compose down`.
2. Copy one of the backup files — they are named like `opentrack-YYYYMMDD-HHMMSS.db`, where the numbers are the date and time the backup was made (for example, `opentrack-20260812-031500.db` is a backup from August 12, 2026 at 03:15:00). Copy your chosen backup file over the main database file at `/data/opentrack.db`, replacing it.
3. Start the container again with `docker compose up -d`.

## Ports, HTTPS, and reverse proxies

A **port** is like a numbered door that a program listens on. By default, the OpenTrack container listens on plain **HTTP (Hypertext Transfer Protocol, ordinary un-encrypted web traffic) on port 8080**.

For a trusted home or office network (a LAN, or Local Area Network — the private network in your home or building), plain HTTP is fine.

If OpenTrack will be reachable from the internet, you should protect the connection with **HTTPS** (the secure, encrypted version of HTTP — the padlock you see in your browser). There are two common ways to do this:

- Put OpenTrack behind a **reverse proxy** — a separate helper program (such as Caddy, Nginx, or Traefik) that sits in front, handles the encryption, and passes traffic through to OpenTrack.
- Or set `OpenTrack__RequireHttps=true` and provide your own security certificates.

To change the door number the app is published on, edit the `ports:` line in `docker-compose.yml`. For example, `"9000:8080"` makes OpenTrack reachable at port 9000 on your computer instead of 8080 (the second number, 8080, is the port inside the container and stays the same).

## Configuration (changing other settings)

OpenTrack has a settings file called `appsettings.json`. Any setting from that file can also be provided as an environment variable — you just replace each `:` (colon) with `__` (two underscores) in the setting's name.

For example, to turn on the AI assist feature, you would use these two settings:

```
OpenTrack__Ai__Enabled=true
OpenTrack__Ai__ApiKey=sk-ant-...
```

The `sk-ant-...` shown above is only a placeholder — do not type it literally. Replace it with your own real API key (a secret password-like code from your AI provider), which will be a long string that starts with `sk-ant-` followed by many more characters.

To use settings like these, add them under the `environment:` section in `docker-compose.yml`. For more detail on the AI feature, see `docs/guides/AI_ASSIST.md`.

## Plain Docker (without Compose)

If you prefer not to use Docker Compose, you can run OpenTrack with plain Docker commands instead. Type these commands one at a time, pressing Enter after each. (The `\` at the ends of lines below just means "this command continues on the next line" — you can type it all as one long line if you prefer.)

```bash
docker build -t opentrack .
docker volume create opentrack-data
docker run -d --name opentrack -p 8080:8080 \
  -v opentrack-data:/data \
  -e "ConnectionStrings__Default=Data Source=/data/opentrack.db;Cache=Shared" \
  -e OpenTrack__Backup__Enabled=true -e OpenTrack__Backup__Directory=/data/backups \
  opentrack
```

Here is what each command does:

- `docker build -t opentrack .` — builds the OpenTrack box and names ("tags") it `opentrack`. The `.` (period) at the end means "use the files in the current folder," so run this from inside the OpenTrack folder.
- `docker volume create opentrack-data` — creates the permanent storage area named `opentrack-data`.
- `docker run ...` — starts OpenTrack. The `-p 8080:8080` publishes it on port 8080, `-v opentrack-data:/data` attaches your storage area, and the `-e` options pass in settings (the database location and backup options). After it starts, open **http://localhost:8080** in your browser, just like before.
