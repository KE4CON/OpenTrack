# OpenTrack Installation Guide

*Set up the server, the Windows & macOS apps, tablets, and local AI — step by step, in plain language.*

*Generated August 12, 2026 · Markdown is the living source of truth.*


---


# 1. Before You Begin

*What OpenTrack is, how the pieces fit together, and what you'll need.*


## What you're installing

OpenTrack is a **self-hosted issue and bug tracker** — think of it as your own private version of a help desk / bug list for all of your projects. “Self-hosted” means *you* run it on *your own* computer; nothing lives on someone else's cloud, and there is no monthly fee. You own the data.

OpenTrack comes in a few pieces that work together. You do **not** install all of them on every machine — each device gets the piece that fits it:

| Piece | Runs on | Who uses it |
| --- | --- | --- |
| **Server** (web app + API + database) | Your **mini-PC or Mac mini** (always on) | Everything connects to it |
| **Desktop app** | A **Windows** PC and/or a **Mac** | You, day to day, as a native app |
| **Browser / tablet access** | Any phone, tablet, Linux laptop, Raspberry Pi | Anyone, through a web browser |
| **AI assistant** (optional) | The server (local) or the cloud | Adds ✨ smart triage & search |

A few quick terms from that table: the **API** stands for **Application Programming Interface** — it's the behind-the-scenes service the desktop apps talk to. Your **LAN** is your **Local Area Network** — simply the home or office network your devices share. And **AI** just means **artificial intelligence** — the optional smart-suggestions helper (Chapter 3). Whenever this guide uses a short form like these, it's spelled out the first time.


## The one rule to remember

> **THE KEY IDEA** — The **server is the single source of truth**. It runs the database and the app. Every other device — your Windows PC, your Mac, tablets — is just a **window** into the server over your home/office network (your LAN). Set the server up first; everything else simply points at it.

That's why this guide installs the **server first** (Chapter 2), then the optional **AI** on it (Chapter 3), then the **desktop apps** and **browser access** that connect to it (Chapters 4–6), and finally the **first-run setup and backups** (Chapters 7–8).


## What you'll need


### Hardware: the server (any small always-on computer)

The server can be **any always-on computer** that meets the modest requirements below — a Windows mini-PC (the author uses a Beelink EQi12, but any brand works), a small desktop, a spare laptop, or a **Mac mini**. It just needs to stay powered on and on your network. This guide sets it up on **Windows or macOS** — pick whichever you have.

|  | Minimum | Recommended |
| --- | --- | --- |
| **CPU** (processor) | Any modern 2-core 64-bit | Quad-core or better — especially if you run the local AI on it |
| **RAM** (memory) | 4 GB | 8 GB — or **16–24 GB** if you'll run the local AI here |
| **Storage** | 128 GB SSD (solid-state drive) | A **second drive** (or partition) for the data, so it survives a Windows reinstall |
| **OS** (operating system) | Windows 10/11 (64-bit) **or** macOS 13+ | Windows 11, or macOS on Apple Silicon |
| **Network** | Wi-Fi | **Wired Ethernet** — steadier for an always-on server |

> **WINDOWS PC OR MAC MINI — EITHER WORKS** — There's nothing special about a Beelink — it's just a quiet, low-power mini-PC that's easy to leave on. Any machine meeting the specs above works the same way. A **Mac mini (Apple Silicon)** is an excellent choice too, and its unified memory makes the optional local AI (Chapter 3) notably faster. More RAM only matters if you also host that local AI.


### The other devices (all optional)

- A **Windows PC** and/or a **Mac** for the native desktop app (optional — you can use only the browser).
- Tablets, phones, Linux laptops, a Raspberry Pi — anything with a web browser — for browser or web-app access.


### Software you'll download (links in the steps)

- **Git** — to download (clone) the OpenTrack code.
- **.NET 10 Software Development Kit (SDK)** — to build and run OpenTrack (free, from Microsoft).
- **Ollama** — only if you want the free local AI on the server (Chapter 3).

> **TWO WAYS TO INSTALL THE APPS** — The Windows and Mac desktop apps can be installed either from a **signed installer** (a normal double-click, when one is published — see Chapters 4 and 5) or **built from source** with the free .NET tools. The **server** (Chapter 2) is set up by building it from source. Either way, every command is written out for you, step by step — if you can copy and paste, you can do this.


