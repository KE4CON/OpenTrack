#!/usr/bin/env python
# OpenTrack — Installation Guide generator.
# Produces a styled Word .docx (house style shared with the other repos, via style.py).
# The .docx is the review/screenshot copy; export to PDF from Word when ready.
#
#   pip install python-docx
#   python install_guide_build.py            # writes ../docs/guides/OpenTrack_Installation_Guide.docx
#
# Content lives in CHAPTERS below (one dict per chapter, blocks rendered by style.render_chapter),
# so a wording change is a one-line edit and the build can't be broken by a bad block.

import os
import datetime
import style as S

TODAY = datetime.date.today().strftime("%B %d, %Y")

# --------------------------------------------------------------------------------------
# Content
# --------------------------------------------------------------------------------------

CHAPTERS = [
    # 1 -------------------------------------------------------------------------------
    {
        "title": "Before You Begin",
        "subtitle": "What OpenTrack is, how the pieces fit together, and what you'll need.",
        "in_this_chapter": [
            "What OpenTrack is and how it's laid out across your devices",
            "The one rule that makes everything else make sense: the server is the single source of truth",
            "The hardware, downloads, and accounts to have ready",
        ],
        "blocks": [
            {"h1": "What you're installing"},
            {"p": "OpenTrack is a **self-hosted issue and bug tracker** — think of it as your own private "
                  "version of a help desk / bug list for all of your projects. \u201cSelf-hosted\u201d means "
                  "*you* run it on *your own* computer; nothing lives on someone else's cloud, and there is no "
                  "monthly fee. You own the data."},
            {"p": "OpenTrack comes in a few pieces that work together. You do **not** install all of them on "
                  "every machine — each device gets the piece that fits it:"},
            {"table": {
                "headers": ["Piece", "Runs on", "Who uses it"],
                "rows": [
                    ["**Server** (web app + API + database)", "Your **mini-PC or Mac mini** (always on)", "Everything connects to it"],
                    ["**Desktop app**", "A **Windows** PC and/or a **Mac**", "You, day to day, as a native app"],
                    ["**Browser / tablet access**", "Any phone, tablet, Linux laptop, Raspberry Pi", "Anyone, through a web browser"],
                    ["**AI assistant** (optional)", "The server (local) or the cloud", "Adds \u2728 smart triage & search"],
                ],
                "widths": [2.5, 2.1, 1.9],
            }},
            {"p": "A few quick terms from that table: the **API** stands for **Application Programming Interface** — "
                  "it's the behind-the-scenes service the desktop apps talk to. Your **LAN** is your **Local Area "
                  "Network** — simply the home or office network your devices share. And **AI** just means "
                  "**artificial intelligence** — the optional smart-suggestions helper (Chapter 3). Whenever this "
                  "guide uses a short form like these, it's spelled out the first time."},
            {"h1": "The one rule to remember"},
            {"callout": {"kind": "important", "label": "THE KEY IDEA",
                         "text": "The **server is the single source of truth**. It runs the database and the "
                                 "app. Every other device \u2014 your Windows PC, your Mac, tablets \u2014 is just a "
                                 "**window** into the server over your home/office network (your LAN). Set the "
                                 "server up first; everything else simply points at it."}},
            {"p": "That's why this guide installs the **server first** (Chapter 2), then the optional "
                  "**AI** on it (Chapter 3), then the **desktop apps** and **browser access** that connect to it "
                  "(Chapters 4\u20136), and finally the **first-run setup and backups** (Chapters 7\u20138)."},
            {"h1": "What you'll need"},
            {"h2": "Hardware: the server (any small always-on computer)"},
            {"p": "The server can be **any always-on computer** that meets the modest requirements below \u2014 a "
                  "Windows mini-PC (the author uses a Beelink EQi12, but any brand works), a small desktop, a spare "
                  "laptop, or a **Mac mini**. It just needs to stay powered on and on your network. This guide sets "
                  "it up on **Windows or macOS** — pick whichever you have."},
            {"table": {
                "headers": ["", "Minimum", "Recommended"],
                "rows": [
                    ["**CPU** (processor)", "Any modern 2-core 64-bit", "Quad-core or better \u2014 especially if you run the local AI on it"],
                    ["**RAM** (memory)", "4 GB", "8 GB \u2014 or **16\u201324 GB** if you'll run the local AI here"],
                    ["**Storage**", "128 GB SSD (solid-state drive)", "A **second drive** (or partition) for the data, so it survives a Windows reinstall"],
                    ["**OS** (operating system)", "Windows 10/11 (64-bit) **or** macOS 13+", "Windows 11, or macOS on Apple Silicon"],
                    ["**Network**", "Wi-Fi", "**Wired Ethernet** \u2014 steadier for an always-on server"],
                ],
                "widths": [1.1, 2.3, 3.1],
            }},
            {"callout": {"kind": "note", "label": "WINDOWS PC OR MAC MINI — EITHER WORKS",
                         "text": "There's nothing special about a Beelink \u2014 it's just a quiet, low-power mini-PC "
                                 "that's easy to leave on. Any machine meeting the specs above works the same way. "
                                 "A **Mac mini (Apple Silicon)** is an excellent choice too, and its unified "
                                 "memory makes the optional local AI (Chapter 3) notably faster. More RAM only "
                                 "matters if you also host that local AI."}},
            {"h2": "The other devices (all optional)"},
            {"bullets": [
                "A **Windows PC** and/or a **Mac** for the native desktop app (optional \u2014 you can use only the "
                "browser).",
                "Tablets, phones, Linux laptops, a Raspberry Pi \u2014 anything with a web browser \u2014 for browser "
                "or web-app access.",
            ]},
            {"h2": "Software you'll download (links in the steps)"},
            {"bullets": [
                "**Git** \u2014 to download (clone) the OpenTrack code.",
                "**.NET 10 Software Development Kit (SDK)** \u2014 to build and run OpenTrack (free, from Microsoft).",
                "**Ollama** \u2014 only if you want the free local AI on the server (Chapter 3).",
            ]},
            {"callout": {"kind": "note", "label": "TWO WAYS TO INSTALL THE APPS",
                         "text": "The Windows and Mac desktop apps can be installed either from a **signed "
                                 "installer** (a normal double-click, when one is published \u2014 see Chapters 4 and 5) "
                                 "or **built from source** with the free .NET tools. The **server** (Chapter 2) is "
                                 "set up by building it from source. Either way, every command is written out for "
                                 "you, step by step \u2014 if you can copy and paste, you can do this."}},
            {"h2": "Give the server a fixed address (recommended)"},
            {"p": "Because every device points at the server, it helps if the server's network address \u2014 its "
                  "**IP (Internet Protocol)** address \u2014 doesn't change. The easiest way is a **DHCP (Dynamic Host "
                  "Configuration Protocol) reservation** in your router, which tells the router to always hand the "
                  "server the same address. Write that address down; you'll use it a lot. Throughout this guide "
                  "we'll call it **SERVER-IP** (for example, `192.168.1.50`)."},
        ],
    },

    # 2 -------------------------------------------------------------------------------
    {
        "title": "Set Up the Server (Windows or a Mac)",
        "subtitle": "Run one setup script that installs, builds, and starts the whole server — with by-hand steps as a fallback.",
        "in_this_chapter": [
            "The easy way: run the one-command setup script",
            "Change ports, the data location, or add the AI with a settings file",
            "Create the first administrator",
            "The by-hand alternative, step by step, if you'd rather not use the script",
        ],
        "blocks": [
            {"h1": "The easy way: one setup script"},
            {"p": "Almost everything in this chapter is automated by a **setup script** that ships with OpenTrack "
                  "— a PowerShell script for **Windows** (`Install-OpenTrackServer.ps1`) and a bash script for "
                  "**macOS** (`install-opentrack-server.sh`), both in the `scripts` folder. Either one installs the "
                  "tools it needs (the .NET 10 Software Development Kit and Git), downloads and builds OpenTrack, "
                  "points the web app and the API at one shared database, sets up networking, and registers both "
                  "programs to start automatically at boot. You run it once; re-run it anytime to update."},
            {"callout": {"kind": "note", "label": "WHAT YOU NEED FIRST",
                         "text": "A Windows 10/11 **or** macOS 13+ machine, an internet connection, and about 10\u201315 minutes. The "
                                 "script needs elevated rights (it runs as **Administrator** on Windows, or asks "
                                 "for your password for `sudo` on a Mac) to set up networking and auto-start. It "
                                 "installs the .NET SDK and Git for you if they're missing."}},
            {"h2": "On Windows"},
            {"steps": [
                "Get the OpenTrack code onto the server: on the project's GitHub page click the green **Code** "
                "button \u2192 **Download ZIP**, then unzip it (for example to `C:\\OpenTrack`). Or, if you already "
                "have Git, `git clone https://github.com/KE4CON/OpenTrack.git`.",
                "Open **PowerShell as Administrator**: right-click the Start button \u2192 **Terminal (Admin)** (or "
                "**Windows PowerShell (Admin)**).",
                "Go to the `scripts` folder inside the code you just unzipped, and run the setup script:",
            ]},
            {"code": [
                "cd C:\\OpenTrack\\OpenTrack\\scripts",
                "powershell -ExecutionPolicy Bypass -File .\\Install-OpenTrackServer.ps1",
            ]},
            {"h2": "On a Mac"},
            {"steps": [
                "Get the OpenTrack code onto the Mac the same way \u2014 **Download ZIP** from the GitHub **Code** "
                "button and unzip it (into your home folder, say), or `git clone` it if you have Git.",
                "Open the **Terminal** app (Applications \u2192 Utilities \u2192 Terminal).",
                "Go to the `scripts` folder, make the script executable, and run it. It asks for your password "
                "once, for the always-on setup:",
            ]},
            {"code": [
                "cd ~/OpenTrack/OpenTrack/scripts",
                "chmod +x install-opentrack-server.sh",
                "./install-opentrack-server.sh",
            ]},
            {"p": "Watch it work. When it finishes, it prints the **web address** to open and how to create the "
                  "**first administrator** \u2014 skip to \u201cCreate the first administrator\u201d below."},
            {"callout": {"kind": "tip", "label": "CHANGE PORTS, DATA LOCATION, OR ADD AI",
                         "text": "To change anything \u2014 the ports, where the database lives (for example put it on "
                                 "a D: drive), or to install the local AI at the same time \u2014 copy "
                                 "`opentrack-server.sample.conf` to `opentrack-server.conf`, edit the values, and add "
                                 "`-ConfigFile .\\opentrack-server.conf` to the command. For example set "
                                 "`DataDir = D:\\OpenTrackData` to keep data on the D: drive, or `InstallAi = true` "
                                 "to also set up the free local AI (Chapter 3). On Windows add the `-InstallAi` "
                                 "switch; on a Mac use `--install-ai` (and `--config` instead of `-ConfigFile`). The "
                                 "same settings file works for both."}},
            {"h2": "What the script set up"},
            {"bullets": [
                "The **two server programs** built and running \u2014 the **web app** on port **5035** (browsers) and "
                "the **API** on port **5003** (desktop apps).",
                "**One shared database** in your data folder (default `C:\\OpenTrack\\data` \u2014 change it with "
                "`DataDir`).",
                "On Windows, the **Firewall** opened for both ports so other devices can connect (a Mac on a "
                "trusted network needs nothing here).",
                "Both programs **registered to start at boot** — a scheduled task on Windows, a launchd service on "
                "a Mac — and started now.",
                "The **local AI** as well, if you asked for it.",
            ]},
            {"h1": "Create the first administrator"},
            {"steps": [
                "On any computer on the network, open the web address the script printed: "
                "**http://SERVER-IP:5035**.",
                "Choose **Register** and create your account \u2014 the **first** account registered becomes the "
                "**Administrator**. Do this soon, before anyone else.",
            ]},
            {"callout": {"kind": "tip", "label": "OR SET THE ADMIN AUTOMATICALLY",
                         "text": "Put `AdminEmail` and `AdminPassword` in the settings file and the script hands "
                                 "those to the server, which creates/promotes that account to Administrator at "
                                 "startup \u2014 so you never rely on \u201cfirst to register wins.\u201d"}},
            {"h1": "Updating later"},
            {"p": "To update OpenTrack, just **re-run the same script**. It pulls the latest code, rebuilds both "
                  "programs, and restarts them. Your database and settings are untouched."},

            {"h1": "The by-hand alternative (optional)"},
            {"callout": {"kind": "note", "label": "YOU CAN SKIP THIS",
                         "text": "The steps below do the same work as the script, by hand, and are written for "
                                 "**Windows** (on a Mac the script does the equivalent using Homebrew and launchd). "
                                 "You only need them if you want to understand or customize the setup, or the script "
                                 "doesn't fit your environment. If the script worked, you're done \u2014 go on to "
                                 "Chapter 3 (AI) or Chapter 4 (desktop app)."}},
            {"h1": "Step A \u2014 Prepare Windows and the drives"},
            {"p": "If your server has a **second drive** (or you can make a second partition), keep **Windows and "
                  "the app on C:** and put **your data on D:**. That way, if you ever have to reinstall Windows, "
                  "your projects and issues on D: are untouched. **Only have one drive?** No problem \u2014 just use a "
                  "folder on C: for the data instead, and make sure it's part of your off-machine backup."},
            {"steps": [
                "Finish Windows setup and install all **Windows Updates** (Settings \u2192 Windows Update).",
                "**If you have a second drive:** confirm it shows as **D:** (This PC). If it isn't formatted yet, "
                "open **Disk Management**, initialize it, and create a simple **D:** volume.",
                "Create a folder for the app and one for the data. We'll use `C:\\OpenTrack` (app) and "
                "`D:\\OpenTrackData` (data) throughout \u2014 if you only have C:, use `C:\\OpenTrackData` for the data "
                "and substitute that path wherever you see `D:\\OpenTrackData`.",
            ]},
            {"code": ["mkdir C:\\OpenTrack", "mkdir D:\\OpenTrackData"]},
            {"callout": {"kind": "tip", "label": "TIP",
                         "text": "Set the server's power settings so it **never sleeps** (Settings \u2192 System \u2192 "
                                 "Power \u2192 Screen and sleep \u2192 set sleep to *Never* when plugged in). It's your "
                                 "server \u2014 it should stay awake and reachable."}},

            {"h1": "Step B \u2014 Install the .NET 10 SDK and Git"},
            {"steps": [
                "In a browser on the server, go to Microsoft's .NET download page and install the **.NET 10 Software Development Kit (SDK)** "
                "(the SDK, not just the runtime). Accept the defaults.",
                "Install **Git for Windows** from git-scm.com (defaults are fine).",
                "Close and reopen your terminal (Windows Terminal or PowerShell) so it picks up the new tools, then "
                "confirm both are installed:",
            ]},
            {"code": ["dotnet --version", "git --version"]},
            {"p": "Each command should print a version number. If `dotnet` prints 10.x, you're set."},

            {"h1": "Step C \u2014 Download (clone) OpenTrack"},
            {"steps": [
                "In your terminal, go to the app folder and clone the repository:",
            ]},
            {"code": ["cd C:\\OpenTrack", "git clone https://github.com/KE4CON/OpenTrack.git", "cd OpenTrack"]},
            {"p": "You now have the full OpenTrack source in `C:\\OpenTrack\\OpenTrack`."},

            {"h1": "Step D \u2014 Build the two server programs"},
            {"p": "The server runs **two** small programs that share one database:"},
            {"bullets": [
                "**The web app** \u2014 what phones, tablets, and other computers open in a **browser**.",
                "**The API** \u2014 what the **desktop apps** (Windows/Mac) talk to.",
            ]},
            {"p": "Build (\u201cpublish\u201d) both into your app folder:"},
            {"code": [
                "dotnet publish src\\OpenTrack.Web -c Release -o C:\\OpenTrack\\web",
                "dotnet publish src\\OpenTrack.API -c Release -o C:\\OpenTrack\\api",
            ]},
            {"callout": {"kind": "note", "label": "FIRST BUILD IS SLOW",
                         "text": "The very first build downloads packages and can take several minutes. That's "
                                 "normal and only happens once."}},

            {"h1": "Step E \u2014 Point both programs at the D: drive"},
            {"p": "Tell both programs to keep the database on D: (so your data survives a Windows reinstall). "
                  "You do this by setting one environment variable, `ConnectionStrings__Default`. The cleanest way "
                  "is a **system environment variable** so it applies every time the server starts."},
            {"steps": [
                "Open **Edit the system environment variables** (search for it in the Start menu) \u2192 "
                "**Environment Variables\u2026** \u2192 under *System variables* click **New\u2026**",
                "Name: `ConnectionStrings__Default`  \u2014  Value: the line below (this is one line).",
            ]},
            {"code": "Data Source=D:\\OpenTrackData\\opentrack.db;Cache=Shared"},
            {"p": "Both the web app and the API read this same setting, so they share one database file."},

            {"h1": "Step F \u2014 Run the server so the whole network can reach it"},
            {"p": "By default a fresh program only answers itself. We start each one bound to **all network "
                  "addresses** (`0.0.0.0`) on a fixed port so other devices can connect. We'll use **5003** for the "
                  "API and **5035** for the web app."},
            {"p": "Open **two** terminal windows. In the first, start the API:"},
            {"code": [
                "cd C:\\OpenTrack\\api",
                "set ASPNETCORE_URLS=http://0.0.0.0:5003",
                "OpenTrack.API.exe",
            ]},
            {"p": "In the second, start the web app:"},
            {"code": [
                "cd C:\\OpenTrack\\web",
                "set ASPNETCORE_URLS=http://0.0.0.0:5035",
                "OpenTrack.Web.exe",
            ]},
            {"p": "Leave both windows running. On the server itself, open a browser to "
                  "**http://localhost:5035** \u2014 you should see the OpenTrack sign-in page."},

            {"h1": "Step G \u2014 Open the Windows Firewall"},
            {"p": "So other devices on your network can reach the two ports, allow them through the firewall. In an "
                  "**Administrator** terminal:"},
            {"code": [
                'netsh advfirewall firewall add rule name="OpenTrack API 5003" dir=in action=allow protocol=TCP localport=5003',
                'netsh advfirewall firewall add rule name="OpenTrack Web 5035" dir=in action=allow protocol=TCP localport=5035',
            ]},
            {"p": "Now from another computer on the same network, browse to **http://SERVER-IP:5035** (using the "
                  "server's address). If the sign-in page loads, your server is reachable. \U0001F389"},

            {"h1": "Step H \u2014 Create the first administrator"},
            {"steps": [
                "On the sign-in page, choose **Register** and create your account.",
                "The **first** account registered automatically becomes the **Administrator**.",
            ]},
            {"callout": {"kind": "tip", "label": "OPTIONAL: SET THE ADMIN AHEAD OF TIME",
                         "text": "If you'd rather not rely on \u201cfirst to register wins,\u201d set "
                                 "`OpenTrack__BootstrapAdmin__Email` and `OpenTrack__BootstrapAdmin__Password` as "
                                 "system environment variables before first launch \u2014 the web app promotes that "
                                 "account to Administrator at startup. See docs/guides/DEPLOYMENT.md."}},

            {"h1": "Step I \u2014 Make it start automatically at boot"},
            {"p": "You don't want to start the programs by hand every time the server reboots. The simplest "
                  "reliable way on Windows is **Task Scheduler**, which can launch each program at startup."},
            {"steps": [
                "Open **Task Scheduler** \u2192 **Create Task** (not *Basic*).",
                "General tab: name it *OpenTrack API*; select **Run whether user is logged on or not**; check "
                "**Run with highest privileges**.",
                "Triggers tab: **New\u2026** \u2192 Begin the task **At startup**.",
                "Actions tab: **New\u2026** \u2192 Start a program \u2192 Program: `C:\\OpenTrack\\api\\OpenTrack.API.exe`; "
                "*Start in*: `C:\\OpenTrack\\api`.",
                "Repeat the whole process for a second task, *OpenTrack Web*, pointing at "
                "`C:\\OpenTrack\\web\\OpenTrack.Web.exe` (start-in `C:\\OpenTrack\\web`).",
            ]},
            {"callout": {"kind": "note", "label": "SETTING THE PORTS FOR AUTO-START",
                         "text": "Because Task Scheduler doesn't run your `set ASPNETCORE_URLS` line, add a system "
                                 "environment variable `ASPNETCORE_URLS` = `http://0.0.0.0:5035` for the web task, "
                                 "or (cleaner) create a tiny `.bat` for each program that sets the URL then launches "
                                 "the .exe, and point the task at the `.bat`."}},
            {"callout": {"kind": "important", "label": "PREFER DOCKER? (ALTERNATIVE)",
                         "text": "If you'd rather not manage services, install **Docker Desktop** on the server and "
                                 "run `docker compose up -d` from the OpenTrack folder \u2014 it starts the web app on "
                                 "port **8080** with automatic backups, and restarts on reboot. See "
                                 "docs/guides/DOCKER.md. (Desktop-app clients still need the API running as above.)"}},
        ],
    },

    # 3 -------------------------------------------------------------------------------
    {
        "title": "Install the AI on the Server (Optional)",
        "subtitle": "Add the free, private, local AI assistant with Ollama \u2014 no account, no cloud, no per-use fee.",
        "in_this_chapter": [
            "What the AI does and why local is the private choice",
            "Install Ollama on the server and pull a model",
            "Turn on OpenTrack's AI and point it at the local model",
            "The cloud option (Claude / OpenAI) if you prefer",
        ],
        "blocks": [
            {"callout": {"kind": "tip", "label": "THE SETUP SCRIPT CAN DO ALL OF THIS",
                         "text": "If you ran Chapter 2's setup script with `InstallAi = true` (or the `-InstallAi` "
                                 "switch), the local AI below is already installed and turned on — you can skip "
                                 "this chapter. The steps here are the by-hand version, and the place to look if you "
                                 "want the cloud option instead."}},
            {"h1": "What the AI adds"},
            {"p": "With **AI (artificial intelligence)** turned on, OpenTrack gains a **\u2728 Suggest with AI** button on the New-issue page "
                  "(it fills in severity, priority, category, and tags from what you typed) and an **\u2728 Ask in "
                  "plain English** search box. It's **off by default**, opt-in, and every suggestion is just that "
                  "\u2014 a suggestion you accept or change."},
            {"callout": {"kind": "important", "label": "LOCAL = PRIVATE & FREE",
                         "text": "Running the model **locally on the server** with Ollama means your issue text "
                                 "never leaves the machine, there's no API key, and there's no per-use charge. On a "
                                 "CPU-only mini-PC it's not instant, but it's fine for the occasional triage/search."}},

            {"h1": "Step A \u2014 Install Ollama and pull a model"},
            {"steps": [
                "On the server, download and install **Ollama** from ollama.com (Windows installer).",
                "Open a terminal and pull a model. A good balance for 24 GB RAM is an 8-billion-parameter model:",
            ]},
            {"code": "ollama pull llama3.1:8b"},
            {"p": "Ollama now serves an OpenAI-compatible endpoint at **http://localhost:11434/v1** on the server."},
            {"callout": {"kind": "tip", "label": "TOO SLOW? USE A SMALLER MODEL",
                         "text": "If suggestions feel sluggish on the CPU, pull a smaller model and use it instead: "
                                 "`ollama pull llama3.2:3b` (then set the Model to `llama3.2:3b` below). Faster, still "
                                 "fine for triage."}},

            {"h1": "Step B \u2014 Turn on OpenTrack's AI"},
            {"p": "Set these as **system environment variables** on the server (same place you set the connection "
                  "string), then restart the web app and API:"},
            {"table": {
                "headers": ["Setting", "Value"],
                "rows": [
                    ["`OpenTrack__Ai__Enabled`", "`true`"],
                    ["`OpenTrack__Ai__Provider`", "`openai`"],
                    ["`OpenTrack__Ai__BaseUrl`", "`http://localhost:11434/v1`"],
                    ["`OpenTrack__Ai__Model`", "`llama3.1:8b`"],
                ],
                "widths": [2.6, 3.9],
            }},
            {"callout": {"kind": "note", "label": "NO API KEY FOR LOCAL",
                         "text": "Leave `OpenTrack__Ai__ApiKey` unset \u2014 a local Ollama model needs no key. The "
                                 "`openai` provider just means \u201cany OpenAI-compatible endpoint,\u201d which Ollama is."}},
            {"steps": [
                "Restart the web app and API (stop the windows / restart the scheduled tasks).",
                "Open **New Issue** in the web app \u2014 you should now see the **\u2728 Suggest with AI** button. Type a "
                "title and click it to confirm the model responds.",
            ]},

            {"h1": "Prefer the cloud instead?"},
            {"p": "You can point OpenTrack at cloud Claude or OpenAI instead of local Ollama. Set "
                  "`OpenTrack__Ai__Provider` to `anthropic` (with an Anthropic key) or `openai` (with an OpenAI key "
                  "and no BaseUrl). Cloud is faster but bills a separate API account and sends issue text off the "
                  "machine. Full details \u2014 including running Ollama on a separate box \u2014 are in "
                  "**docs/guides/AI_ASSIST.md**."},
        ],
    },

    # 4 -------------------------------------------------------------------------------
    {
        "title": "Install the Desktop App on Windows",
        "subtitle": "Install the signed app, or build it from source \u2014 then point it at your server.",
        "in_this_chapter": [
            "Method A: install the signed Windows app (the easy way)",
            "Method B: build it from source (always works, no signing needed)",
            "Point it at the server and sign in",
        ],
        "blocks": [
            {"p": "There are **two ways** to get the Windows desktop app, and they produce the exact same app. "
                  "Use **Method A** if a signed installer is available \u2014 it's a normal double-click install. Use "
                  "**Method B** to build it yourself, which always works even if no signed build is published."},

            {"h1": "Method A \u2014 Install the signed app (recommended)"},
            {"p": "When a **code-signed** Windows build is available, this is the easy path. \u201cCode-signed\u201d "
                  "means the app carries a digital signature from a trusted certificate, so Windows installs it "
                  "without the scary \u201cunknown publisher\u201d warning."},
            {"steps": [
                "On the OpenTrack releases page, download the latest **Windows installer** (a signed installer file, "
                "typically an `.msix` or `.exe`).",
                "Double-click it and follow the prompts. Because it's signed, Windows installs it cleanly.",
                "Launch **OpenTrack** from the Start menu, and skip to \u201cPoint it at the server\u201d below.",
            ]},
            {"callout": {"kind": "note", "label": "SIGNING IS OPTIONAL",
                         "text": "A signed installer requires a paid developer signing certificate (for Windows, a "
                                 "Microsoft/Azure code-signing identity). If a signed build isn't published \u2014 or "
                                 "you'd rather not depend on paying for one \u2014 **Method B** below always works with "
                                 "the free tools and produces the identical app."}},

            {"h1": "Method B \u2014 Build it from source"},
            {"p": "This path needs no installer and no signing \u2014 you build the app with the free Microsoft tools. "
                  "Do this on the **Windows PC** that will run it (which can be the server itself, or any Windows "
                  "machine)."},
            {"steps": [
                "Install the **.NET 10 Software Development Kit (SDK)** and **Git** (same as the server, "
                "Chapter 2 Step B). The SDK is the free Microsoft toolkit that builds .NET apps.",
                "Install the **.NET MAUI (Multi-platform App User Interface)** build components once \u2014 MAUI is the "
                "framework the desktop app is built with:",
            ]},
            {"code": "dotnet workload install maui"},
            {"steps": ["Clone (download) OpenTrack if you haven't already, and build the Windows app:"]},
            {"code": [
                "git clone https://github.com/KE4CON/OpenTrack.git",
                "cd OpenTrack",
                "dotnet build src\\OpenTrack.Desktop -c Release -f net10.0-windows10.0.19041.0",
            ]},
            {"p": "The built app lands under `src\\OpenTrack.Desktop\\bin\\Release\\...`; run the "
                  "`OpenTrack.Desktop.exe` it produces."},
            {"callout": {"kind": "note", "label": "FIRST LAUNCH: \u201cWINDOWS PROTECTED YOUR PC\u201d",
                         "text": "Because a self-built app isn't signed, Windows SmartScreen may show a blue warning "
                                 "the first time you run it. Click **More info \u2192 Run anyway**. (Method A's signed "
                                 "installer avoids this.)"}},

            {"h1": "Point it at the server and sign in"},
            {"steps": [
                "Launch the desktop app.",
                "Open its **Settings** and set the **server address** to your Application Programming Interface "
                "(API) endpoint: **http://SERVER-IP:5003** (the API port from Chapter 2).",
                "Sign in with the account you created on the server. You're in \u2014 same projects and issues as the "
                "web app, in a native window.",
            ]},
            {"callout": {"kind": "tip", "label": "THE ADDRESS IS REMEMBERED PER MACHINE",
                         "text": "Each computer stores its own server address, so you set it once. If the server's "
                                 "address ever changes, just update it here."}},
        ],
    },

    # 5 -------------------------------------------------------------------------------
    {
        "title": "Install the Desktop App on macOS",
        "subtitle": "Install the signed Mac app, or build it from source \u2014 then point it at your server.",
        "in_this_chapter": [
            "Method A: install the signed Mac app (the easy way)",
            "Method B: build it from source on a Mac (always works)",
            "Point it at the server and sign in",
        ],
        "blocks": [
            {"p": "As on Windows, there are **two ways** to get the Mac app \u2014 the same app either way. Use "
                  "**Method A** if a signed, notarized build is available; otherwise use **Method B** to build it "
                  "yourself on the Mac."},

            {"h1": "Method A \u2014 Install the signed app (recommended)"},
            {"p": "When a build signed with an **Apple Developer ID** and **notarized** (checked and stamped by "
                  "Apple) is available, macOS opens it normally \u2014 no warnings."},
            {"steps": [
                "On the OpenTrack releases page, download the latest **Mac app** (a `.dmg` disk image or `.app`).",
                "Open the `.dmg` and drag **OpenTrack** into your **Applications** folder.",
                "Launch it from Applications, and skip to \u201cPoint it at the server\u201d below.",
            ]},
            {"callout": {"kind": "note", "label": "SIGNING IS OPTIONAL",
                         "text": "A signed, notarized Mac build requires a paid **Apple Developer** account. If one "
                                 "isn't published \u2014 or you'd rather not depend on paying for it \u2014 **Method B** below "
                                 "always works and builds the identical app."}},

            {"h1": "Method B \u2014 Build it from source (on a Mac)"},
            {"callout": {"kind": "important", "label": "MAC APPS ARE BUILT ON A MAC",
                         "text": "Apple requires the Mac version to be built on macOS \u2014 it can't be built from the "
                                 "Windows server. Do these steps on the **Mac** itself."}},
            {"steps": [
                "On the Mac, install the **.NET 10 Software Development Kit (SDK)** (the macOS installer from "
                "Microsoft) and **Git** (it comes with the Xcode Command Line Tools; run `xcode-select --install` "
                "if needed).",
                "Install the **.NET MAUI (Multi-platform App User Interface)** build components once:",
            ]},
            {"code": "dotnet workload install maui"},
            {"steps": ["Clone (download) OpenTrack and build the Mac app:"]},
            {"code": [
                "git clone https://github.com/KE4CON/OpenTrack.git",
                "cd OpenTrack",
                "dotnet build src/OpenTrack.Desktop -c Release -f net10.0-maccatalyst",
            ]},
            {"p": "The build produces an **OpenTrack.Desktop.app** under "
                  "`src/OpenTrack.Desktop/bin/Release/net10.0-maccatalyst/`."},
            {"callout": {"kind": "note", "label": "FIRST LAUNCH: RIGHT-CLICK \u2192 OPEN",
                         "text": "Because a self-built app isn't signed/notarized, macOS Gatekeeper blocks a normal "
                                 "double-click the first time. **Right-click (or Control-click) the app \u2192 Open**, then "
                                 "confirm \u2014 you only do this once. (Method A's signed build avoids this.)"}},

            {"h1": "Point it at the server and sign in"},
            {"steps": [
                "Launch the app and open its **Settings**.",
                "Set the **server address** to your Application Programming Interface (API) endpoint: "
                "**http://SERVER-IP:5003**.",
                "Sign in with your account. Done \u2014 the Mac now shows the same data as everything else.",
            ]},
        ],
    },

    # 6 -------------------------------------------------------------------------------
    {
        "title": "Access from Tablets, Phones & Other Computers",
        "subtitle": "No install needed \u2014 just a browser. Plus a one-tap \u201capp\u201d icon for tablets.",
        "in_this_chapter": [
            "Open OpenTrack in any browser on the network",
            "Add it to a tablet's home screen as an installable web app",
            "Where the browser fits (Linux, Raspberry Pi, phones)",
        ],
        "blocks": [
            {"h1": "Open it in a browser"},
            {"p": "Any device on the same network \u2014 an iPad, an Android tablet, a phone, a Linux laptop, a "
                  "Raspberry Pi \u2014 can use OpenTrack with **no install at all**. Just open a browser to:"},
            {"code": "http://SERVER-IP:5035"},
            {"p": "Sign in with your account. That's it. This is the universal way in, and it's the *only* option "
                  "on devices that can't run the native desktop app (Linux, Raspberry Pi, phones, tablets)."},

            {"h1": "Add it to a tablet's home screen"},
            {"p": "OpenTrack is a **Progressive Web App (PWA)**, so a tablet can pin it to the home screen and open it "
                  "like a real app \u2014 full screen, its own icon, and it even keeps working through brief network "
                  "drops for checklists."},
            {"steps": [
                "**iPad (Safari):** open the URL, tap the **Share** button, then **Add to Home Screen**.",
                "**Android (Chrome):** open the URL, tap the **\u22ee** menu, then **Install app** / **Add to Home "
                "screen**.",
                "Launch it from the new icon \u2014 it opens full-screen, like an installed app.",
            ]},
            {"callout": {"kind": "tip", "label": "GREAT FOR RUNNING A CHECKLIST",
                         "text": "The tablet PWA is ideal for walking a project's bug-hunt checklist \u2014 you can "
                                 "check items off even if the Wi-Fi hiccups, and it syncs when you're back online."}},
        ],
    },

    # 7 -------------------------------------------------------------------------------
    {
        "title": "First-Run Configuration",
        "subtitle": "Create your first project, add people, and turn on the features you want.",
        "in_this_chapter": [
            "Create a project and invite people (with the right roles)",
            "Optional power features: SLA targets, automation, Git, public intake + QR",
        ],
        "blocks": [
            {"h1": "Create your first project"},
            {"steps": [
                "Sign in as the administrator.",
                "Go to **Projects \u2192 New project**, give it a name (e.g. the name of the software or system you're "
                "tracking bugs for), and save.",
                "Open the project and add a couple of **categories** and, if you use them, **versions** under "
                "**Settings**.",
            ]},
            {"h1": "Add people and roles"},
            {"p": "Invite the people who'll use OpenTrack and give each the right **role** on each project. Roles "
                  "range from **Reporter** (can file issues) up through **Manager** (can configure the project) and "
                  "**Administrator** (runs the whole instance)."},
            {"h1": "Optional power features"},
            {"p": "None of these are required, but they're what make OpenTrack shine. Turn on the ones you want, "
                  "per project, under the project's **Settings**:"},
            {"table": {
                "headers": ["Feature", "What it does", "Where"],
                "rows": [
                    ["**SLA (Service-Level Agreement) targets**", "Flags issues at-risk/breached by priority; escalates breaches", "Settings \u2192 SLA targets"],
                    ["**Automation**", "\u201cWhen a new issue matches \u2192 do\u201d (auto-tag/assign/set)", "Settings \u2192 Automation"],
                    ["**Git integration**", "Link commits to issues; auto-resolve on \u201cfixes #123\u201d", "Settings \u2192 Git"],
                    ["**Public intake + QR**", "Let anyone report a problem via a link or a scanned QR (Quick Response) code poster", "Settings \u2192 Public intake"],
                ],
                "widths": [1.7, 3.1, 1.7],
            }},
            {"callout": {"kind": "note", "label": "EACH HAS ITS OWN GUIDE",
                         "text": "The full how-to for the public trouble-ticket intake is in "
                                 "docs/guides/PUBLIC_TICKETS.md, and AI is in docs/guides/AI_ASSIST.md. The upcoming "
                                 "**User Manual** covers every feature in depth."}},
        ],
    },

    # 8 -------------------------------------------------------------------------------
    {
        "title": "Backups & Keeping It Running",
        "subtitle": "Turn on automatic backups, know how to restore, and keep OpenTrack updated.",
        "in_this_chapter": [
            "Turn on automatic database backups",
            "How to restore from a backup",
            "Updating OpenTrack and a quick troubleshooting checklist",
        ],
        "blocks": [
            {"h1": "Turn on automatic backups"},
            {"p": "OpenTrack can write a **consistent snapshot** of the database on a schedule (safe to run while "
                  "the app is live). Set these system environment variables on the server and restart the web app:"},
            {"table": {
                "headers": ["Setting", "Value", "Meaning"],
                "rows": [
                    ["`OpenTrack__Backup__Enabled`", "`true`", "Turn backups on"],
                    ["`OpenTrack__Backup__IntervalHours`", "`24`", "How often (hours)"],
                    ["`OpenTrack__Backup__Directory`", "`D:\\OpenTrackData\\backups`", "Where snapshots go"],
                    ["`OpenTrack__Backup__Retention`", "`14`", "How many to keep"],
                ],
                "widths": [2.7, 1.9, 1.9],
            }},
            {"p": "Snapshots are named like `opentrack-20260812-013000.db`. Keeping them on **D:** puts your app "
                  "*and* its backups on the drive that survives a Windows reinstall."},

            {"h1": "How to restore"},
            {"steps": [
                "Stop the web app and API.",
                "Copy the snapshot you want over the live database, e.g. copy "
                "`D:\\OpenTrackData\\backups\\opentrack-YYYYMMDD-HHMMSS.db` to "
                "`D:\\OpenTrackData\\opentrack.db` (replacing it).",
                "Start the web app and API again. You're back to that point in time.",
            ]},
            {"callout": {"kind": "warning", "label": "BACK UP OFF THE BEELINK TOO",
                         "text": "Scheduled snapshots protect against mistakes and corruption, but they're on the "
                                 "same machine. For real safety, periodically copy the `backups` folder to another "
                                 "drive or a cloud folder \u2014 so a dead server doesn't take your data with it."}},

            {"h1": "Updating OpenTrack"},
            {"p": "Easiest: **re-run the setup script** from Chapter 2 (`Install-OpenTrackServer.ps1`). It pulls "
                  "the latest code, rebuilds both programs, and restarts them \u2014 the database upgrades itself on "
                  "first start, with no manual database steps."},
            {"p": "By hand instead: in your OpenTrack folder run `git pull`, re-publish both programs (Chapter 2's "
                  "manual Step D), and restart the web app and API."},

            {"h1": "Quick troubleshooting"},
            {"table": {
                "headers": ["Symptom", "Check this"],
                "rows": [
                    ["Another device can't reach the server", "Firewall rules added (Ch. 2 Step G); using the right **SERVER-IP** and port; both devices on the same network"],
                    ["Web app loads on the server but not elsewhere", "Programs started with `0.0.0.0` URLs, not `localhost`"],
                    ["Desktop app can't connect", "Its server address points at the **API** port (5003), not the web port"],
                    ["\u2728 AI button missing", "`OpenTrack__Ai__Enabled=true` and the model/endpoint set; app restarted"],
                    ["Server didn't come back after reboot", "The two scheduled tasks exist and set the port (Ch. 2 Step I)"],
                ],
                "widths": [2.4, 4.1],
            }},
            {"callout": {"kind": "note", "label": "YOU'RE DONE",
                         "text": "Your server is serving OpenTrack to your whole network, your Windows and Mac apps "
                                 "connect to it, tablets can scan and report, and (optionally) a private local AI is "
                                 "helping triage. Next up: the **User Manual** walks through every feature in detail."}},
        ],
    },
]


def build():
    doc = S.new_document(
        header_title="OpenTrack — Installation Guide",
        header_sub="Self-hosted issue & bug tracking  ·  server + Windows/macOS clients",
        footer_left="OpenTrack  ·  Open-source (AGPL v3)  ·  KE4CON",
    )
    S.cover(
        doc,
        kicker="OPENTRACK",
        big_title="OpenTrack",
        subtitle="Self-Hosted Issue Tracker",
        doc_kind="INSTALLATION GUIDE",
        version="v1.0",
        tagline="Set up the server, the Windows & macOS apps, tablets, and local AI \u2014 step by step.",
        author="James Rospopo  ·  KE4CON",
        date_str=TODAY,
    )
    S.section_title(doc, "Contents")
    S.toc(doc)
    for i, ch in enumerate(CHAPTERS, 1):
        S.render_chapter(doc, ch, i)

    out = os.environ.get("OT_OUT") or os.path.join(
        os.path.dirname(__file__), "..", "docs", "guides", "OpenTrack_Installation_Guide.docx")
    out = os.path.abspath(out)
    doc.save(out)
    print("wrote", out)


if __name__ == "__main__":
    build()
