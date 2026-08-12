#!/usr/bin/env python
# OpenTrack — User Manual generator (styled Word .docx, house style via style.py).
#
#   pip install python-docx
#   python user_manual_build.py     # writes ../docs/guides/OpenTrack_User_Manual.docx
#
# Content is CHAPTERS below; each block is rendered by style.render_chapter, so a wording change is a
# one-line edit and a bad block can only affect itself. Two house rules are honored throughout:
#   * every acronym is spelled out in full on first use, then the short form;
#   * the manual is self-contained — feature setup (AI, Git, backups) is explained here, not punted
#     to another document.

import os
import datetime
import style as S

TODAY = datetime.date.today().strftime("%B %d, %Y")

CHAPTERS = [
    # 1 ------------------------------------------------------------------
    {
        "title": "Welcome & Getting Around",
        "subtitle": "What OpenTrack is, how to sign in, and a guided tour of the screen.",
        "in_this_chapter": [
            "What OpenTrack does and why it's built this way",
            "Creating your account and signing in",
            "A tour of every part of the screen",
            "Web app vs. desktop app — and pointing the desktop app at your server",
        ],
        "blocks": [
            {"h1": "What OpenTrack is"},
            {"p": "OpenTrack is a **self-hosted issue and bug tracker** — a private, organized place to record "
                  "problems (bugs, tasks, feature requests, support tickets), decide what matters most, assign the "
                  "work, and follow each item through to resolution. Think of it as a shared, searchable memory for "
                  "everything that's wrong, planned, or in progress across your projects, so nothing lives only in "
                  "someone's head or an email thread."},
            {"p": "“Self-hosted” means it runs on **your own** server (a small mini-PC on your network, for "
                  "example) rather than on someone else's cloud service. The practical upshot: your data stays with "
                  "you, there's no per-user subscription, and you decide who can reach it. OpenTrack is "
                  "**open-source** software released under the GNU Affero General Public License version 3 (AGPL "
                  "v3), so you're free to run, inspect, and modify it."},
            {"p": "You'll use OpenTrack one of two ways, and they look and behave the same — everything in this "
                  "manual applies to both:"},
            {"bullets": [
                "**The web app** — open a web address in any browser (Chrome, Edge, Safari, Firefox) on a computer, "
                "tablet, or phone. Nothing to install.",
                "**The desktop app** — a native application for Windows or Mac that talks to the same server. Handy "
                "if you'd rather launch an app than a browser.",
            ]},
            {"callout": {"kind": "note", "label": "IF IT ISN'T INSTALLED YET",
                         "text": "Someone has to set up the OpenTrack **server** once before anyone can sign in — "
                                 "that one-time job (choosing the mini-PC, installing the software, opening the "
                                 "network port) is covered start to finish in the separate **Installation Guide**. "
                                 "This manual assumes the server is already running and you have its web address."}},
            {"h1": "Creating your account and signing in"},
            {"p": "OpenTrack keeps its own list of accounts — an email address and a password per person. You don't "
                  "need any outside account (no Google, no Microsoft) to use it."},
            {"steps": [
                "Open OpenTrack — the web address your administrator gave you (something like "
                "`http://192.168.1.50:5003`), or the desktop app.",
                "**If you already have an account:** type your email and password and select **Log in**.",
                "**If it's your first time and registration is open:** select **Register**, enter your email and a "
                "password (twice), and submit. You're taken straight in.",
            ]},
            {"screenshot": "The OpenTrack sign-in page"},
            {"callout": {"kind": "important", "label": "THE FIRST ACCOUNT BECOMES THE ADMINISTRATOR",
                         "text": "On a brand-new server, the **very first account registered** automatically "
                                 "becomes the **Administrator** — the person who runs the whole instance. If that's "
                                 "you, guard that account. (An administrator can also be set up ahead of time by "
                                 "whoever installs the server, so it doesn't depend on who registers first.)"}},
            {"callout": {"kind": "note", "label": "ABOUT THAT CONFIRMATION EMAIL",
                         "text": "Many self-hosted OpenTrack setups don't send email at all (there's no mail server "
                                 "on a home network by default). That's fine — **confirming your email is not "
                                 "required to sign in**. If a “confirm your account” link never arrives, you can "
                                 "still log in and use everything normally."}},
            {"h1": "A tour of the screen"},
            {"p": "Once you're in, the window has three areas. Get comfortable with these and you'll always know "
                  "where to look:"},
            {"table": {
                "headers": ["Area", "What lives there"],
                "rows": [
                    ["**Left navigation**", "The main menu: Dashboard, Projects, Issues, Reports, the SLA status "
                                            "board, Notifications, Backup & export, Import, and your Preferences. "
                                            "This is how you move between the big areas of the app."],
                    ["**Top bar**", "The title of the page you're on, and — on list pages — a search box and the "
                                    "main action buttons (like New Issue)."],
                    ["**Main area**", "The actual content: a list of issues, a single issue, a project page, a "
                                      "chart, and so on. This is where you do the work."],
                ],
                "widths": [1.7, 4.8],
            }},
            {"screenshot": "The main screen with the left navigation, top bar, and main area labeled"},
            {"callout": {"kind": "tip", "label": "YOUR ROLE DECIDES WHAT YOU SEE",
                         "text": "OpenTrack only shows you the buttons your **role** allows. A Reporter sees the "
                                 "controls to file and comment on issues; a Manager also sees a project's Settings; "
                                 "an Administrator sees everything. So if a button described in this manual isn't on "
                                 "your screen, it's almost always because your role doesn't include it — see the "
                                 "*People & Roles* chapter. The same is true of optional features (AI, Git, SLAs, "
                                 "public intake): their buttons only appear once someone turns them on."}},
            {"h1": "Desktop app: which server it talks to"},
            {"p": "The desktop app needs to know your server's web address. It ships with a default, and you can "
                  "change it anytime:"},
            {"steps": [
                "Open the desktop app's **Settings** from its menu.",
                "Type the server address — for example `http://192.168.1.50:5003`, or an `https://…` address if "
                "your server uses encryption.",
                "Save. The change takes effect on your next action — no reinstall needed. The address is remembered "
                "on that computer.",
            ]},
        ],
    },

    # 2 ------------------------------------------------------------------
    {
        "title": "The Dashboard",
        "subtitle": "Your at-a-glance answer to “where should I look first?”",
        "in_this_chapter": [
            "What each dashboard tile means",
            "Understanding “open,” “overdue,” and “stale”",
            "Jumping from the dashboard straight into the work",
        ],
        "blocks": [
            {"h1": "What the dashboard is for"},
            {"p": "The **Dashboard** is your home base — the first thing to open at the start of a session. It "
                  "gathers, in one place, a summary of everything you're allowed to see across **all** your "
                  "projects, so you can spot where attention is needed without opening each project one by one."},
            {"h1": "Reading the tiles"},
            {"table": {
                "headers": ["Tile", "What it tells you"],
                "rows": [
                    ["**Totals**", "How many issues are **open**, **overdue**, and **stale** across every project "
                                   "you can see — the big picture in three numbers."],
                    ["**By project**", "Open and overdue counts broken down per project, so you can see which "
                                       "project is carrying the load."],
                    ["**Open by severity**", "How the open work splits across severities — a quick way to notice a "
                                             "pile of serious ones hiding inside a healthy-looking total."],
                    ["**Recent activity**", "The issues that changed most recently, as one-click links — a fast way "
                                            "back to whatever you or your team just touched."],
                ],
                "widths": [1.9, 4.6],
            }},
            {"screenshot": "The Dashboard with its tiles"},
            {"callout": {"kind": "note", "label": "THREE WORDS WORTH KNOWING",
                         "text": "**Open** — the issue isn't resolved or closed yet. **Overdue** — it has a due "
                                 "date that has already passed and it's still open. **Stale** — it's open but "
                                 "nobody has touched it in a while, so it may have quietly fallen through the "
                                 "cracks. Stale is about *silence*, overdue is about a *deadline* — an issue can be "
                                 "one, both, or neither."}},
            {"h1": "Using it"},
            {"steps": [
                "Select **Dashboard** in the left navigation.",
                "Scan the three totals first, then look at *Open by severity* for anything alarming.",
                "Select a project row, a severity, or a *Recent* item to jump straight into that slice of work.",
            ]},
        ],
    },

    # 3 ------------------------------------------------------------------
    {
        "title": "Projects & Their Settings",
        "subtitle": "Create a project, then tune its categories, versions, and options.",
        "in_this_chapter": [
            "What a project is and how to create one",
            "Public vs. private projects",
            "Categories and versions — what they do and how to add them",
            "Where the rest of a project's settings live",
        ],
        "blocks": [
            {"h1": "What a project is"},
            {"p": "A **project** is a container for issues — usually one piece of software, one system, one product, "
                  "or one area of responsibility. Every issue belongs to exactly one project, and access is granted "
                  "**per project**: being a member of one project says nothing about the others. Most of OpenTrack's "
                  "organizing power comes from this simple idea, so it's worth creating a project per real “thing” "
                  "you track rather than dumping everything into one."},
            {"h1": "Create a project"},
            {"steps": [
                "Select **Projects** in the left navigation, then **New project**.",
                "Enter a **name** and an optional **description** (a sentence on what it covers helps newcomers).",
                "Choose whether it's **public** or **private** (explained just below).",
                "Select **Create**. You land on the new project's page, ready to add issues and settings.",
            ]},
            {"screenshot": "Creating a new project"},
            {"callout": {"kind": "note", "label": "PUBLIC VS. PRIVATE",
                         "text": "A **public** project is visible to any signed-in user of your OpenTrack (good for "
                                 "shared, non-sensitive work). A **private** project is visible only to the people "
                                 "you add as members. This is separate from the *public trouble-ticket intake* "
                                 "feature (covered later), which lets people **without any account** file a ticket."}},
            {"h1": "Categories and versions"},
            {"p": "Two optional lists make a project's issues much easier to organize and report on. Both are set up "
                  "on the project's **Settings** page (select **Settings** on the project):"},
            {"bullets": [
                "**Categories** — buckets you file issues under, like “User Interface,” “Database,” "
                "“Documentation,” or “Hardware.” Categories let you filter and group; pick a handful that "
                "match how you actually think about the project.",
                "**Versions** — the releases of your software or product. An issue can record the version it "
                "**affects** and the version it's **fixed in**. Those two facts are what power the **Roadmap** "
                "(what's coming) and the **Changelog** (what shipped), so filling them in pays off later.",
            ]},
            {"steps": [
                "On the project, select **Settings**.",
                "Under **Categories**, type a name and select **Add**. Repeat for each. (Select a category and "
                "**Delete** to remove one.)",
                "Under **Versions**, enter a name (for example `1.0`), an optional description and release date, "
                "and tick **Released** once it has shipped. Select **Add version**.",
            ]},
            {"screenshot": "The project Settings page showing Categories and Versions"},
            {"callout": {"kind": "tip", "label": "THE SETTINGS PAGE IS THE PROJECT'S CONTROL PANEL",
                         "text": "Beyond Categories and Versions, the Settings page (and the buttons across its top) "
                                 "is where a Manager sets up **Members, Custom fields, Automation, Service-Level "
                                 "Agreement (SLA) targets, Workflow rules, outgoing integrations, Git, and public "
                                 "intake**. Each of those has its own chapter in this manual — this is just where "
                                 "you'll find them."}},
        ],
    },

    # 4 ------------------------------------------------------------------
    {
        "title": "People & Roles",
        "subtitle": "Add people to a project and give each exactly the access they need.",
        "in_this_chapter": [
            "What each role can and can't do",
            "Per-project roles vs. the global Administrator",
            "Adding, changing, and removing members",
        ],
        "blocks": [
            {"h1": "How access works"},
            {"p": "OpenTrack grants access by **role**, and — except for the Administrator — roles are assigned "
                  "**per project**. So the same person can be a Developer on one project, a Manager on another, and "
                  "have no access at all to a third. This keeps sensitive projects genuinely private while letting "
                  "you open others up widely."},
            {"h1": "The roles, from least to most powerful"},
            {"table": {
                "headers": ["Role", "What it adds (each includes everything above it)"],
                "rows": [
                    ["**Reporter**", "File new issues, and read and comment on the issues they're allowed to see"],
                    ["**Updater**", "Also edit issues — change fields, add tags, tidy details"],
                    ["**Developer**", "Also be **assigned** issues and move them through the workflow (the person "
                                      "who actually does the work)"],
                    ["**Manager**", "Also configure the project: members, categories, versions, custom fields, "
                                    "automation, SLA targets, workflow, integrations, Git, and public intake"],
                    ["**Administrator**", "A **global** role, not per-project: runs the whole instance — every "
                                          "project, every user account, and the server-level settings"],
                ],
                "widths": [1.5, 5.0],
            }},
            {"callout": {"kind": "note", "label": "PICK THE LOWEST ROLE THAT FITS",
                         "text": "A good habit: give each person the least powerful role that still lets them do "
                                 "their job. Someone who only files bugs is a Reporter; someone who fixes them is a "
                                 "Developer; reserve Manager for the one or two people who actually configure the "
                                 "project. You can always raise a role later."}},
            {"h1": "Add someone to a project"},
            {"steps": [
                "Open the project and select **Members** (Managers and Administrators only).",
                "Enter the person's email (they need an OpenTrack account), choose their **role**, and select "
                "**Add**.",
                "To change someone's access later, pick a new role next to their name; to remove them, select "
                "**Remove**. Changes take effect immediately.",
            ]},
            {"screenshot": "The project Members screen showing people and their roles"},
            {"callout": {"kind": "important", "label": "PRIVACY IS ENFORCED EVERYWHERE, NOT JUST IN LISTS",
                         "text": "OpenTrack never reveals an issue — or even that it exists — to anyone whose role "
                                 "doesn't allow it. That rule holds in the issue list, in search results, in "
                                 "reports, in the dashboard totals, and even if someone is handed a direct link. "
                                 "There's no “hidden but linkable” back door."}},
        ],
    },

    # 5 ------------------------------------------------------------------
    {
        "title": "Reporting an Issue",
        "subtitle": "File a new issue with all the detail that makes it fixable — plus the fast and assisted ways.",
        "in_this_chapter": [
            "The New Issue form, field by field, with what each choice means",
            "Writing a description that helps (Markdown, logs, screenshots-to-come)",
            "Optional: AI triage suggestions and attaching your location",
            "The fast lane: Quick-capture, and catching duplicates before you file",
        ],
        "blocks": [
            {"h1": "Filing a good issue"},
            {"p": "A well-written issue saves everyone time later. You don't have to fill in every field — only a "
                  "**title** and **description** are required — but the more context you give, the faster it gets "
                  "resolved. Here's the whole form."},
            {"steps": [
                "Open the project and select **New Issue**.",
                "Fill in the fields below (only Title and Description are required):",
            ]},
            {"table": {
                "headers": ["Field", "What to put, and why it matters"],
                "rows": [
                    ["**Title**", "A short, specific summary. “Export button does nothing on the Reports page” "
                                  "beats “export broken.” This is what everyone sees in lists."],
                    ["**Description**", "The full story. Supports **Markdown** formatting (see below) — great for "
                                        "pasting logs, error messages, and stack traces in code blocks."],
                    ["**Steps to Reproduce**", "A numbered recipe to make the problem happen. The single most "
                                               "useful thing you can provide for a bug."],
                    ["**Expected Behavior / Actual Behavior**", "What *should* happen versus what *does*. The gap "
                                                                "between them is the bug, stated plainly."],
                    ["**Category**", "The project bucket it belongs to (from the project's category list). Optional "
                                     "but helps filtering."],
                    ["**Severity**", "How *bad* the impact is if it happens — from **Feature** (a request), through "
                                     "Trivial / Text / Tweak / Minor / Major, up to **Crash** and **Block** (work "
                                     "can't continue). Severity is about impact, not urgency."],
                    ["**Priority**", "How *urgent* it is to act — None, Low, Normal, High, Urgent, Immediate. A typo "
                                     "might be low severity but high priority (it's on the front page); a rare crash "
                                     "might be high severity but low priority."],
                    ["**Reproducibility**", "How reliably you can trigger it — Always, Sometimes, Random, "
                                            "Have-not-tried, Unable-to-reproduce, N/A. Tells whoever fixes it what "
                                            "they're up against."],
                    ["**Due Date**", "An optional target date. Overdue open issues are flagged on the dashboard and "
                                     "in lists."],
                    ["**Affects / Fixed-in Version**", "Which release shows the problem, and (later) which release "
                                                       "resolves it. Powers the Roadmap and Changelog."],
                ],
                "widths": [2.0, 4.5],
            }},
            {"steps": [
                "Select **Submit Issue**. You land on the new issue's own page, where all the richer tools "
                "(notes, attachments, relationships, and so on) live.",
            ]},
            {"screenshot": "The New Issue form with the fields filled in"},
            {"h1": "Writing the description (Markdown)"},
            {"p": "The Description and Notes understand **Markdown**, a simple way to format plain text. You don't "
                  "have to use any of it, but a little goes a long way:"},
            {"table": {
                "headers": ["Type this", "To get"],
                "rows": [
                    ["`**important**`", "**important** (bold)"],
                    ["`` `Login.cs` ``", "`Login.cs` (inline code / file names)"],
                    ["a line of three backticks, your log, then three backticks", "a shaded **code block** that "
                     "keeps logs and stack traces neatly monospaced"],
                    ["`- first` / `- second` on their own lines", "a bulleted list"],
                ],
                "widths": [3.2, 3.3],
            }},
            {"h1": "Optional: let the AI suggest the triage"},
            {"p": "If your administrator has turned on the **artificial intelligence (AI)** assistant, a "
                  "**✨ Suggest with AI** button appears on this page. Type a Title and Description, select it, and "
                  "OpenTrack proposes a severity, priority, category, and tags for you to accept or change — a "
                  "helpful starting point when you're not sure how to classify something. It's always a suggestion; "
                  "you stay in control. The *AI Assistant* chapter explains it fully, including turning it on."},
            {"screenshot": "The New Issue page showing the Suggest with AI button and its proposed values"},
            {"h1": "Optional: attach your location"},
            {"p": "For a problem tied to a physical place — a device in the field, a site inspection, a piece of "
                  "equipment — select **📍 Attach my location**. Your browser or device asks permission first, then "
                  "OpenTrack records the coordinates on the issue and shows a **view map** link. Nobody is tracked; "
                  "it only captures the spot at the moment you tap the button."},
            {"h1": "The fast lane: Quick-capture"},
            {"p": "When you just need to get a bug written down before you forget it, use **Quick-capture** (in the "
                  "left navigation). It asks only for the **project, a title, and a short description** — you or "
                  "someone else can flesh out severity, steps, and the rest later. Great for capturing things on a "
                  "phone or mid-meeting."},
            {"screenshot": "The Quick-capture screen"},
            {"callout": {"kind": "tip", "label": "IT WATCHES FOR DUPLICATES AS YOU TYPE",
                         "text": "As you type a **Title**, OpenTrack quietly searches for **similar existing "
                                 "issues** and lists any it finds. If your problem is already filed, jump to it "
                                 "instead of creating a duplicate — less clutter, and the discussion stays in one "
                                 "place."}},
        ],
    },

    # 6 ------------------------------------------------------------------
    {
        "title": "The Issue Page — a Complete Tour",
        "subtitle": "Everything you can see and do on a single issue, section by section.",
        "in_this_chapter": [
            "The header and the key fields at a glance",
            "Every section down the page and what it holds",
            "The actions along the top",
        ],
        "blocks": [
            {"h1": "The top of the page"},
            {"p": "Opening an issue shows its **number** (a permanent identifier you can quote, like #123), its "
                  "title, and its project at the top. Just below sits a compact panel of the fields that describe "
                  "its current state:"},
            {"bullets": [
                "**Status** — where it is in its life (New, Acknowledged, Confirmed, Assigned, Resolved, Closed, "
                "and so on).",
                "**Severity, Priority, Reproducibility** — the classifications you set when filing it.",
                "**Reporter, Assignee, Category** — who filed it, who's working it, and its bucket.",
                "**Updated** — when it last changed, so you can tell fresh from stale at a glance.",
                "**Location** (if attached) — coordinates with a **view map** link.",
                "**Service-Level Agreement (SLA) badge** (if targets are set for the project) — **On track**, **At "
                "risk**, or **Breached**, telling you how it's doing against its resolution deadline.",
            ]},
            {"screenshot": "The top of an issue page showing the number, title, and key fields"},
            {"h1": "The sections down the page"},
            {"p": "Below the fields, each part of the issue has its own clearly labeled section. You'll return to "
                  "these constantly:"},
            {"table": {
                "headers": ["Section", "What it holds", "Covered in"],
                "rows": [
                    ["**Description / Steps / Expected / Actual**", "The full write-up, formatted from your Markdown", "This chapter"],
                    ["**Attachments**", "Files added to the issue (logs, screenshots, sample data)", "Notes, Attachments… chapter"],
                    ["**Notes**", "The running discussion — comments from you and everyone else", "Notes, Attachments… chapter"],
                    ["**Relationships**", "Links to related issues (blocks, duplicate of, related to)", "Notes, Attachments… chapter"],
                    ["**Tags**", "Free-form labels for grouping and filtering", "Notes, Attachments… chapter"],
                    ["**Custom fields**", "Any extra fields this project defines", "Custom Fields chapter"],
                    ["**Linked commits**", "Code commits that referenced this issue (if Git is on)", "Git Integration chapter"],
                    ["**Time log**", "Work logged against the issue, with totals", "Notifications… chapter"],
                    ["**History**", "A dated, automatic trail of every change ever made", "This chapter"],
                ],
                "widths": [2.4, 3.1, 1.0],
            }},
            {"callout": {"kind": "note", "label": "HISTORY IS AUTOMATIC AND COMPLETE",
                         "text": "You never have to update the **History** — OpenTrack records every field change, "
                                 "status move, assignment, and note automatically, with who and when. It's the "
                                 "issue's paper trail, and it can't be edited away."}},
            {"h1": "The actions along the top"},
            {"p": "Near the issue's title you'll find its action buttons — the exact set depends on your role:"},
            {"bullets": [
                "**Edit** — change any field (see the next chapter).",
                "**Monitor** — get notified whenever this issue changes (see the Notifications chapter).",
                "**Print / PDF** — open a clean, print-friendly version to print or save as a Portable Document "
                "Format (PDF) file (see the Printing chapter).",
            ]},
        ],
    },

    # 7 ------------------------------------------------------------------
    {
        "title": "Working an Issue",
        "subtitle": "Edit fields, move it through its statuses, assign it, and resolve or close it.",
        "in_this_chapter": [
            "Editing an issue's fields",
            "The status life-cycle and what each status means",
            "Assigning work, and resolving with the right resolution",
        ],
        "blocks": [
            {"h1": "Editing an issue"},
            {"steps": [
                "On the issue page, select **Edit**.",
                "Change any field — title, description, severity, priority, category, due date, versions, and (with "
                "the right role) the **assignee**, the **private** flag, and the **sticky** flag.",
                "Select **Save**. Every change is written to the issue's History automatically.",
            ]},
            {"screenshot": "Editing an issue"},
            {"callout": {"kind": "note", "label": "PRIVATE AND STICKY",
                         "text": "**Private** hides an issue from lower-access viewers (for example a public "
                                 "reporter) while keeping it visible to the project team — use it for anything "
                                 "sensitive. **Sticky** pins an important issue to the top of the list so it doesn't "
                                 "scroll away."}},
            {"h1": "The status life-cycle"},
            {"p": "An issue moves through **statuses** as work progresses. You change the status on the Edit screen. "
                  "The usual path is:"},
            {"table": {
                "headers": ["Status", "Means"],
                "rows": [
                    ["**New**", "Just filed; nobody has looked at it yet"],
                    ["**Feedback**", "Waiting on more information from the reporter"],
                    ["**Acknowledged**", "Seen and accepted as something to look at"],
                    ["**Confirmed**", "Reproduced / verified as real"],
                    ["**Assigned**", "Handed to a specific person to work"],
                    ["**Resolved**", "The work is done (with a resolution — see below)"],
                    ["**Closed**", "Confirmed resolved and filed away"],
                ],
                "widths": [1.7, 4.8],
            }},
            {"callout": {"kind": "note", "label": "YOUR PROJECT MAY RESTRICT THE MOVES",
                         "text": "By default any status change is allowed. If a Manager has set up **workflow "
                                 "rules** (see the *Workflow Rules* chapter), only the transitions they've allowed "
                                 "appear — this keeps issues flowing through your process in the intended order."}},
            {"h1": "Assigning work"},
            {"p": "Set the **Assignee** (on the Edit screen) to the Developer who'll handle it. The assignee is "
                  "automatically notified and the issue shows up as theirs across the app — on their dashboard, in "
                  "filters, and on the board. Assigning is how work stops being “someone should” and becomes "
                  "“this person will.”"},
            {"h1": "Resolving and closing"},
            {"steps": [
                "When the work is done, set the status to **Resolved** and choose a **Resolution** that says *how* "
                "it ended: Fixed, Won't-fix, Duplicate, No-change-required, Unable-to-reproduce, Suspended, and so "
                "on.",
                "Optionally set the **Fixed-in version** so it shows up in that release's Changelog.",
                "Once everyone's satisfied, set the status to **Closed**.",
            ]},
            {"p": "Resolved and Closed issues drop out of the “open” counts and default lists, but they're never "
                  "deleted — they stay fully searchable, and their History is intact, so you can always see how a "
                  "past problem was handled."},
        ],
    },

    # 8 ------------------------------------------------------------------
    {
        "title": "Notes, Attachments, Tags & Relationships",
        "subtitle": "Discuss an issue, attach files, label it, and connect it to related issues.",
        "in_this_chapter": [
            "Adding notes, formatting them, and keeping some private",
            "Uploading and managing attachments",
            "Tagging issues, and linking related issues together",
        ],
        "blocks": [
            {"h1": "Notes — the discussion"},
            {"p": "**Notes** are the running conversation on an issue: questions, findings, decisions, “tried X, "
                  "didn't work.” Keeping the discussion on the issue (rather than in chat or email) means the whole "
                  "story lives in one place that anyone can catch up on later."},
            {"steps": [
                "On the issue page, scroll to **Notes**, type in the box, and select **Add note**.",
                "Format with **Markdown** if you like — `**bold**`, `` `code` ``, and triple-backtick code blocks "
                "for logs — exactly as in a description.",
                "Tick **private** to make a note visible only to project members with the right role. A private "
                "note is hidden from lower-access viewers, such as someone who filed the issue through the public "
                "intake form.",
            ]},
            {"screenshot": "Adding a note to an issue, with the private option"},
            {"h1": "Attachments"},
            {"p": "Attach the evidence: a log file, a screenshot, a sample document, a configuration that triggers "
                  "the bug."},
            {"steps": [
                "In the **Attachments** section, choose a file and upload it.",
                "Anyone who can see the issue can download it; those with the right role can delete one that's no "
                "longer needed.",
            ]},
            {"h1": "Tags"},
            {"p": "**Tags** are free-form labels — “regression,” “customer-reported,” “needs-design” "
                  "— that cut across categories and projects. In the **Tags** section, type a tag and add it; if "
                  "the tag doesn't exist yet it's created on the spot. Because tags are shared across the whole "
                  "instance, you can later filter every project by the same label."},
            {"h1": "Relationships — linking issues"},
            {"p": "Real work is connected: one bug blocks another, two reports are the same problem, a task belongs "
                  "to a bigger effort. **Relationships** capture those links."},
            {"steps": [
                "In the **Relationships** section, pick the relationship type — for example **blocks**, **is "
                "blocked by**, **duplicate of**, or **related to** — and enter the other issue's number.",
                "Add it. The link appears on **both** issues, worded correctly from each side (if A *blocks* B, "
                "then B shows *is blocked by* A).",
            ]},
            {"screenshot": "The Relationships section linking two issues"},
        ],
    },

    # 9 ------------------------------------------------------------------
    {
        "title": "Custom Fields",
        "subtitle": "Add your own project-specific fields to capture exactly what your team needs.",
        "in_this_chapter": [
            "When custom fields help",
            "Defining them (Managers) and filling them in (everyone)",
        ],
        "blocks": [
            {"h1": "Why custom fields exist"},
            {"p": "The built-in fields (severity, priority, category, and so on) fit most work, but every team has "
                  "something extra it always wants to record. **Custom fields** let a Manager add those to a "
                  "project — for example a **Customer** name, an **Environment** (Production / Staging / Test), a "
                  "**Hardware revision**, or a **Steps-taken** checklist. They appear on every issue in that "
                  "project and are yours to define."},
            {"h1": "Define a custom field (Manager)"},
            {"steps": [
                "On the project, go to **Settings** and select **Custom fields** (across the top).",
                "Add a field: give it a **name** and pick a **type** — such as a line of text, a number, a date, a "
                "yes/no checkbox, or a **dropdown** with a fixed list of options you supply.",
                "Save. The field now appears on that project's issues. You can edit or remove it later.",
            ]},
            {"screenshot": "Defining a custom field"},
            {"h1": "Fill it in"},
            {"p": "On any issue in the project, the custom fields show in their own **Custom fields** section — set "
                  "or change their values there, just like the built-in fields. They're searchable and show in the "
                  "History like everything else."},
        ],
    },

    # 10 -----------------------------------------------------------------
    {
        "title": "Finding Issues: List, Filters & Search",
        "subtitle": "Narrow the list, search every word (including notes), and save the views you use most.",
        "in_this_chapter": [
            "Filtering and sorting the issue list",
            "Full-text search — and why it finds more than you'd expect",
            "Saving filters, sharing them by link, and the keyboard command palette",
        ],
        "blocks": [
            {"h1": "The issue list"},
            {"p": "Select **Issues** in the left navigation to see every issue you're allowed to view, across "
                  "projects. A filter bar runs along the top — this is where most day-to-day finding happens."},
            {"h1": "Filter and sort"},
            {"steps": [
                "Set any combination of **Project, Status, Severity, Priority**, and **Tag**, plus a free **Text** "
                "box, then select **Search**. Filters combine — each one you add narrows the results further.",
                "Choose a **Sort by** order — most-recently-updated, priority, severity, newest, and so on.",
                "Tick **Stale only** to show just the open issues nobody has touched in a while — your “what's "
                "been forgotten?” view.",
            ]},
            {"screenshot": "The issue list with the filter bar and results"},
            {"callout": {"kind": "tip", "label": "SEARCH LOOKS INSIDE THE NOTES, TOO",
                         "text": "The **Text** box searches an issue's title, description, **and every note on it**. "
                                 "So a detail someone mentioned only in a comment three weeks ago still surfaces the "
                                 "issue — you don't have to remember where it was written."}},
            {"h1": "Save a filter, and share it"},
            {"p": "Built a filter you'll want again? Don't rebuild it each time."},
            {"steps": [
                "Set up the filter, type a name in **Save current filter as…**, and select **Save**.",
                "Your saved filters appear as one-click pills above the list.",
                "The web address (URL) updates to match your filter, so you can also **bookmark** a view or **paste "
                "the link** to a colleague — they'll see the same filtered list (limited, of course, to what "
                "they're allowed to view).",
            ]},
            {"h1": "The command palette (keyboard)"},
            {"p": "For fast, mouse-free navigation, OpenTrack has a **command palette** — a quick-jump box you pop "
                  "open from anywhere:"},
            {"steps": [
                "Press **Ctrl + K** (on a Mac, **Cmd + K**).",
                "Start typing — a project name, an issue number or word, or an action. Use the **↑/↓ arrow keys** "
                "to move through the matches and **Enter** to go. Press **Esc** to close it.",
            ]},
            {"screenshot": "The command palette open, showing quick matches"},
        ],
    },

    # 11 -----------------------------------------------------------------
    {
        "title": "The Board (Kanban View)",
        "subtitle": "See a project's work as cards in columns, and move them as they progress.",
        "in_this_chapter": [
            "What the board shows",
            "Moving an issue between columns",
        ],
        "blocks": [
            {"h1": "A visual way to see the work"},
            {"p": "The **Board** (often called a *Kanban* board, a Japanese term for a visual work-tracking card "
                  "wall) shows a project's issues as **cards arranged in columns by status** — for example New, "
                  "Acknowledged, Assigned, Resolved. In one glance you see how much is in each stage and where work "
                  "is piling up, which a flat list can't show as clearly."},
            {"steps": [
                "Open the project and select **Board**.",
                "Read each column top to bottom; the card shows the issue's number, title, and key badges.",
                "Move a card to the next column (using its controls) to change that issue's status — the same as "
                "editing the status, but faster and more visual. (Workflow rules, if set, still apply.)",
            ]},
            {"screenshot": "The Board (Kanban) view with cards in status columns"},
        ],
    },

    # 12 -----------------------------------------------------------------
    {
        "title": "Notifications, Monitoring & Time Logging",
        "subtitle": "Get told when things change, and record the effort you spend.",
        "in_this_chapter": [
            "Monitoring an issue, and what you're notified about automatically",
            "Reading your notifications",
            "Logging time against an issue",
        ],
        "blocks": [
            {"h1": "Monitor an issue"},
            {"p": "**Monitoring** (sometimes called *watching*) means “tell me when this changes.” Select "
                  "**Monitor** on any issue and you'll be notified of new notes, status changes, and edits."},
            {"callout": {"kind": "note", "label": "YOU'RE ALREADY WATCHING SOME ISSUES",
                         "text": "You're automatically notified about issues you **reported** and issues **assigned "
                                 "to you** — no need to press Monitor on those. Use Monitor for the *other* issues "
                                 "you want to keep an eye on."}},
            {"h1": "Your notifications"},
            {"p": "Select **Notifications** in the left navigation to see what's happened on the issues you follow, "
                  "newest first, with a count of unread items. Select any notification to jump straight to the "
                  "issue it's about; reading it clears the unread mark."},
            {"screenshot": "The Notifications list with unread items"},
            {"h1": "Time logging"},
            {"p": "**Time logging** records how much effort an issue took — useful for billing, estimating, or just "
                  "understanding where the hours go."},
            {"steps": [
                "On an issue, find the **Time log** section.",
                "Enter the **minutes** worked, an optional note describing what you did, and the date. Add it.",
                "Everyone who can see the issue sees the entries and the running **total** — so effort is visible, "
                "not guessed at.",
            ]},
            {"screenshot": "Logging time on an issue"},
        ],
    },

    # 13 -----------------------------------------------------------------
    {
        "title": "Service-Level Agreements (SLA) & Escalation",
        "subtitle": "Set resolution deadlines by priority, and catch issues before they miss them.",
        "in_this_chapter": [
            "What a Service-Level Agreement is, in plain terms",
            "Setting targets per priority (Managers)",
            "The status board, the badges, and automatic escalation",
        ],
        "blocks": [
            {"h1": "What an SLA is"},
            {"p": "A **Service-Level Agreement (SLA)** is simply a promise about how quickly issues get dealt with "
                  "— for example, “urgent problems are resolved within 24 hours.” OpenTrack lets a Manager set a "
                  "target number of **hours to resolve** for each **priority**, then watches every open issue "
                  "against its clock and flags the ones drifting toward — or past — their deadline."},
            {"h1": "Set targets (Manager)"},
            {"steps": [
                "On the project, go to **Settings** and select **SLA targets** (across the top).",
                "For each priority, enter how many **hours** a new issue of that priority may stay open before it "
                "breaks the promise. For example: Immediate 4, Urgent 24, High 72, Normal 168.",
                "Leave a priority **blank** to not track it at all. Save.",
            ]},
            {"screenshot": "Setting SLA targets per priority"},
            {"callout": {"kind": "note", "label": "THE THREE STATES",
                         "text": "**On track** — comfortably within the target. **At risk** — it has passed **80%** "
                                 "of its allowed time and isn't resolved yet, so it needs attention now. "
                                 "**Breached** — the target time has passed and it's still open. Each open issue "
                                 "shows its state as a colored badge on its own page."}},
            {"h1": "The SLA status board"},
            {"p": "Select **SLA status** in the left navigation for a live, cross-project list of the open issues "
                  "that are **breached or at risk**, most-overdue first. This is your triage screen — the single "
                  "best place to answer “what most needs attention right now?”"},
            {"screenshot": "The SLA status board, breached items first"},
            {"callout": {"kind": "important", "label": "BREACHES ESCALATE BY THEMSELVES",
                         "text": "When an issue **breaches**, OpenTrack automatically notifies the **assignee** and "
                                 "the project's **managers** — once per issue, so nobody gets spammed but nothing "
                                 "slips silently past its deadline either. You don't have to watch the clock; the "
                                 "system does."}},
        ],
    },

    # 14 -----------------------------------------------------------------
    {
        "title": "Automation Rules",
        "subtitle": "“When a new issue looks like this, do that” — handled for you.",
        "in_this_chapter": [
            "What automation runs on, and when",
            "Building a rule: conditions and actions",
        ],
        "blocks": [
            {"h1": "What automation does"},
            {"p": "An **automation rule** runs the moment a **new issue is created** in the project. If the issue "
                  "matches the rule's **conditions**, the rule's **actions** are applied automatically. It's a way "
                  "to bake your triage habits into the system so routine sorting happens without anyone lifting a "
                  "finger."},
            {"h1": "Create a rule (Manager)"},
            {"steps": [
                "On the project, go to **Settings** and select **Automation** (across the top).",
                "Give the rule a **name**.",
                "Set its **conditions** — any mix of: the title or description **contains** some text; the "
                "**severity is**…; the **priority is**…; the **category is**…. Leave a condition on **Any** to "
                "ignore it. (All the conditions you set must match.)",
                "Set its **actions** — set the severity, set the priority, set the status, **assign** it to a "
                "member, and/or **add a tag**. Leave an action blank to skip it.",
                "Save. Rules run in order on each new issue; you can edit, reorder, disable, or delete them.",
            ]},
            {"screenshot": "The Automation rules editor showing conditions and actions"},
            {"callout": {"kind": "note", "label": "A WORKED EXAMPLE",
                         "text": "Rule: **if** a new issue's title contains *crash* → **set** Severity to Crash, "
                                 "**set** Priority to High, and **add** the tag *crash*. From then on, every crash "
                                 "report is triaged and labeled the instant it's filed, day or night."}},
        ],
    },

    # 15 -----------------------------------------------------------------
    {
        "title": "Workflow Rules",
        "subtitle": "Restrict which status changes are allowed, so issues follow your process.",
        "in_this_chapter": [
            "Open workflow vs. a defined workflow",
            "Adding the allowed transitions",
        ],
        "blocks": [
            {"h1": "Controlling how issues move"},
            {"p": "Out of the box, an issue can jump to any status — the **workflow is open**. Some teams want more "
                  "discipline: a bug shouldn't go straight from New to Closed without being Confirmed and Resolved "
                  "first. A Manager can define a **workflow** — the explicit list of status changes that are "
                  "allowed — to enforce that order."},
            {"steps": [
                "On the project, go to **Settings** and scroll to the **Workflow** section.",
                "Add each allowed move as a **from → to** pair — for example New → Acknowledged, Acknowledged → "
                "Confirmed, Confirmed → Assigned, Assigned → Resolved, Resolved → Closed.",
                "As soon as you add even one rule, **only** the moves you've listed are permitted anywhere in the "
                "app (the Edit screen and the Board both respect it). Remove all rules to go back to an open "
                "workflow.",
            ]},
            {"screenshot": "Defining allowed workflow transitions"},
            {"callout": {"kind": "tip", "label": "START OPEN, TIGHTEN LATER",
                         "text": "If you're not sure you need a workflow, leave it open. Add rules only once you've "
                                 "felt the pain of issues skipping steps — and remember to include every move your "
                                 "team legitimately makes, or you'll block yourselves."}},
        ],
    },

    # 16 -----------------------------------------------------------------
    {
        "title": "Roadmap, Changelog & Reports",
        "subtitle": "See what's planned, what shipped, and how the numbers are trending.",
        "in_this_chapter": [
            "How versions drive the Roadmap and Changelog",
            "Reading the Reports charts",
        ],
        "blocks": [
            {"h1": "Roadmap and Changelog"},
            {"p": "When issues carry a **Fix version** (set on the issue, from the project's version list), "
                  "OpenTrack builds two automatic views for the project — no extra bookkeeping required:"},
            {"bullets": [
                "**Roadmap** — for versions **not yet released**: what's planned for each upcoming release and how "
                "far along it is (a progress bar of resolved vs. total). Great for answering “what's in the next "
                "version?”",
                "**Changelog** — for **released** versions: the list of what shipped in each. Great for release "
                "notes and for telling users what changed.",
            ]},
            {"steps": [
                "Open the project and select **Roadmap** to see both, split into upcoming and released.",
            ]},
            {"screenshot": "The Roadmap and Changelog view"},
            {"h1": "Reports and trends"},
            {"p": "Select **Reports** in the left navigation for charts drawn from the issues you're allowed to see "
                  "— headline totals, **issues created per month** (is the inflow rising or falling?), and open "
                  "issues broken down **by status** and **by severity**. These turn a pile of individual issues "
                  "into a trend you can act on."},
            {"screenshot": "The Reports page with its charts"},
        ],
    },

    # 17 -----------------------------------------------------------------
    {
        "title": "Bug-Hunt Checklists",
        "subtitle": "Work a repeatable list of things to test, and turn any failure into a tracked issue.",
        "in_this_chapter": [
            "What a bug-hunt checklist is for",
            "Building one: paste a whole list, or add items one at a time (Managers)",
            "Editing items, grouping them into sections, and deleting",
            "Working it: Pass / Fail / N/A, and turning a failure into an issue",
            "Working offline in the field",
        ],
        "blocks": [
            {"h1": "What a bug-hunt checklist is for"},
            {"p": "A **bug-hunt checklist** is a reusable list of things to check on a project — the sweep you do "
                  "before a release, a Quality Assurance (QA) pass, or a routine inspection. Instead of trying to "
                  "remember every corner to test, you work down the list, marking each item **Pass**, **Fail**, or "
                  "**Not-applicable (N/A)**, and any failure becomes a real issue with one tap. You build the list "
                  "**once** and it stays on the project for every future pass."},
            {"h1": "Building a checklist (Manager)"},
            {"p": "Only a **Manager** can build or change a project's checklist; anyone on the project can then "
                  "work it. There's nothing to install and no file to upload — you create the list right in the "
                  "app, either by pasting a whole list at once or by adding items one at a time. Open the project "
                  "and select **Checklist** to start."},
            {"h2": "The fast way: paste a whole list"},
            {"p": "If you already have a list — in a document, a past release checklist, a wiki page — you don't "
                  "retype it. You paste it, and OpenTrack turns each line into an item."},
            {"steps": [
                "On the checklist page, find the **Import a checklist** box.",
                "Paste your list, **one item per line**.",
                "Formatting is forgiving: a line that starts with **`#`** becomes a **section heading**; plain "
                "lines, bullet lines (starting with `-`), checkbox lines (`- [ ]`), and numbered lines all become "
                "checklist items.",
                "Select **Import items**. Every line is added at once, grouped under whatever headings you "
                "included.",
            ]},
            {"p": "For example, pasting this:"},
            {"code": [
                "# Concurrency",
                "- [ ] Message store is thread-safe",
                "- [ ] Geofence service is locked",
                "# RF identity",
                "- TX blocked on N0CALL",
            ]},
            {"p": "…creates five items in two sections (“Concurrency” and “RF identity”)."},
            {"callout": {"kind": "note", "label": "“IMPORT” MEANS PASTE, NOT A FILE",
                         "text": "This is the closest thing to “uploading” a checklist: you paste the text of a "
                                 "list rather than choosing a file. It's the quickest way to stand up a full "
                                 "checklist, or to bulk-add a batch of items to an existing one."}},
            {"h2": "One at a time"},
            {"steps": [
                "On the checklist page, find the **Add one item** box.",
                "Enter a **title** (for example, “TX blocked on N0CALL”), an optional **Area / section** to "
                "group it under, and optional **Details** describing how or what to check.",
                "Select **Add item**. It appears in the list, under its section if you gave one.",
            ]},
            {"h2": "Edit, group, or remove items"},
            {"p": "Each item has an **Edit / note** link. Use it to change the item's **title**, its **Area** (the "
                  "section heading it's grouped under), or its **Details**; to jot **Notes** on what you found; or "
                  "to **Delete** the item. Items that share an Area are automatically grouped together under that "
                  "heading, which is how you organize a long checklist into readable sections."},
            {"screenshot": "The Import-a-checklist and Add-one-item boxes for building the list"},
            {"h1": "Working the checklist"},
            {"steps": [
                "Open the project and select **Checklist** on any device on your network.",
                "For each item, tap **Pass**, **Fail**, or **N/A**. A progress bar and a “X of Y checked” "
                "count (with passed/failed totals) update at the top as you go.",
                "Got something wrong? **Reset** an item to clear its result.",
            ]},
            {"p": "When you mark an item **Fail**, a **Create issue from this failure →** button appears on it. "
                  "Select it to open a new issue **linked** to that checklist item, which you then triage like any "
                  "other bug — and the item shows a link to the issue it spawned. (Anything you notice that isn't "
                  "on the checklist, you just file as a normal issue.)"},
            {"screenshot": "A project bug-hunt checklist being worked, with a failed item ready to become an issue"},
            {"callout": {"kind": "tip", "label": "IT KEEPS WORKING WHEN THE NETWORK DOESN'T",
                         "text": "On a tablet or phone, the checklist keeps working through brief network drops — "
                                 "tick items off out in the field or a signal-dead corner of a building, and your "
                                 "marks sync automatically when you're back online. (Creating an issue from a "
                                 "failure does need a connection; OpenTrack tells you if you're offline and the "
                                 "action waits.) See the *Mobile, Tablets & the Field* chapter."}},
        ],
    },

    # 18 -----------------------------------------------------------------
    {
        "title": "Public Trouble-Ticket Intake & QR Posters",
        "subtitle": "Let anyone report a problem without an account — by link or by scanning a code.",
        "in_this_chapter": [
            "Turning on public intake (Managers)",
            "The public report form and the status-lookup page",
            "Printing a Quick Response (QR) code poster",
        ],
        "blocks": [
            {"h1": "What public intake is"},
            {"p": "**Public trouble-ticket intake** lets people who have **no OpenTrack account** submit a problem "
                  "to a project through a simple “Report a problem” web page. It's perfect for a helpdesk, a "
                  "club, an event, or field reports from the general public. Submissions arrive as normal issues in "
                  "your project, ready to triage. It's **off by default** — you turn it on per project."},
            {"h1": "Turn it on (Manager)"},
            {"steps": [
                "On the project, go to **Settings** and scroll to **Public trouble-ticket intake**.",
                "Select **Turn on public intake**.",
                "Two links appear: the **public report link** to share, and a **status-lookup link** where "
                "reporters can check on a ticket later.",
            ]},
            {"h1": "The public report form"},
            {"p": "Anyone with the link opens a plain form — a summary, the details, and an optional name and email "
                  "— and submits. It lands as a new issue in your project. If they leave an **email**, they can "
                  "later visit the status-lookup page, enter their **reference number** and that email, and see how "
                  "their ticket is doing — without ever creating an account or seeing anyone else's tickets."},
            {"screenshot": "The public “Report a problem” form"},
            {"h1": "Print a QR poster"},
            {"p": "For a physical place — a workshop, a trailhead, an event booth, a piece of equipment — OpenTrack "
                  "can print a poster with a **Quick Response (QR) code** (the square, scannable barcode). People "
                  "point a phone camera at it and the report form opens on their phone; no typing a web address."},
            {"steps": [
                "With intake turned on, in that same **Public trouble-ticket intake** section select "
                "**📱 Printable QR poster**.",
                "Print the page and post it where people can reach it. Scanning the code opens the report form "
                "pre-pointed at this project.",
            ]},
            {"screenshot": "The printable QR intake poster"},
        ],
    },

    # 19 -----------------------------------------------------------------
    {
        "title": "Importing & Exporting Your Data",
        "subtitle": "Bring issues in from other tools, and get your own data out — including automatic backups.",
        "in_this_chapter": [
            "Importing from MantisBT, a spreadsheet, Jira, or GitHub",
            "Exporting to JSON or a spreadsheet",
            "Turning on automatic scheduled backups",
        ],
        "blocks": [
            {"h1": "Importing issues"},
            {"p": "Moving to OpenTrack from another tracker? You don't have to retype anything. OpenTrack imports "
                  "issues from several sources. You need the **Manager** role on the project you're importing "
                  "**into**."},
            {"bullets": [
                "**MantisBT** (a common older tracker) — use the dedicated MantisBT importer on the **Backup & "
                "export** page; it can even create the matching projects for you.",
                "**Spreadsheet (Comma-Separated Values, or CSV)**, **Jira export (CSV)**, or **GitHub Issues "
                "(JavaScript Object Notation, or JSON)** — use the **Import** page.",
            ]},
            {"steps": [
                "Select **Import** in the left navigation.",
                "Choose the **target project** and the **file type** (CSV, Jira, or GitHub), pick your file, and "
                "select **Import**.",
                "A summary reports how many issues were imported and how many were skipped. Re-running the **same "
                "file** is safe — rows already brought in are recognized and skipped, so you won't get duplicates.",
            ]},
            {"screenshot": "The Import page"},
            {"callout": {"kind": "note", "label": "WHAT THOSE FILE TYPES ARE",
                         "text": "**CSV** (Comma-Separated Values) is the plain export format every spreadsheet and "
                                 "most trackers can produce. **JSON** (JavaScript Object Notation) is the structured "
                                 "text format GitHub uses when it exports issues. OpenTrack recognizes the common "
                                 "column and field names in each automatically."}},
            {"h1": "Exporting your data"},
            {"p": "Your data is always yours to take. Select **Backup & export** in the left navigation to download "
                  "your issues as:"},
            {"bullets": [
                "**JSON** — a complete, structured copy (best for a full backup or moving to another system).",
                "**CSV** — a spreadsheet-friendly copy (best for reporting, sorting, or sharing a snapshot).",
            ]},
            {"h1": "Automatic scheduled backups"},
            {"p": "The manual export above is on-demand. For peace of mind, the **server** can also make **automatic "
                  "backups** on a schedule — consistent snapshots of the whole database, taken safely while the app "
                  "keeps running. This is a server setting (whoever runs the server turns it on), and it's **off by "
                  "default**. To enable it, set these on the server, then restart OpenTrack:"},
            {"code": [
                "OpenTrack__Backup__Enabled=true",
                "OpenTrack__Backup__IntervalHours=24     # how often to snapshot",
                "OpenTrack__Backup__Directory=           # blank = a 'backups' folder next to the database",
                "OpenTrack__Backup__Retention=14         # keep the newest 14 snapshots, delete older ones",
            ]},
            {"p": "Snapshots are named `opentrack-YYYYMMDD-HHMMSS.db` (date and time). To **restore** one: stop the "
                  "server, copy the chosen snapshot over the live `opentrack.db` file, and start the server again."},
            {"callout": {"kind": "tip", "label": "KEEP A COPY OFF THE MACHINE",
                         "text": "Automatic backups protect against mistakes and corruption, but not against the "
                                 "whole server dying or being lost. Every so often, copy a recent snapshot (or a "
                                 "manual JSON export) to a different device or a cloud drive, so one failure can't "
                                 "take everything."}},
        ],
    },

    # 20 -----------------------------------------------------------------
    {
        "title": "The AI Assistant (Optional)",
        "subtitle": "Smart triage, plain-English search, and thread summaries — and how to turn it on.",
        "in_this_chapter": [
            "What the AI can do — and its firm limits",
            "Turning it on: local & free, or a cloud provider with a key",
            "Getting an API key, the settings to enter, cost, and privacy",
        ],
        "blocks": [
            {"h1": "What it is"},
            {"p": "OpenTrack can optionally use an **artificial intelligence (AI)** language model to speed up a few "
                  "chores. It is **off by default** and only ever runs when someone turns it on and points it at a "
                  "provider. Crucially, everything it produces is a **suggestion a person accepts or changes** — it "
                  "never files, changes, or resolves anything on its own, and if it's off (or a call fails) "
                  "OpenTrack behaves exactly as it always does."},
            {"h1": "What it can do"},
            {"table": {
                "headers": ["Helper", "Where", "What it does"],
                "rows": [
                    ["**Smart triage**", "New Issue page", "The **✨ Suggest with AI** button reads your summary "
                                                           "and details and proposes a severity, priority, "
                                                           "category, and tags"],
                    ["**Plain-English search**", "Issues list", "The **✨ Ask in plain English** box turns a "
                                                                "request like *“high-priority crashes nobody has "
                                                                "touched in a month”* into the normal filters"],
                    ["**Thread summary**", "A busy issue", "The **✨ Summarize thread** button gives a plain-language "
                                                          "recap of the problem, what's been tried, and what's next"],
                ],
                "widths": [1.8, 1.4, 3.3],
            }},
            {"callout": {"kind": "note", "label": "IT CAN'T WIDEN YOUR ACCESS",
                         "text": "Plain-English search only ever produces a filter you could have set by hand, over "
                                 "the projects you're already allowed to see. The AI never becomes a way around "
                                 "OpenTrack's privacy rules."}},
            {"h1": "Turning it on — two paths"},
            {"p": "The AI is configured on the **server** (by whoever runs it), not in the web pages, because it "
                  "involves a secret key that must never reach a browser. There are two very different ways to run "
                  "it, and the choice is mostly about **privacy** and **cost**:"},
            {"table": {
                "headers": ["Path", "What it means", "Good when"],
                "rows": [
                    ["**Local & free** (Ollama or LM Studio)", "A model runs on **your own** computer. No key, no "
                                                                "bill, and **the issue text never leaves your "
                                                                "machine**.", "Privacy matters, or you'd rather not "
                                                                "pay per use. Needs a reasonably capable PC."],
                    ["**Cloud provider** (Anthropic Claude, OpenAI, Azure, Groq, and others)", "You get an "
                                                                "**Application Programming Interface (API)** key "
                                                                "from the provider; OpenTrack calls their service.",
                                                                "You want the strongest results with no local "
                                                                "hardware, and are okay sending issue text to that "
                                                                "provider and paying a small per-use fee."],
                ],
                "widths": [2.1, 2.6, 1.8],
            }},
            {"h1": "Option A — local, free & private (Ollama)"},
            {"steps": [
                "On the machine that will run the model, install **Ollama** from `https://ollama.com` (Windows, "
                "Mac, or Linux).",
                "Download a model, for example: `ollama pull llama3.1`. Ollama then serves it locally.",
                "On the OpenTrack server, set the AI settings to point at it (no key needed), then restart "
                "OpenTrack:",
            ]},
            {"code": [
                "OpenTrack__Ai__Enabled=true",
                "OpenTrack__Ai__Provider=openai",
                "OpenTrack__Ai__BaseUrl=http://localhost:11434/v1   # or a LAN address like http://192.168.1.50:11434/v1",
                "OpenTrack__Ai__Model=llama3.1",
            ]},
            {"callout": {"kind": "note", "label": "HOW MUCH COMPUTER DO YOU NEED?",
                         "text": "For OpenTrack's short, occasional AI calls, **memory (RAM) matters most**: about "
                                 "**16 gigabytes (GB)** is the sweet spot (runs a capable 7–8-billion-parameter "
                                 "model); 8 GB works but gives weaker suggestions. An **Apple Silicon Mac** (M-series "
                                 "mini) or a machine with an **NVIDIA graphics card** feels snappy; a plain "
                                 "mini-PC's processor works, just slower. You can even run it on the same mini-PC "
                                 "as OpenTrack."}},
            {"h1": "Option B — a cloud provider (get a key)"},
            {"p": "Cloud AI is billed to a **developer API account** at the provider, which is **separate** from any "
                  "monthly chat subscription (an Anthropic API key is **not** a Claude Pro plan; an OpenAI API key "
                  "is **not** ChatGPT Plus). Costs are small — a triage suggestion is typically a fraction of a "
                  "cent — but set a spend limit where the provider offers one. To get a key (Anthropic Claude "
                  "shown; OpenAI is nearly identical):"},
            {"steps": [
                "Go to `https://console.anthropic.com` — the developer **Console**, not the claude.ai chat site "
                "(they're separate accounts even with the same email). Sign in or sign up.",
                "Open **API Keys** (in Settings), select **Create Key**, name it `OpenTrack`, and **copy the key "
                "now** — it's shown only once. It looks like `sk-ant-…`.",
                "Add a little **credit** under Billing (even a few dollars is plenty for triage) and set a monthly "
                "spend limit.",
                "On the OpenTrack server, enter the settings and restart OpenTrack:",
            ]},
            {"code": [
                "OpenTrack__Ai__Enabled=true",
                "OpenTrack__Ai__Provider=anthropic",
                "OpenTrack__Ai__ApiKey=sk-ant-...          # your key — keep it on the server only",
                "OpenTrack__Ai__Model=claude-haiku-4-5-20251001   # fast and inexpensive",
            ]},
            {"p": "For **OpenAI**, use `Provider=openai`, an `sk-…` OpenAI key, and a model like `gpt-4o-mini`. For "
                  "**Azure OpenAI, Groq, OpenRouter**, or similar, use `Provider=openai`, set `BaseUrl` to that "
                  "service's address, and use its key and model name. (The one `openai` setting covers every "
                  "service that speaks OpenAI's common format — which is most of them, including the free local "
                  "ones.)"},
            {"callout": {"kind": "important", "label": "PRIVACY IN ONE LINE",
                         "text": "With a **cloud** provider, generating a suggestion sends that issue's text to that "
                                 "provider — so don't enable a cloud provider for projects whose contents can't "
                                 "leave your environment; use a **local** model instead. The key is read only on "
                                 "the server and is never sent to the browser or stored in the database."}},
        ],
    },

    # 21 -----------------------------------------------------------------
    {
        "title": "Integrations: Git & Chat Notifications",
        "subtitle": "Link code commits to issues, and get pinged in Slack or Discord when things change.",
        "in_this_chapter": [
            "Connecting a GitHub repository so commits link and auto-resolve",
            "Reading the Linked commits section",
            "Sending change notifications to Slack, Discord, or your own service",
        ],
        "blocks": [
            {"h1": "Git integration — how it works"},
            {"p": "When you connect a **GitHub** code repository to a project, OpenTrack watches for commits (saved "
                  "changes to your code) whose message mentions an issue, and links the two automatically. This "
                  "closes the gap between “here's the bug” and “here's the change that fixed it.”"},
            {"bullets": [
                "A commit message like **`fixes #123`** (or *closes #123* / *resolves #123*) links that commit to "
                "issue #123 **and**, if you turn on auto-resolve, moves the issue to **Resolved**.",
                "A plain mention like **`#123`** just links the commit, without changing the status.",
            ]},
            {"p": "OpenTrack only **receives** notifications from GitHub — it never needs access to your code or "
                  "your GitHub account."},
            {"h1": "Connect a repository (Manager)"},
            {"steps": [
                "On the project, go to **Settings** and select **Git** (across the top).",
                "Tick **Enable Git integration** and enter a **webhook secret** — a long random string you make up "
                "(treat it like a password; it's what proves a notification really came from your GitHub).",
                "Decide whether to tick **auto-resolve** (move an issue to Resolved on a `fixes #id` commit), then "
                "**Save**. OpenTrack shows you a **Payload URL** to use in the next step.",
                "In your GitHub repository, go to **Settings → Webhooks → Add webhook**, and fill in: **Payload "
                "URL** = the address OpenTrack showed; **Content type** = `application/json`; **Secret** = the same "
                "secret you entered; **Events** = “Just the push event.” Save it.",
            ]},
            {"screenshot": "The Git integration settings and the GitHub webhook fields"},
            {"callout": {"kind": "note", "label": "REACHABILITY & THE GREEN CHECK",
                         "text": "For GitHub to reach your server, the server must be reachable from the internet (a "
                                 "public address or a secure tunnel) and — since the secret is what guards it — is "
                                 "best served over encrypted **HTTPS**. When you add the webhook, GitHub sends a "
                                 "test “ping”; a green check mark there means the address and secret are right."}},
            {"h1": "See the linked commits"},
            {"p": "Once connected, an issue that's been referenced grows a **Linked commits** section. Each entry "
                  "shows a short commit identifier that links back to the commit on GitHub, with a **resolved** "
                  "badge if that commit closed the issue — so the trail from report to fix is one click away."},
            {"h1": "Chat notifications — Slack, Discord, or your own service"},
            {"p": "Separately from Git, a project can **push a short notification to a chat channel** whenever one "
                  "of its issues is created, changed, or commented on — so your team hears about activity where "
                  "they already are."},
            {"steps": [
                "In the chat tool, create an **incoming webhook** and copy its URL (Slack and Discord both provide "
                "these; they contain a secret token, so treat the URL as private).",
                "On the project, go to **Settings** and scroll to **Integrations — outgoing webhooks**.",
                "Paste the URL, choose its **format** — **Slack**, **Discord**, or **Generic** (which posts the "
                "full details as JSON to your own service) — and select **Add**. Remove one anytime.",
            ]},
            {"screenshot": "Adding an outgoing Slack/Discord webhook to a project"},
            {"callout": {"kind": "warning", "label": "ONLY ADD URLS YOU TRUST",
                         "text": "An outgoing-webhook URL sends your issue activity to whatever service it points "
                                 "at. Only paste URLs you created and trust — never one someone else handed you "
                                 "without knowing where it goes."}},
        ],
    },

    # 22 -----------------------------------------------------------------
    {
        "title": "Mobile, Tablets & the Field",
        "subtitle": "Use OpenTrack from a phone or tablet, install it like an app, and work off-network.",
        "in_this_chapter": [
            "Installing OpenTrack as an app (PWA) on a tablet or phone",
            "Working offline, attaching your location, and scan-to-report",
        ],
        "blocks": [
            {"h1": "Install it like an app (PWA)"},
            {"p": "OpenTrack is a **Progressive Web App (PWA)** — a website modern enough to install and run like a "
                  "regular app, with its own icon and a full-screen window (no browser bars). There's nothing to "
                  "download from a store."},
            {"steps": [
                "Open OpenTrack in the device's browser.",
                "**On an iPad or iPhone (Safari):** tap the **Share** button, then **Add to Home Screen**.",
                "**On Android (Chrome):** tap the **⋮** menu, then **Install app** (or **Add to Home screen**).",
                "Launch it from the new icon — it opens full-screen, like any other app.",
            ]},
            {"screenshot": "OpenTrack installed on a tablet home screen"},
            {"h1": "Made for the field"},
            {"bullets": [
                "**Offline checklists** — work a bug-hunt checklist through brief network drops; your marks sync "
                "when you're back online.",
                "**Attach your location** — on an issue, **📍 Attach my location** stamps your Global Positioning "
                "System (GPS) coordinates onto it, with a map link, for anything tied to a physical place.",
                "**Scan to report** — anyone can point a phone at a project's printed **QR code** poster to open "
                "the report form instantly (see the *Public Trouble-Ticket Intake* chapter).",
            ]},
            {"callout": {"kind": "note", "label": "SAME APP, SMALLER SCREEN",
                         "text": "The mobile experience isn't a stripped-down version — it's the same OpenTrack, "
                                 "laid out to fit a smaller screen. Every feature in this manual is available on a "
                                 "tablet or phone."}},
        ],
    },

    # 23 -----------------------------------------------------------------
    {
        "title": "Printing, Preferences & Administration",
        "subtitle": "Print an issue, set your personal defaults, and (for admins) run the instance.",
        "in_this_chapter": [
            "Printing or saving an issue as a PDF",
            "Your personal preferences",
            "Administrator tasks: users, first admin, email, and encryption",
        ],
        "blocks": [
            {"h1": "Print or save an issue as a PDF"},
            {"steps": [
                "On an issue, select **Print / PDF**.",
                "A clean, print-friendly version of the issue opens (no menus or buttons — just the content).",
                "Use your browser's print dialog to print it, or choose **Save as PDF** to keep a Portable Document "
                "Format copy to file or email.",
            ]},
            {"h1": "Your preferences"},
            {"p": "Select **Preferences** in the left navigation to set personal defaults — such as your default "
                  "project and default sort order — so OpenTrack opens the way you like it. Preferences are yours "
                  "alone and don't affect anyone else."},
            {"screenshot": "The Preferences page"},
            {"h1": "Administration (Administrators only)"},
            {"p": "The **Administrator** — the global role, usually the person who installed the server — has a few "
                  "extra responsibilities. Most are one-time or occasional:"},
            {"table": {
                "headers": ["Task", "Where / how"],
                "rows": [
                    ["**Manage users**", "The admin **Users** screen: view accounts, set each user's global role, "
                                         "and activate or deactivate an account (deactivating blocks sign-in "
                                         "without deleting the person's history)"],
                    ["**Set the first admin ahead of time**", "Instead of relying on “first to register,” the "
                                                              "server can be told an admin email and password "
                                                              "before first run, via the `OpenTrack__BootstrapAdmin__…` "
                                                              "settings"],
                    ["**Turn on email**", "Optional. By default OpenTrack sends no email and writes reset links to "
                                          "its log; to send real email, fill in the `OpenTrack__Email__…` settings "
                                          "with your mail (SMTP) server details"],
                    ["**Require encryption (HTTPS)**", "On a trusted home network OpenTrack runs over plain HTTP; "
                                                       "if the server is reachable from outside, set "
                                                       "`OpenTrack__RequireHttps=true` and provide a certificate"],
                ],
                "widths": [2.3, 4.2],
            }},
            {"screenshot": "The administrator user-management screen"},
            {"callout": {"kind": "note", "label": "THAT'S THE WHOLE TOUR",
                         "text": "You've now seen every part of OpenTrack, from filing your first issue to SLAs, "
                                 "automation, Git, and the field. Keep this manual as a reference — and remember the "
                                 "two things that shape what you see on screen: your **role**, and which optional "
                                 "features (AI, Git, SLAs, intake, chat notifications) your administrator has turned "
                                 "on."}},
        ],
    },
]


def build():
    doc = S.new_document(
        header_title="OpenTrack — User Manual",
        header_sub="Self-hosted issue & bug tracking",
        footer_left="OpenTrack  ·  Open-source (AGPL v3)  ·  KE4CON",
    )
    S.cover(
        doc,
        kicker="OPENTRACK",
        big_title="OpenTrack",
        subtitle="Self-Hosted Issue Tracker",
        doc_kind="USER MANUAL",
        version="v1.0",
        tagline="Every feature, explained and step by step — from your first issue to SLAs, automation, Git, AI, and the field.",
        author="James Rospopo  ·  KE4CON",
        date_str=TODAY,
    )
    S.section_title(doc, "Contents")
    S.toc(doc)
    for i, ch in enumerate(CHAPTERS, 1):
        S.render_chapter(doc, ch, i)

    out = os.environ.get("OT_OUT") or os.path.join(
        os.path.dirname(__file__), "..", "docs", "guides", "OpenTrack_User_Manual.docx")
    out = os.path.abspath(out)
    doc.save(out)
    print("wrote", out)


if __name__ == "__main__":
    build()