### Give the server a fixed address (recommended)

Because every device points at the server, it helps if the server's network address — its **IP (Internet Protocol)** address — doesn't change. The easiest way is a **DHCP (Dynamic Host Configuration Protocol) reservation** in your router, which tells the router to always hand the server the same address. Write that address down; you'll use it a lot. Throughout this guide we'll call it **SERVER-IP** (for example, `192.168.1.50`) — so wherever you see `SERVER-IP`, type that actual number in its place, not the letters “SERVER-IP.”


# 2. Set Up the Server (Windows or a Mac)

*Run one setup script that installs, builds, and starts the whole server — with by-hand steps as a fallback.*


## The easy way: one setup script

You do not have to be technical to do this. OpenTrack comes with a **setup script** that does almost everything for you — it installs the tools it needs, downloads and builds OpenTrack, sets it up to run for your whole network, and starts it. There is a Windows version and a Mac version. You run it once, answer a few simple questions (mostly by just pressing **Enter**), and you're done.

> **IN A NUTSHELL** — **(1)** Download the OpenTrack code and unzip it. **(2)** Run the one setup script. **(3)** Answer a few questions — or just press **Enter** for each. **(4)** Open the web address it prints, and register. That is the whole install; the steps below just walk you through it slowly.

> **WHAT YOU NEED FIRST** — A Windows 10/11 **or** macOS 13+ computer, an internet connection, and about 10–15 minutes. That is it — the script installs everything else it needs.


### On Windows

**Step 1 — Download and unzip the OpenTrack code.**

1. In a web browser, go to **`https://github.com/KE4CON/OpenTrack`**.
2. Click the green **Code** button, then click **Download ZIP**. A file called **OpenTrack-main.zip** downloads to your **Downloads** folder.
3. Open your **Downloads** folder, **right-click** the file **OpenTrack-main.zip**, choose **Extract All…**, then click **Extract**. This creates a folder called **OpenTrack-main**.
4. Open the **OpenTrack-main** folder, then open the **scripts** folder inside it. Leave this window open — you will need it in the next steps.

**Step 2 — Open PowerShell as Administrator.**

1. **Right-click the Start button** (the Windows logo in the bottom-left corner of the screen).
2. Click **Terminal (Admin)** — or **Windows PowerShell (Admin)** on older Windows. If Windows asks “Do you want to allow this app to make changes?”, click **Yes**.
3. A dark window with a blinking cursor opens. That is where the next commands go.

**Step 3 — Run the setup script.**

1. In the dark window, type **cd** followed by **one space** — but do **not** press Enter yet.
2. Now **drag the `scripts` folder** (from the window you left open in Step 1) **into the dark window**. It types the folder's location for you, so you can't mistype it. Press **Enter**.
3. Type (or copy and paste) the line below exactly as shown, then press **Enter**:

```
powershell -ExecutionPolicy Bypass -File .\Install-OpenTrackServer.ps1
```

> **IF IT MENTIONS “RUNNING SCRIPTS IS DISABLED”** — The `-ExecutionPolicy Bypass` part of that line is what prevents that message, so make sure you typed the **whole** line, including that part.


### On a Mac

**Step 1 — Download and unzip the OpenTrack code.**

1. In Safari, go to **`https://github.com/KE4CON/OpenTrack`**, click the green **Code** button, then **Download ZIP**. Safari usually unzips it for you into **Downloads**, leaving a folder called **OpenTrack-main** (if it does not, double-click the `.zip` file to unzip it).

**Step 2 — Open Terminal and run the script.**

1. Open the **Terminal** app: press **Cmd+Space**, type **Terminal**, and press **Enter**.
2. Type **cd** and a **space** (do not press Enter yet), then **drag the `scripts` folder** (inside **OpenTrack-main**, in your Downloads) **into the Terminal window** to fill in its location, and press **Enter**.
3. Type the two lines below, pressing **Enter** after each. It will ask for your Mac password once — the typing stays invisible, which is normal; just type it and press **Enter**:

```
chmod +x install-opentrack-server.sh
./install-opentrack-server.sh
```

The script first **asks you a few simple questions** — where to keep the data, whether to install the local AI now, and whether to set up the first administrator — each with a sensible default you accept by just pressing **Enter**. Then it does everything automatically. When it finishes, it prints the **web address** to open.

> **THE QUESTIONS IT ASKS** — In order: **where to keep the database** — on Windows it first asks whether the PC has a **second drive** and, if so, suggests putting the data there (so it survives a Windows reinstall); **install the free local AI?** (default No); **set up the first administrator now?** (default Yes — then it asks for an email and password); and **use the standard ports?** (default Yes). It shows a summary and asks you to confirm before changing anything. Accept every default and you'll have a working server — you just register the first account afterward.


### Optional: an unattended install (settings file)

Because the script **asks** about the data location, the local AI, and the first administrator, most people never need this. But if you'd rather **not** be asked — for a hands-off install, or to set up several servers the same way — you can put all the answers in a small **settings file** and the script will skip the questions and just run. The same file works on both Windows and a Mac.

1. In the `scripts` folder, **make a copy** of the file `opentrack-server.sample.conf` and name the copy `opentrack-server.conf`. (Copy, don't rename — keep the original sample as a reference.)
2. **Open** `opentrack-server.conf` in any plain-text editor: **Notepad** on Windows, or **TextEdit** on a Mac.
3. **Change only the line(s) you care about** and leave the rest exactly as they are. Each line is `Setting = Value`. The most common changes:

| If you want to… | Change this line to… |
| --- | --- |
| Keep the database on the **D: drive** (Windows) | `DataDir = D:\OpenTrackData` |
| Keep the database in a **specific folder** (Mac) | `DataDir = /Users/Shared/OpenTrack` |
| Use a different **browser** port (default 5035) | `WebPort = 5035` |
| Use a different **desktop-app** port (default 5003) | `ApiPort = 5003` |
| **Install the free local AI** at the same time | `InstallAi = true` |
| **Set the first administrator** automatically | `AdminEmail = you@example.com` (and `AdminPassword = a-strong-password`) |

1. **Save** the file.
2. **Run the script the same way as before, but add your settings file** at the end. On **Windows**:

```
cd C:\OpenTrack\OpenTrack\scripts
powershell -ExecutionPolicy Bypass -File .\Install-OpenTrackServer.ps1 -ConfigFile .\opentrack-server.conf
```

1. On a **Mac**:

```
cd ~/OpenTrack/OpenTrack/scripts
./install-opentrack-server.sh --config ./opentrack-server.conf
```

> **SHORTCUT IF YOU ONLY WANT THE AI** — If the **only** thing you want to change is turning on the local AI, you don't even need the settings file — just add `-InstallAi` on Windows (or `--install-ai` on a Mac) to the end of the normal command from the previous section.


### What the script sets up

- The **two server programs** built and running — the **web app** on port **5035** (browsers) and the **API** on port **5003** (desktop apps).
- **One shared database** in the data folder you chose when it asked (its suggestion is `C:\OpenTrack\data`, or a second drive like `D:\OpenTrack\Data`, on Windows; `/usr/local/opentrack/data` on a Mac).
- On Windows, the **Firewall** opened for both ports so other devices can connect (a Mac on a trusted network needs nothing here).
- Both programs **registered to start at boot** — a scheduled task on Windows, a launchd service on a Mac — and started now.
- The **local AI** as well, if you asked for it.


## Create the first administrator

1. Look at the lines the script printed when it finished — it shows the **exact** web address to open, something like `http://192.168.1.50:5035` (your number will be different).
2. On any computer on the same network, type **that exact address** into a web browser and press Enter. (On the server itself, `http://localhost:5035` also works.)
3. Choose **Register** and create your account — the **first** account registered becomes the **Administrator**. Do this soon, before anyone else.

> **DON'T TYPE “SERVER-IP” LITERALLY** — In this guide, **SERVER-IP** is just a stand-in for your server's own network address — a number like `192.168.1.50`. Wherever you see something like `http://SERVER-IP:5035`, type your server's actual number in place of the words `SERVER-IP`. The easiest way to get it right: **copy the exact address the setup script printed.**

> **OR SET THE ADMIN AUTOMATICALLY** — Put `AdminEmail` and `AdminPassword` in the settings file and the script hands those to the server, which creates/promotes that account to Administrator at startup — so you never rely on “first to register wins.”


## Updating later

To update OpenTrack, just **re-run the same script**. It pulls the latest code, rebuilds both programs, and restarts them. Your database and settings are untouched.


## The by-hand alternative (optional)

> **YOU CAN SKIP THIS** — The steps below do the same work as the script, by hand, and are written for **Windows** (on a Mac the script does the equivalent using Homebrew and launchd). You only need them if you want to understand or customize the setup, or the script doesn't fit your environment. If the script worked, you're done — go on to Chapter 3 (AI) or Chapter 4 (desktop app).


## Step A — Prepare Windows and the drives

If your server has a **second drive** (or you can make a second partition), keep **Windows and the app on C:** and put **your data on D:**. That way, if you ever have to reinstall Windows, your projects and issues on D: are untouched. **Only have one drive?** No problem — just use a folder on C: for the data instead, and make sure it's part of your off-machine backup.

1. Finish Windows setup and install all **Windows Updates** (Settings → Windows Update).
2. **If you have a second drive:** confirm it shows as **D:** (This PC). If it isn't formatted yet, open **Disk Management**, initialize it, and create a simple **D:** volume.
3. Create a folder for the app and one for the data. We'll use `C:\OpenTrack` (app) and `D:\OpenTrackData` (data) throughout — if you only have C:, use `C:\OpenTrackData` for the data and substitute that path wherever you see `D:\OpenTrackData`.

```
mkdir C:\OpenTrack
mkdir D:\OpenTrackData
```

> **TIP** — Set the server's power settings so it **never sleeps** (Settings → System → Power → Screen and sleep → set sleep to *Never* when plugged in). It's your server — it should stay awake and reachable.


## Step B — Install the .NET 10 SDK and Git

1. In a browser on the server, go to Microsoft's .NET download page and install the **.NET 10 Software Development Kit (SDK)** (the SDK, not just the runtime). Accept the defaults.
2. Install **Git for Windows** from git-scm.com (defaults are fine).
3. Close and reopen your terminal (Windows Terminal or PowerShell) so it picks up the new tools, then confirm both are installed:

```
dotnet --version
git --version
```

Each command should print a version number. If `dotnet` prints 10.x, you're set.


## Step C — Download (clone) OpenTrack

1. In your terminal, go to the app folder and clone the repository:

```
cd C:\OpenTrack
git clone https://github.com/KE4CON/OpenTrack.git
cd OpenTrack
```

You now have the full OpenTrack source in `C:\OpenTrack\OpenTrack`.


## Step D — Build the two server programs

The server runs **two** small programs that share one database:

- **The web app** — what phones, tablets, and other computers open in a **browser**.
- **The API** — what the **desktop apps** (Windows/Mac) talk to.

Build (“publish”) both into your app folder:

```
dotnet publish src\OpenTrack.Web -c Release -o C:\OpenTrack\web
dotnet publish src\OpenTrack.API -c Release -o C:\OpenTrack\api
```

> **FIRST BUILD IS SLOW** — The very first build downloads packages and can take several minutes. That's normal and only happens once.


## Step E — Point both programs at the D: drive

Tell both programs to keep the database on D: (so your data survives a Windows reinstall). You do this by setting one environment variable, `ConnectionStrings__Default`. The cleanest way is a **system environment variable** so it applies every time the server starts.

1. Open **Edit the system environment variables** (search for it in the Start menu) → **Environment Variables…** → under *System variables* click **New…**
2. Name: `ConnectionStrings__Default`  —  Value: the line below (this is one line).

```
Data Source=D:\OpenTrackData\opentrack.db;Cache=Shared
```

Both the web app and the API read this same setting, so they share one database file.


## Step F — Run the server so the whole network can reach it

By default a fresh program only answers itself. We start each one bound to **all network addresses** (`0.0.0.0`) on a fixed port so other devices can connect. We'll use **5003** for the API and **5035** for the web app.

Open **two** terminal windows. In the first, start the API:

```
cd C:\OpenTrack\api
set ASPNETCORE_URLS=http://0.0.0.0:5003
OpenTrack.API.exe
```

In the second, start the web app:

```
cd C:\OpenTrack\web
set ASPNETCORE_URLS=http://0.0.0.0:5035
OpenTrack.Web.exe
```

Leave both windows running. On the server itself, open a browser to **http://localhost:5035** — you should see the OpenTrack sign-in page.


## Step G — Open the Windows Firewall

So other devices on your network can reach the two ports, allow them through the firewall. In an **Administrator** terminal:

```
netsh advfirewall firewall add rule name="OpenTrack API 5003" dir=in action=allow protocol=TCP localport=5003
netsh advfirewall firewall add rule name="OpenTrack Web 5035" dir=in action=allow protocol=TCP localport=5035
```

Now from another computer on the same network, browse to your server's address with `:5035` on the end (type the server's actual number, for example `http://192.168.1.50:5035`). If the sign-in page loads, your server is reachable. 🎉


## Step H — Create the first administrator

1. On the sign-in page, choose **Register** and create your account.
2. The **first** account registered automatically becomes the **Administrator**.

> **OPTIONAL: SET THE ADMIN AHEAD OF TIME** — If you'd rather not rely on “first to register wins,” set `OpenTrack*BootstrapAdmin*Email` and `OpenTrack*BootstrapAdmin*Password` as system environment variables before first launch — the web app promotes that account to Administrator at startup. See docs/guides/DEPLOYMENT.md.


## Step I — Make it start automatically at boot

You don't want to start the programs by hand every time the server reboots. The simplest reliable way on Windows is **Task Scheduler**, which can launch each program at startup.

1. Open **Task Scheduler** → **Create Task** (not *Basic*).
2. General tab: name it *OpenTrack API*; select **Run whether user is logged on or not**; check **Run with highest privileges**.
3. Triggers tab: **New…** → Begin the task **At startup**.
4. Actions tab: **New…** → Start a program → Program: `C:\OpenTrack\api\OpenTrack.API.exe`; *Start in*: `C:\OpenTrack\api`.
5. Repeat the whole process for a second task, *OpenTrack Web*, pointing at `C:\OpenTrack\web\OpenTrack.Web.exe` (start-in `C:\OpenTrack\web`).

> **SETTING THE PORTS FOR AUTO-START** — Because Task Scheduler doesn't run your `set ASPNETCORE_URLS` line, add a system environment variable `ASPNETCORE_URLS` = `http://0.0.0.0:5035` for the web task, or (cleaner) create a tiny `.bat` for each program that sets the URL then launches the .exe, and point the task at the `.bat`.

> **PREFER DOCKER? (ALTERNATIVE)** — If you'd rather not manage services, install **Docker Desktop** on the server and run `docker compose up -d` from the OpenTrack folder — it starts the web app on port **8080** with automatic backups, and restarts on reboot. See docs/guides/DOCKER.md. (Desktop-app clients still need the API running as above.)


# 3. Install the AI on the Server (Optional)

*Add the free, private, local AI assistant with Ollama — no account, no cloud, no per-use fee.*

> **THE SETUP SCRIPT CAN DO ALL OF THIS** — If you ran Chapter 2's setup script with `InstallAi = true` (or the `-InstallAi` switch), the local AI below is already installed and turned on — you can skip this chapter. The steps here are the by-hand version, and the place to look if you want the cloud option instead.


## What the AI adds

With **AI (artificial intelligence)** turned on, OpenTrack gains a **✨ Suggest with AI** button on the New-issue page (it fills in severity, priority, category, and tags from what you typed) and an **✨ Ask in plain English** search box. It's **off by default**, opt-in, and every suggestion is just that — a suggestion you accept or change.

> **LOCAL = PRIVATE & FREE** — Running the model **locally on the server** with Ollama means your issue text never leaves the machine, there's no API key, and there's no per-use charge. On a CPU-only mini-PC it's not instant, but it's fine for the occasional triage/search.


## Step A — Install Ollama and pull a model

1. On the server, download and install **Ollama** from ollama.com (Windows installer).
2. Open a terminal and pull a model. A good balance for 24 GB RAM is an 8-billion-parameter model:

```
ollama pull llama3.1:8b
```

Ollama now serves an OpenAI-compatible endpoint at **http://localhost:11434/v1** on the server.

> **TOO SLOW? USE A SMALLER MODEL** — If suggestions feel sluggish on the CPU, pull a smaller model and use it instead: `ollama pull llama3.2:3b` (then set the Model to `llama3.2:3b` below). Faster, still fine for triage.


## Step B — Turn on OpenTrack's AI

Set these as **system environment variables** on the server (same place you set the connection string), then restart the web app and API:

| Setting | Value |
| --- | --- |
| `OpenTrack*Ai*Enabled` | `true` |
| `OpenTrack*Ai*Provider` | `openai` |
| `OpenTrack*Ai*BaseUrl` | `http://localhost:11434/v1` |
| `OpenTrack*Ai*Model` | `llama3.1:8b` |

> **NO API KEY FOR LOCAL** — Leave `OpenTrack*Ai*ApiKey` unset — a local Ollama model needs no key. The `openai` provider just means “any OpenAI-compatible endpoint,” which Ollama is.

1. Restart the web app and API (stop the windows / restart the scheduled tasks).
2. Open **New Issue** in the web app — you should now see the **✨ Suggest with AI** button. Type a title and click it to confirm the model responds.


## Prefer the cloud instead?

You can point OpenTrack at cloud Claude or OpenAI instead of local Ollama. Set `OpenTrack*Ai*Provider` to `anthropic` (with an Anthropic key) or `openai` (with an OpenAI key and no BaseUrl). Cloud is faster but bills a separate API account and sends issue text off the machine. Full details — including running Ollama on a separate box — are in **docs/guides/AI_ASSIST.md**.


# 4. Install the Desktop App on Windows

*Install the signed app, or build it from source — then point it at your server.*

There are **two ways** to get the Windows desktop app, and they produce the exact same app. Use **Method A** if a signed installer is available — it's a normal double-click install. Use **Method B** to build it yourself, which always works even if no signed build is published.


## Method A — Install the signed app (recommended)

When a **code-signed** Windows build is available, this is the easy path. “Code-signed” means the app carries a digital signature from a trusted certificate, so Windows installs it without the scary “unknown publisher” warning.

1. On the OpenTrack releases page, download the latest **Windows installer** (a signed installer file, typically an `.msix` or `.exe`).
2. Double-click it and follow the prompts. Because it's signed, Windows installs it cleanly.
3. Launch **OpenTrack** from the Start menu, and skip to “Point it at the server” below.

> **SIGNING IS OPTIONAL** — A signed installer requires a paid developer signing certificate (for Windows, a Microsoft/Azure code-signing identity). If a signed build isn't published — or you'd rather not depend on paying for one — **Method B** below always works with the free tools and produces the identical app.


## Method B — Build it from source

This path needs no installer and no signing — you build the app with the free Microsoft tools. Do this on the **Windows PC** that will run it (which can be the server itself, or any Windows machine).

1. Install the **.NET 10 Software Development Kit (SDK)** and **Git** (same as the server, Chapter 2 Step B). The SDK is the free Microsoft toolkit that builds .NET apps.
2. Install the **.NET MAUI (Multi-platform App User Interface)** build components once — MAUI is the framework the desktop app is built with:

```
dotnet workload install maui
```

1. Clone (download) OpenTrack if you haven't already, and build the Windows app:

```
git clone https://github.com/KE4CON/OpenTrack.git
cd OpenTrack
dotnet build src\OpenTrack.Desktop -c Release -f net10.0-windows10.0.19041.0
```

The built app lands under `src\OpenTrack.Desktop\bin\Release\...`; run the `OpenTrack.Desktop.exe` it produces.

> **FIRST LAUNCH: “WINDOWS PROTECTED YOUR PC”** — Because a self-built app isn't signed, Windows SmartScreen may show a blue warning the first time you run it. Click **More info → Run anyway**. (Method A's signed installer avoids this.)


## Point it at the server and sign in

1. Launch the desktop app.
2. Open its **Settings** and set the **server address**. Use your server's number ending in **:5003** — the **same** number as the web address the setup script printed, just ending in **5003** instead of 5035. For example, if the web address was `http://192.168.1.50:5035`, type `http://192.168.1.50:5003` here.
3. Sign in with the account you created on the server. You're in — same projects and issues as the web app, in a native window.

> **THE ADDRESS IS REMEMBERED PER MACHINE** — Each computer stores its own server address, so you set it once. If the server's address ever changes, just update it here.


# 5. Install the Desktop App on macOS

*Install the signed Mac app, or build it from source — then point it at your server.*

As on Windows, there are **two ways** to get the Mac app — the same app either way. Use **Method A** if a signed, notarized build is available; otherwise use **Method B** to build it yourself on the Mac.


## Method A — Install the signed app (recommended)

When a build signed with an **Apple Developer ID** and **notarized** (checked and stamped by Apple) is available, macOS opens it normally — no warnings.

1. On the OpenTrack releases page, download the latest **Mac app** (a `.dmg` disk image or `.app`).
2. Open the `.dmg` and drag **OpenTrack** into your **Applications** folder.
3. Launch it from Applications, and skip to “Point it at the server” below.

> **SIGNING IS OPTIONAL** — A signed, notarized Mac build requires a paid **Apple Developer** account. If one isn't published — or you'd rather not depend on paying for it — **Method B** below always works and builds the identical app.


## Method B — Build it from source (on a Mac)

> **MAC APPS ARE BUILT ON A MAC** — Apple requires the Mac version to be built on macOS — it can't be built from the Windows server. Do these steps on the **Mac** itself.

1. On the Mac, install the **.NET 10 Software Development Kit (SDK)** (the macOS installer from Microsoft) and **Git** (it comes with the Xcode Command Line Tools; run `xcode-select --install` if needed).
2. Install the **.NET MAUI (Multi-platform App User Interface)** build components once:

```
dotnet workload install maui
```

1. Clone (download) OpenTrack and build the Mac app:

```
git clone https://github.com/KE4CON/OpenTrack.git
cd OpenTrack
dotnet build src/OpenTrack.Desktop -c Release -f net10.0-maccatalyst
```

The build produces an **OpenTrack.Desktop.app** under `src/OpenTrack.Desktop/bin/Release/net10.0-maccatalyst/`.

> **FIRST LAUNCH: RIGHT-CLICK → OPEN** — Because a self-built app isn't signed/notarized, macOS Gatekeeper blocks a normal double-click the first time. **Right-click (or Control-click) the app → Open**, then confirm — you only do this once. (Method A's signed build avoids this.)


## Point it at the server and sign in

1. Launch the app and open its **Settings**.
2. Set the **server address** to your server's number ending in **:5003** — the **same** number as the web address the setup script printed, just ending in **5003** instead of 5035 (for example `http://192.168.1.50:5003`).
3. Sign in with your account. Done — the Mac now shows the same data as everything else.


# 6. Access from Tablets, Phones & Other Computers

*No install needed — just a browser. Plus a one-tap “app” icon for tablets.*


## Open it in a browser

Any device on the same network — an iPad, an Android tablet, a phone, a Linux laptop, a Raspberry Pi — can use OpenTrack with **no install at all**. Just open a browser and go to the **exact same web address the setup script printed** — the one you used to register on the server. It looks like this (your number will be different):

```
http://192.168.1.50:5035
```

> **USE YOUR SERVER'S REAL NUMBER** — `192.168.1.50` above is just an example. Type **your** server's number (the one the setup script printed), not this one. Same address on every device.

Sign in with your account. That's it. This is the universal way in, and it's the *only* option on devices that can't run the native desktop app (Linux, Raspberry Pi, phones, tablets).


## Add it to a tablet's home screen

OpenTrack is a **Progressive Web App (PWA)**, so a tablet can pin it to the home screen and open it like a real app — full screen, its own icon, and it even keeps working through brief network drops for checklists.

1. **iPad (Safari):** open the URL, tap the **Share** button, then **Add to Home Screen**.
2. **Android (Chrome):** open the URL, tap the **⋮** menu, then **Install app** / **Add to Home screen**.
3. Launch it from the new icon — it opens full-screen, like an installed app.

> **GREAT FOR RUNNING A CHECKLIST** — The tablet PWA is ideal for walking a project's bug-hunt checklist — you can check items off even if the Wi-Fi hiccups, and it syncs when you're back online.


# 7. First-Run Configuration

*Create your first project, add people, and turn on the features you want.*

You now have a working server — nice work. There are only **two things you actually need to do** here: create a project, and add the people who'll use it. Everything else on this page is optional; turn it on only if you want it.


## Create your first project

1. Sign in with the administrator account you registered earlier.
2. In the menu on the **left side of the screen**, click **Projects**, then click the **New project** button.
3. Give it a **name** (usually the software, system, or thing you're tracking bugs for) and click **Create**. That's the minimum — you have a project now.
4. Optional: open the project, click **Settings**, and add a few **categories** (buckets like “User Interface” or “Database”) and, if you track releases, **versions**.


## Add the people who'll use it

Everyone needs their own account, and you give each person a **role** on each project — the role decides what they're allowed to do.

1. Have each person open your server's web address in a browser and click **Register** to make an account (or you can do it for them).
2. Open the project, click **Members**, type the person's **email**, pick a **role**, and click **Add**.

Roles go from **Reporter** (can file issues), up through **Developer** (can be assigned work), **Manager** (can change the project's settings), and **Administrator** (runs everything). A good rule: give each person the **lowest** role that still lets them do their job.


## Optional power features

None of these are required, but they're what make OpenTrack shine. Turn on the ones you want, per project, under the project's **Settings**:

| Feature | What it does | Where |
| --- | --- | --- |
| **SLA (Service-Level Agreement) targets** | Flags issues at-risk/breached by priority; escalates breaches | Settings → SLA targets |
| **Automation** | “When a new issue matches → do” (auto-tag/assign/set) | Settings → Automation |
| **Git integration** | Link commits to issues; auto-resolve on “fixes #123” | Settings → Git |
| **Public intake + QR** | Let anyone report a problem via a link or a scanned QR (Quick Response) code poster | Settings → Public intake |

> **DON'T WORRY ABOUT THESE YET** — You do **not** need any of these to start using OpenTrack — skip them for now and come back if you ever want them. The **User Manual** explains each one, step by step, when you're ready.


# 8. Backups & Keeping It Running

*Turn on automatic backups, know how to restore, and keep OpenTrack updated.*

> **THE EASIEST BACKUP — ANYONE CAN DO IT** — At any time, sign in and click **Backup & export** in the left menu to download a full copy of your data. No setup, no typing — just click and save the file somewhere safe (do it before any big change). The automatic option below is nicer, but this one click is the simplest safety net.


## Turn on automatic backups (optional, a bit technical)

For hands-off safety, OpenTrack can also write a **snapshot** of the database on a schedule (safe while the app is running). This one is more technical: set these system environment variables on the server and restart the web app. Not comfortable with that? The one-click export above is plenty.

| Setting | Value | Meaning |
| --- | --- | --- |
| `OpenTrack*Backup*Enabled` | `true` | Turn backups on |
| `OpenTrack*Backup*IntervalHours` | `24` | How often (hours) |
| `OpenTrack*Backup*Directory` | (leave blank) | Where snapshots go — blank means a `backups` folder next to your database |
| `OpenTrack*Backup*Retention` | `14` | How many to keep |

Snapshots are named like `opentrack-20260812-013000.db` and land in your data folder (right next to the database) unless you set a different folder above.


## How to restore

1. Stop the web app and API.
2. In your data folder, find the snapshot you want (a file named like `opentrack-YYYYMMDD-HHMMSS.db`) and **copy it over** the live `opentrack.db`, replacing it.
3. Start the web app and API again. You're back to that point in time.

> **KEEP A COPY OFF THE SERVER** — Snapshots protect against mistakes and corruption, but they sit on the **same machine**. For real safety, every so often copy a snapshot (or a one-click export) to another drive or a cloud folder — so if the whole server ever dies, your data doesn't die with it.


## Updating OpenTrack

Easiest: **re-run the setup script** from Chapter 2 (`Install-OpenTrackServer.ps1`). It pulls the latest code, rebuilds both programs, and restarts them — the database upgrades itself on first start, with no manual database steps.

By hand instead: in your OpenTrack folder run `git pull`, re-publish both programs (Chapter 2's manual Step D), and restart the web app and API.


## Quick troubleshooting

| Symptom | Check this |
| --- | --- |
| Another device can't reach the server | You're using the server's **actual number** (not the words “SERVER-IP”) with the right port; both devices are on the same network; the firewall was opened for both ports |
| Web app loads on the server but not on other devices | The server must listen on `0.0.0.0`, not `localhost` (the setup script does this automatically — re-run it if unsure) |
| Desktop app can't connect | Its server address must end in the **API** port **:5003**, not the web port :5035 |
| ✨ AI button is missing | AI wasn't turned on — re-run the setup script and answer **Yes** to the local-AI question (or set it up in Chapter 3) |
| Server didn't come back after a reboot | The auto-start didn't register — just re-run the setup script; it puts it back |

> **YOU'RE DONE** — Your server is serving OpenTrack to your whole network, your Windows and Mac apps connect to it, tablets can scan and report, and (optionally) a private local AI is helping triage. Next up: the **User Manual** walks through every feature in detail.
