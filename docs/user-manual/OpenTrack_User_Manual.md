# OpenTrack User Manual

*Every feature, explained and step by step — in plain language.*

*Generated August 24, 2026 · Markdown is the living source of truth.*


---


# 1. Welcome & Getting Around

*What OpenTrack is, how to sign in, and a guided tour of the screen.*

> **QUICK VERSION** — Open OpenTrack in a web browser (or the desktop app), sign in — or select **Register** the first time — and you're in. There's a menu down the **left** and your work in the **middle**. That's the whole layout.


## What OpenTrack is

OpenTrack is a **self-hosted issue and bug tracker** — a private, organized place to record problems (bugs, tasks, feature requests, support tickets), decide what matters most, assign the work, and follow each item through to resolution. Think of it as a shared, searchable memory for everything that's wrong, planned, or in progress across your projects, so nothing lives only in someone's head or an email thread.

“Self-hosted” means it runs on **your own** server (a small mini-PC on your network, for example) rather than on someone else's cloud service. The practical upshot: your data stays with you, there's no per-user subscription, and you decide who can reach it. OpenTrack is **open-source** software released under the GNU Affero General Public License version 3 (AGPL v3), so you're free to run, inspect, and modify it.

You'll use OpenTrack one of two ways, and they look and behave almost the same — nearly everything in this manual applies to both:

- **The web app** — open a web address in any browser (Chrome, Edge, Safari, Firefox) on a computer, tablet, or phone. Nothing to install.
- **The desktop app** — a native application for Windows or Mac that talks to the same server. Handy if you'd rather launch an app than a browser.

> **IF IT ISN'T INSTALLED YET** — Someone has to set up the OpenTrack **server** once before anyone can sign in — that one-time job (choosing the mini-PC, installing the software, opening the network port) is covered start to finish in the separate **Installation Guide**. This manual assumes the server is already running and you have its web address.


## Creating your account and signing in

OpenTrack keeps its own list of accounts — an email address and a password per person. You don't need any outside account (no Google, no Microsoft) to use it. When you first open the app without being signed in, you land on the **Log in** page, titled *Log in* at the top with the sub-heading *Use a local account to log in.*


### Signing in with a password

1. Open OpenTrack — the web address your administrator gave you (something like `http://192.168.1.50:5003`), or the desktop app.
2. In the **Email** box, type the email address your account uses.
3. In the **Password** box, type your password.
4. If this is your own computer and you'd like to stay signed in, tick **Remember me**. Leave it unticked on a shared or public machine.
5. Select the **Log in** button. On success you're taken to your home screen (the Dashboard).

> _[Figure: The OpenTrack Log in page: Email and Password boxes, Remember me checkbox, and the Log in button]_

The **Log in** page has three helper links beneath the form, for the moments things go sideways:

| Link | What it's for |
| --- | --- |
| **Forgot your password?** | Starts a password reset. (On setups that don't send email, this may not deliver a reset link — see the note about email below.) |
| **Register as a new user** | Opens the account-creation page (same as **Register** in the menu). |
| **Resend email confirmation** | Sends the “confirm your account” email again, if your setup uses email confirmation. |


### Signing in with a passkey

Below the password form, after an *OR* divider, is a **Log in with a passkey** button. A *passkey* is a modern, password-free sign-in that uses your device's fingerprint reader, face scan, or screen lock (or a hardware security key) instead of a typed password. If you have set one up already (see the *Your Account & Preferences* material), type your email in the **Email** box, then select **Log in with a passkey** and follow your device's prompt. If you've never created a passkey, ignore this button and use your password.


### Registering a new account

If you don't have an account yet and registration is open, select **Register** (in the left navigation, or the *Register as a new user* link on the Log in page). The **Register** page, titled *Create a new account.*, asks for three things:

| Field | What to enter |
| --- | --- |
| **Email** | The email address that will be your account name. |
| **Password** | A password at least 6 characters long. |
| **Confirm Password** | The exact same password again, so a typo can't lock you out. |

1. Fill in **Email**, **Password**, and **Confirm Password**.
2. Select the **Register** button.
3. If your server doesn't require email confirmation, you're signed in immediately and land in the app. If it does, you'll see a page asking you to confirm your email before signing in.

> _[Figure: The Register page with Email, Password, and Confirm Password boxes and the Register button]_

> **THE FIRST ACCOUNT BECOMES THE ADMINISTRATOR** — On a brand-new server, if no administrator has been set up in advance, the **very first account registered** automatically becomes the **Administrator** — the person who runs the whole instance. If that's you, guard that account. (An administrator can also be configured ahead of time by whoever installs the server, so it doesn't have to depend on who registers first.)

> **ABOUT THAT CONFIRMATION EMAIL** — Many self-hosted OpenTrack setups don't send email at all (there's no mail server on a home network by default). That's fine — on those setups **confirming your email is not required to sign in**. If a “confirm your account” link never arrives, you can still log in and use everything normally.


## A tour of the screen

Once you're in, the window has three areas. Get comfortable with these and you'll always know where to look:

| Area | What lives there |
| --- | --- |
| **Left navigation** | The main menu down the left edge, with a brand name at the top (*OpenTrack*). Every big area of the app is one click away here — the full list is in the next section. |
| **Top bar** | The title of the page you're on, and — on list pages — a search box and the main action buttons (like **New Project** or **New Issue**). |
| **Main area** | The actual content: a list of issues, a single issue, a project page, a chart, and so on. This is where you do the work. |

> _[Figure: The main screen with the left navigation, top bar, and main area labeled]_

> **ON A NARROW SCREEN** — On a phone or a narrow window the left navigation collapses behind a small “hamburger” toggle button (three stacked lines) at the top. Select it to slide the menu in and out. Select any menu item and the menu tucks itself away again so the main area has room.


## Every entry in the left navigation

Here is the full left-navigation menu you see once signed in to the **web app**, top to bottom, and what each one opens:

| Menu item | What it opens |
| --- | --- |
| **Home** | The landing page. (In the **desktop app** this entry is labeled **Dashboard** and opens your overview directly.) |
| **Projects** | The list of projects you can see, and the door to creating one — see the *Projects & Their Settings* chapter. |
| **Issues** | The master list of issues across your projects, with search and filters. |
| **Quick add** | A fast, stripped-down form for logging a problem in seconds. |
| **Reports** | Charts and breakdowns of your issues. |
| **SLA status** | The Service-Level Agreement (SLA) board — which issues are close to, or past, their response and resolution targets. (Web app only.) |
| **Notifications** | Your alerts. A red number badge on this item shows how many are unread. |
| **Backup & export** | Download a copy of your data. |
| **Import** | Bring issues in from a file. (Web app only.) |
| **Preferences** | Your personal settings for the app. |
| **Users** | The instance-wide account list. This item appears **only for Administrators**. |
| **(your email)** | Your account page, where you manage your password, passkeys, and email. |
| **Logout** / **Sign out** | Ends your session and returns you to the Log in page. |

> **YOUR ROLE DECIDES WHAT YOU SEE** — OpenTrack only shows you the buttons your **role** allows. A Reporter sees the controls to file and comment on issues; a Manager also sees a project's Settings; an Administrator sees everything, including the **Users** menu item. So if a button described in this manual isn't on your screen, it's almost always because your role doesn't include it — see the *People & Roles* chapter. The same is true of optional features (AI, Git, SLAs, public intake): their buttons only appear once someone turns them on.

> **A KEYBOARD SHORTCUT WORTH KNOWING** — In the web app, the left navigation shows a small reminder: *Ctrl+K to jump* (⌘K on a Mac). Press it from anywhere to pop open a quick jump-to box, so you can hop to an issue or project without hunting through menus.


## Desktop app: which server it talks to

The desktop app needs to know your server's web address. It ships with a default, and you can change it anytime from the **Settings** entry in its left navigation — which stays visible even before you sign in, precisely so you can point the app at the right server first.

1. In the desktop app's left navigation, select **Settings**.
2. In the **Server address (OpenTrack server)** box, type your server's address — for example `http://192.168.1.50:5003`, or an `https://…` address if your server uses encryption.
3. Select the **Save** button. A green confirmation reads *Saved. New requests (including sign-in) will use this server.*
4. Select **Back** to return, then sign in as usual.

> _[Figure: The desktop app Settings page with the Server address box and the Save button]_

> **THE ADDRESS MUST START WITH http:// OR https://** — If you type an address the app can't understand, saving shows a red message: *Enter a valid address that starts with http:// or https://*. Include the `http://` (or `https://`) at the front, and the port number if your server uses one (the `:5003` part). The address is remembered on that computer, so you only set it once.


## Troubleshooting

- *My email and password are rejected (“Invalid login attempt”).* Double-check for typos and stray spaces, and confirm Caps Lock is off. Passwords are case-sensitive. If you truly can't get in, use **Forgot your password?** or ask your Administrator to reset it.
- *I don't have an account and there's no Register link.* Registration may be closed on your server, or an Administrator may create accounts for you. Ask whoever runs the server to add you.
- *The page says I must confirm my email, but no email ever arrives.* Many home setups don't send email. If confirmation isn't actually required on your server you can still sign in; if it is, ask your Administrator to confirm your account or turn email confirmation off.
- *The desktop app can't reach the server.* Open **Settings** and check the **Server address** is exactly right — the `http://` or `https://` prefix, the correct IP address or name, and the port number. Make sure the server is powered on and on the same network.
- *A menu item this manual mentions isn't in my left navigation.* It's almost certainly your **role** (for example, **Users** shows only for Administrators) or an optional feature that's turned off. See *People & Roles*. Note too that the desktop app's menu is slightly shorter than the web app's (no **SLA status** or **Import**).
- *The menu disappeared on my phone.* It collapsed to save space. Select the small toggle button (three stacked lines) at the top to bring it back.
- *“Log in with a passkey” does nothing.* You need to have created a passkey first, and typed your email in the **Email** box. If you've never set one up, sign in with your password instead.


# 2. The Dashboard

*Your at-a-glance answer to “where should I look first?”*

> **QUICK VERSION** — The **Dashboard** is your home screen. Across every project you can see, it shows how many issues are **open**, **overdue**, and **stale** (quietly forgotten), plus a severity breakdown, a per-project table, and a list of what changed most recently — all with links to jump straight in.


## What the dashboard is for

The **Dashboard** is your home base — the first thing to open at the start of a session. It gathers, in one place, a summary of everything you're allowed to see across **all** your projects, so you can spot where attention is needed without opening each project one by one. Every number on it is already filtered to what your role lets you see: you will never see counts for projects or issues you don't have access to.

To open it, select **Home** in the left navigation (in the desktop app this entry is labeled **Dashboard**). The page title reads *Dashboard*.

> _[Figure: The Dashboard: the row of summary tiles across the top, with the By project and Recently updated sections below]_


## The four summary tiles

Across the top sits a row of four cards (they stack into a single column on a narrow screen). Reading left to right:

| Tile | What it shows | Plain meaning |
| --- | --- | --- |
| **Open issues** | One big number: the total count of open issues across every project you can see. | How much unfinished work is on the books right now. |
| **Overdue** | A big number with the note *Open past their due date.* The whole tile turns red when the count is above zero. | Work that has already blown its deadline — the most time-sensitive pile. |
| **Stale** | A big number with the note *Open, untouched 30+ days.* The tile takes on a warning (amber) tint when the count is above zero. The whole tile is a link. | Open issues nobody has touched in over a month — the stuff quietly slipping through the cracks. |
| **Open by severity** | A set of small colored pills, one per severity, each reading the severity name and its count (for example *Major: 4*). | How your open work splits by how bad each issue is — a quick way to spot serious ones hiding inside a healthy-looking total. |

> **THREE WORDS WORTH KNOWING** — **Open** — the issue isn't resolved or closed yet. **Overdue** — it has a due date that has already passed and it's still open. **Stale** — it's open but nobody has touched it in over 30 days, so it may have quietly fallen through the cracks. Stale is about *silence*; overdue is about a *deadline*. An issue can be one, both, or neither.

> **LET THE COLORS DO THE TRIAGE** — You don't have to read every number. A **red** Overdue tile or an **amber** Stale tile is the dashboard waving a hand at you. If both are calm-colored and the counts are zero, there's nothing on fire — glance at *Open by severity* and move on.


### Reading the severity pills

The **Open by severity** tile color-codes each pill so the worst categories stand out. The exact severities come from your issues, but the coloring follows this pattern:

| Pill color | Severity level |
| --- | --- |
| Red | The most serious — a blocker or a crash. |
| Amber | Major — significant but not show-stopping. |
| Blue | Minor — small problems. |
| Gray | Everything else (for example, trivial or cosmetic). |

If there are no open issues at all, this tile simply reads *No open issues.* instead of showing pills.


## The two lists below the tiles

Under the tiles are two side-by-side lists that turn the summary numbers into places you can actually go.


### By project

A small table with one row per project that has open work. Its columns are:

| Column | What it means |
| --- | --- |
| **Project** | The project name, shown as a link. Select it to jump to that project's issue list. |
| **Open** | How many open issues that project has. |
| **Overdue** | How many of those are past due. This number turns red and bold when it's above zero, so a struggling project catches your eye. |

If no project has any open issues, this section reads *No open issues in any project.*


### Recently updated

A list of the issues that changed most recently — a fast way back to whatever you or your team just touched. Each row shows the issue number and title as a link (for example *#42 Login button misaligned*), and underneath it the project name and current status (like *Website · New*). Off to the right is the date and time it was last updated, in your computer's local time. Select any row to open that issue. If nothing has happened yet, the list reads *No activity yet.*

> _[Figure: The By project table and the Recently updated list side by side]_


## The controls at the top of the dashboard

Just above the tiles are two handy controls:

- **＋ Quick add a problem** — a button that jumps straight to the Quick add form, so you can log something the moment you notice it without leaving the dashboard.
- **Live** — a small checkbox labeled *Live*. Tick it and the dashboard refreshes itself automatically whenever something changes, so the numbers stay current while you watch. Leave it unticked and the page holds still until you reload it.


## Using the dashboard

1. Select **Home** (or **Dashboard** in the desktop app) in the left navigation.
2. Scan the four tiles first. Check whether **Overdue** is red or **Stale** is amber.
3. Glance at **Open by severity** for any red pills — a pile of serious issues deserves attention even if the total looks fine.
4. In **By project**, find the project carrying the most overdue work and select its name to dive in.
5. Or, in **Recently updated**, select an issue to pick up wherever the team just left off.
6. Select the **Stale** tile to jump straight to a filtered list of only the stale issues, so you can revive or close them.

> **THE STALE TILE IS A SHORTCUT** — Because the whole **Stale** tile is a link, selecting it takes you directly to the Issues list already filtered to show only stale items. It's the quickest way to work through the forgotten pile in one sitting.


## Troubleshooting

- *The dashboard says “Nothing to show yet.”* You either belong to no projects yet, or no issues have been logged. Select the **Create a project** link in that message (or **Projects** in the menu) to get started. Once issues exist, the tiles fill in.
- *My counts look lower than a teammate's.* That's expected. Every number is filtered to your access. If you can't see a project, its issues never reach your totals — see *People & Roles*.
- *The numbers don't update when something changes.* Tick the **Live** checkbox at the top to turn on auto-refresh, or simply reload the page.
- *A project I know has bugs isn't in the By project table.* That table lists only projects with **open** issues. If everything in it is resolved or closed, it won't appear here.
- *The Overdue tile is red but I don't see due dates anywhere.* Overdue counts only issues that have a due date set and have passed it. Open the project's issue list to find which ones carry due dates.
- *A time in Recently updated looks wrong by a few hours.* Times are shown in your computer's local time zone. If your device's clock or time zone is off, these will be too — fix the device's date-and-time settings.


# 3. Projects & Their Settings

*Create a project, then tune its categories, versions, and options.*

> **QUICK VERSION** — A **project** holds issues. To make one: select **Projects** on the left, then **New Project**, give it a name, and select **Create Project**. Optionally add **categories** and **versions** on its **Settings** page.


## What a project is

A **project** is a container for issues — usually one piece of software, one system, one product, or one area of responsibility. Every issue belongs to exactly one project, and access is granted **per project**: being a member of one project says nothing about the others. Most of OpenTrack's organizing power comes from this simple idea, so it's worth creating a project per real “thing” you track rather than dumping everything into one.


## The Projects list

Select **Projects** in the left navigation. The page title reads *Projects*, and you see a table of every project you're allowed to view. Its columns are:

| Column | What it shows |
| --- | --- |
| **Name** | The project's name, as a link. Select it to open the project's page. |
| **Description** | The one-line summary, in muted gray text (blank if none was written). |
| **Visibility** | A badge reading either **Public** or **Private** (explained below). |
| **Open Issues** | How many issues in that project are still open. |
| **(action)** | An **Edit** button at the end of the row — shown only to Managers and Administrators. |

If no projects exist yet, the page shows a short note instead of a table. Managers see *No projects yet. Create the first one to get started.*; everyone else sees *No projects yet. Check back once one has been created.* A **New Project** button sits at the top-right of the page, but only if your role is Manager or Administrator.

> _[Figure: The Projects list with the Name, Description, Visibility, and Open Issues columns and the New Project button]_


## Create a project

Creating a project is a Manager-or-Administrator job. Select **New Project** on the Projects page to open the *New Project* form. It has four fields and two buttons:

| Field | What to enter | Notes |
| --- | --- | --- |
| **Name** | A short, clear name for the project. | Required. Keep it recognizable at a glance in the list. |
| **Description** | A sentence on what the project covers. | Optional, but a good hint for newcomers. |
| **Ticket key** | A short code, like `WEB`, for friendly ticket numbers such as *WEB-42*. | Optional. Leave blank to use plain numbers (#42). Explained in full just below. |
| **Public** | A checkbox labeled *Public (visible to all users, not just members)*. | Ticked by default. Untick it to make the project private. |

1. Select **Projects** in the left navigation, then **New Project**.
2. Type a **Name**, and optionally a **Description**.
3. Optionally type a **Ticket key** (for example `WEB`) to get friendly ticket numbers like *WEB-42*; leave it blank for plain numbers.
4. Decide **Public** vs. private with the checkbox (it starts ticked, meaning public).
5. Select **Create Project**. You land on the new project's own page, ready to add issues and settings.
6. Changed your mind? Select **Cancel** to return to the Projects list without creating anything.

> _[Figure: The New Project form with Name, Description, the Ticket key box, the Public checkbox, and the Create Project button]_


## The Ticket key (friendly ticket numbers)

Every issue always has a plain number behind it, like **42** — that's its permanent internal identity and it never changes. The **Ticket key** is an optional short label you can put in front of that number so tickets read more naturally. Give a project the key `WEB`, and its issues show up as **WEB-42** instead of **#42** — on the issue page, in search results, and anywhere else a ticket is named. Leave the key blank and everything just shows the plain **#42** form. This is purely about **display**: the key never changes the underlying number, and turning a key on or off (or changing it) only changes how existing numbers are shown, never the issues themselves.

> **BEST WHEN ONE OPENTRACK TRACKS SEVERAL APPS** — Ticket keys shine when a single OpenTrack tracks more than one product. Give each project its own key — `WEB`, `SHOP`, `RADIO` — and a number like **WEB-42** instantly tells everyone which product it belongs to, with no chance of confusing it with **SHOP-42**. It's also much easier to quote over the phone or in an email than a bare number.

A few simple rules govern what a key can be. Type it however you like; OpenTrack tidies it up for you:

- It's made **uppercase** automatically — type `web` and it's stored as `WEB`.
- Only **letters and numbers** are kept; spaces, dashes, and punctuation are dropped.
- It's capped at **10 characters** — anything longer is trimmed to the first 10.
- It's completely **optional**. Blank means plain `#42`-style numbers, which is a perfectly good choice.

> **PUBLIC VS. PRIVATE** — A **public** project is visible to any signed-in user of your OpenTrack (good for shared, non-sensitive work). A **private** project is visible only to the people you add as members. This is separate from the *public trouble-ticket intake* feature (covered under Settings), which lets people **without any account** file a ticket.


## The project page

Selecting a project's name opens its page, headed by the project name and description. Along the top-right is a row of buttons that lead everywhere else you can go for this project:

| Button | Where it goes |
| --- | --- |
| **Board** | A drag-and-drop board view of the project's issues by status. |
| **Roadmap** | What's planned per upcoming version. |
| **Bug-hunt checklist** | A structured checklist for systematically hunting bugs in this project. |
| **Members** | Add people and set their roles — see the *People & Roles* chapter. |
| **Settings** | The project's control panel (categories, versions, and more — below). |
| **Edit Project** | Change the name, description, or visibility. Shown only to Managers. |

Below those buttons is the project's **Issues** heading with a **New Issue** button, and a table of the issues in this project (number, title, status, severity, priority, assignee). If none exist yet it reads *No issues reported yet.*


## Categories and versions

Two optional lists make a project's issues much easier to organize and report on. Both are set up on the project's **Settings** page (select **Settings** on the project). You need the **Manager** role on the project to change them; without it the page shows a yellow notice reading *You need the Manager role on this project to change its settings.*

- **Categories** — buckets you file issues under, like “User Interface,” “Database,” “Documentation,” or “Hardware.” Categories let you filter and group; pick a handful that match how you actually think about the project.
- **Versions** — the releases of your software or product. An issue can record the version it **affects** and the version it's **fixed in**. Those two facts are what power the **Roadmap** (what's coming) and the **Changelog** (what shipped), so filling them in pays off later.


### Adding and removing categories

The **Categories** section (top-left of the Settings page) lists the categories you already have, then gives you two small forms:

1. In the *New category name* box, type a name.
2. Select **Add**. The new category appears in the list above, and a green *Saved.* message confirms it.
3. To remove one, use the *Select category…* dropdown to choose it, then select the red **Delete** button.


### Adding and removing versions

The **Versions** section (top-right) works the same way, but a version carries more detail. Its add-form has these fields:

| Field | What to enter |
| --- | --- |
| **Version name** | The release label, for example `1.0`. (Placeholder: *Version name (e.g. 1.0)*.) Required. |
| **Description** | An optional note about the release. |
| **Release date** | An optional date picker for when it shipped (or is due). |
| **Released** | A checkbox. Tick it once the version has actually shipped; released versions show *(released)* next to their name in the list. |

1. Fill in at least the **Version name** (for example `1.0`).
2. Optionally add a **Description** and a **Release date**, and tick **Released** if it has already shipped.
3. Select **Add version**. It appears in the list, marked *(released)* if you ticked the box.
4. To remove one, pick it in the *Select version…* dropdown and select the red **Delete**.

> _[Figure: The project Settings page showing the Categories and Versions sections side by side]_


## Editing a project

To change a project's basics, select **Edit Project** on the project page (or the **Edit** button on the Projects list). The *Edit Project* form is the same four fields as creation — **Name**, **Description**, the **Ticket key**, and the **Public** checkbox — pre-filled with the current values. You can add, change, or clear the Ticket key here at any time; because the key only affects display, changing it is safe and instantly re-labels existing tickets (an issue that read *#42* starts reading *WEB-42*, and vice versa).

1. Change the **Name**, **Description**, **Ticket key**, or **Public** checkbox as needed.
2. Select **Save** to apply the changes and return to the project page, or **Cancel** to discard them.

> **IF SOMEONE ELSE EDITED IT FIRST** — If another Manager saved a change to the same project while you had the Edit form open, saving shows a red conflict message rather than silently overwriting their work. Reopen **Edit Project** to see the current values, then make your change again on top of theirs.


## Where the rest of a project's settings live

> **THE SETTINGS PAGE IS THE PROJECT'S CONTROL PANEL** — Beyond Categories and Versions, the Settings page is where a Manager sets up the project's deeper machinery. Across the top of the page are buttons for **Custom fields**, **Automation**, **SLA targets** (Service-Level Agreement), and **Git**. Further down the same page are **Integrations (outgoing webhooks)**, **Workflow** rules, and **Public trouble-ticket intake**. Each of those has its own chapter in this manual — the Settings page is simply where you'll find them. (Note that **Members** is reached from the project page, not the Settings page.)

Here's a quick map of what each of those settings does, so you know which chapter to turn to:

| Setting | In one line |
| --- | --- |
| **Custom fields** | Extra fields of your own on every issue in the project. |
| **Automation** | Rules that act automatically when issues change. |
| **SLA targets** | Response and resolution deadlines the SLA board tracks against. |
| **Git** | Link commits and branches to issues. |
| **Integrations (outgoing webhooks)** | Ping Slack, Discord, or your own service when issues change. |
| **Workflow** | Restrict which status changes are allowed. |
| **Public trouble-ticket intake** | Let people without an account submit tickets. |


## Troubleshooting

- *There's no New Project button.* Creating projects needs the **Manager** or **Administrator** role. If you only see a plain list, your role doesn't include it — ask an Administrator.
- *The Settings page shows a yellow “you need the Manager role” warning.* You can view the project but not configure it. Only a Manager (or Administrator) on that specific project can add categories, versions, and the rest.
- *I created a category/version but it's not showing.* Check for a red error message at the top of the Settings page. A blank name is the usual culprit — a version in particular needs at least a **Version name**.
- *A project I made isn't in the Projects list.* If you made it **private**, only its members see it — and you're added as its owner automatically, so it should appear for you. Confirm you're signed in as the same account that created it.
- *My Edit didn't save — I got a conflict message.* Another Manager changed the project at the same time. Reopen **Edit Project**, which now shows the latest values, and reapply your change.
- *I typed a Ticket key but it looks different than I typed.* That's expected — OpenTrack forces the key to uppercase, keeps only letters and numbers, and trims it to 10 characters. So `my-app!` becomes `MYAPP`. Retype it within those rules if the result isn't what you wanted.
- *My tickets still show #42, not my key.* Confirm the project's **Ticket key** is actually filled in on **Edit Project** (a blank key always shows plain numbers). If you just set it, reopen the issue — the friendly number appears wherever the ticket is named.
- *I want to delete a whole project.* Project deletion isn't done from these screens. Ask your Administrator, who manages instance-wide cleanup.
- *Public vs. private didn't do what I expected.* **Public** means every signed-in user can see the project; **private** limits it to members. Neither controls the separate *public intake* page, which is for people with no account at all.


# 4. People & Roles

*Add people to a project and give each exactly the access they need.*

> **QUICK VERSION** — Open a project, select **Members**, type someone's email, pick a **role**, and select **Add**. The role decides what they can do — from **Viewer** (read only) and **Reporter** (file issues) up to **Manager** (configure the project). **Administrator** runs the whole instance and is set elsewhere.


## How access works

OpenTrack grants access by **role**, and — except for the Administrator — roles are assigned **per project**. So the same person can be a Developer on one project, a Manager on another, and have no access at all to a third. This keeps sensitive projects genuinely private while letting you open others up widely.

There's one exception: the **Administrator** is a single, instance-wide (global) role, not something you hand out project by project. An Administrator can reach every project and every account, and is managed from the instance-wide **Users** screen (visible only to Administrators), not from a project's Members page.


## The roles, from least to most powerful

Roles stack: each one can do everything the roles below it can, plus a bit more. From least to most powerful:

| Role | What it adds (each includes everything above it) |
| --- | --- |
| **Viewer** | Read the issues in the project — look but don't touch. No filing, no editing. |
| **Reporter** | Also file new issues, and comment on the issues they're allowed to see. |
| **Updater** | Also edit issues — change fields, add tags, tidy details. |
| **Developer** | Also be **assigned** issues and move them through the workflow (the person who actually does the work). |
| **Manager** | Also configure the project: members, categories, versions, custom fields, automation, SLA targets, workflow, integrations, Git, and public intake. |
| **Administrator** | A **global** role, not per-project: runs the whole instance — every project, every user account, and the server-level settings. |

> **WHICH ROLES YOU CAN ASSIGN ON A PROJECT** — On a project's **Members** page you can assign any role from **Viewer** through **Manager**. You cannot assign **Administrator** there — that's a global role handled on the instance-wide **Users** screen by an existing Administrator. So the roles offered in the Members dropdowns are Viewer, Reporter, Updater, Developer, and Manager.

> **PICK THE LOWEST ROLE THAT FITS** — A good habit: give each person the least powerful role that still lets them do their job. Someone who only needs to watch is a Viewer; someone who files bugs is a Reporter; someone who fixes them is a Developer; reserve Manager for the one or two people who actually configure the project. You can always raise a role later.


## The Members page

Open a project and select **Members**. The page is headed *Members — (project name)*, with a **Back to project** button at the top-right. If you don't have the Manager role on this project, you'll see a yellow notice reading *You need the Manager role on this project to manage its members.* and nothing else.

Managers see a table of the current members with these columns:

| Column | What it shows |
| --- | --- |
| **Member** | The person's name (their email/username). The project's creator is marked *(owner)*. |
| **Role** | Their current role on this project (Viewer, Reporter, Updater, Developer, or Manager). |

Below the table are three small forms, side by side: **Add a member**, **Change a role**, and **Remove a member**.

> _[Figure: The project Members page: the members table on top, and the Add / Change role / Remove forms beneath it]_


## Add someone to a project

Use the **Add a member** form (left) to bring someone in:

1. In the *user's email* box, type the email address of the person's OpenTrack account.
2. In the role dropdown, choose their role (Viewer, Reporter, Updater, Developer, or Manager).
3. Select **Add**. They appear in the members table, and a green *Saved.* message confirms it.

> **THEY NEED AN ACCOUNT FIRST** — The form reminds you: *The user must already have an OpenTrack account.* You add people by the email of an existing account — you can't invite a stranger from here. If they don't have an account yet, they register first (or an Administrator creates one), and then you add them.


## Change someone's role

Use the **Change a role** form (middle) to raise or lower access:

1. In the *Select member…* dropdown, choose the person.
2. In the role dropdown, pick their new role.
3. Select **Update role**. The change takes effect immediately and the table updates.


## Remove someone

Use the **Remove a member** form (right) to take someone off the project entirely:

1. In the *Select member…* dropdown, choose the person. (The owner isn't listed here — see below.)
2. Select **Remove**. They lose all access to this project right away.

> **THE OWNER CAN'T BE REMOVED** — The person who created the project is its **owner**, marked *(owner)* in the members table. The owner is always a member and never appears in the **Remove a member** dropdown, so a project can't be left with nobody in charge. To hand a project off, add the new person as a Manager; changing ownership itself is an Administrator-level task.


## Privacy is built in

> **PRIVACY IS ENFORCED EVERYWHERE, NOT JUST IN LISTS** — OpenTrack never reveals an issue — or even that it exists — to anyone whose role doesn't allow it. That rule holds in the issue list, in search results, in reports, in the dashboard totals, and even if someone is handed a direct link. There's no “hidden but linkable” back door.

The same principle protects the Members page itself: the list of who's on a private project is only returned to that project's Managers. So membership isn't just about what people can edit — it's about what they can even see exists.


## Troubleshooting

- *The Members page shows a yellow “you need the Manager role” warning.* Only a Manager (or Administrator) on that specific project can manage its members. Ask the project's owner or a Manager to add you or bump your role.
- *Adding a member fails or shows a red message.* The most common cause is that the email doesn't match an existing OpenTrack account. Confirm the person has registered, and that you typed their exact account email.
- *I can't find someone in the Remove dropdown.* The project **owner** is deliberately excluded so a project can't be orphaned. If you're trying to remove the owner, you instead need to reassign ownership — an Administrator task.
- *I want to make someone an Administrator.* You can't do that from a project's Members page. Administrator is a global role set on the instance-wide **Users** screen by an existing Administrator.
- *Change a role or Remove didn't seem to do anything.* Make sure you actually picked a person in the *Select member…* dropdown first; leaving it on the placeholder does nothing. Watch for the green *Saved.* (or a red error) message after you submit.
- *A member says they still can't see the project.* Double-check they were added to the right project and with at least the **Viewer** role, and that they're signing in with the same account email you added.
- *I lowered someone's role but they still have access.* Role changes apply immediately, but the person may need to reload the page to see their newly limited (or expanded) options.


# 5. Reporting an Issue

*File a new issue with all the detail that makes it fixable — plus the fast lane and the AI-assisted lane.*

> **QUICK VERSION** — Open a project, select **New Issue**, type a **Title** and **Description**, set **Severity** and **Priority**, and select **Submit Issue**. Done — a bug is filed and you land on its page. In a hurry? Use **Quick add** and fill in the rest later.


## What this is and what it is for

An *issue* is OpenTrack's word for a single tracked item — a bug, a defect, a feature request, or any task you want to record and follow to completion. This chapter covers the *New Issue* form: the place where an issue is born, and your one chance to hand whoever fixes it the context they need. A well-written issue saves everyone time later. You do not have to fill in every field — only **Title** and **Description** are required — but the more you provide, the faster it gets resolved.


## Opening the New Issue form

A new issue always belongs to a *project* (the container that groups related issues — see the Projects chapter), so you start from inside one.

1. Open the project you want to file the issue against.
2. Select **New Issue**.
3. The form opens with the heading *New Issue — [project name]* at the top, so you can confirm you are filing against the right project before you type a word.

> _[Figure: The New Issue form, empty, with the 'New Issue — [project name]' heading at the top]_

> **NOTE** — If you land on the form and the heading names the wrong project, go back and open the correct project first. An issue's project is set when you create it from that project's New Issue button.


## The form, field by field

The fields appear top to bottom in the order below. Required fields are marked; everything else is optional and can be added or changed later on the issue's own page.

| Field | Required? | What to put, and why it matters |
| --- | --- | --- |
| **Title** | Yes | A short, specific summary. `Export button does nothing on the Reports page` beats `export broken`. This single line is what everyone sees in every list, so make it stand on its own. |
| **Description** | Yes | The full story of the problem. Supports **Markdown** formatting (covered below) — ideal for pasting logs, error messages, and stack traces into code blocks. A small note under the box reminds you: *Markdown supported — bold, code, and fenced code blocks for stack traces.* |
| **Steps to Reproduce** | No | A numbered recipe that makes the problem happen every time. For a bug, this is the single most useful thing you can provide — it lets the fixer see the failure with their own eyes. |
| **Category** | No | A dropdown of the buckets this project defines (for example *UI*, *Backend*, *Docs*). Leave it on *(none)* if you are unsure. Picking one helps people filter later. |
| **Expected Behavior** | No | What you thought *should* happen. Stating it plainly removes any guesswork about what 'correct' looks like. |
| **Actual Behavior** | No | What actually happened instead. The gap between Expected and Actual is the bug, stated in one glance. |
| **Severity** | No (defaults to Minor) | How *bad* the impact is if the problem happens. See the full table below. Severity is about impact, not urgency. |
| **Priority** | No (defaults to Normal) | How *urgent* it is to act. See the table below. A typo on the front page can be low severity but high priority; a rare crash can be high severity but low priority. |
| **Reproducibility** | No (defaults to Have-not-tried) | How reliably you can trigger the problem. See the table below. It tells whoever fixes it what they are up against. |
| **Due Date** | No | An optional target date, chosen from a date picker. Overdue open issues get flagged on the dashboard and in lists. |
| **Affects Version** | No | The release where the problem shows up, from this project's version list (or *(none)*). Powers the Roadmap view. |
| **Fix Version** | No | The release you expect to resolve it in (or *(none)*). Once set and the issue is resolved, it appears in that release's Changelog. |


### Severity — how bad it is

The **Severity** dropdown lists these choices, from lightest to heaviest impact:

| Choice | What it means |
| --- | --- |
| **Feature** | Not a defect at all — a request for something new to be added. |
| **Trivial** | A cosmetic nit that barely matters, like a slightly-off margin. |
| **Text** | A wording, spelling, or label problem — the text is wrong, not the behavior. |
| **Tweak** | A small adjustment or improvement to something that already mostly works. |
| **Minor** | A real bug, but with a workaround or limited impact. This is the default. |
| **Major** | A serious bug that significantly affects the ability to use the feature. |
| **Crash** | The program (or a part of it) crashes or aborts. |
| **Block** | A show-stopper — work cannot continue until it is fixed. |


### Priority — how urgent it is

| Choice | What it means |
| --- | --- |
| **None** | No particular urgency assigned yet. |
| **Low** | Can wait; handle it when convenient. |
| **Normal** | The standard, everyday level. This is the default. |
| **High** | Should be handled soon, ahead of normal work. |
| **Urgent** | Needs attention right away. |
| **Immediate** | Drop everything — this comes first. |


### Reproducibility — how reliably it happens

The exact words in the dropdown are shown in bold below (some run together on screen, like *HaveNotTried*):

| Choice | What it means |
| --- | --- |
| **Always** | It happens every single time you follow the steps. |
| **Sometimes** | It happens on some tries but not others. |
| **Random** | It happens unpredictably, with no pattern you can see. |
| **HaveNotTried** | You have not yet tried to make it happen again. This is the default. |
| **UnableToReproduce** | You tried to trigger it again and could not. |
| **NotApplicable** | Reproducing does not apply — for example, on a feature request. |


## Writing the description with Markdown

The **Description** box (and the Steps, Expected, and Actual boxes, and Notes later on) understands **Markdown**, a simple way to format plain text by typing a few symbols. You do not have to use any of it, but a little goes a long way — especially fenced code blocks, which keep logs and stack traces neatly lined up instead of turning into a wall of run-together text.

| Type this | To get |
| --- | --- |
| `**important**` | **important** — bold text for emphasis |
| `` `Login.cs` `` | `Login.cs` — inline code, good for file names and short snippets |
| a line of three backticks, then your log, then a line of three backticks | a shaded **code block** that keeps logs and stack traces monospaced and unwrapped |
| `- first` and `- second` on their own lines | a bulleted list |

> **TIP** — Pasting a full error message or stack trace? Wrap it in a fenced code block (three backticks above and below). It stays readable, and OpenTrack will not try to reformat the punctuation inside it.


## Submitting the issue

1. Once the required fields are filled, select **Submit Issue** at the bottom of the form.
2. OpenTrack creates the issue and takes you straight to its own page — the full issue view, where notes, attachments, relationships, tags, and history all live.
3. From there you can add anything you left off. Nothing is locked in; the issue page is fully editable (with the right role).

Next to Submit sits a **Cancel** link. Selecting it abandons the form and returns you to the project without creating anything.

> _[Figure: The New Issue form filled in, with the Submit Issue button, the Suggest with AI button, and the Cancel link at the bottom]_


## Optional: attach your location

For a problem tied to a physical place — a device in the field, a site inspection, a piece of equipment — you can pin coordinates to the issue. Near the bottom of the form is a **📍 Attach my location** button with a small line of text beside it that starts out reading *Optional — useful for a problem out in the field.*

1. Select **📍 Attach my location**.
2. Your browser or device asks permission to share your location — approve it.
3. The text beside the button changes to *Location attached: [latitude], [longitude]*, confirming the coordinates were captured.
4. Submit the issue as usual. The location rides along and shows on the issue page with a **view map** link.

> **NOTHING IS TRACKED** — Attaching your location captures a single spot at the moment you select the button. OpenTrack does not follow you, and it never updates the location on its own. If you never select the button, no location is recorded.


## Optional: let the AI suggest the triage

If your administrator has turned on the **artificial intelligence (AI)** assistant, an extra **✨ Suggest with AI** button appears beside **Submit Issue**. It reads your Title and Description and proposes a Severity, Priority, Category, and Tags for you to accept or change — a helpful starting point when you are not sure how to classify something.

1. Type at least a **Title** (a Description helps the suggestion too).
2. Select **✨ Suggest with AI**. This does *not* create the issue — it only fills the form in for you.
3. OpenTrack sets the Severity, Priority, and Category dropdowns to its best guess. Any suggested tags appear in a blue bar reading *✨ Suggested tags: … — add them on the issue after you create it* (tags are added on the issue's own page, not on this form).
4. Review every suggestion, change anything you disagree with, then select **Submit Issue** to actually file it.

The suggestion is always just a suggestion; you stay in control. The AI Assistant chapter covers it fully, including how an administrator turns it on. If the AI cannot produce a suggestion, a short red message says so and nothing on the form changes.

> _[Figure: The New Issue form after Suggest with AI ran, showing the dropdowns pre-filled and the suggested-tags bar]_


## The fast lane: Quick add

When you just need to get a problem written down before you forget it, use **Quick add** (its own page, titled *Quick add a problem*). It asks for the bare minimum so you can capture the thought in seconds — perfect on a phone or mid-meeting — and turns it into a normal issue you or someone else can flesh out later.

1. Open **Quick add**. Under the heading a line reads: *Just pick a project and describe it in a few words — everything else can be filled in later.*
2. **Project** — choose from the dropdown (*Choose a project…*). If you have set a default project and can still see it, it is pre-selected for you.
3. **What's the problem?** — type a one-line title. The box shows an example: *e.g. App crashes when saving an empty note*.
4. **More detail (optional)** — add anything else you remember, or leave it blank. Markdown and code blocks work here too.
5. Select **Save issue**. OpenTrack files it with sensible defaults (Severity *Minor*, Priority *Normal*, Reproducibility *HaveNotTried*) and opens the new issue's page.

Two more buttons sit beside Save: **Check for duplicates** (see below) and a **Cancel** link that discards the entry.

> _[Figure: The Quick add page: project dropdown, 'What's the problem?' box, optional detail box, and the Save issue button]_


### Catching duplicates before you file

Before saving, you can check whether the problem is already on file. Select **Check for duplicates**. OpenTrack searches for **similar existing issues** and, if it finds any, shows a yellow **Possible duplicates** box listing each match as a link — with its issue number, title, project, and current status. A line underneath asks: *is it one of these already?*

- If your problem is already filed, select the matching link and go there instead of creating a second copy — less clutter, and the whole discussion stays in one place.
- If none of them match, the box also says *Not one of these? Go ahead and save.* — so select **Save issue** with a clear conscience.

> **TIP** — The full New Issue form and the issue page also surface possible duplicates for you automatically, so you get a second chance to spot a repeat even if you skip the Quick add check.


## Troubleshooting

- *Submit Issue does nothing and red text appears.* A required field is empty. Both **Title** and **Description** must be filled in; a validation message points at whichever one is missing. Fill it and submit again.
- *The title will not fit.* Titles have a length limit. Shorten it to a crisp summary and move the detail into the **Description** — that is what the description is for.
- *The heading names the wrong project.* The project is fixed when you open the form from a project's **New Issue** button. Cancel, open the correct project, and start again.
- *My log or stack trace turned into one run-on paragraph.* You pasted it as plain text. Wrap it in a fenced code block — three backticks on a line above and below — to keep it monospaced and unwrapped.
- *There is no Suggest with AI button.* The AI assistant is off for your instance. It only appears when an administrator has enabled it (see the AI Assistant chapter).
- *Attach my location did nothing.* Your browser or device blocked the location request, or you dismissed the permission prompt. Allow location access and select the button again; success shows as *Location attached: …* beside the button.
- *Quick add says I lack permission.* You do not have rights to report an issue on the chosen project. Pick a different project, or ask a Manager to grant you access.
- *Suggest with AI created the issue by accident.* It should not — that button only fills the form. If an issue was created, you selected **Submit Issue**. Open the new issue and edit or resolve it as needed; issues are never lost.


# 6. The Issue Page — a Complete Tour

*Everything you can see and do on a single issue, from the number at the top to the history at the bottom.*

> **QUICK VERSION** — Everything about one issue lives on its own page. The **number, title, and action buttons** sit at the top, then the **key fields** (status, severity, and so on), then the full write-up, attachments, notes, related issues, and a complete **History** at the bottom. Scroll down — it is all there on one page.


## What this is and how you get here

Every issue has one page that shows and holds everything about it. You reach it by selecting an issue anywhere it is listed — a project's issue list, a search result, the dashboard, or the link you land on right after creating it. This chapter is a guided walk down that page from top to bottom, naming every section so you always know where a thing lives. Some sections only appear when they have content or when your role allows an action, so do not worry if you do not see every one on every issue.


## The header: number, title, and actions

At the very top, a small gray line reads *[project name] / Issue #[number]*. The **number** (like `#123`) is a permanent identifier you can quote in conversation, in commit messages, or to a colleague — it never changes and always points to this one issue. Directly below it, in large type, is the issue's **title**.

To the right of the title sit the action buttons. The exact set depends on your role:

| Button | What it does |
| --- | --- |
| **Monitor** / **Monitoring ✓** | Turns notifications for this issue on or off. When you are monitoring, the button reads *Monitoring ✓* and is filled in; select it again to stop. A line under the header reminds you: *Monitoring an issue notifies you of changes to it.* (See the Notifications chapter.) |
| **Print / PDF** | Opens a clean, print-friendly version of the issue you can print on paper or save as a Portable Document Format (PDF) file. (See the Printing chapter.) |
| **Edit** | Opens the edit form to change any field. This button only appears if you have permission to update the issue. (See the Working an Issue chapter.) |

> _[Figure: The top of an issue page: the project/number line, the title, and the Monitor, Print / PDF, and Edit buttons]_


## The key-fields panel

Just below the header is a compact block of the fields that describe the issue's current state. They are laid out in rows so you can read the whole situation at a glance.

The first row shows the four classifications:

- **Status** — where the issue is in its life: *New*, *Feedback*, *Acknowledged*, *Confirmed*, *Assigned*, *Resolved*, or *Closed* (the Working an Issue chapter explains each one).
- **Severity** — how bad the impact is (Feature, Trivial, Text, Tweak, Minor, Major, Crash, Block).
- **Priority** — how urgent it is (None, Low, Normal, High, Urgent, Immediate).
- **Reproducibility** — how reliably it can be triggered (Always, Sometimes, Random, HaveNotTried, UnableToReproduce, NotApplicable).

The next row shows who and when:

- **Reporter** — the person who filed the issue.
- **Assignee** — the person working it, or *Unassigned* if nobody has it yet.
- **Category** — the project bucket it belongs to, or *—* if none was chosen.
- **Updated** — the date and time it last changed, so you can tell a fresh issue from a stale one at a glance.

A third row shows the resolution and version details:

- **Resolution** — how the issue ended, once it is resolved (starts at *Open*; the full list is in the Working an Issue chapter).
- **Due** — the target date, or *—* if none was set.
- **Affects** — the release the problem shows up in, or *—*.
- **Fixed in** — the release that resolves it, or *—*.

> _[Figure: The key-fields panel showing the status/severity/priority/reproducibility row and the reporter/assignee rows]_


### The SLA badge

If the project has *Service-Level Agreement (SLA)* targets set — deadlines the team commits to for responding to or resolving issues — a colored **SLA** badge appears in this area with a due time in Coordinated Universal Time (UTC). It tells you how the issue is doing against its deadline:

| Badge | Meaning |
| --- | --- |
| **On track** (green) | Comfortably inside the deadline. |
| **At risk** (yellow) | The deadline is close; act soon to avoid missing it. |
| **Breached** (red) | The deadline has passed without the issue being resolved. |

If the project has no SLA targets, no badge shows at all — that is normal.


### Location and the private marker

- If a location was attached when the issue was filed, a **📍 Location** line shows the latitude and longitude with a **view map** link that opens the spot on an online map in a new tab.
- If the issue is marked private, a yellow **Private issue** badge appears. A private issue is hidden from lower-access viewers (see the Working an Issue chapter).


## Tags

The **Tags** section shows the free-form labels attached to the issue as small gray badges, or *none* if there are none. Tags are handy for grouping and filtering across projects (for example *regression* or *needs-design*). If you have permission to update the issue, you also get controls here to manage them:

- An **add a tag** box with an **Add** button. As you type, it suggests tags already in use so you can reuse an existing label instead of inventing a near-duplicate.
- A **Remove a tag…** dropdown with a **Remove** button, listing the tags currently on the issue.


## Custom fields

If this project defines any *custom fields* — extra fields beyond the built-in ones, like *Environment* or *Customer* — they appear in a **Custom fields** section. Viewers see them as a simple list of name-and-value. If you can update the issue, each one becomes an editable control (a text box, number box, date picker, or dropdown, depending on how it was set up), with a **Save custom fields** button. Required custom fields are marked with an asterisk (`*`). The Custom Fields chapter covers these in full.


## Possibly related

OpenTrack quietly looks for other issues that resemble this one and, if it finds any, lists them in a **Possibly related** box — each as a link with its number, title, and status. It is an automatic nudge to help you spot duplicates or connected problems. Unlike the **Relationships** section further down, this list is just a suggestion; it does not create any formal link.


## The AI summary card

If the AI assistant is turned on, a **✨ AI summary** card sits above the write-up. Select **Summarize thread** and OpenTrack reads the whole issue and its notes and produces a plain-language recap — the problem, what has been tried, and what is next. Once generated, the button changes to **Refresh** so you can regenerate it after new activity. A small note reminds you the text is AI-generated, so double-check anything important. If the AI is off, the card does not appear.

> _[Figure: The AI summary card with a generated recap and the Refresh button]_


## The write-up

Below the panels comes the heart of the issue — the text that was written when it was filed, with your Markdown rendered into real formatting. Each part gets its own clearly labeled heading, and a part is simply left out if it was never filled in:

- **Description** — always shown; the full story of the problem.
- **Steps to Reproduce** — the recipe to trigger it, if provided.
- **Expected Behavior** — what should happen, if provided.
- **Actual Behavior** — what happens instead, if provided.


## Attachments

The **Attachments** section lists files added to the issue — logs, screenshots, sample data — each as a link showing its file name, size, and who uploaded it. Below the list is the control to add a file, and (with the right role) a **Delete** button beside each one. The Notes, Attachments, and Relationships chapter walks through uploading and downloading in detail. If there are none, it simply reads *No attachments.*


## Notes

The **Notes** section is the running discussion — comments from you and everyone else, oldest to newest, each stamped with the author's name and the time. A note marked private is outlined in yellow with a **Private** badge. At the bottom is an **Add a note…** box; below it a **Private note (visible to developers and up)** checkbox, and an **Add Note** button. Notes also understand Markdown, so you can format them just like the description.


## Relationships

The **Relationships** section is where issues are formally linked to one another — this is the deliberate version of the automatic *Possibly related* list above. Each existing link is shown with its type, the other issue's number and title, and that issue's project. If you can update the issue, controls let you add a link (choose a type, type the other issue's number, and select **Add**) or remove one. The relationship types are:

| Type | What it says |
| --- | --- |
| **related to** | The two issues are connected, with no direction implied. This link reads the same from both sides. |
| **duplicate of** | This issue is the same problem as another. From the other issue's side it shows as *has duplicate*. |
| **parent of** | This issue is a parent that contains or oversees another. From the other side it shows as *child of*. |
| **blocks** | This issue is holding up another — the other cannot proceed until this is done. From the other side it shows as *blocked by*. |


## Linked commits

If the project has Git integration turned on and a code commit mentioned this issue's number, a **Linked commits** section lists those commits — each with its short identifier (a link to the code host if available), the commit message, and the author and time. A commit that closed the issue carries a green **resolved** badge. This section is hidden when there are no linked commits. The Git Integration chapter explains how commits get linked.


## Time log

The **Time log** section records work spent on the issue. When entries exist, it shows a **Total** at the top and a table with columns **When**, **Who**, **Time**, and **Note**, plus a **Remove** link on each row (with the right role). Below the table, an entry form lets you log more:

- **Hours** and **Minutes** — how long you spent.
- **Date** — the day the work was done (defaults to today).
- **Note (optional)** — a short description of what you did.
- A **Log time** button to save the entry.

If nothing has been logged, the section reads *No time logged yet.*


## History — the automatic paper trail

At the very bottom, the **History** section is a dated, automatic record of every change ever made to the issue, shown as a table with columns **When**, **Who**, **Field**, **From**, and **To**. Every field change, status move, and assignment lands here on its own row, so you can always reconstruct exactly how a problem was handled and by whom.

> **HISTORY IS AUTOMATIC AND CANNOT BE EDITED** — You never update the **History** yourself — OpenTrack writes every entry for you, with who and when, the moment a change is saved. It is the issue's permanent paper trail, and there is no way to edit or erase a line of it. If a fresh issue has none, it simply reads *No changes recorded yet.*

> _[Figure: The History table at the bottom of an issue, showing When / Who / Field / From / To columns]_


## Troubleshooting

- *There is no Edit button.* You do not have permission to change this issue. Editing requires an updater role or higher; ask a Manager if you need it.
- *I do not see the tag, relationship, custom-field, or time-log controls.* Those add and remove controls only show for people who can update the issue. Viewers see the values but not the editing controls.
- *There is no SLA badge.* This project has no Service-Level Agreement targets set, so there is nothing to measure against. That is normal.
- *There is no AI summary card.* The AI assistant is turned off for your instance. It only appears when an administrator has enabled it.
- *A whole section is missing (Linked commits, Custom fields, Location, Possibly related).* Those sections only appear when they have something to show — no linked commits, no custom fields defined, no location attached, or no similar issues found, respectively.
- *The Assignee reads Unassigned.* Nobody has taken the issue yet. Anyone with the right role can set an assignee on the Edit screen (see the Working an Issue chapter).
- *The view map link opens a blank or wrong spot.* The link uses the exact coordinates that were captured when the issue was filed. If they look wrong, the location was captured in the wrong place; there is no way to move it after the fact — file a note explaining the correct location.
- *I expected a note I wrote and it is not here.* Check whether you marked it **Private** — private notes are only visible to developers and up, and are outlined in yellow. A lower-access viewer will not see them.


# 7. Working an Issue

*Edit its fields, move it through its statuses, assign it to a person, and resolve or close it the right way.*

> **QUICK VERSION** — Select **Edit** to change any field. Move the issue along its **Status** (New → … → Resolved → Closed), set an **Assignee**, and when it is done pick a **Resolution** that says how it ended. Select **Save Changes** — every change is written to the History automatically.


## What this is and who can do it

Filing an issue records the problem; *working* it is everything that happens afterward — refining the details, handing it to a person, walking it through its stages, and finally marking it done. All of that happens on the *Edit* screen. Editing requires an updater role or higher, so if you do not see an **Edit** button on an issue, you do not currently have permission to change it (ask a Manager). Everything you change here is stamped into the issue's History automatically, with your name and the time.


## Opening the Edit screen

1. Open the issue and select **Edit** (near the title).
2. The Edit form opens under the heading *Edit Issue*, with *[project name] / Issue #[number]* above it so you know which issue you are changing.
3. The form comes pre-filled with the issue's current values — you are changing what is there, not starting blank.

> _[Figure: The Edit Issue form, pre-filled, showing the text fields and the Status / Severity / Priority dropdowns]_


## The fields you can change

The Edit form gathers every changeable field of the issue in one place. From top to bottom:

| Field | What it is |
| --- | --- |
| **Title** | The one-line summary (required — it cannot be left empty). |
| **Description** | The full write-up (required). Markdown is supported, exactly as on the New Issue form. |
| **Steps to Reproduce** | The recipe to trigger the problem. |
| **Expected Behavior** | What should happen. |
| **Actual Behavior** | What happens instead. |
| **Status** | Where the issue is in its life-cycle (see the table below). |
| **Severity** | How bad the impact is (Feature, Trivial, Text, Tweak, Minor, Major, Crash, Block). |
| **Priority** | How urgent it is (None, Low, Normal, High, Urgent, Immediate). |
| **Resolution** | How the issue ended, once resolved (see the table below). |
| **Assignee** | The person responsible for the issue — *Unassigned*, or any member of the project. |
| **Category** | The project bucket, or *(none)*. |
| **Reproducibility** | How reliably it can be triggered (Always, Sometimes, Random, HaveNotTried, UnableToReproduce, NotApplicable). |
| **Due Date** | An optional target date. |
| **Affects Version** | The release the problem appears in, or *(none)*. |
| **Fix Version** | The release that resolves it, or *(none)*. |
| **Sticky** | A checkbox that pins the issue to the top of the project's issue list (see below). |
| **Private** | A checkbox that hides the issue from lower-access viewers (see below). |

1. Change any of the fields above.
2. Select **Save Changes** at the bottom. You return to the issue page with your changes in place, and each one recorded in the History.
3. Or select **Cancel** to discard everything and go back untouched.


### The Sticky and Private flags

Two checkboxes near the bottom deserve special mention because they change how the issue behaves rather than just describing it:

- **Sticky** — labeled *Sticky (pinned to top of project issue list)*. Turn it on to pin an important issue to the top of the list so it does not scroll away as newer issues pile up. Use it sparingly, for the one or two things everyone needs to see.
- **Private** — hides the issue from lower-access viewers (for example a public reporter) while keeping it fully visible to the project team. Use it for anything sensitive. A private issue shows a yellow *Private issue* badge on its page.


## The status life-cycle

An issue moves through **statuses** as work progresses. You change the status from the **Status** dropdown on the Edit screen. The usual path runs from New at the top to Closed at the bottom:

| Status | What it means |
| --- | --- |
| **New** | Just filed; nobody has looked at it yet. |
| **Feedback** | Waiting on more information — usually a question back to the reporter before work can continue. |
| **Acknowledged** | Seen and accepted as something worth looking at, but not yet reproduced. |
| **Confirmed** | Reproduced and verified as a real problem. |
| **Assigned** | Handed to a specific person to work on. |
| **Resolved** | The work is done, paired with a **Resolution** that says how (see below). |
| **Closed** | Confirmed resolved and filed away — the end of the line. |

> **YOUR PROJECT MAY RESTRICT THE MOVES** — By default any status change is allowed. If a Manager has set up **workflow rules** (see the Workflow Rules chapter), only the moves they permit go through — try a disallowed one and the save is rejected with an explanation. This keeps issues flowing through your process in the intended order.


## Assigning work

Set the **Assignee** dropdown to the person who will handle the issue. The dropdown lists *Unassigned* plus every member of the project. Assigning is how work stops being 'someone should' and becomes 'this person will':

- The assignee is notified that the issue is now theirs.
- The issue shows up as theirs across the app — on their dashboard, in filters, and on the board.
- Assigning typically goes hand in hand with moving the **Status** to *Assigned*.


## Resolving and closing

When the work is finished, you record *how* it ended using the **Resolution** dropdown, and move the status along to Resolved and then Closed.

1. Set **Status** to **Resolved**.
2. Choose a **Resolution** that describes the outcome (the choices are in the table below).
3. Optionally set **Fix Version** so the issue appears in that release's Changelog.
4. Select **Save Changes**.
5. Once everyone is satisfied that it is truly done, open Edit again and set **Status** to **Closed**.


### The resolution choices

The **Resolution** dropdown offers these outcomes (some words run together on screen, like *WontFix*):

| Resolution | What it says about the outcome |
| --- | --- |
| **Open** | Not resolved yet — the starting value while work is still going on. |
| **Fixed** | The problem was corrected. The most common resolution for a real bug. |
| **Reopened** | It was resolved before, but the problem came back or was not actually fixed, so it is active again. |
| **UnableToReproduce** | The team could not make the problem happen, so there was nothing to fix. |
| **NotFixable** | The problem is real but cannot be fixed (for example, it is caused by something outside the project). |
| **Duplicate** | The same problem is already tracked in another issue; this one is closed in favor of that one. |
| **NotABug** | The reported behavior is actually correct or intended — nothing needs changing. |
| **Suspended** | Work is paused indefinitely — set aside for now rather than finished. |
| **WontFix** | A deliberate decision not to fix it, even though it is a genuine issue. |

Resolved and Closed issues drop out of the *open* counts and the default lists, so your working views stay focused on what is still active. But they are never deleted — they remain fully searchable, and their History stays intact, so you can always look back and see exactly how a past problem was handled.

> **TIP** — Closing is not the end of the record. If a closed problem resurfaces, you can reopen it: set the **Status** back to an active state and the **Resolution** to *Reopened*, so the history shows the full round-trip.


## Troubleshooting

- *There is no Edit button on the issue.* You lack permission to change it. Editing needs an updater role or higher; ask a Manager to grant it.
- *Save Changes was rejected with a message about the status.* A workflow rule blocks that particular status move. Pick a status the rule allows, or ask a Manager which transitions are permitted (see the Workflow Rules chapter).
- *Save Changes failed saying someone else changed the issue.* Another person saved edits while you had the form open, so OpenTrack stopped your save to avoid overwriting their work. Go back to the issue, re-open Edit to load the latest values, and redo your change on top of theirs.
- *The title will not save.* Both **Title** and **Description** are required and cannot be blank, and the title has a length limit. Fill them in and keep the title short.
- *The person I want is not in the Assignee dropdown.* Only members of this project appear there. Add them to the project first (see the Projects chapter), then assign.
- *I resolved the issue but it still shows as open.* Setting a **Resolution** alone is not enough — you also need to move the **Status** to **Resolved** (and later **Closed**). Check the Status dropdown.
- *A closed issue disappeared from my list.* Closed and Resolved issues are hidden from the default open-only views. They are not gone — search for the issue or turn off the open-only filter to see it again.
- *My change is not showing on the issue page.* Make sure you selected **Save Changes** and not **Cancel**. If the save went through, the change also appears as a new row in the issue's History.


# 8. Notes, Attachments, Tags & Relationships

*Discuss an issue, attach files to it, label it, and connect it to related issues.*

> **QUICK VERSION** — Open any issue. Scroll down to add **Notes** (the running discussion), upload **Attachments** (files and screenshots), add **Tags** (free-form labels), and add **Relationships** (links to other issues). Each of these has its own labeled section on the issue page.


## Where these sections live

Everything in this chapter happens on a single **issue detail page** — the page you land on when you click an issue's title anywhere in OpenTrack, or when you open the web address `issues/` followed by the issue number. Below the issue's main facts (status, severity, reporter, and the written-up description) the page continues downward through a series of clearly headed sections. In top-to-bottom order they are: **Tags** (up near the top, just under the issue's fields), the **Custom fields** section, the write-ups (**Description**, and if filled in **Steps to Reproduce**, **Expected Behavior**, and **Actual Behavior**), then **Attachments**, then **Notes**, then **Relationships**, and finally the automatic sections (**Linked commits**, **Time log**, and **History**). This chapter covers Tags, Attachments, Notes, and Relationships; Custom fields have their own chapter.

> **WHO CAN CHANGE THINGS** — Adding notes and uploading attachments is open to anyone who can see the issue. Adding or removing **tags** and **relationships**, and deleting an attachment, requires the **Updater** role (a developer-level account or higher) on that project. If you don't have it, you'll still see everything — you just won't see the add/remove controls.


## Notes — the running discussion

**Notes** are the conversation on an issue: questions, findings, decisions, and the plain record of "tried this, it didn't work." Keeping the discussion attached to the issue — rather than scattered across chat messages and email — means the whole story lives in one place that anyone can read from the top later. The **Notes** section lists every note already added, oldest at the top, each in its own bordered box showing who wrote it, the date and time, and a *Private* badge if it was marked private. If nothing has been added yet, the section simply reads *No notes yet.*


### Adding a note

1. On the issue page, scroll to the **Notes** heading.
2. Click into the box labeled *Add a note…* and type your message.
3. If this note should be seen only by the project's own team, tick the checkbox **Private note (visible to developers and up)** just beneath the box.
4. Click **Add Note**. Your note appears immediately at the bottom of the list, stamped with your name and the current time.

> _[Figure: The Notes section: existing notes in bordered boxes, the Add a note box, the Private note checkbox, and the Add Note button]_


### Formatting a note with Markdown

Notes accept **Markdown**, the same lightweight formatting you can use in an issue's description. You don't have to use any of it — plain typing works fine — but it's there when a note needs structure. The most useful marks are:

| You type | You get |
| --- | --- |
| `**important**` | **important** (bold) |
| `` `Ctrl+C` `` | `Ctrl+C` shown in a fixed-width code font |
| A line starting with `- ` or `* ` | A bulleted list item |
| Triple back-ticks on their own line, your text, then triple back-ticks again | A code block — ideal for pasting a log, a stack trace, or a configuration snippet so it keeps its line breaks and spacing |

> **TIP** — Paste long logs and error output inside a triple-back-tick code block. Without it, the raw text can wrap and collapse into an unreadable run; inside the block it stays exactly as you pasted it.


### Private notes

A **private note** is visible only to project members with a developer-level role or higher — the label on the checkbox says *visible to developers and up*. Use it for internal discussion you don't want a lower-access viewer to see: someone who filed the issue through a public intake form, or a reporter-only account, will not see private notes at all. On the list, a private note is drawn with a warning-colored border and carries a *Private* badge next to the author's name, so it's always obvious at a glance which notes are internal and which are open.

> **DECIDE BEFORE YOU POST** — Whether a note is private is set when you add it, using the checkbox. Treat that choice as final — post sensitive internal discussion only after you've ticked **Private note**, not after.


## Attachments — files and screenshots

Attach the evidence: a log file, a screenshot of the error, a sample document that reproduces the problem, or a configuration that triggers the bug. The **Attachments** section lists every file already on the issue. Each row shows the **file name** (as a link), the **file size**, and the **name of the person who uploaded it**. Sizes are shown in the friendliest unit — bytes (`B`), kilobytes (`KB`), or megabytes (`MB`). If nothing is attached yet, the section reads *No attachments.*


### Uploading a file

1. Scroll to the **Attachments** heading.
2. Use the file picker just below the current list of attachments — click it and choose a file from your computer.
3. In the browser version of OpenTrack, click **Upload** to send it. In the desktop app, the upload starts as soon as you pick the file.
4. The new file appears in the list, with your name shown as the uploader.

> **SIZE LIMIT** — A single attachment can be up to **10 MB**. If a file is larger than that, the upload is refused and an error message is shown; compress the file or trim it (for example, attach only the relevant portion of a large log) and try again.

> _[Figure: The Attachments section: a list of files with names, sizes, and uploaders, each with a Delete button, and the file picker below]_


### Downloading and deleting

To open or save an attachment, click its **file name** in the list — anyone who can see the issue can download any of its attachments. To remove a file that's no longer needed, click the **Delete** button on its row. Deleting requires the **Updater** role, so if you don't see a Delete button next to a file, your account doesn't have permission to remove it. Deletion is immediate and there is no undo, so make sure you have the right file before you click.


## Tags — shared, free-form labels

**Tags** are short, free-form labels — *regression*, *customer-reported*, *needs-design*, *good-first-issue* — that cut across the built-in fields and across projects. Where category and severity are fixed choices, a tag is anything you want it to be. The **Tags** area sits near the top of the issue page, just under the issue's main fields. It shows the issue's current tags as small gray badges, or the word *none* if it has no tags yet.


### Adding a tag

1. Find the **Tags** line near the top of the issue page.
2. In the box labeled *add a tag*, start typing. As you type, a drop-down of existing tags appears — these are tags already used anywhere in OpenTrack — so you can pick one and stay consistent instead of inventing a near-duplicate.
3. To reuse an existing tag, click it in the drop-down. To create a brand-new tag, just finish typing the new name.
4. Click **Add**. The tag appears as a badge on the issue right away. If the tag was new, it's created on the spot and becomes available to every project from then on.

> **TAGS ARE INSTANCE-WIDE** — A tag is shared across the whole OpenTrack instance, not tied to one project. That's the point: because *regression* means the same tag everywhere, you can later filter every project at once by that one label (see the Finding Issues chapter). Watch the type-ahead drop-down to avoid creating *regression*, *Regressions*, and *regressed* as three separate tags.


### Removing a tag

To take a tag off an issue, use the **Remove a tag…** drop-down next to the add box, choose the tag you want gone, and click **Remove**. This only removes the tag from *this* issue — the tag itself still exists and stays on any other issues that use it. Adding and removing tags both require the **Updater** role; without it you'll see the badges but no add or remove controls.


## Relationships — linking issues together

Real work is connected: one bug blocks another from being fixed, two reports turn out to be the same problem, a big task is really the parent of several smaller ones. **Relationships** capture those links so that opening one issue shows you the others it touches. The **Relationships** section lists the issue's current links, each worded from this issue's point of view — for example *blocks #142 — Login screen freezes* — with the linked issue's number as a clickable link and its project shown in gray. If there are no links yet, the section reads *No related issues.*


### The four relationship types

When you add a link you pick its type. There are four, and each one automatically shows the correct wording on **both** issues — the issue you add it from, and the issue you point it at. The table shows what you pick and how it reads from each side:

| Type you pick | Shows on this issue as | Shows on the other issue as | Use it when |
| --- | --- | --- | --- |
| **related to** | related to | related to | Two issues are connected but neither controls the other. The wording is the same from both sides. |
| **duplicate of** | duplicate of | has duplicate | This issue is the same problem as another one that's already filed. Point the newer/duplicate one at the original. |
| **parent of** | parent of | child of | This issue is a larger effort and the other is a piece of it. The other issue will read *child of* this one. |
| **blocks** | blocks | blocked by | This issue must be done before the other can move. The other issue will read *blocked by* this one. |

> **PICK THE RIGHT DIRECTION** — Direction matters for **duplicate of**, **parent of**, and **blocks**. Ask yourself which issue is the original, the parent, or the blocker, and add the link from that side (or from whichever side makes the sentence read true). If you get it backwards, remove the link and add it again the other way. Only **related to** reads the same in both directions.


### Adding a link

1. Scroll to the **Relationships** heading.
2. In the row of controls, open the **type** drop-down and choose *related to*, *duplicate of*, *parent of*, or *blocks*.
3. In the box marked *issue #*, type the number of the other issue you want to link to.
4. Click **Add**. A confirmation message appears, and the link now shows on this issue's list — and, worded from its own side, on the other issue's list too.

> _[Figure: The Relationships section: the list of existing links, the type drop-down, the issue-number box, and the Add button]_


### Removing a link

To remove a relationship, use the **Remove a link…** drop-down next to the add controls. It lists each current link by its wording and the other issue's number — for example *blocks #142*. Choose the one you want to remove and click **Remove**. The link disappears from both issues at once. As with tags, adding and removing relationships requires the **Updater** role.

> **RELATED, AUTOMATICALLY** — Separately from the links you add by hand, an issue page may show a *Possibly related* box listing issues with similar titles. That's a suggestion OpenTrack generates for you — it is not a relationship until you deliberately add one using the steps above.


## Troubleshooting

- *I don't see the boxes to add a tag or link an issue.* Those controls require the **Updater** role on the project. If you can read the issue but only see the existing tags and links (not the add/remove controls), your account is view-only for that project. Ask a project Manager to raise your role.
- *My private note is showing to everyone / isn't private.* Whether a note is private is set by the **Private note** checkbox at the moment you add it. Check the note in the list — a private note has a warning-colored border and a *Private* badge. If it's missing those, the checkbox wasn't ticked; there's no toggle after the fact, so add a fresh private note and, if needed, have a Manager remove the open one.
- *My pasted log looks like a jumbled run of text.* Wrap it in a triple-back-tick code block (back-ticks on their own line before and after the text). That preserves line breaks and spacing.
- *My upload was refused.* The file is probably over the **10 MB** limit. Compress it, or attach only the relevant part (for example, the tail of a large log). The error message on the upload box will confirm the reason.
- *There's no Delete button on an attachment.* Deleting requires the **Updater** role. If you don't have it you can still download the file, but not remove it.
- *I linked the wrong two issues, or the direction is backwards.* Use the **Remove a link…** drop-down to remove it, then add it again with the correct type or from the correct issue. Remember *duplicate of*, *parent of*, and *blocks* are directional; *related to* is not.
- *I typed an issue number to link but got an error.* Make sure the number is a real issue you're allowed to see, and that you typed the number only (not the `#`). The *issue #* box already supplies the `#`.


# 9. Custom Fields

*Add your own project-specific fields to capture exactly what your team needs to track.*

> **QUICK VERSION** — Need to track something the built-in fields don't cover (like a **Customer** or an **Environment**)? A project **Manager** adds the field once, under the project's **Custom fields** page, choosing a type (text, number, date, or a pick-one list). After that it appears on every issue in the project, and anyone who can edit an issue fills it in.


## Why custom fields exist

OpenTrack's built-in fields — status, severity, priority, category, reproducibility, versions, and so on — fit most work. But almost every team has something extra it always wants recorded, something specific to what they do. **Custom fields** let a project add exactly those. A field you define shows up on every issue in that project, right alongside the built-in fields, and its values are searchable and tracked in history like everything else.

Some common examples:

- A **Customer** name, so support issues can be tied to the account that reported them.
- An **Environment** chosen from a fixed list — *Production*, *Staging*, *Test* — so it's clear where a bug appeared.
- A **Hardware revision** or **Firmware version**, for teams tracking physical devices.
- An **Estimated hours** number, or a **Target date** for when something is expected.
- A **Severity tier** or any other pick-one label your team uses that the built-in severity list doesn't match.

> **PER PROJECT, NOT GLOBAL** — Custom fields belong to the project they're defined on. A field you add to the *Website* project does not appear on issues in the *Mobile App* project. If two projects need the same field, define it in each — this keeps every project's issue page free of fields that don't apply to it.


## The four field types

Every custom field has a **type** that decides what kind of value it accepts and how the input looks on the issue page. There are four:

| Type | What it accepts | How it looks on the issue | Good for |
| --- | --- | --- | --- |
| **Text** | Any free text, a single line. | A plain text box. | Names, short notes, reference codes — anything that doesn't fit the other three. |
| **Number** | A numeric value (checked that it reads as a number). | A number box. | Estimated hours, counts, revisions, any figure you might want to sort or compare. |
| **Date** | A single calendar date. | A date picker. | Target dates, dates something was observed, deadlines specific to your team. |
| **List (choose one)** | Exactly one value from a fixed list you define. | A drop-down of your options, plus a *— none —* choice. | Environment, tier, region — anywhere you want consistent values rather than free typing. |

> **PICK THE TYPE CAREFULLY** — A field's **type cannot be changed after it's created** — changing it would invalidate values people have already saved. If you get the type wrong, you have to delete the field and add it again with the right type (which clears any values already entered). So decide *List* vs. *Text*, or *Number* vs. *Text*, before you save.


## Defining a custom field (Manager)

Only a project **Manager** can add, edit, or remove a project's custom fields. If you open the Custom fields page without the Manager role, you'll see a message reading *You need the Manager role on this project to manage its custom fields* and no editing controls. Everyone else simply fills the fields in on issues, covered later in this chapter.


### Opening the Custom fields page

1. Go to the project you want to add a field to.
2. Open its **Custom fields** page. (The web address is the project's page followed by `/custom-fields`; there is also a link into it from the project's own screens.)
3. The page splits into two columns: on the left, **Fields on this project** — a table of what already exists; on the right, **Add a field** — the form for creating a new one. If no fields exist yet, the left side reads *No custom fields yet. Add one on the right.*

> _[Figure: The Custom fields page: the Fields on this project table on the left and the Add a field form on the right]_


### Adding a field

1. In the **Add a field** form, type a **Name** — this is the label operators will see on each issue (for example, `Customer`).
2. Choose a **Type** from the drop-down: *Text*, *Number*, *Date*, or *List (choose one)*. A helper line under the drop-down reminds you what each one means.
3. If — and only if — you picked **List**, fill in the **Options** box: one option per line. For example, type *High*, *Medium*, and *Low* on three separate lines. (For the other three types this box is ignored, so you can leave it blank.)
4. Tick **Required** if every issue must have this field filled in. Required fields are flagged with an asterisk (`*`) on the issue page and cannot be left blank.
5. Click **Add field**. It appears in the table on the left and, from now on, on every issue in the project.

| Column in the table | What it shows |
| --- | --- |
| **Name** | The field's label as operators see it. |
| **Type** | Text, Number, Date, or Enum (the internal name for the List type). |
| **Required** | *Yes* if the field must be filled in, *No* if it's optional. |
| **Options** | For a List field, the choices you defined, separated by commas. For the other types, a dash (*—*). |


### Editing a field

Click **Edit** on a field's row in the table. The right-hand form switches to *Edit* mode and pre-fills with that field's current settings. From here you can change:

- The **Name** — safe to change any time; it just relabels the field.
- The **Options** (List fields only) — one per line, same as when you created it.
- The **Display order** — a number that decides where this field sits relative to the others on the issue page. *Lower numbers sort first; 0 puts the field at the top.* Use this to group related fields together in a sensible reading order.
- **Required** — tick or untick whether the field must be filled in.

One thing you **cannot** change here is the **Type** — it's shown but greyed out, with a note explaining that changing it would invalidate values already saved. Click **Save changes** to keep your edits, or **Cancel** to back out without changing anything.

> _[Figure: The Edit form for a custom field, showing Name, the disabled Type, Options, Display order, and the Required checkbox]_


### Deleting a field

1. Below the fields table, find the **Select a field to delete…** drop-down.
2. Choose the field you want to remove.
3. Click **Delete**.

> **DELETING REMOVES ITS VALUES TOO** — Deleting a field also **removes that field's value from every issue in the project**, not just the field definition. There is no undo. If you only want to stop using a field but keep its history, consider leaving it in place (and un-requiring it) rather than deleting it.


## Filling a field in (everyone with edit rights)

Once a Manager has defined the fields, anyone with the **Updater** role (a developer-level account or higher) fills them in. On any issue in that project, the custom fields appear in their own **Custom fields** section, near the top of the page. Each field is drawn with the right kind of input for its type:

- A **Text** field shows a text box.
- A **Number** field shows a number box.
- A **Date** field shows a date picker.
- A **List** field shows a drop-down of the defined options, with *— none —* at the top so you can leave it unset.
- A **required** field is marked with an asterisk (`*`) next to its name.

1. Open the issue and find the **Custom fields** section.
2. Set or change each value using its input.
3. Click **Save custom fields**. A short *Saved.* confirmation appears. If something's wrong (for example a required field left blank, or a number that doesn't parse), an error message explains what to fix.
4. To clear an optional field, empty its box (or choose *— none —* on a List field) and save. You can't clear a required field this way — it has to hold a value.

Viewers who don't have edit rights still see the custom fields, but as a simple read-only list of *name: value* — an unfilled field shows a dash (*—*). And because custom-field values are ordinary issue data, they're covered by search and they show up in the issue's **History** whenever they change, just like the built-in fields.

> _[Figure: The Custom fields section on an issue, with a text box, a number box, a date picker, and a list drop-down, and the Save custom fields button]_


## Troubleshooting

- *I can't add or edit fields — I only see a warning.* Managing custom fields requires the **Manager** role on that project. The message *You need the Manager role on this project…* means your account isn't a Manager there. Ask an existing Manager to add the field, or to raise your role.
- *My List field's drop-down is empty (only shows — none —).* The field was created without options, or the **Options** box was left blank. Edit the field and enter the choices, one per line.
- *I picked the wrong type.* Type can't be changed after creation. Delete the field and add it again with the correct type — remembering this clears any values already entered for it.
- *A field won't save — it says it's required.* A required field can't be left blank. Enter a value, or ask a Manager to make the field optional (untick **Required** in the edit form) if it genuinely shouldn't be mandatory.
- *My number won't save.* A Number field only accepts values that read as a number. Remove any stray text, currency symbols, or letters and save again.
- *The fields are in an odd order on the issue.* A Manager can set each field's **Display order** in the edit form — lower numbers appear first, and 0 goes to the top. Adjust the numbers to group related fields together.
- *A field disappeared from every issue.* Someone likely deleted it on the Custom fields page, which also removes its values project-wide. It can be re-created, but the old values are gone.


# 10. Finding Issues: List, Filters & Search

*Narrow the list, search every word (including notes), save the views you use most, and jump anywhere from the keyboard.*

> **QUICK VERSION** — Click **Issues** in the left navigation, set any mix of filters (project, status, severity, priority, tag, or a text search), and click **Search**. Save a filter you use often so it becomes a one-click pill, or press **Ctrl+K** (**Cmd+K** on a Mac) to jump anywhere without the mouse.


## The issue list

Select **Issues** in the left navigation to open the issue list. It shows every issue you're allowed to view, across all your projects at once — the place most day-to-day finding happens. A **filter bar** runs along the top; the results appear as a table below it, and a small line reads how many matched, for example *24 issue(s) match*. If nothing matches, it reads *No issues match this filter.*

Each row in the results table shows, left to right:

| Column | What it shows |
| --- | --- |
| **#** | The issue's number — its permanent ID. Handy for the command palette and for linking relationships. |
| **Project** | Which project the issue belongs to. |
| **Title** | The issue's title, as a link. Click it to open the full issue. |
| **Status** | Where the issue is in its lifecycle (New, Assigned, Resolved, and so on). |
| **Severity** | How serious the issue is (Minor, Major, Crash, and so on). |
| **Priority** | How urgently it should be handled (Low, Normal, High, and so on). |
| **Assignee** | Who it's assigned to, or *Unassigned*. |
| **Updated** | When the issue last changed. |

> _[Figure: The issue list: the filter bar across the top, the saved-filter pills, and the results table below]_


## Filtering

The filter bar is a row of drop-downs and boxes. You set any combination of them and click **Search** — the filters **combine**, so each one you add narrows the results further (project *Website* *and* status *Assigned* *and* severity *Crash* returns only issues matching all three). Leaving a control on its *Any* or *All* setting means "don't filter on this." The controls are:

| Control | Choices | What it does |
| --- | --- | --- |
| **Project** | *All projects*, or any one project you can see. | Limits the list to a single project. |
| **Status** | *Any*, New, Feedback, Acknowledged, Confirmed, Assigned, Resolved, Closed. | Shows only issues in the chosen lifecycle state. |
| **Severity** | *Any*, Feature, Trivial, Text, Tweak, Minor, Major, Crash, Block. | Shows only issues of the chosen seriousness. |
| **Priority** | *Any*, None, Low, Normal, High, Urgent, Immediate. | Shows only issues at the chosen urgency. |
| **Tag** | *Any*, or any tag in use across the instance. | Shows only issues carrying that tag — the payoff of tagging consistently. |
| **Text** | A free-text box. | Full-text search across title, description, and notes (see below). |

Two buttons finish the bar: **Search** applies whatever you've set, and **Clear** wipes every filter and returns the list to its default, unfiltered state.

> **YOU ONLY EVER SEE WHAT YOU'RE ALLOWED TO** — Filtering can only ever narrow what you already have permission to view. Your access is applied first, then your filters on top. So a text search or a shared filter link can never surface an issue in a project you don't have access to.


## Sorting, and the Stale-only view

The **Sort by** drop-down (at the right of the filter bar) sets the order of the results. Your choices are:

- **Updated (newest)** — most recently changed first. This is the usual default.
- **Updated (oldest)** — least recently touched first.
- **Created (newest)** and **Created (oldest)** — by when the issue was first filed.
- **Priority (high→low)** — most urgent at the top.
- **Severity (high→low)** — most serious at the top.
- **Status** — grouped by lifecycle state.
- **ID (newest)** and **ID (oldest)** — by issue number.

Next to the sort control is a checkbox: **Stale only (open, 30+ days idle)**. Tick it to show just the open issues that nobody has touched in over 30 days — your "what's been forgotten?" view. It's the fastest way to surface work that quietly stalled. Combine it with a project or severity filter to focus the cleanup.

> **SET YOUR OWN DEFAULT SORT** — If you didn't pick a sort, the list uses your preferred default order (set in your preferences), falling back to *Updated (newest)* if you haven't chosen one. Pick a sort here any time to override it for the current search.


## Full-text search — it looks inside the notes, too

The **Text** box does more than match titles. It searches an issue's **title, its description, and every note on it**. That last part matters: a detail someone mentioned only in a comment three weeks ago will still surface the issue, even if that word appears nowhere in the title or description. You don't have to remember *where* something was written — only that it was.

> **SEARCH IS CASE-INSENSITIVE AND PARTIAL** — Matching ignores capitalization and matches on part of a word, so *login* finds *Login* and *logins*. When a search returns too much, add a filter (a project, a status, or a tag) rather than a longer phrase — the filters and the text box work together.


## Save a filter, and share it

Built a filter combination you'll want again? Don't rebuild it by hand each time — save it. Saved filters appear as one-click **pills** in a row just above the results.

1. Set up the filter exactly as you want it (any mix of drop-downs, the text box, sort, and the stale toggle).
2. In the box labeled *Save current filter as…*, type a name for it.
3. Click **Save**. The filter becomes a pill above the list; click that pill any time to re-run the whole filter in one click.
4. To remove a saved filter, use the **Delete a saved filter…** drop-down next to the save box, pick it, and click **Delete**.

> **BOOKMARK OR SHARE ANY VIEW BY LINK** — Every filter is written into the page's web address (URL). Whenever you run a search, the address bar updates to match — so you can **bookmark** that view in your browser, or **paste the link** to a colleague and they'll open the very same filtered list. (They'll still only see the issues they're allowed to, so a shared link never leaks anything.)

> _[Figure: The saved-filter row: existing filter pills, the Save current filter as box, and the Delete a saved filter drop-down]_


## Plain-English search (when AI is turned on)

If your OpenTrack administrator has turned on the optional AI features, an extra box appears above the filter bar labeled **✨ Ask in plain English**. Type a request the way you'd say it out loud — for example, *high-priority crashes nobody has touched in a month* — and click **Search with AI**. OpenTrack turns your sentence into the ordinary filters below (status, severity, priority, keywords, stale, project) and runs them. You can then tweak those filters by hand and search again.

> **IT ONLY FILLS IN THE FILTERS** — Plain-English search can only ever produce a filter you could have built yourself with the controls above — it adds no hidden search power and can't reach issues you're not permitted to see. If it can't make sense of your request, it says so; try naming a status, severity, priority, keyword, or "stale." If AI is switched off for your instance, this box simply isn't there.


## The command palette (keyboard)

For fast, mouse-free navigation, OpenTrack has a **command palette** — a quick-jump box you can pop open from any page. It only ever navigates (it changes which page you're on); it never changes data, so it's completely safe to open and poke around in.

1. Press **Ctrl + K** (on a Mac, **Cmd + K**). A search box drops down over the middle of the screen, prompting *Type to search, #123 to jump to an issue, or a command…*.
2. Start typing. The list below the box updates as you type.
3. Use the **↓** and **↑** arrow keys to move the highlight through the matches (the top one is highlighted to start).
4. Press **Enter** to go to the highlighted match — or click any match with the mouse.
5. Press **Esc**, or click outside the box, to close it without going anywhere.

What you can type into it:

| Type this | What you get |
| --- | --- |
| A number like `142` or `#142` | A *Go to issue #142* result that opens that issue directly. |
| A word or phrase | Matching **commands** (see below), plus a *Search issues for "…"* result that runs a text search for what you typed. |
| Part of a command name | That command — for example typing *back* finds *Backup & export*. |

The built-in commands the palette can jump to are: **Dashboard**, **All issues**, **Quick add a problem** (the fast issue-reporting form), **Projects**, **Notifications**, and **Backup & export**.

> _[Figure: The command palette open over the page, showing the search box and a list of quick matches including Go to issue and command entries]_


## Troubleshooting

- *My search returns nothing.* A leftover filter is probably too tight. Click **Clear** to reset everything, then add filters back one at a time. Remember the filters combine — project *and* status *and* severity all have to match at once.
- *An issue I expected isn't in the list.* Check the **Project** filter isn't pinned to the wrong project, that **Status** isn't hiding it (a Closed issue won't show if Status is set to New), and that **Stale only** isn't ticked. If you still can't see it, you may not have access to its project.
- *My text search isn't finding a word I know is there.* The Text box matches title, description, and notes — but not attachments' contents. If the word only lives inside an attached file, search won't find it. Otherwise, check for a typo and remember matching ignores capitalization.
- *I saved a filter but it's not in the pills.* Give the filter a name in the *Save current filter as…* box before clicking **Save** — an unnamed filter isn't saved. Then look for its pill in the row above the results.
- *I pasted a filter link to a colleague and they see fewer issues.* That's expected: a shared filter link shows each person only the issues they're allowed to view. The filter is the same; the access is theirs.
- *The ✨ Ask in plain English box isn't there.* AI features are optional and off unless an administrator enables them. Use the regular filter bar instead — it can do everything the plain-English box turns your request into.
- *Ctrl+K does nothing.* Make sure the OpenTrack page has focus (click once on the page first). On a Mac use **Cmd+K**. If a browser extension has claimed that shortcut, you can still open any page from the left navigation instead.


# 11. The Board (Kanban View)

*See a project's work as cards in columns, and move each card along as it progresses.*

> **QUICK VERSION** — The **Board** shows one project's issues as cards arranged in columns by status. Use the **◀** and **▶** arrow buttons on a card to move it one step back or forward through the workflow — that changes the issue's status. Turn on **Live** to have the board refresh itself when something changes.


## A visual way to see the work

The **Board** (often called a *Kanban* board — a Japanese term, roughly "signboard," for a visual card wall that tracks work) shows a single project's issues as **cards arranged in columns**. Each column is one **status**, and every issue sits in the column that matches where it is in its life. In one glance you can see how much work sits at each stage and where things are piling up — something a flat list can hint at but never show as clearly.

The board and the ordinary issue **list** show the same issues; they are just two ways of looking at the same project. The list is better for searching, filtering, and bulk edits. The board is better for a quick read of "where is everything right now?" and for nudging issues forward one step at a time. You can jump between the two at any moment using the buttons in the top-right corner.


## Opening the board

1. Open the project you want to look at (from **Projects** in the left navigation, then the project's name).
2. Select **Board**. The board opens on its own page, titled **Board — *ProjectName*** across the top.
3. The columns fill in with the project's current issues, each as a card in the column that matches its status.

> _[Figure: The Board view with issue cards spread across the status columns]_

Across the top-right of the board are two buttons and one checkbox:

| Control | What it does |
| --- | --- |
| **Live** (checkbox) | Turns on auto-refresh. When checked, the board reloads by itself whenever something on it changes, so you don't have to refresh the page to see a card someone else just moved. Leave it unchecked to keep the view still. |
| **List view** (button) | Switches to the ordinary issue list for this same project — the searchable, filterable table version of the same issues. |
| **Back to project** (button) | Returns to the project's main page. |


## The columns

There is one column for every status an issue can have, laid out left to right in workflow order — from just-filed on the left to finished on the right. If a project has more columns than fit on your screen, the row of columns scrolls sideways; drag the horizontal scrollbar underneath, or swipe, to reach the ones off to the right.

The columns, in order, are:

| Column | What it means |
| --- | --- |
| **New** | Just filed. Nobody has looked at it yet. |
| **Feedback** | Waiting on more information from the person who reported it. |
| **Acknowledged** | A maintainer has seen it and agrees it is worth tracking. |
| **Confirmed** | Reproduced or verified — it is a real issue. |
| **Assigned** | Someone is now responsible for working on it. |
| **Resolved** | A fix or a decision is in; the issue is awaiting final closure. |
| **Closed** | Done and put to bed. |

At the top of each column is a **header** showing the status name on the left and a small gray **count badge** on the right — the number of cards currently in that column. A quick scan of the badges tells you the shape of the project: a big pile under *New* means a triage backlog; a big pile under *Assigned* means a lot of work in flight. A column with no issues shows a single dash (*—*) instead of cards.

> **SAME ORDER AS THE STATUS LADDER** — The columns are the issue **statuses** in their built-in order. Everything from **New** through **Assigned** is an issue that is still open and being worked; **Resolved** and **Closed** are the finished end of the line. Moving a card into **Resolved** or beyond is what stops an issue's service-level clock (covered in the SLA chapter).


## What each card shows

Within a column, cards are stacked top to bottom, with the highest-priority issues first. Each card is compact but carries the essentials:

- The issue's **number and title** on the first line — for example `#42 Login button does nothing`. This is a link; select it to open the full issue.
- A smaller gray line underneath showing three facts separated by dots: the issue's **severity**, its **priority**, and its **assignee** — for example *Major · High · Priya*. If nobody is assigned, that last part reads *Unassigned*.
- A pair of **◀** and **▶** arrow buttons along the bottom (shown only if you are allowed to move the issue — see below).

> _[Figure: A close-up of a single board card: number and title, the severity-priority-assignee line, and the two move arrows]_


## Moving a card

Moving a card is how you change an issue's status from the board. It does exactly the same thing as editing the status field on the issue itself — it is just faster and more visual. OpenTrack's board uses **arrow buttons**, not drag-and-drop, so it works the same whether you are on a desktop, a laptop trackpad, or a touch screen.

1. Find the card you want to move.
2. Select **▶** (the right arrow) to move it one column to the right — forward one step in the workflow.
3. Select **◀** (the left arrow) to move it one column to the left — back one step.
4. The board reloads and the card reappears in its new column. A short confirmation line appears near the top, for example *Moved #42 to Confirmed.*

Each press moves the card exactly **one column**. To move an issue several stages at once, press the arrow that many times. The arrows know where the ends are: the **◀** button is grayed out (disabled) on any card in the leftmost column, and the **▶** button is grayed out on any card in the rightmost column, so you can never push a card off either end.

> **WORKFLOW RULES STILL APPLY** — Moving a card obeys the same rules as any other status change. If your project has a workflow that restricts which statuses can follow which, or if you don't have permission to make a particular move, the board respects that — it will tell you rather than making an invalid change.


## Who can move cards

Seeing the board is open to anyone who can see the project. **Moving** a card, though, changes an issue, so it requires the same permission as editing an issue — an updater role or higher on that project. If you have only view access, the board still shows you every card and every count, but the **◀** and **▶** arrows simply do not appear on the cards. Nothing is broken; you are looking at a read-only version of the board.

If your role changes, or an occasional issue is one you specifically cannot touch, an attempted move is refused cleanly. Instead of a confirmation you will see a line such as *You don't have permission to move that issue.* and the card stays where it was.


## The Live checkbox in daily use

The **Live** checkbox is worth turning on when you are watching a project during a busy stretch — a triage session, an incident, or an event where several people are working the same board. With **Live** on, cards other people move slide into their new columns without you touching anything, so the board on your screen stays honest. When you are done, uncheck it to freeze the view; a still board is easier to study when you are planning rather than reacting.


## Troubleshooting

- *The move arrows aren't on my cards.* Moving a card needs updater (or higher) permission on the project. If you have only view access, the arrows are hidden by design — you can read the board but not change it. Ask a project manager to raise your role if you need to move issues.
- *One arrow is grayed out.* That is expected at the ends of the workflow. A card in the leftmost column has no column to its left, so **◀** is disabled; a card in the rightmost column has nowhere further right, so **▶** is disabled.
- *I pressed an arrow and got 'You don't have permission to move that issue.'* The move was refused. Usually this means your role doesn't allow changing that particular issue's status. The card stays put; nothing was changed.
- *A column is empty and just shows a dash.* That simply means no issues currently have that status. It is normal — for example, a healthy project often has an empty *Feedback* column.
- *Someone moved a card but I still see it in the old column.* The board you are looking at was loaded a moment ago and hasn't caught up. Turn on **Live** for automatic updates, or reload the page to pull the latest.
- *The board looks cut off on the right.* There are more columns than fit your window. Scroll the row of columns sideways using the scrollbar beneath them (or swipe) to reach *Resolved* and *Closed*.
- *I want to search or filter, but the board won't let me.* The board is for a quick visual read, not searching. Select **List view** at the top-right to switch to the searchable, filterable table of the same issues.


# 12. Notifications, Monitoring & Time Logging

*Get told when things change, and record the effort you spend on an issue.*

> **QUICK VERSION** — Select **Monitor** at the top of an issue to be told when it changes. Check **Notifications** in the left navigation to read what's happened, and select **mark read** to clear an item. Record the effort you spend in the issue's **Time log** section near the bottom of the issue page.


## Monitoring an issue

**Monitoring** an issue (some trackers call it *watching*) means "tell me whenever this changes." Once you monitor an issue, OpenTrack sends you a notification every time it is edited, gets a new note, or changes status — so you can keep an eye on something without opening it over and over.

The control is a single button at the top-right of any issue page, next to **Print / PDF**:

1. Open the issue you want to follow.
2. At the top-right, find the **Monitor** button.
3. Select it. It changes to read **Monitoring ✓** and fills in with a solid color, confirming you are now following the issue.
4. To stop following, select the button again. It returns to the plain **Monitor** label and you will no longer be notified about that issue.

A small reminder line sits just beneath the issue's title: *Monitoring an issue notifies you of changes to it.* The button is a simple on/off toggle — there is nothing else to set.

> _[Figure: An issue page with the Monitor button at the top-right, shown in its 'Monitoring ✓' on state]_

> **YOU ALREADY FOLLOW SOME ISSUES AUTOMATICALLY** — You are notified about every issue you **reported** and every issue **assigned to you**, without pressing anything. There is no need to Monitor those — you already follow them. Use **Monitor** for the *other* issues you want to keep an eye on: a teammate's bug you depend on, a report you triaged, or anything you just want to stay aware of.


### What actually triggers a notification

A notification goes out when an issue you follow **changes** — for example a new note is posted, its status moves, its assignee changes, or its fields are edited. The message you receive names the issue and gives a short summary of what changed, in the form *Issue #42 — Login button does nothing: status changed to Confirmed*.

Three rules keep notifications sensible rather than noisy:

- **You are never notified of your own changes.** If you are the one who made the edit, you don't get pinged about it — only the *other* followers do.
- **Access is re-checked every time.** If an issue has become private, or you have lost access to its project, you stop receiving notifications about it. The message text includes the issue's title, so OpenTrack makes sure it only ever reaches people who are still allowed to see that issue.
- **A breached service-level target notifies you too.** If an issue you are the assignee or a project manager for passes its resolution deadline, OpenTrack escalates it to you automatically (see the SLA chapter). That arrives as a notification reading *SLA breached — this issue has passed its resolution target.*

> **EMAIL, IF IT'S SET UP** — If your OpenTrack administrator has configured a mail server, the same notifications are also emailed to you, with a subject line like **[OpenTrack] Issue #42 updated**. If no mail server is set up, notifications still appear in the app — you just won't get email as well. Email delivery is best-effort: a mail hiccup never blocks the change that caused it.


## Reading your notifications

Select **Notifications** in the left navigation (the bell icon) to open your notifications page. It lists what has happened on the issues you follow, **newest first**. Unread items are highlighted with a colored band so they stand out from ones you have already seen.

Each row shows:

- The **notification text** — a link. Select it to jump straight to the issue it is about.
- A small gray **date and time** to the right of the text, showing when the change happened.
- A **mark read** link on the far right, shown only while the item is still unread.

> _[Figure: The Notifications page: unread items highlighted, each with its text, timestamp, and a 'mark read' link]_


### Clearing notifications

You have two ways to clear the unread highlight:

1. To clear a single item, select its **mark read** link. The highlight disappears and the *mark read* link is removed from that row.
2. To clear everything at once, select the **Mark all as read** button at the top of the list.

Selecting the notification's text link opens the issue so you can act on it, but it leaves the item marked as unread — use **mark read** when you actually want to clear it. The list keeps your most recent notifications (up to a couple hundred), so older, already-read items eventually roll off on their own. If you have nothing outstanding, the page simply reads *No notifications.*


## Logging time on an issue

**Time logging** records how much effort an issue took. It is useful for billing a client, for estimating how long similar work will take next time, or just for understanding where the hours actually go. Every entry is visible to everyone who can see the issue, so effort is on the record rather than guessed at.

The **Time log** section is near the bottom of every issue page, below the notes and relationships. Anyone with updater permission (or higher) on the project can add an entry.

1. Open the issue and scroll down to the **Time log** heading.
2. In the entry row, fill in the fields below.
3. Select **Log time**. Your entry is added and the running **Total** updates immediately.

The entry row has these fields:

| Field | What to enter |
| --- | --- |
| **Hours** | The whole hours of effort. Leave it at 0 for anything under an hour. |
| **Minutes** | The minutes of effort, on top of the hours. For example, an hour and a half is Hours 1, Minutes 30. |
| **Date** | The day the work was done. It defaults to today, but you can back-date an entry to when you actually did the work. |
| **Note (optional)** | A short description of what you did — for example *traced the crash to the login form*. Optional, but it makes the log far more useful to read later. |

> _[Figure: The Time log entry row with Hours, Minutes, Date, and Note fields and the Log time button]_


### Reading the time log

Once entries exist, the section shows a **Total** at the top — the sum of everyone's logged time, written in plain form such as *3h 45m* — followed by a table with one row per entry:

| Column | What it shows |
| --- | --- |
| **When** | The date the work was done. |
| **Who** | The person who logged it. |
| **Time** | How much time that entry was, in hours and minutes. |
| **Note** | The optional description, if one was entered. |
| **Remove** | A link (shown to updaters) that deletes that one entry. The Total re-adds itself afterward. |

If nobody has logged anything yet, the section simply reads *No time logged yet.* There is no timer to start or stop — you record effort after the fact, which keeps the log honest and lets you back-date work you forgot to enter at the time.


## Troubleshooting

- *I pressed Monitor but I'm not getting notifications.* Confirm the button reads **Monitoring ✓** (solid), not plain **Monitor**. Remember you are never notified about changes *you* make — only changes by other people. Test by having a teammate add a note.
- *I'm getting notifications for an issue I never chose to follow.* You are notified automatically about issues you **reported** or that are **assigned to you**. That is expected. If you don't want them, they will stop when you are no longer the reporter or assignee.
- *Notifications stopped for one issue.* You may have lost access to it — for example it was made private, or you were removed from the project. OpenTrack deliberately stops notifying anyone who can no longer see the issue, because the message text includes the issue's title.
- *I'm not getting emails, only in-app notices.* Email requires a mail server your administrator sets up. Without one, notifications still appear on the **Notifications** page — that part always works.
- *Selecting a notification opened the issue but it's still highlighted as unread.* Opening the issue does not clear the mark. Select **mark read** on the row (or **Mark all as read**) to clear it.
- *There's no Log time button, or no Remove link.* Logging and removing time need updater (or higher) permission on the project. With only view access you can read the time log and the total, but not add to it or remove entries.
- *My total looks wrong.* The **Total** is the sum of *everyone's* entries, not just yours. Check the **Who** column — other people may have logged time too.


# 13. Service-Level Agreements (SLA) & Escalation

*Set resolution deadlines by priority, and catch issues before they miss them.*

> **QUICK VERSION** — A **Manager** sets a resolve-by deadline (in hours) for each priority under **Settings → SLA targets**. OpenTrack then flags every open issue **On track**, **At risk**, or **Breached**, and the **SLA status** page in the left navigation lists everything that needs attention right now. Breaches notify the assignee and managers by themselves.


## What an SLA is

A **Service-Level Agreement (SLA)** is simply a promise about how quickly issues get dealt with — for example, "urgent problems are resolved within 24 hours." OpenTrack lets a Manager set a target number of **hours to resolve** for each **priority**, then quietly watches every open issue against its own clock and flags the ones drifting toward, or already past, their deadline. You don't have to remember which issue is due when; the system keeps time for you.

> **THE THREE STATES** — **On track** — comfortably within the target, plenty of time left. **At risk** — the issue has used up **80%** of its allowed time and still isn't resolved, so it needs attention now. **Breached** — the deadline has passed and the issue is still open. Each open issue shows its state as a colored badge, and the states are the whole point of the feature: green means fine, yellow means hurry, red means missed.


## How the clock works

Understanding the timing makes everything else clear:

- The clock **starts when the issue is created**.
- The clock **stops when the issue reaches Resolved** (or Closed). Anything from New through Assigned is still "open" and still on the clock; Resolved and Closed have stopped it.
- **At risk** kicks in once 80% of the target has elapsed. If the target is 10 hours, an issue is flagged at risk after 8 hours still open.
- **Breached** kicks in the moment the full target passes with the issue still open.
- A **resolved or closed** issue is no longer tracked — it reports *Not tracked* and shows no SLA badge, because its clock has already stopped.

> **ONLY OPEN ISSUES ARE ON THE CLOCK** — The SLA in this version tracks the resolution clock for **open** issues only. Once an issue is Resolved or Closed, it drops off the SLA radar — it can't be "at risk" or "breached" anymore because there is nothing left to wait for. Reopening an issue puts it back on the clock.


## Setting targets (Manager)

Resolution targets are set per project, and only a **Manager** on that project can change them. If you open the targets page without the Manager role, you will see the message *You need the Manager role on this project to set SLA targets.* and no editable fields.

1. Open the project, go to **Settings**, and select **SLA targets** across the top. (The page is titled **SLA targets — *ProjectName***.)
2. You will see a table with one row per priority. For each priority, type how many **hours** a new issue of that priority may stay open before it breaches.
3. Leave a priority's box **blank** (or 0) to not track that priority at all — no clock, no badge, no escalation for it.
4. Select **Save targets**. A confirmation reads *SLA targets saved.*

The priorities are listed most-urgent first, and the sensible pattern is shorter targets at the top:

| Priority | Resolve within (hours) — example | Roughly |
| --- | --- | --- |
| **Immediate** | 4 | Same working day |
| **Urgent** | 24 | 1 day |
| **High** | 72 | 3 days |
| **Normal** | 168 | 1 week |
| **Low** | (blank) | Not tracked |
| **None** | (blank) | Not tracked |

> **HANDY HOUR CONVERSIONS** — Because targets are entered in hours, keep these in mind: 1 day = **24** hours, 3 days = **72** hours, 1 week = **168** hours. The values above are only a starting point — set whatever your team actually promises.

> _[Figure: The SLA targets page: a row per priority with an hours box, and the Save targets button]_


## The SLA badge on an issue

Open any tracked issue and, near the top under its status and priority, you will see an **SLA** line with a colored badge showing its current state, plus the exact due-by time in Coordinated Universal Time (UTC): for example *SLA: At risk · due 8/14/2026 5:00 PM UTC*. The badge colors mirror the three states — green for *On track*, yellow for *At risk*, red for *Breached*. Issues whose priority isn't tracked (or that are already resolved) show no SLA line at all.


## The SLA status board

Select **SLA status** in the left navigation (the alarm-clock icon) to open your triage screen. This is a live, **cross-project** list of every open issue that has already breached or is at risk — pulled from all the projects you can see, most-overdue first. It is the single best place to answer "what most needs attention right now?"

The page has two sections, one above the other:

- **Breached (*N*)** — issues that have already passed their deadline, listed most-overdue first. The count *N* is right in the heading.
- **At risk (*N*)** — issues that have crossed the 80% mark but not yet the deadline.

Each row in either table shows these columns:

| Column | What it shows |
| --- | --- |
| **#** | The issue number. |
| **Project** | Which project the issue belongs to (the board spans all of them). |
| **Title** | The issue's title, as a link straight to the issue. |
| **Priority** | The issue's priority — the field the target was keyed to. |
| **Assignee** | Who is responsible, or *Unassigned*. |
| **Due (UTC)** | The exact resolve-by moment, in Coordinated Universal Time. |
| **Overdue by / Due in** | A badge showing time. In the Breached table it reads *Overdue by* with a red badge (for example *2d 3h*); in the At risk table it reads *Due in* with a yellow badge counting down. |

> _[Figure: The SLA status board: a red Breached section over a yellow At risk section, most-overdue first]_

If nothing is breaching or at risk across your projects, the page shows a cheerful green message instead of tables: *Nothing is breaching or at risk right now.* The board only ever shows issues you already have permission to see, so it is safe to leave open on a shared screen.


## Automatic escalation

> **BREACHES ESCALATE BY THEMSELVES** — You don't have to watch the clock — OpenTrack does. When an issue **breaches**, it automatically notifies the issue's **assignee** and the project's **managers** with a message reading *SLA breached — this issue has passed its resolution target.* This happens on its own, in the background, shortly after the deadline passes.

The escalation is deliberately quiet and tidy:

- It fires **once per issue**, so nobody is spammed with repeated warnings about the same breach.
- It reaches the **assignee and every project manager**, so the breach lands with the people who can actually act on it.
- If a breached issue has **nobody to escalate to yet** — no assignee and no managers — OpenTrack holds off and re-checks it later, so a manager or assignee added afterward still gets told rather than missing the already-breached issue.

Escalations arrive the same way as other notifications: on your **Notifications** page, and by email as well if a mail server is configured (see the Notifications chapter).


## Troubleshooting

- *I can't edit the SLA targets.* Setting targets needs the **Manager** role on that project. If you see *You need the Manager role on this project to set SLA targets*, ask a project manager to make the change or to raise your role.
- *An issue has no SLA badge.* Either its priority has no target set (its box is blank or 0), or the issue is already Resolved/Closed and has left the clock. Both are normal.
- *An issue I resolved still showed as breached earlier.* The clock stops at Resolved. Once resolved, the issue drops off the SLA status board and reports *Not tracked*. If it looks stuck, reload the page.
- *The SLA status board is empty.* That is good news — it means nothing you can see is breaching or at risk. You will see the green *Nothing is breaching or at risk right now* message.
- *A breached issue didn't notify anyone.* If it had no assignee and no project managers at the time, OpenTrack held the escalation and will send it once someone is in place. Assign it or add a manager and it will escalate on the next background check.
- *The 'Due (UTC)' time looks off by several hours.* It is shown in Coordinated Universal Time (UTC), not your local time, so it reads the same for everyone regardless of time zone. Convert to your local time if needed.
- *I set 0 hoping for an instant deadline.* A target of 0 (or blank) means *not tracked*, not "due immediately." Enter the actual number of hours you want to allow.


# 14. Automation Rules

*"When a new issue looks like this, do that" — handled for you, the moment it's filed.*

> **QUICK VERSION** — A **Manager** can set "when a new issue looks like this, do that" rules under **Settings → Automation** — so routine sorting (set the priority, assign it, add a tag) happens by itself the instant an issue is filed. A rule runs only on **new** issues, applies its **actions** only if the issue matches **all** its **conditions**, and rules run in order.


## What automation does

An **automation rule** runs the moment a **new issue is created** in the project. If the new issue matches the rule's **conditions**, the rule's **actions** are applied to it automatically. It is a way to bake your triage habits into the system so routine sorting happens without anyone lifting a finger — every crash report tagged and prioritized the second it lands, every issue in a certain category assigned to the right person, day or night.

> **NEW ISSUES ONLY** — Rules fire **once**, at creation time. They do not re-run when an existing issue is later edited, and changing a rule does not reach back and re-sort issues that already exist. Automation is about catching issues as they arrive, not tidying up old ones.


## Opening the rules editor

Automation is set per project, and only a **Manager** on that project can create or change rules. Opening the page without the Manager role shows *You need the Manager role on this project to manage automation rules.* and nothing editable.

1. Open the project, go to **Settings**, and select **Automation** across the top. The page is titled **Automation rules — *ProjectName***.
2. The top of the page lists any **Existing rules**; the bottom is the form for adding a new one (or editing an existing one).
3. Fill in the form and select **Add rule** (or **Save changes** when editing).

> _[Figure: The Automation rules page: a table of existing rules above, the add/edit form below]_


## The rule's name and run order

Every rule starts with three basic settings at the top of the form:

| Field | What it does |
| --- | --- |
| **Rule name** | A label so you can recognize the rule later — for example *Auto-tag crashes*. It is for your eyes only and has no effect on what the rule does. |
| **Run order** | A number that decides the order rules run in. **Lower numbers run first.** When several rules could touch the same new issue, order matters (see below). |
| **Enabled** | A checkbox. Uncheck it to switch a rule off without deleting it — handy for pausing a rule you might want back later. |


## The conditions — "When a new issue matches"

The first card in the form, headed **When a new issue matches**, is where you describe which issues the rule should act on. You can set any mix of the four conditions below. A rule fires only when the new issue matches **all** of the conditions you set — they are combined with "and," not "or." Leave any condition on **Any** (or blank) to ignore it.

| Condition | How it matches |
| --- | --- |
| **Title or description contains** | A text box. The rule matches if the words you type appear anywhere in the new issue's title or its description. Leave it blank (*any text*) to ignore text entirely. |
| **Category is** | A dropdown of the project's categories, plus **Any**. Pick one to match only issues filed in that category. |
| **Severity is** | A dropdown of the severity levels (Feature, Trivial, Text, Tweak, Minor, Major, Crash, Block), plus **Any**. Pick one to match only issues at that severity. |
| **Priority is** | A dropdown of the priorities (None, Low, Normal, High, Urgent, Immediate), plus **Any**. Pick one to match only issues at that priority. |

> **A RULE WITH NO CONDITIONS MATCHES EVERYTHING** — If you leave every condition on **Any**, the rule matches every new issue in the project. That can be exactly what you want — for example a rule that assigns every brand-new issue to a triage lead — but set it deliberately, not by accident.


## The actions — "Then do"

The second card, headed **Then do**, is where you set what happens to a matching issue. Set as many actions as you like; **leave an action blank (or on "— leave —") to skip it**, and the issue keeps whatever it came in with for that field.

| Action | What it does to the matching issue |
| --- | --- |
| **Set severity** | Changes the issue's severity to the level you choose. |
| **Set priority** | Changes the issue's priority to the level you choose. |
| **Set status** | Moves the issue straight to a status — for example jumping a known-good report to *Confirmed*. |
| **Assign to** | Assigns the issue to a project member. Note: this is applied only if that person is still a member of the project when the issue is created. |
| **Add tag** | Adds a tag to the issue. If the tag doesn't exist yet, it is created automatically. |

> _[Figure: The rule form's two cards: 'When a new issue matches' conditions above, 'Then do' actions below]_

> **A WORKED EXAMPLE** — Rule name **Auto-tag crashes**. Condition: *Title or description contains* *crash*. Actions: *Set severity* to Crash, *Set priority* to High, and *Add tag* *crash*. From then on, every report that mentions a crash is triaged and labeled the instant it is filed — no manual sorting, at any hour.


## How rules run together

A project can have many rules, and more than one may match the same new issue. When that happens, they run in **run-order** — lowest number first — and a **later matching rule can override an earlier one**. If rule 10 sets the priority to High and rule 20 also matches and sets it to Urgent, the issue ends up Urgent, because 20 ran after 10 and had the last word on priority.

This is worth keeping in mind when you build several rules: put broad, general rules earlier (lower numbers) and specific, override rules later (higher numbers), so the specific ones get the final say. If two rules touch *different* fields, order doesn't matter — they simply both apply.


## Managing existing rules

The **Existing rules** table at the top of the page lists every rule you have built, with these columns:

| Column | What it shows |
| --- | --- |
| **#** | The run order number (lowest runs first). |
| **Name** | The rule's name. |
| **When** | A plain-language summary of the rule's conditions — for example *text contains "crash"* — or *any new issue* if it has none. |
| **Then** | A plain-language summary of the rule's actions — for example *severity → Crash, priority → High, tag → crash*. |
| **Enabled** | *Yes* or *No*, so you can see at a glance which rules are live. |
| **Edit / Delete** | Buttons on each row. **Edit** loads the rule back into the form below for changes; **Delete** removes it. |

1. To **change** a rule, select **Edit** on its row. The form at the bottom fills in with that rule's settings and its heading changes to *Edit rule #*. Make your changes and select **Save changes**. Use the **Cancel edit** link to back out without saving.
2. To **pause** a rule without losing it, edit it and uncheck **Enabled**, then save. A disabled rule stays in the list (Enabled reads *No*) but never fires.
3. To **remove** a rule for good, select **Delete** on its row.


## Troubleshooting

- *I can't see or change automation rules.* Managing rules needs the **Manager** role on that project. If you see *You need the Manager role on this project to manage automation rules*, ask a project manager to make the change or raise your role.
- *My rule didn't fire on an existing issue.* Rules run only on **new** issues, at creation. They never re-run on issues that already exist, and editing a rule doesn't reach back to re-sort old ones.
- *My rule seems to match issues I didn't intend.* Check for conditions left on **Any** — those are ignored, so a rule with everything on Any matches *every* new issue. Tighten the conditions you care about.
- *Two of my rules fight over the same field.* Later rules (higher run-order numbers) override earlier ones. If the result isn't what you want, adjust the run-order numbers so the rule you want to win runs last.
- *The 'Assign to' action didn't stick.* Assignment is applied only if that person is still a member of the project when the issue is created. If they have since left the project, the assignment is skipped.
- *A new tag appeared that I didn't create.* The **Add tag** action creates the tag automatically if it doesn't already exist. That is expected — check whether a rule adds that tag.
- *I switched a rule off but it's still listed.* That is correct. Unchecking **Enabled** pauses a rule without deleting it; it stays in the table with *No* under Enabled and never fires until you re-enable it. Use **Delete** to remove it entirely.


# 15. Workflow Rules

*Restrict which status changes are allowed, so issues follow your process instead of jumping around at random.*

> **QUICK VERSION** — By default an issue can jump to any status. A **Manager** can limit which status changes are allowed under **Settings → Workflow** by listing each permitted **from → to** move. Add even one rule and only the moves you list are allowed anywhere in the app. Delete every rule to go back to an open workflow.


## What a workflow is for

Every issue in OpenTrack has a **status** — a single word that says where it stands, such as *New*, *Confirmed*, or *Closed*. As work happens, people change that status: a tester confirms a bug, a developer resolves it, a lead closes it out. A **workflow** is the set of rules that decides which of those changes are allowed.

Out of the box, OpenTrack uses an **open workflow**: an issue can move from any status to any other status, in any order. That is simple and flexible, and for a small team it is often all you need. But some teams want more discipline. They don't want a bug to leap straight from *New* to *Closed* without ever being confirmed and resolved along the way. For them, a Manager can define a workflow — an explicit list of the moves that are permitted — and OpenTrack will refuse every move that isn't on the list.

> **MANAGERS ONLY** — Only a **Manager** on the project can see or change the Workflow rules. If you open project Settings and see a yellow banner reading *You need the Manager role on this project to change its settings*, ask a project Manager to make the changes for you.


## The statuses you're working with

Before you write a single rule, it helps to know the full set of statuses an issue can hold. OpenTrack ships with seven, listed here in the order they normally flow. Both dropdowns in the Workflow section — the *from* side and the *to* side — offer exactly these seven.

| Status | What it usually means |
| --- | --- |
| **New** | Just filed. Nobody has looked at it yet. This is where public reports and freshly created issues land. |
| **Feedback** | Waiting on more information — usually a question back to the reporter before work can continue. |
| **Acknowledged** | Someone has read it and agrees it's worth tracking, but hasn't yet reproduced or committed to it. |
| **Confirmed** | Reproduced and verified as a real problem. |
| **Assigned** | Handed to a specific person to work on. |
| **Resolved** | The work is done — fixed, or otherwise dealt with — pending a final check. |
| **Closed** | Finished and filed away. No more work expected. |

> **"RESOLVED" IS THE DIVIDING LINE** — Elsewhere in OpenTrack — on the dashboard, in Reports, on the Roadmap — an issue counts as **open** while its status sits below *Resolved*, and as **done** once it reaches *Resolved* or *Closed*. Keep that in mind when you design a workflow: the path you draw is the path an issue takes from open to done.


## Opening the Workflow section

The Workflow rules live on the project's **Settings** page, partway down, under their own heading.

1. Open the project you want to set rules for.
2. Select **Settings** (or **Settings — *ProjectName*** at the top of the settings page).
3. Scroll down past **Categories**, **Versions**, and **Integrations — outgoing webhooks** until you reach the **Workflow** heading.
4. Read the short explanation under the heading: *Restrict which status changes are allowed. Leave this empty to allow any change (the default). Once you add even one rule, only the rules you list are permitted.*

> _[Figure: The Workflow section of project Settings, with its explanation text, current rule list, and the add-a-rule controls]_

What you see next depends on whether any rules exist yet:

- If there are **no rules**, a gray line reads *Open workflow — any status change is allowed.* This is the default state.
- If there **are rules**, each one is listed on its own line as a *from → to* pair, with a small red **remove** link beside it.


## Adding an allowed transition

A rule is a single permitted move: "an issue is allowed to go *from* this status *to* that status." You build your workflow one move at a time. The controls sit just below the current rule list: two dropdowns joined by an arrow, and an **Allow** button.

1. In the **left dropdown** (the *from* status), pick the status an issue would be leaving. It starts on *New*.
2. In the **right dropdown** (the *to* status), pick the status you want to allow it to move to. It starts on *Acknowledged*.
3. Select **Allow**.
4. The page saves, shows a green *Saved.* message at the top, and your new rule appears in the list above as a *from → to* line. The dropdowns reset, ready for the next rule.

> _[Figure: The add-a-rule row: the from-status dropdown, an arrow, the to-status dropdown, and the Allow button]_

Repeat that for every move your team legitimately makes. A common, orderly workflow looks like this:

| From | To | Meaning of the step |
| --- | --- | --- |
| New | Acknowledged | A triager has read the new report. |
| Acknowledged | Confirmed | The problem has been reproduced. |
| Confirmed | Assigned | Handed to someone to fix. |
| Assigned | Resolved | The fix is done. |
| Resolved | Closed | Signed off and filed away. |

> **LIST EVERY MOVE YOU ACTUALLY MAKE** — The moment you add your first rule, OpenTrack stops allowing anything you didn't list. If your team sometimes sends an issue back — say from *Confirmed* to *Feedback* to ask a question, or reopens a *Resolved* issue back to *Assigned* — you must add those moves too, or people will find themselves unable to make a change they legitimately need. When in doubt, add the extra rule.


## Removing a rule

Rules aren't permanent. To take one out:

1. Find the rule in the list of *from → to* lines.
2. Select the red **remove** link at the end of that line.
3. The page saves and the rule disappears from the list.

Remove rules until the list is empty and the *Open workflow — any status change is allowed.* line comes back. At that point the project is fully open again and OpenTrack will accept any status change. There is no separate "turn off workflow" switch — an empty list *is* the off state.


## Where the rules take effect

Once you've defined a workflow, it is enforced everywhere a status can change — you don't have to remember it or police it by hand. The same rules apply in all of these places:

- **The issue Edit screen.** When someone edits an issue, the status dropdown (or the save) will refuse a move that isn't allowed, so an out-of-order change can't be saved.
- **The Board.** Dragging an issue card from one status column to another is itself a status change, so the board honors the workflow too — a drag onto a status you haven't allowed won't stick.
- **Bulk actions.** When you change many issues at once (for example, a bulk *Close*), OpenTrack quietly skips any issue whose current status isn't allowed to reach the target status, rather than forcing an illegal move.

> **START OPEN, TIGHTEN LATER** — If you're not sure you need a workflow, leave it open. A workflow is worth adding once you've actually felt the pain of issues skipping steps — not before. When you do add one, sketch the whole path first (including the "send it back" moves), then enter each move as a rule.


## Troubleshooting

- *The Workflow section isn't on my Settings page, or everything is grayed out.* You're not a Manager on this project. Only Managers see and edit workflow rules. A yellow banner near the top of Settings — *You need the Manager role on this project to change its settings* — confirms it. Ask a project Manager to make the change.
- *A status change I expected won't save, or a board card snaps back.* That move isn't on the allowed list. Open **Settings → Workflow** and check the *from → to* lines. Either add the move you need, or route the issue through the intermediate statuses that *are* allowed.
- *A bulk Close skipped some issues.* Those issues were in a status that isn't allowed to move to *Closed* under your workflow. Move them through the allowed steps first, or add a rule permitting the jump.
- *I added one rule and now everyone is stuck.* That's expected — one rule closes the workflow. Add the rest of your legitimate moves (including any "send back" or "reopen" steps), or remove that single rule to reopen the workflow entirely.
- *I want to switch a project's workflow off completely.* There's no off switch. Remove every rule with the **remove** links until the list shows *Open workflow — any status change is allowed.*
- *My rules seem to apply to the wrong project.* Workflow rules are per project. Make sure you're on the Settings page of the project you meant — the heading reads *Settings — *ProjectName____.


# 16. Roadmap, Changelog & Reports

*See what's planned, what shipped, and how the numbers are trending — all built automatically from your issues.*

> **QUICK VERSION** — Put a **Fix version** on your issues and OpenTrack builds a **Roadmap** (what's coming, with progress bars) and a **Changelog** (what shipped) for the project — no extra bookkeeping. Separately, **Reports** turns your issues into charts: totals, issues created per month, and open issues by status and by severity.


## How it all hangs together

This chapter covers three read-only views that you never have to fill in by hand. They draw themselves from information already on your issues. Two of them — the **Roadmap** and the **Changelog** — are two halves of a single page and are driven entirely by one field on each issue: its **Fix version**. The third — **Reports** — is a separate page of charts driven by the issues you're allowed to see. Get the underlying data right and these views keep themselves current.

> **"OPEN" vs. "DONE"** — Throughout these views, an issue counts as **done** once its status reaches *Resolved* or *Closed*, and as **open** (still in progress) while its status is below *Resolved* — that is, *New*, *Feedback*, *Acknowledged*, *Confirmed*, or *Assigned*. The progress bars and the report counts all use that same dividing line.


## Setting up versions

The Roadmap and Changelog only have something to show once your project has **versions** and issues are pointed at them. A version is just a named release — *1.0*, *1.1*, *Spring Release* — that you create in project Settings. Each version is either **not yet released** (still being worked toward) or **released** (already shipped).

1. A Manager creates versions under **Settings → Versions**, giving each a **Name**, an optional **Description**, an optional **release date**, and a **Released** checkbox.
2. On each issue, someone sets the **Fix version** field to the version that issue belongs to — the release it's planned for, or the one it shipped in.
3. That's the only bookkeeping. From there, OpenTrack sorts everything onto the Roadmap and Changelog for you.

> **FIX VERSION IS THE HOOK** — An issue with no **Fix version** set simply doesn't appear on the Roadmap or Changelog. If you expect to see an issue there and don't, the usual reason is that its Fix version is blank.


## The Roadmap and Changelog page

Open the project and select **Roadmap** (labeled *Roadmap & changelog* at the top of the page). A gray note reminds you how it's built: *Issues are grouped by the version they're fixed in. Set an issue's "Fix version" to place it here.* The page is split into two stacked sections — **Roadmap** on top, **Changelog** below.


### Roadmap — what's coming

The **Roadmap** section lists every **unreleased** version that has issues pointed at it. Each version gets its own card. Reading a card from top to bottom:

| Part of the card | What it shows |
| --- | --- |
| **Version name** | The name you gave the version (for example *1.1*), in the card's heading. |
| **"X / Y done" count** | How many of the version's issues are done out of the total — for example *3 / 8 done*. |
| **Green progress bar** | The same fraction drawn as a bar, so you can gauge at a glance how close the release is. |
| **Issue list** | Every issue tagged with this Fix version. Done issues get a green check (*✔*) and a struck-through title; still-open issues get a hollow circle (*○*). Each line shows *#number*, the title (a link to the issue), and its *status · severity* in small gray text. |

> _[Figure: A Roadmap card for an upcoming version, showing the done count, the green progress bar, and the checked and unchecked issue lines]_

If no unreleased version has any planned issues, the section simply reads *No unreleased versions with planned issues.* The Roadmap is the place to answer "what's in the next version, and how far along is it?"


### Changelog — what shipped

The **Changelog** section, below the Roadmap, lists every **released** version and the work that shipped in it. It's your ready-made release notes. Each released version shows:

- The **version name** and, if you set one, the words *— released* followed by the release date.
- A list of the **done issues** for that version (only issues that reached *Resolved* or *Closed* appear — unfinished work is left out of the shipped list). Each is a link — *#number* and title — with its status in small gray text.

> _[Figure: The Changelog section listing released versions, each with its release date and the issues that shipped]_

If a released version has no done issues recorded, its entry reads *No resolved issues recorded for this version.* If you have no released versions at all, the whole section reads *No released versions yet. Mark a version "released" in project settings to build the changelog.*


### Moving a version from Roadmap to Changelog

A version lives on the **Roadmap** while it's unreleased and jumps to the **Changelog** the moment it's released. To make that move, a Manager marks the version released:

1. Go to **Settings → Versions** on the project.
2. Create the version with its **Released** box checked, or delete-and-recreate it as released (versions are edited through the same add controls).
3. Return to the **Roadmap** page — the version now appears under **Changelog** instead of Roadmap, with its shipped issues listed.


## Reports

**Reports** is a separate page — reached from **Reports** in the left-hand navigation, not from inside a single project — that turns your pile of individual issues into trends you can act on. Everything on it is drawn only from issues you're allowed to see, so two people may see slightly different numbers depending on their access.


### Choosing what to report on

1. Select **Reports** in the left navigation.
2. At the top, use the **Project** dropdown to pick a single project, or leave it on **All projects** to report across everything you can see.
3. Select **Show**. The page redraws for your choice.

> _[Figure: The Reports page: the Project dropdown and Show button at top, the three headline number cards, and the charts below]_


### The three headline numbers

Across the top sit three large-number cards:

| Card | What it counts |
| --- | --- |
| **Total** | Every issue in the selected scope, regardless of status. |
| **Open** | Issues still in progress — status below *Resolved* (New, Feedback, Acknowledged, Confirmed, or Assigned). |
| **Resolved this month** | Issues that became done (reached *Resolved* or *Closed*) and were last updated in the current calendar month. |


### The three charts

Below the headline numbers are three bar charts. Together they answer "is the workload growing, and where is it piling up?"

- **Issues created per month** — one bar per month, showing how many issues were filed. Watch whether the bars are rising (more coming in) or falling (inflow easing).
- **Open by status** — the open issues broken out by their status, so you can see whether they're stuck in triage, waiting on Feedback, or sitting Assigned.
- **Open by severity** — the open issues broken out by severity — from *Feature* and *Trivial* up through *Major*, *Crash*, and *Block* — so you can see how much of the backlog is serious.

> **THE CHARTS ARE ABOUT OPEN WORK** — Two of the three charts count only **open** issues. Something you resolved last week won't inflate the *Open by status* or *Open by severity* bars — those are a picture of what's still on your plate, not a running history.


## Troubleshooting

- *My Roadmap or Changelog is empty.* The most common cause is that issues have no **Fix version**. Set the Fix version on the issues you expect to see. The page also needs versions to exist at all — create them under **Settings → Versions**.
- *A version isn't showing on the Roadmap.* The Roadmap only lists **unreleased** versions that have at least one issue pointed at them. Check that the version isn't marked released, and that at least one issue has it as its Fix version.
- *A finished version won't appear in the Changelog.* Mark it **Released** under **Settings → Versions**. Unreleased versions never appear in the Changelog no matter how much work is done.
- *A shipped version shows "No resolved issues recorded."* Only **done** issues (status *Resolved* or *Closed*) count as shipped. Issues still open under that version are hidden from the changelog list. Move them to Resolved/Closed if they really did ship.
- *The Reports numbers look too low.* Reports only counts issues you're allowed to see. If you don't have access to a project, its issues won't be in your totals. Someone with broader access will see larger numbers.
- *"Resolved this month" seems wrong.* It counts issues that are done **and** were last updated this calendar month. An issue resolved last month, or one reopened and touched again this month, can shift what's counted.
- *A chart looks empty.* If there are no open issues in the selected scope, the *Open by status* and *Open by severity* charts have nothing to draw. Switch the **Project** dropdown to a project that has open work, or to **All projects**.


# 17. Bug-Hunt Checklists

*Work a repeatable list of things to test, and turn any failure into a tracked issue with one tap.*

> **QUICK VERSION** — Build a reusable checklist for a project — paste a whole list at once, or add items one at a time — then work down it tapping **Pass**, **Fail**, or **N/A**. A **Fail** becomes a linked, tracked issue in one tap. Building the list is a **Manager** job; anyone on the project can then work it.


## What a bug-hunt checklist is for

A **bug-hunt checklist** is a reusable list of things to check on a project — the sweep you do before a release, a Quality Assurance (QA) pass, or a routine inspection. Instead of trying to remember every corner to test, you work down the list, marking each item **Pass**, **Fail**, or **Not-applicable (N/A)**, and any failure becomes a real, tracked issue with one tap. You build the list **once** and it stays on the project for every future pass — the results reset each time you rework it, but the items themselves live on.

The page explains this at the top: *Work down the list on any device on your network — tap Pass, Fail, or N/A for each item. A Fail can create a linked issue you then triage like any other bug. Anything not on the list is still logged as a normal issue.*

> **WHO CAN DO WHAT** — Only a **Manager** can build or change a project's checklist — adding items, editing them, marking Pass/Fail/N/A, and creating issues from failures. Anyone on the project can open the page and read the list. If the project has no checklist yet and you're not a Manager, you'll see *This project has no bug-hunt checklist yet. A project manager can add one.*


## Opening the checklist

1. Open the project you want to check.
2. Select **Checklist** (the page is titled *Bug-hunt checklist — *ProjectName____).
3. If a checklist already exists, its items appear grouped under their section headings, with a progress bar at the top. If it's empty and you're a Manager, you'll see the tools to build one.


## Building a checklist (Manager)

There's nothing to install and no file to upload. You create the list right in the app, either by pasting a whole list at once or by adding items one at a time. The two building tools sit side by side near the bottom of the page: **Import a checklist** on the left, **Add one item** on the right.


### The fast way: paste a whole list

If you already have a list — in a document, a past release checklist, a wiki page — you don't retype it. You paste it, and OpenTrack turns each line into an item.

1. Find the **Import a checklist** box on the left.
2. Paste your list into the large text area, **one item per line**.
3. Select **Import items**. Every line is added at once, grouped under whatever headings you included, and a green message confirms *Imported N item(s).*

The formatting is forgiving — you don't have to clean up your source text first. OpenTrack reads each line like this:

| A line that looks like… | …becomes |
| --- | --- |
| `# Concurrency`  (starts with a hash) | A **section heading** that groups the items beneath it. |
| `- [ ] Message store is thread-safe`  (a checkbox line) | A checklist **item** (the *- [ ]* is stripped). |
| `- TX blocked on N0CALL`  (a bullet line) | A checklist **item** (the *-* is stripped). |
| `1. Geofence service is locked`  (a numbered line) | A checklist **item** (the number is stripped). |
| `Plain text with no marker` | A checklist **item**, taken as-is. |

For example, pasting this:

```
# Concurrency
- [ ] Message store is thread-safe
- [ ] Geofence service is locked
# RF identity
- TX blocked on N0CALL
```

…creates five items in two sections (*Concurrency* and *RF identity*).

> **"IMPORT" MEANS PASTE, NOT A FILE** — This is the closest thing to "uploading" a checklist: you paste the text of a list rather than choosing a file. It's the quickest way to stand up a full checklist, or to bulk-add a batch of items to an existing one — importing again just adds more items on top of what's there.


### One at a time

For a single item, or when you're building the list from scratch as you think, use the **Add one item** box on the right.

1. Enter an **item title** — the thing to check — in the first box (for example, *TX blocked on N0CALL*).
2. Optionally enter an **Area / section** to group it under. Items sharing an Area are automatically gathered under that heading.
3. Optionally enter **How/what to check** details describing exactly what to look for.
4. Select **Add item**. It appears in the list, under its section if you gave one, and a green *Saved.* message confirms it.

> _[Figure: The two building tools side by side: the Import-a-checklist paste box on the left and the Add-one-item form on the right]_


### Editing, grouping, notes, and deleting

Every item carries an **Edit / note** button (Managers only). Select it to open the **Edit item** panel, where you can change:

| Field | What it does |
| --- | --- |
| **Title** | The item's wording. |
| **Area (optional grouping)** | The section heading this item is filed under. Give two items the same Area and they group together automatically. |
| **Details — what/how to check** | Guidance shown under the item's title, in small gray text. |
| **Notes — what you found** | A place to record what you saw when you checked it. Notes show under the item in italics, prefixed *Notes:*. |

Select **Save** to keep your changes (the page returns to the checklist), or **Cancel** to back out. To remove the item entirely, select **Delete this item** at the bottom of the Edit panel. Grouping is how you turn a long, flat list into readable sections: set the same **Area** on related items and OpenTrack draws a section heading with an underline above each group.


## Working the checklist

Once the list exists, working it is fast and touch-friendly — the buttons are large on purpose, for use on a tablet or phone.

1. Open the project and select **Checklist** on any device on your network.
2. For each item, tap **Pass**, **Fail**, or **N/A**. The button lights up in its color — green for Pass, red for Fail, gray for N/A — and the item's badge and card border update to match.
3. Made a mistake? Tap **Reset** on that item to clear its result and send it back to *To do*.
4. Watch the top of the page: a green **progress bar** and a *X of Y checked* line fill in as you go, with a running *N failed · N passed* tally beside it.

> _[Figure: A checklist being worked: the progress bar and counts at top, items grouped under section headings, with Pass/Fail/N/A buttons and colored status badges]_

The status of each item is shown three ways at once so it reads at a glance: a colored **badge** on the left (*Pass*, *Fail*, *N/A*, or *To do*), the highlighted button, and a colored **card border** (green for pass, red for fail).


### Turning a failure into an issue

The whole point of marking something **Fail** is to get it fixed. So when you fail an item, OpenTrack offers to file it for you.

1. Mark an item **Fail**. A red **Create issue from this failure →** button appears on that item.
2. Select it. OpenTrack creates a new issue **linked** to the checklist item and takes you straight to that new issue.
3. Triage the issue like any other bug — set its severity, assign it, comment, and so on.
4. Back on the checklist, the failed item now shows a small *↳ Issue #number* link, so the item and the bug it spawned always point at each other.

> **ONE ISSUE PER FAILURE** — The **Create issue from this failure →** button only appears on a failed item that hasn't already spawned an issue. Once an item is linked to an issue, the button is replaced by the *↳ Issue #number* link, so you can't accidentally file the same failure twice. Anything you notice that **isn't** on the checklist, you just file as a normal issue in the usual way.

> **IT KEEPS WORKING WHEN THE NETWORK DOESN'T** — On a tablet or phone, the checklist keeps working through brief network drops — tick items off out in the field or in a signal-dead corner of a building, and your marks sync automatically when you're back online. (Creating an issue from a failure does need a live connection; OpenTrack tells you if you're offline and the action waits.) See the *Mobile, Tablets & the Field* chapter for more.


## Troubleshooting

- *I don't see the building tools (Import / Add one item).* Those are Manager-only. If you're not a Manager on the project, you can read and see the checklist but not change it. Ask a project Manager to add or edit items.
- *There are no Pass/Fail/N/A buttons on the items.* Same reason — marking items is a Manager action. Non-Managers see the list and its current results but can't change them.
- *My paste created one giant item instead of many.* Import reads **one item per line**. If your source pasted as a single line (no line breaks), split it onto separate lines and import again.
- *A pasted line I meant as an item became a heading.* Lines that start with *#* become section headings. Remove the leading *#* from any line you want treated as a checklist item.
- *The "Create issue from this failure" button isn't there.* It only shows on an item marked **Fail** that hasn't already been turned into an issue. If the item already links to *↳ Issue #number*, it's been filed; open that issue instead.
- *I want to remove a result without deleting the item.* Use **Reset** on the item to clear its Pass/Fail/N/A and return it to *To do*. **Delete this item** (in the Edit panel) removes the item itself.
- *My items aren't grouping into sections.* Grouping is by **Area**. Give related items the exact same Area text (in the Add box or via Edit / note) and they'll gather under one heading.
- *Nothing saved / I saw a red message.* A red banner usually means you don't have permission (not a Manager) or the title was empty. Check the message text, confirm your role, and make sure the item title isn't blank.


# 18. Public Trouble-Ticket Intake & QR Posters

*Let anyone report a problem without an account — by link or by scanning a code — and have it arrive as a normal issue.*

> **QUICK VERSION** — Turn on **public intake** under **Settings → Public trouble-ticket intake** so people **without an account** can report a problem — through a link or a scannable **Quick Response (QR)** poster. Their reports arrive as normal issues (status *New*) for you to triage. It's off by default and is a **Manager** setting.


## What public intake is

**Public trouble-ticket intake** lets people who have **no OpenTrack account** submit a problem to a project through a simple "Report a problem" web page. It's built for a helpdesk, a club, an event, a workshop, or field reports from the general public — anyone you want to be able to flag a problem without giving them a login. Submissions arrive as ordinary issues in your project, ready to triage exactly like any other. It's **off by default**, and you turn it on one project at a time.

> **MANAGERS ONLY** — Turning intake on or off, and printing the poster, are **Manager** actions. If you open project Settings and can't change these controls, you're not a Manager on the project — ask one to set it up.


## Turning it on

1. Open the project and select **Settings**.
2. Scroll to the bottom, to the **Public trouble-ticket intake** heading.
3. Read the short explanation: *Let anyone submit a problem without an account through a public "Report a problem" page — handy for a helpdesk, a club, or field reports. Submissions arrive as normal issues. Off by default.*
4. Check the current **Status** line — it reads either *Off* or *On*.
5. Select **Turn on public intake**.
6. The status flips to **On** and two links appear.

> _[Figure: The Public trouble-ticket intake section of Settings, showing the Status line, the two links, the Printable QR poster button, and the on/off button]_

Once intake is on, the section shows you what to share:

| Link | What it's for |
| --- | --- |
| **Public link** (ends in */report/* and the project number) | The "Report a problem" form. Share this with anyone who should be able to submit a ticket. It opens in a new tab so you can preview it. |
| **Status link** (ends in */report/status*) | Where a reporter can come back later to check on a ticket they filed. Same page for everyone; individual tickets are protected by reference number plus email. |


## What the reporter sees

Anyone who opens the public link gets a plain, account-free form titled *Report a problem — *ProjectName*__, with the note *Tell us what's wrong. You don't need an account. Leave your email if you'd like updates.__ The fields are:

| Field | Required? | What it's for |
| --- | --- | --- |
| **Your name** | Optional | So you know who reported it. Can be left blank. |
| **Your email** | Optional | Only needed if the reporter wants to check status later or get updates. No email means they can't look the ticket up afterward. |
| **Summary** | Required (marked with a red ***) | A short description of the problem — this becomes the issue's title. |
| **Details** | Optional | The fuller story: what happened, what they were doing, anything else that helps. |

When they select **Submit report**, the submission lands in your project as a new issue at status *New*, alongside everything your team files internally.

> _[Figure: The public "Report a problem" form with its name, email, summary, and details fields]_

> **SPAM PROTECTION IS BUILT IN** — The form carries a hidden "honeypot" field that real people never see and never fill in. Automated spam bots tend to fill every field, including that one — and OpenTrack silently drops those submissions. Your reporters don't have to do anything; this just quietly keeps junk out of your project.


## The thank-you page and reference number

After a successful submission the reporter sees a **Thank you** page: *Your report was received. Your reference number is* followed by the ticket's number. It tells them to keep that number and offers a link to *check its status* anytime, plus a **Submit another** button to file a second report. That **reference number** is the reporter's key to looking the ticket up later (together with the email they used).

> **THE REFERENCE NUMBER USES THE PROJECT'S TICKET KEY** — If the project has a **Ticket key** (see the *Projects & Their Settings* chapter), the reference number the reporter sees is the friendly form — for example **WEB-42** — on the thank-you page, in the acknowledgement email, and on the status page. If the project has no key, it's the plain number, like **42**. Either way it's the same ticket; the key just makes it clearer and easier to quote, which is handy when one OpenTrack handles reports for several products.

If the reporter left an **email**, OpenTrack also sends a short acknowledgement to that address — *We received your report (ref WEB-42)* — with the same reference number and a reminder that they can check status any time. (This only happens when your server has email set up.)


## How a reporter checks status later

The status-lookup page (the */report/status* link) lets a reporter check on a ticket without an account and without seeing anyone else's tickets. It's titled *Check your ticket* with the note *Enter the reference number you were given and the email you used.*

1. The reporter opens the status link.
2. They enter the **Reference number** they were given. The box accepts it in any form — the friendly *WEB-42*, the plain *42*, or even *#42* — so they can just type whatever they were sent (the on-screen hint reads *e.g. WEB-42 or 42*).
3. They enter the **Email you used** — it must match the email on the original report.
4. They select **Check status**.
5. If both match, a green box shows *Ticket WEB-42* (or the plain number if the project has no key), its title, and its current **Status**. If they don't match, a yellow note reads *No matching ticket found. Double-check the reference number and the email address you submitted with.*

> **THE KEY PREFIX IS OPTIONAL WHEN LOOKING UP** — Because the number behind a ticket never changes, the status page finds the ticket whether or not the reporter includes the key. **WEB-42**, **42**, and **#42** all look up the same ticket. So a reporter who half-remembers their reference — or who only wrote down the digits — can still check status.

> **EMAIL IS THE KEY** — A reporter can only look a ticket up if they gave an **email** when they filed it, and they must enter that same email plus the reference number to see it. Reference-number-only lookups don't work — this is what keeps one person from browsing another's tickets. If a reporter left the email blank, they won't be able to check status afterward.


## Print a QR poster

For a physical place — a workshop, a trailhead, an event booth, a piece of equipment — OpenTrack can print a poster with a **Quick Response (QR) code** (the square, scannable barcode). People point a phone camera at it and the report form opens on their phone, already pointed at your project; no typing a web address.

1. With intake turned **on**, in that same **Public trouble-ticket intake** section select **📱 Printable QR poster**.
2. The poster page opens. It reads *Spot a problem?* across the top, *Scan to report it — *ProjectName*__ beneath, a large QR code in the middle, and the plain-text link underneath for anyone who'd rather type it. A gray line reassures readers: *No account needed — it takes less than a minute.__
3. Select the **🖨️ Print** button (top right) to print it.
4. Post the printed page where people can reach it. Scanning the code opens the report form pre-pointed at this project.

> _[Figure: The printable QR intake poster: the "Spot a problem?" heading, the project name, the large QR code, and the fallback link]_

> **TURN INTAKE ON BEFORE YOU PRINT** — If public intake is **off** for the project, the poster page shows a warning — *Public intake is currently off for this project, so this QR code won't accept reports yet* — and the code will lead to a page that declines submissions. Turn intake on under **Settings → Public trouble-ticket intake** first, then print. The Print button and warning don't appear on the printed copy, only on screen.


## Tickets can also arrive by email

Besides the link and the QR poster, a project can also collect tickets straight from **email**. When this is set up, someone reports a problem through the contact form on your own website, that form sends an email to a special "tickets" address, and OpenTrack turns that email into a normal ticket in your project — the sender's name and email become the ticket's contact details, the email's subject becomes the ticket's title, and the message becomes the description. The person never has to visit OpenTrack at all; they just use the website form they already trust.

Which project an email lands in is decided by the project's **Ticket key** (the short code like *WEB* described in the *Projects & Their Settings* chapter). An address such as *tickets+WEB@yourdomain.com* files the email under the project whose key is *WEB*, so one OpenTrack can quietly sort email reports for several different products.

> **AN ADMINISTRATOR SETS THIS UP ON THE SERVER** — Email-to-ticket isn't a button inside OpenTrack — it's switched on **on the server** by whoever runs it, and it needs a mail service to forward received email to OpenTrack. It's **off until they turn it on**, and it only works for a project that already has **public intake on** and a **Ticket key** set. If you'd like reports to arrive by email, ask your administrator to follow the *Public trouble tickets* guide (the "Email-to-ticket" section).


## Turning it off

To stop accepting public reports, return to **Settings → Public trouble-ticket intake** and select **Turn off public intake**. The status flips back to **Off**, the links and poster button disappear, and the public report form starts telling visitors *This project isn't accepting public submissions right now.* Issues already submitted stay in your project — turning intake off only stops new ones.


## Troubleshooting

- *I can't find the intake controls, or they're not clickable.* These are Manager-only. If you're not a Manager on the project, ask one to turn intake on and print the poster.
- *The public link says "isn't accepting public submissions right now."* Intake is off for that project. A Manager turns it on under **Settings → Public trouble-ticket intake**.
- *The QR poster shows a warning about intake being off.* Same cause — turn intake on first, then print. The QR code won't accept reports until intake is on.
- *A reporter can't check their ticket's status.* Status lookup needs **both** the reference number and the **same email** used on the report. If they left email blank when filing, there's no way to look it up. If they're sure of both, confirm they're using the exact email address they typed originally.
- *A reporter lost their reference number.* Without it (plus the email), they can't self-check status. They can submit a fresh report, or a team member can find the original issue inside the project and update them directly.
- *Public submissions aren't arriving.* Confirm intake shows **On** in Settings, and that reporters are using the current public link for the right project. Submissions land as new issues at status *New* — check your project's New issues.
- *I'm worried about spam.* The form's hidden honeypot field silently drops obvious bot submissions, so most junk never becomes an issue. Genuine-looking spam that slips through is triaged and closed like any other issue.
- *Email reports aren't turning into tickets.* Email-to-ticket is a server setting, not a project button. Confirm with your administrator that it's switched on, that this project has **public intake on** and a **Ticket key** set, and that the email is being sent to the matching address (for example *tickets+WEB@yourdomain.com* for a project whose key is *WEB*). The *Public trouble tickets* guide has the full checklist.


# 19. Importing & Exporting Your Data

*Bring issues in from other tools, get your own data out, and set up automatic backups.*

> **QUICK VERSION** — Moving in? Use **Import** to pull issues from a spreadsheet, a Jira export, or GitHub — or the dedicated **MantisBT** importer on the **Backup & export** page. Your data is always yours: **Backup & export** downloads it any time as JSON or CSV, and the server can also take automatic backups on a schedule.


## What this chapter covers

OpenTrack keeps everything you enter — issues, notes, tags, and the rest — but it never traps that data inside itself. This chapter covers the three ways data moves in and out: *importing* (bringing issues in from another tracker so you don't retype anything), *exporting* (downloading your own issues to a file), and *backing up* (keeping a safe copy of the whole database in case the computer running OpenTrack ever fails). Two pages in the left navigation do this work: the **Import** page and the **Backup & export** page.

> **TWO FILE FORMATS, SPELLED OUT** — Two file types come up throughout this chapter. **CSV** (Comma-Separated Values) is the plain, flat format every spreadsheet program and most trackers can produce — a header row of column names, then one row per issue. **JSON** (JavaScript Object Notation) is a structured text format that can hold richer, nested detail (an issue plus its notes, tags, and custom fields all together). CSV is best for reports and spreadsheets; JSON is best for a complete backup or a full move to another system.


## Importing issues

If you're switching to OpenTrack from another tracker, you don't have to re-enter your old issues by hand. OpenTrack reads exports from four common sources. Which tool you use depends on where the data is coming from:

| Coming from | File it produces | Where to import it |
| --- | --- | --- |
| **MantisBT** (a widely used older tracker) | An **XML** file from MantisBT's *Export Issues* | The **Import from MantisBT** card on the **Backup & export** page |
| A **spreadsheet** (Excel, Numbers, Google Sheets) | A **CSV** file with a header row | The **Import** page (file type **CSV**) |
| **Jira** | Jira's **CSV** export | The **Import** page (file type **Jira**) |
| **GitHub Issues** | The issues **JSON** from the GitHub Application Programming Interface (API) | The **Import** page (file type **GitHub**) |

> **YOU NEED THE MANAGER ROLE** — Importing adds issues into an existing project, so you must have the **Manager** role on the project you're importing **into**. If you don't manage any projects yet, create one first (see the Projects chapter), then come back. The Import page tells you plainly if you have no projects to import into.


## The Import page, step by step

Use the **Import** page for CSV spreadsheets, Jira exports, and GitHub issues. (MantisBT has its own importer — covered further down.) Open it from **Import** in the left navigation. You'll see a short explanation at the top, then a form with three fields and an **Import** button.

1. Select **Import** in the left navigation.
2. Under **Target project**, choose the project the issues should land in. This is a dropdown of every project you manage.
3. Under **File type**, pick **CSV**, **Jira**, or **GitHub** to match the file you're uploading (see the table below for exactly what each means).
4. Under **File**, select **Choose file** and pick your export from your computer. OpenTrack accepts files ending in `.csv`, `.json`, or `.txt`.
5. Select **Import**.
6. A colored banner reports the result — for example, *Imported 42 issue(s), skipped 3 already-imported, linked 7 tag(s)*.

> _[Figure: The Import page showing the Target project dropdown, File type dropdown, file picker, and Import button]_

| File type option | What to upload |
| --- | --- |
| **CSV — a spreadsheet with a header row** | Any comma-separated file whose first row is column names. This is the catch-all for spreadsheets and most trackers. |
| **Jira — CSV export from Jira** | The CSV that Jira produces from its issue export. Same idea as plain CSV, tuned for Jira's column names. |
| **GitHub — Issues JSON (from the GitHub API)** | The JSON array returned by GitHub for a repository's issues (the `/repos/OWNER/REPO/issues?state=all` call). Pull requests in that file are skipped automatically. |


### Which column and field names OpenTrack recognizes

For **CSV** and **Jira** files, the very first row must be the column headers. You don't have to rename anything first — OpenTrack recognizes the common names for each field and matches them automatically, whichever your old tool happened to use:

| OpenTrack field | Header names it recognizes |
| --- | --- |
| Title | *Title* or *Summary* |
| Description | *Description* |
| Status | *Status* |
| Severity | *Severity* |
| Priority | *Priority* |
| Category | *Category* or *Component* |
| Tags | *Labels* or *Tags* |
| Assignee | *Assignee* |
| Reporter | *Reporter* |
| Original ID | *Key* or *ID* (used to recognize a row on a re-run) |

> **RE-RUNNING THE SAME FILE IS SAFE** — Import remembers the original ID of every row it brings in. If you import the **same file** again — say you added a few more issues to it and want to top up — OpenTrack recognizes the rows it already has and **skips** them. You won't get duplicates. The result banner tells you how many were skipped for exactly this reason.


### When an import is refused

If something isn't right, the banner turns yellow or red and tells you what to fix. The common messages are:

| Message | What it means |
| --- | --- |
| *Please choose a file to upload.* | You selected **Import** without picking a file. |
| *Please choose a target project.* | No project was selected in the **Target project** dropdown. |
| *That file is larger than the 25 MB limit.* | The upload is over the 25-megabyte (MB) size cap. Split a very large export into smaller files. |
| *That file couldn't be read as the selected type.* | The file doesn't match the **File type** you chose — for example a Jira CSV uploaded as GitHub JSON. Double-check the format and the File type dropdown. |
| *You need the Manager role on that project to import into it.* | You're not a Manager of the target project. Ask a Manager, or import into a project you manage. |


### Importing from MantisBT

MantisBT has its own importer because it carries more than a flat list — it brings projects, issues, notes, and tags across in one step. You'll find it as the **Import from MantisBT** card on the **Backup & export** page (it appears only if you have the **Manager** role). To use it:

1. In MantisBT, use *Export Issues* to download an **XML** file.
2. In OpenTrack, open **Backup & export** from the left navigation and find the **Import from MantisBT** card.
3. Select **Choose file**, pick your `.xml` export, and select **Import**.
4. The banner reports what came in — issues across however many projects (new and existing), notes, and tag links.

A few things worth knowing about the MantisBT import: it can *create the matching projects for you* if they don't exist yet; statuses, severities, and priorities are mapped across exactly; MantisBT users are matched to your OpenTrack accounts by username where possible, and where there's no match the original name is kept in the issue text so nothing is lost. Imported projects start out **private**. Unlike the CSV/Jira/GitHub importer, the MantisBT importer does *not* de-duplicate — run it *once* per export, because re-uploading the same file adds the issues a second time.


## Exporting your data

Your data is always yours to take with you. Open **Backup & export** in the left navigation. In the **web** version of OpenTrack you'll see one-click download buttons; every export includes *only what you're allowed to see*, so it never leaks issues from projects you can't access. There are two cards:

- **Spreadsheet (CSV)** — the **All issues (CSV)** button downloads a flat list of every issue you can see, ready to open in Excel, Numbers, or Google Sheets. Best for reporting, sorting, or sharing a quick snapshot.
- **Full backup (JSON), per project** — a small table lists each of your projects with a **CSV** and a **JSON** button beside it. The per-project **JSON** download is the complete copy: issues together with their notes, tags, custom-field values, and bug-hunt checklist, all in one file you can archive or move to another system.

> _[Figure: The Backup & export page with the Spreadsheet (CSV) card and the per-project Full backup (JSON) table]_

> **DESKTOP APP VS. WEB** — The one-click CSV and JSON download buttons appear in the **web version** (OpenTrack opened in a browser). In the **desktop app**, those buttons are replaced by a note pointing you to the whole-database backup described next — copying the database file is the way to back up from the desktop app.


## Backing up the whole database

Everything OpenTrack stores lives in a single database file on the computer that runs the server. Exporting issues to CSV or JSON is great for taking data elsewhere, but the surest *complete* backup is a copy of that one file. The **Backing up the whole database** card at the bottom of **Backup & export** explains it:

- The file is named `opentrack.db` by default, in the server's data folder.
- **Stop the server first** (or copy the file when the server is idle) so the copy is consistent and not caught mid-write.
- If the files `opentrack.db-wal` and `opentrack.db-shm` are sitting next to it, copy those too — they can hold recent changes.
- To **restore**, put the file back in the same place and start the server again.


## Automatic scheduled backups

The manual export above is on-demand — you click when you want a copy. For peace of mind, the **server** can also make **automatic backups** on a schedule: consistent snapshots of the whole database, taken safely while the app keeps running. This is a *server setting* (whoever runs the server turns it on, not something in the web pages), and it's **off by default**. To enable it, set these values on the server and then restart OpenTrack:

```
OpenTrack__Backup__Enabled=true
OpenTrack__Backup__IntervalHours=24     # how often to snapshot
OpenTrack__Backup__Directory=           # blank = a 'backups' folder next to the database
OpenTrack__Backup__Retention=14         # keep the newest 14 snapshots, delete older ones
```

Each snapshot is named `opentrack-YYYYMMDD-HHMMSS.db` — the date and time it was taken, so the newest is easy to spot. To **restore** one: stop the server, copy the chosen snapshot over the live `opentrack.db` file, and start the server again.

> **KEEP A COPY OFF THE MACHINE** — Automatic backups protect against mistakes and file corruption, but not against the whole server dying, being stolen, or being lost in a fire or flood. Every so often, copy a recent snapshot (or a manual JSON export) to a different device or a cloud drive, so that a single failure can't take everything with it.


## Troubleshooting

- *The Import page says I don't manage any projects.* Importing needs the **Manager** role on a target project. Create a project first (or ask to be made a Manager of one), then return to **Import**.
- *My import was refused as the wrong type.* The **File type** dropdown must match the file. A Jira CSV must be imported as **Jira** or **CSV**, and a GitHub export as **GitHub**. Re-check the dropdown and try again.
- *Some columns didn't come across.* For CSV and Jira, the first row must be the column headers, and OpenTrack matches only the recognized names (see the table above). Rename an unusual header to a recognized one — for example *Summary* for the title — and re-import.
- *I imported the same file twice and worried about duplicates.* For CSV, Jira, and GitHub, re-running the same file is safe — matching rows are skipped, and the banner tells you how many. The **MantisBT** importer is the exception: it does not de-duplicate, so run each MantisBT export only once.
- *The file is too big.* Uploads are capped at 25 MB. Split a very large export into smaller files and import them one after another.
- *I don't see the CSV/JSON download buttons.* You're likely in the **desktop app**, where those buttons are replaced by a note. Open OpenTrack in a browser for one-click downloads, or back up by copying the `opentrack.db` file.
- *Where did automatic backups go?* They're **off by default** and controlled on the server, not in the web pages. Whoever runs the server sets `OpenTrack*Backup*Enabled=true` (and the related values) and restarts OpenTrack.
- *I need to restore a backup.* Stop the server, copy your chosen `opentrack-…​.db` snapshot over the live `opentrack.db` file (in the server's data folder), and start the server again.


# 20. The AI Assistant (Optional)

*Smart triage, plain-English search, thread summaries, and a fix suggestion — what they do, and how they get turned on.*

> **QUICK VERSION** — If your administrator turns it on, extra buttons appear: **✨ Suggest with AI** on a new issue, **✨ Ask in plain English** on the issues list, **✨ Summarize thread** on a busy issue, and **🛠️ Suggest a fix** on an individual issue's page. Every result is a **suggestion you can accept or change** — the AI never files, edits, or resolves anything on its own. The server can even run the three quick jobs on a **free local model** and send only the harder **Suggest a fix** to cloud Claude.


## What the AI assistant is

OpenTrack can optionally use an **artificial intelligence (AI)** language model to speed up a few chores — proposing how to triage a new bug, turning a plain-English request into search filters, summarizing a long, noisy issue, and suggesting how a problem might be fixed. It is **off by default** and only ever runs after someone deliberately turns it on and points it at a provider. If it's off, OpenTrack looks and behaves exactly as it always does; the extra buttons simply aren't there.

The single most important thing to understand is that everything the AI produces is a **suggestion a person reviews**. It never creates, changes, resolves, or deletes anything by itself. You always see its proposal and decide whether to keep it, edit it, or ignore it. And if a call fails or times out, nothing breaks — you just carry on doing the same thing by hand.


## What it can do — the four helpers

There are four AI helpers. Three are marked with a sparkle (**✨**); the newest, **🛠️ Suggest a fix**, uses a small wrench (**🛠️**) instead. Each lives on a specific page:

| Helper | Where it appears | What it does |
| --- | --- | --- |
| **✨ Suggest with AI** | The **New Issue** page, next to **Submit Issue** | Reads the title and description you've typed and proposes a **severity**, **priority**, **category**, and a set of **tags** |
| **✨ Ask in plain English** | The top of the **Issues** list | Turns a request like *high-priority crashes nobody has touched in a month* into the normal search filters |
| **✨ Summarize thread** | The **AI summary** card on an individual issue's page | Gives a plain-language recap of the problem, what's been tried, and what's next |
| **🛠️ Suggest a fix** | The **Suggest a fix** card on an individual issue's page | Reads the issue, its notes, any text/log attachments, and similar **already-fixed** issues, then proposes likely causes and concrete steps to try |


### ✨ Suggest with AI (triage a new issue)

On the **New Issue** page, once you've typed at least a **Title** (a description helps too), an extra **✨ Suggest with AI** button sits beside the blue **Submit Issue** button. Clicking it does *not* create the issue — instead it reads what you've written and fills in the triage fields for you:

1. Type the issue's **Title**, and ideally its **Description**.
2. Select **✨ Suggest with AI** (it appears only when the AI is turned on).
3. OpenTrack fills in the **Severity**, **Priority**, and **Category** dropdowns with its best guess.
4. Any suggested **tags** appear in a blue *✨ Suggested tags: …​* note — you add those to the issue after you create it.
5. Review every field, change anything you disagree with, then select **Submit Issue** to actually create the issue.

> _[Figure: The New Issue page with the ✨ Suggest with AI button and the blue Suggested tags note above the buttons]_

> **IT ONLY PRE-FILLS THE FORM** — Suggest with AI changes the dropdowns and shows suggested tags — nothing more. The issue isn't saved until you select **Submit Issue** yourself, so you're always free to overrule the AI first. If it can't reach the provider, you'll see *AI couldn't suggest a triage right now.* and the form is untouched.


### ✨ Ask in plain English (search the issues list)

At the top of the **Issues** list, above the usual row of filter dropdowns, is a box labeled **✨ Ask in plain English** with a **Search with AI** button. Type what you're looking for in ordinary words and let the AI translate it into filters:

1. In the **✨ Ask in plain English** box, type a request such as *high-priority crashes nobody has touched in a month*.
2. Select **Search with AI**.
3. OpenTrack sets the ordinary filters below — **Status**, **Severity**, **Priority**, keyword **Text**, the **Stale** checkbox, and **Project** — to match, and shows the results.
4. From there you can adjust any filter by hand and search again, exactly as if you'd set them yourself.

> _[Figure: The Issues list with the ✨ Ask in plain English box above the standard filter dropdowns]_

> **IT CAN'T WIDEN YOUR ACCESS** — Plain-English search only ever produces a filter you could have set by hand, over the projects you're **already allowed to see**. It adds no query power and is never a way around OpenTrack's privacy rules. If it can't make sense of your words, you'll see *AI couldn't interpret that search right now* or a nudge to name a status, severity, priority, keywords, or *stale*.


### ✨ Summarize thread (recap a busy issue)

On an individual issue's page, when the AI is on, an **✨ AI summary** card appears with a **Summarize thread** button. A long issue with dozens of notes can be hard to catch up on; this reads the issue and its notes and gives you a short, plain-language recap of the problem, what's already been tried, and what happens next. After the first summary the button reads **Refresh** so you can regenerate it once more has been added. A small reminder underneath — *AI-generated from this issue and its notes — double-check anything important* — is there because a summary is a convenience, not a substitute for reading the details that matter.


### 🛠️ Suggest a fix (get a grounded starting point)

On an individual issue's page, when the AI is on, a **🛠️ Suggest a fix** card appears with a **Suggest a fix** button. This is the newest and most powerful helper. Where **Summarize thread** just recaps the conversation, **Suggest a fix** tries to help you actually solve the problem: it reads the issue and everything attached to it and hands back a plain-language starting point. It's especially handy on a stubborn issue, or when a newer team member picks up something they haven't seen before.

It doesn't guess in a vacuum. Before it answers, OpenTrack gathers real evidence from the issue itself and feeds only that to the AI: the issue's **description and notes**, the text of any **log or text-file attachments** (things like `error.log` or `output.txt`), and a handful of **similar issues that have already been resolved** — along with how each of those was fixed. That last part is what makes it smarter the longer you use OpenTrack: your own past solutions become the material for the next suggestion.

1. Open the issue you want help with.
2. In the **🛠️ Suggest a fix** card, select the **Suggest a fix** button (it appears only when the AI is turned on).
3. Wait a moment while it reads the issue and its evidence. When it's done, the card fills in with the suggestion.
4. Read the **summary** (a plain-language root-cause hypothesis), the **Likely causes** list (most likely first), and the **Steps to try** (a numbered checklist, in order).
5. Check the **confidence** badge — *low*, *medium*, or *high* — and the **Based on:** line, which names the evidence it used (for example, *issue #123, attached log*).
6. Try the steps yourself. If the first suggestion wasn't useful, select **Try again** for a fresh pass.

> _[Figure: The 🛠️ Suggest a fix card on an issue page, showing the summary, Likely causes list, numbered Steps to try, and the confidence badge with the Based on line]_

> **IT NEVER CHANGES THE ISSUE** — Suggest a fix is a **read-only draft**. It does not edit the issue, add a note, change the status, or mark anything resolved — it only shows you a suggestion on the screen. The card even reminds you: *An AI suggestion — a starting point, not a guarantee. Verify before acting; nothing on this issue was changed.* You stay fully in control: try the steps, ignore them, or copy the useful parts into a note yourself.

> **BE HONEST ABOUT LOW CONFIDENCE** — When there's little to go on — a bare one-line issue with no notes, no logs, and nothing similar in your history — the AI is told to **say so and set the confidence to *low***, rather than invent a confident-sounding but made-up answer. Treat a **low** badge as "here's a guess, but don't lean on it," and add more detail to the issue (notes, a log file) before trying again for a stronger result.


## Turning it on — a server task, in two paths

The AI is configured on the **server** (by whoever runs OpenTrack), not from the web pages, because it involves a secret key that must never reach a browser. As a day-to-day user you don't set this up — but it helps to understand the choice, because it decides where your issue text goes. There are two very different ways to run it, and the difference is mostly about **privacy** and **cost**:

| Path | What it means | Good when |
| --- | --- | --- |
| **Local & free** (Ollama or LM Studio) | A model runs on **your own** computer. No key, no bill, and **the issue text never leaves your machine**. | Privacy matters, or you'd rather not pay per use. Needs a reasonably capable PC. |
| **Cloud provider** (Anthropic Claude, OpenAI, Azure, Groq, and others) | You get an **Application Programming Interface (API)** key from the provider; OpenTrack calls their online service. | You want the strongest results with no local hardware, and are okay sending issue text to that provider and paying a small per-use fee. |


### Option A — local, free & private (Ollama)

1. On the machine that will run the model, install **Ollama** from `https://ollama.com` (Windows, Mac, or Linux).
2. Download a model, for example: `ollama pull llama3.1`. Ollama then serves it locally.
3. On the OpenTrack server, set the AI settings to point at it (no key needed), then restart OpenTrack:

```
OpenTrack__Ai__Enabled=true
OpenTrack__Ai__Provider=openai
OpenTrack__Ai__BaseUrl=http://localhost:11434/v1   # or a LAN address like http://192.168.1.50:11434/v1
OpenTrack__Ai__Model=llama3.1
```

> **HOW MUCH COMPUTER DO YOU NEED?** — For OpenTrack's short, occasional AI calls, **memory (RAM) matters most**: about **16 gigabytes (GB)** is the sweet spot (runs a capable 7-to-8-billion-parameter model); 8 GB works but gives weaker suggestions. An **Apple Silicon Mac** (M-series mini) or a machine with an **NVIDIA graphics card** feels snappy; a plain mini-PC's processor works, just slower. You can even run it on the same mini-PC as OpenTrack.


### Option B — a cloud provider (get a key)

Cloud AI is billed to a **developer API account** at the provider, which is **separate** from any monthly chat subscription (an Anthropic API key is *not* a Claude Pro plan; an OpenAI API key is *not* ChatGPT Plus). Costs are small — a triage suggestion is typically a fraction of a cent — but set a spend limit where the provider offers one. To get a key (Anthropic Claude shown; OpenAI is nearly identical):

1. Go to `https://console.anthropic.com` — the developer **Console**, not the claude.ai chat site (they're separate accounts even with the same email). Sign in or sign up.
2. Open **API Keys** (in Settings), select **Create Key**, name it `OpenTrack`, and **copy the key now** — it's shown only once. It looks like `sk-ant-…`.
3. Add a little **credit** under Billing (even a few dollars is plenty for triage) and set a monthly spend limit.
4. On the OpenTrack server, enter the settings and restart OpenTrack:

```
OpenTrack__Ai__Enabled=true
OpenTrack__Ai__Provider=anthropic
OpenTrack__Ai__ApiKey=sk-ant-...          # your key — keep it on the server only
OpenTrack__Ai__Model=claude-haiku-4-5-20251001   # fast and inexpensive
```

For **OpenAI**, use `Provider=openai`, an `sk-…` OpenAI key, and a model like `gpt-4o-mini`. For **Azure OpenAI, Groq, OpenRouter**, or similar, use `Provider=openai`, set `BaseUrl` to that service's address, and use its key and model name. (The one `openai` setting covers every service that speaks OpenAI's common format — which is most of them, including the free local ones.)

| Setting | What it does |
| --- | --- |
| `OpenTrack*Ai*Enabled` | **true** turns the AI on (the ✨ buttons appear); **false** or absent hides it. |
| `OpenTrack*Ai*Provider` | **anthropic** for Claude, or **openai** for OpenAI and every other OpenAI-compatible service (including local Ollama/LM Studio). |
| `OpenTrack*Ai*ApiKey` | The provider's secret key. Left blank for a local model, which needs none. |
| `OpenTrack*Ai*BaseUrl` | The service address. Point it at a local model or an alternative cloud service; omit it for the provider's default. |
| `OpenTrack*Ai*Model` | The exact model name to use, for example `claude-haiku-4-5-20251001`, `gpt-4o-mini`, or `llama3.1`. |


## Two tiers: the quick jobs local, the smart fix in the cloud

Everything above assumes **one** AI provider does all four jobs, which is perfectly fine. But there's an optional third arrangement that many people prefer, because it gets you the best of both worlds — free and private for the everyday chores, top-quality only where it matters. OpenTrack calls these two levels the **base** provider and the **Smart** provider:

- The **base** provider (the `OpenTrack:Ai` settings you saw above) handles the three **menial** jobs — triage, plain-English search, and thread summaries. These are quick and forgiving, so a small **free local model** (Ollama on your own machine or a machine on your network) is ideal here.
- The optional **Smart** provider (a nested `OpenTrack:Ai:Smart` block) handles the one **reasoning-heavy** job — **🛠️ Suggest a fix**. Fixing a real problem is where a stronger model earns its keep, so the classic setup points this at **cloud Claude**.

The result: the everyday AI stays free and keeps your issue text on your own hardware, and only the occasional **Suggest a fix** call goes to the cloud (and bills the cloud account). If you **don't** set a Smart provider, nothing changes — the base provider simply handles **Suggest a fix** too, exactly as it handles everything else. Setting one up is a server task: add these environment variables **in addition to** the base `OpenTrack*Ai**` settings, then restart OpenTrack:

```
# Base tier — a free local model on the LAN does the quick jobs (triage, search, summaries):
OpenTrack__Ai__Enabled=true
OpenTrack__Ai__Provider=openai
OpenTrack__Ai__BaseUrl=http://192.168.1.50:11434/v1   # your Ollama box — use its real address
OpenTrack__Ai__Model=llama3.1

# Smart tier — cloud Claude does the reasoning-heavy 🛠️ Suggest a fix:
OpenTrack__Ai__Smart__Provider=anthropic
OpenTrack__Ai__Smart__ApiKey=sk-ant-...                # your Anthropic Console key — server only
OpenTrack__Ai__Smart__Model=claude-haiku-4-5-20251001
```

> **THE SMART TIER USES THE SAME SETTING NAMES** — The `OpenTrack*Ai*Smart__*` settings mirror the base ones exactly — `Provider`, `ApiKey`, `Model`, and `BaseUrl` all mean the same thing, just for the Smart provider. So the Smart tier can be anything the base tier can: cloud Claude, OpenAI, or even a second, larger local model. Whoever runs your server sets this; you don't touch it from the web pages.

> **PRIVACY IN ONE LINE** — With a **cloud** provider, generating a suggestion sends that issue's text to that provider — so don't enable a cloud provider for projects whose contents can't leave your environment; use a **local** model instead. In a two-tier setup, remember that even if the quick jobs stay local, a cloud **Smart** provider means your **🛠️ Suggest a fix** calls (issue text, notes, and log excerpts) still go to that provider. Every key is read only on the server and is never sent to the browser or stored in the database.


## Troubleshooting

- *I don't see any ✨ buttons.* The AI is **off**, which is the default. It's a server setting; whoever runs OpenTrack has to turn it on and restart the app. Nothing you do in the web pages enables it.
- *✨ Suggest with AI does nothing / says it couldn't suggest.* You may not have entered a **Title** yet — the button needs at least a title to work from. If you see *AI couldn't suggest a triage right now*, the server couldn't reach the provider; fill the triage fields in by hand and submit as normal.
- *Plain-English search says it couldn't interpret that.* Try again with clearer wording that names a status, severity, priority, some keywords, or the word *stale*. Remember it can only ever build a filter you could set by hand — it won't find issues outside the projects you're allowed to see.
- *The AI suggested the wrong severity or category.* That's expected sometimes — every result is just a suggestion. Change the dropdowns to what you want before selecting **Submit Issue**; the AI never has the last word.
- *The thread summary looks incomplete or out of date.* Select **Refresh** on the **✨ AI summary** card to regenerate it after new notes are added, and always double-check anything important against the actual notes.
- *I don't see the 🛠️ Suggest a fix card.* Like the ✨ helpers, it only appears when the AI is turned on. If the other AI cards are missing too, the AI is off — a server setting. If the ✨ cards are there but the wrench card behaves oddly, see the next bullet.
- *Suggest a fix says it couldn't suggest a fix right now.* You'll see *AI couldn't suggest a fix right now.* when the server couldn't reach the AI provider (or the Smart provider, in a two-tier setup). Nothing on the issue is changed. Select **Try again** in a moment; if it keeps failing, ask whoever runs your server to check the AI settings.
- *The fix suggestion has low confidence or feels generic.* That usually means the issue is thin — a one-line summary with no notes, no log attachments, and nothing similar already resolved. Add detail: write a note describing what you've observed, attach the relevant **log or text file**, then select **Try again**. More evidence makes for a stronger, higher-confidence suggestion.
- *The fix suggestion looks wrong.* It's only ever a starting point, never a guarantee, and it changes nothing on the issue. Ignore it, or try the parts that make sense. If a step helped, jot what actually worked into a **note** — that becomes useful evidence for the next similar issue.
- *Is my issue text being sent anywhere?* Only with a **cloud** provider, and only when you press an AI button. With a **local** model (Ollama or LM Studio) the text never leaves the machine. Note the two tiers: your server might keep the quick ✨ jobs local while sending **🛠️ Suggest a fix** to a cloud Smart provider. If in doubt, ask whoever runs your server which paths are configured.


# 21. Integrations: Git & Chat Notifications

*Link code commits to issues, and get pinged in Slack or Discord when things change.*

> **QUICK VERSION** — Connect a **GitHub** repository so a commit that says **fixes #123** links to (and can resolve) that issue. Separately, add a **Slack** or **Discord** webhook on the project's **Settings** page to get pinged whenever an issue is created, changed, or commented on.


## Two integrations, two directions

OpenTrack connects to the tools your team already uses in two independent ways, and it helps to keep them straight. **Git integration** is *incoming*: your code host (GitHub) tells OpenTrack about commits, and OpenTrack links them to the matching issues. **Chat notifications** are *outgoing*: OpenTrack tells a chat channel (Slack, Discord, or your own service) whenever an issue changes. You can turn on either one, both, or neither. Both are set up per project, and both need the **Manager** role.


## Git integration — how it works

A *commit* is a saved change to your code. When you connect a **GitHub** repository (repo) to a project, OpenTrack watches for commits whose message mentions an issue number and links the two automatically. That closes the gap between *here's the bug* and *here's the change that fixed it* — from any issue you can jump straight to the commit that addressed it.

What OpenTrack does with a commit depends on the exact words in the message:

| Commit message contains | What OpenTrack does |
| --- | --- |
| **`fixes #123`** (also **`closes #123`** or **`resolves #123`**) | Links the commit to issue #123 **and**, if you've turned on auto-resolve, moves that issue to **Resolved** |
| **`#123`** (a plain mention) | Links the commit to issue #123 **without** changing its status |

> **OPENTRACK ONLY RECEIVES** — Git integration is strictly one-way and read-only from OpenTrack's side. OpenTrack only **receives** notifications from GitHub — it never needs access to your code, and never signs in to your GitHub account. Auto-resolve, when on, only affects **open** issues and only when your project's workflow allows a move to **Resolved**.


## Connect a repository (Manager)

Setting up Git integration has two halves: first you enable it in OpenTrack and set a shared secret, then you paste OpenTrack's address into GitHub. Do the OpenTrack half first, because it gives you the address (the *Payload URL*) you'll need for GitHub.

1. On the project, go to **Settings**, then select **Git** across the top (this opens the **Git integration** page).
2. Tick **Enable Git integration for this project**.
3. In **Webhook secret**, enter a long random string you make up. This is required when integration is enabled — it's what proves a notification really came from your GitHub. Treat it like a password.
4. Decide whether to tick **Auto-resolve an issue when a pushed commit says `fixes #id`** (or close/resolve). Leave it off if you'd rather commits only link, never change status.
5. Select **Save**. OpenTrack confirms with *Saved.* and shows a **Payload URL** in the *Set up the webhook in GitHub* section below — you'll need it in the next step.

> _[Figure: The Git integration page: the Enable switch, the Webhook secret field, the Auto-resolve checkbox, and the Payload URL below]_

| Control | What it does |
| --- | --- |
| **Enable Git integration for this project** | The master on/off switch. When off, commits are ignored. |
| **Webhook secret** | A shared password you invent. You paste the same value into GitHub; OpenTrack rejects any push whose signature doesn't match, so a strong, random secret is what keeps the endpoint safe. |
| **Auto-resolve** | When ticked, a `fixes #id` (or close/resolve) commit moves that open issue to **Resolved**. A plain `#id` mention only links, whatever this is set to. |


### Set up the webhook in GitHub

Now hand OpenTrack's address to GitHub so it knows where to send commit notifications. The *Set up the webhook in GitHub* section on the Git integration page lists these same steps, with your exact Payload URL ready to copy:

1. In your GitHub repository, go to **Settings → Webhooks → Add webhook**.
2. **Payload URL:** paste the address OpenTrack showed you (it looks like `…/git/webhook/12`).
3. **Content type:** choose `application/json`.
4. **Secret:** enter the same **Webhook secret** you set in OpenTrack.
5. **Events:** choose *Just the `push` event*.
6. Save the webhook.

> **REACHABILITY & THE GREEN CHECK** — For GitHub to reach your server, the server must be reachable from the internet (a public address or a secure tunnel) and — since the secret is what guards it — is best served over encrypted **HTTPS** (HyperText Transfer Protocol Secure). When you add the webhook, GitHub sends a test *ping*; a green check mark there means the address and secret are right.


## See the linked commits

Once everything is connected, any issue that a commit has referenced grows a **Linked commits** section on its page. Each entry shows a short commit identifier that links back to the commit on GitHub, and a **resolved** badge if that particular commit closed the issue. That makes the whole trail — from the original report to the exact change that fixed it — one click away, for anyone reviewing the issue later.

> _[Figure: An issue page showing the Linked commits section with a short commit id and a resolved badge]_


## Chat notifications — Slack, Discord, or your own service

Separately from Git, a project can **push a short notification to a chat channel** whenever one of its issues is created, changed, or commented on — so your team hears about activity where they already are, instead of having to check OpenTrack. This is set up in the **Integrations — outgoing webhooks** area on the project's **Settings** page.

The idea is the mirror image of Git integration: there, GitHub sent messages *in*; here, OpenTrack sends messages *out* to a URL that the chat tool gives you. That URL is called an *incoming webhook* (incoming from the chat tool's point of view).

1. In your chat tool, create an **incoming webhook** and copy its URL. Slack and Discord both provide these; the URL contains a secret token, so treat it as private.
2. On the project, go to **Settings** and scroll to **Integrations — outgoing webhooks**.
3. Paste the URL into the **Webhook URL** box.
4. Choose its **Format** — **Slack**, **Discord**, or **Generic** — from the dropdown.
5. Select **Add**. The webhook appears in the list above, and can be removed anytime with its **Delete** button.

> _[Figure: The Integrations — outgoing webhooks area: the list of existing webhooks and the Webhook URL box with the Format dropdown]_

| Format | What it sends |
| --- | --- |
| **Slack** | A short text message formatted for a Slack channel. |
| **Discord** | A short text message formatted for a Discord channel. |
| **Generic** | The full details as **JSON** (JavaScript Object Notation) — event, project, issue, status, and time — posted to your own service or automation. |

The list of configured webhooks shows each one's **Destination** (its URL) and **Format**, with a **Delete** button beside it. You can add as many as you like — for example a Slack channel for the team and a Generic endpoint feeding your own dashboard — and remove any of them at any time.

> **ONLY ADD URLS YOU TRUST** — An outgoing-webhook URL sends your issue activity to whatever service it points at. Only paste URLs you created yourself and trust — never one someone else handed you without knowing exactly where it goes. Because the URL usually contains a secret token, anyone who has it can receive your project's activity.


## Troubleshooting

- *I can't open the Git page or the webhooks section.* Both need the **Manager** role on the project. If you see *You need the Manager role…​*, ask a Manager to set it up or to grant you the role.
- *Commits aren't linking to issues.* Check that **Enable Git integration** is ticked and saved, that the GitHub webhook's **Payload URL** matches the one OpenTrack shows, and that the **Secret** is identical on both sides. In GitHub's webhook page, a red mark or failed deliveries point to a wrong URL, an unreachable server, or a mismatched secret.
- *The commit linked but the issue didn't resolve.* Auto-resolve must be ticked, the commit must use a resolving keyword (`fixes`, `closes`, or `resolves`) rather than a plain `#id`, the issue must be **open**, and your project's workflow must allow a move to **Resolved**. A plain `#123` only ever links.
- *GitHub shows a red X on the webhook.* GitHub can't reach your server, or the secret is wrong. Make sure the server is reachable from the internet (public address or secure tunnel), ideally over **HTTPS**, then re-check the secret and redeliver the ping.
- *No chat notifications are arriving.* Confirm the webhook is listed under **Integrations — outgoing webhooks** with the right **Format**, and that the incoming-webhook URL is still valid in Slack or Discord (they can be revoked). Send a test change to the issue to trigger one.
- *Notifications go to the wrong place or look wrong.* The **Format** must match the destination — use **Slack** for a Slack URL, **Discord** for a Discord URL, and **Generic** only for your own service that expects JSON. Delete the mismatched webhook and add it again with the correct format.


# 22. Mobile, Tablets & the Field

*Use OpenTrack from a phone or tablet, install it like an app, work off-network, and stamp a problem with where it is.*

> **QUICK VERSION** — OpenTrack installs on a tablet or phone like an app (no app store), keeps a bug-hunt checklist usable through brief network drops and syncs your marks when you're back online, can stamp your **GPS** location onto an issue with **📍 Attach my location**, and lets anyone open a project's report form by scanning its printed **QR code**.


## Install it like an app (PWA)

OpenTrack is a **Progressive Web App (PWA)** — a website built well enough to install and run like a regular app, with its own icon on your home screen and a full-screen window with no browser address bar or tabs in the way. There is nothing to download from an app store, and no separate mobile app to keep updated: you install the same OpenTrack your team uses in a desktop browser, straight from its web address.

When installed, the app opens under the name *OpenTrack*, uses the OpenTrack icon, and launches in **standalone** display mode — meaning it looks and behaves like an app rather than a browser tab. It works in either portrait or landscape, so you can turn a tablet whichever way suits the job.

1. Open OpenTrack in the device's web browser and sign in once, so the app has your session.
2. **On an iPad or iPhone (Safari):** tap the **Share** button (the square with an up-arrow), then tap **Add to Home Screen**, then **Add**.
3. **On Android (Chrome):** tap the **⋮** menu, then tap **Install app** (some versions say **Add to Home screen**), then confirm.
4. Find the new **OpenTrack** icon on your home screen and tap it. It opens full-screen, like any other app.

> _[Figure: OpenTrack installed on a tablet home screen next to other app icons, with its own icon]_

> **SAME APP, SMALLER SCREEN** — The mobile experience is not a stripped-down version — it is the same OpenTrack, laid out to fit a smaller screen. Every feature described in this manual is available on a tablet or phone. Only the field-specific conveniences in this chapter (offline checklists, location capture, QR reporting) are things you are more likely to reach for away from a desk.


## What keeps working offline

OpenTrack installs a small helper called a **service worker** — a background piece of the web app that sits between the app and the network. Its job is to remember pages you have already opened and hand them back if the network drops. Because of it, a page you loaded while you had a signal stays viewable if your connection flickers out a moment later — handy when you are standing next to the equipment with a bug-hunt checklist open on a tablet and the Wi-Fi is patchy.

It is important to be realistic about what "offline" means here, so you are never surprised:

- **Pages you have already visited while online stay readable offline.** A page you have never opened cannot be shown from thin air — you must have loaded it at least once with a connection.
- **Only viewing is cached, not saving.** Reading is safe offline. Actions that change data (creating an issue, adding a note, uploading a file) still need the network at the moment you do them — with one deliberate exception below.
- **Checklist check-offs are the exception** — they are queued while offline and replayed automatically when you reconnect (next section).
- **Anything requiring a live server connection won't refresh offline.** Live counters, notifications, and searches against the server reflect the last time you were connected.

> **BEST ON YOUR OWN DEVICE** — The offline cache lives on the device it was created on, which suits a personal or trusted tablet. On a shared device, signing out clears your session but does not wipe this cache, so treat installed OpenTrack the way you would treat a saved password — put it only on a device you control.


## Offline checklists that sync themselves

The one place OpenTrack lets you keep working with no connection is the **bug-hunt checklist** (the project checklist covered in its own chapter). This is on purpose: a checklist is exactly the kind of thing you run while walking a site, and you should not lose a tap because the signal dropped between two rooms.

Here is how it behaves, step by step:

1. **Online:** tapping **Pass**, **Fail**, or **N/A** on a checklist item saves to the server immediately, exactly as it does on a desktop. Nothing special happens.
2. **Offline:** the same tap is caught before it can fail. OpenTrack records your choice in a small on-device queue and updates the item's badge right away so you can see it took — the item shows its new state (**Pass**, **Fail**, **N/A**, or **To do**) followed by a small amber **• pending sync** note.
3. **Keep working:** you can mark as many items as you like while offline. If you change your mind on an item, just tap again — the queue keeps only your latest choice for each item, so the last tap wins.
4. **Reconnect:** when the device comes back online — either the next time you open the checklist page, or the instant the browser notices the network is back — the queued changes are sent to the server automatically and the page refreshes to show the true, saved state. The amber **pending sync** notes disappear as each change lands.

> **CREATING AN ISSUE FROM A FAILURE NEEDS A SIGNAL** — Marking an item **Fail** works offline, but the separate step of **turning that failure into a linked issue** needs a live connection. If you try it while offline, OpenTrack tells you plainly: the action will be waiting for you when you're back online. Your **Fail** mark is safe in the queue in the meantime.

You do not have to do anything to make the sync happen — no "upload" button, no manual refresh. If a change ever fails to send (for example the server rejects it), OpenTrack keeps it in the queue and tries again on the next reconnect rather than silently dropping it.


## Attach your GPS location to an issue

When a problem lives somewhere physical — a piece of equipment in a shelter, a broken sign on a trail, a fault at a field site — it helps to record *where* it is, not just *what* it is. On the **New Issue** form, OpenTrack can stamp your current **GPS** (Global Positioning System) coordinates onto the issue with one tap, using your device's own location.

1. Start a new issue on the device that is physically at the location.
2. Scroll to the **📍 Attach my location** button, just above the **Submit Issue** button.
3. Tap it. Your device (or browser) shows its own permission prompt asking to share your location — tap **Allow**.
4. While it works, the note beside the button reads *Getting your location…*. When it succeeds, the note changes to *Location attached:* followed by your latitude and longitude, and the coordinates are stored on the issue when you submit.
5. Finish filling in the issue and tap **Submit Issue** as usual.

> _[Figure: The New Issue form on a phone with the 📍 Attach my location button and a Location attached: 34.12345, -84.56789 confirmation]_

A few things worth knowing about how this works:

- **It is entirely opt-in.** OpenTrack never reads your location on its own. Nothing happens until you tap the button, and even then your device asks permission before any coordinates are read.
- **The button explains itself when empty.** Before you use it, the note beside it reads *Optional — useful for a problem out in the field.* so it is clear the location is not required.
- **High accuracy is requested.** OpenTrack asks the device for its most precise fix, which on a phone or tablet usually means the built-in GPS. Accuracy still depends on the device, the sky view, and being outdoors.
- **If it can't get a fix,** the note tells you why in plain words — for example *Couldn't get location:* with the reason, or *This browser can't provide a location.* on a device with no location support. You can still submit the issue without coordinates.

> **CAPTURE ON-SITE** — Because the button reads the device's *current* position, attach the location while you are standing at the problem — not later back at your desk, where you would only be stamping the issue with the office coordinates.


## Report by scanning a QR code

A project can publish a printed **QR code** (Quick Response code — the square barcode you scan with a phone camera) that opens its public report form. This is the fastest way to let someone who has never used OpenTrack file a problem: they point their phone's camera at the poster, tap the link that pops up, and land directly on the report form for that project — no account, no app, no typing a web address.

1. Point the device's camera at the project's printed **QR code** poster.
2. Tap the link the camera offers.
3. The public report form opens straight to that project. Fill in what's wrong and submit.

This is the same public intake described in full in the *Public Trouble-Ticket Intake & QR Posters* chapter — printing the poster, what the reporter sees, and how those reports arrive in your project. On a phone in the field, scanning is simply the quickest doorway to it.


## Troubleshooting

- *I don't see an "Install" option.* You must open OpenTrack over a secure connection and, on iPad or iPhone, use **Safari** (the Share → Add to Home Screen path is a Safari feature). On Android, use **Chrome** and look under the **⋮** menu for **Install app** or **Add to Home screen**.
- *An offline page shows old information.* That's expected — offline you're seeing the last version the device cached while online. Reconnect and reload to get fresh data.
- *A page won't open at all offline.* You can only view pages offline that you opened at least once while connected. If you never loaded that page online, there's nothing cached to show.
- *My checklist taps show "• pending sync" and aren't saving.* You're offline. That note means the change is safely queued — it will send itself the next time you're back online, or when you reopen the checklist page. Don't clear the browser data in the meantime.
- *Turning a failed check into an issue did nothing.* That specific step needs a connection. OpenTrack will have told you it's waiting until you're back online; your **Fail** mark is still recorded.
- *"Attach my location" says it was denied.* Your device or browser blocked the location permission. Allow location for OpenTrack in your device's settings, then tap the button again. Being indoors or with a poor sky view can also make a fix fail or take longer.
- *The location is wrong or way off.* Capture it while physically at the problem, outdoors if possible. A cached or coarse fix (common indoors) can be far from where you actually are.
- *Scanning the QR poster doesn't open anything.* Make sure the camera is close enough to see the whole square, that there's enough light, and that the device is online to load the form. Some older phones need a dedicated QR-scanner app rather than the plain camera.


# 23. Printing, Preferences & Administration

*Print or save an issue as a PDF, set your personal defaults, and — for administrators — manage user accounts and run the instance.*

> **QUICK VERSION** — **Print / Save as PDF** an issue from its own print page. Set your personal defaults — default project and default sort — under **Preferences** in the left navigation. **Administrators** open **Users** to set each person's global role and to activate or deactivate accounts; a handful of server-level options (first admin, email, HTTPS) are set once at install time.


## Print or save an issue as a PDF

Every issue has a clean, print-friendly version of itself — a single page with just the content and none of the menus, buttons, or navigation. From there you can send it to a printer or save it as a **PDF** (Portable Document Format — the universal "looks the same everywhere" document file) to keep on file or email to someone who doesn't use OpenTrack.

1. Open the issue you want to print.
2. Go to its print page (the address is the issue followed by */print* — for example *issues/42/print*). This opens the print-friendly view.
3. At the top you'll see two buttons that do *not* appear on paper: **Print / Save as PDF** and **Back to issue**.
4. Click **Print / Save as PDF**. Your browser's own print dialog opens.
5. To print, pick a printer and print as normal. To keep a file instead, choose **Save as PDF** (or **Microsoft Print to PDF**) as the destination, then **Save**.
6. Click **Back to issue** any time to return to the full, interactive issue.

> _[Figure: The print-friendly issue page with the Print / Save as PDF and Back to issue buttons at the top]_


### What appears on the printed page

The print view is laid out for reading on paper, top to bottom. It gathers the whole record of the issue in one place:

- A heading with the issue number and title, and the project name beneath it.
- A summary table of the key fields: **Status**, **Severity**, **Priority**, **Resolution**, **Reporter**, **Assignee**, **Category**, **Reproducibility**, **Created**, **Updated**, and — if one is set — the **Due** date.
- The full **Description**, with its Markdown formatting rendered (bold, code, lists, and so on).
- **Steps to reproduce**, if the issue has any.
- All **Notes** (comments), each showing the author, the date, and — where applicable — a *(private)* marker.
- The **Time log**, if any time was recorded, listing each entry and a total at the top.
- The **History** — the dated trail of who changed which field, from what old value to what new value.

> **IT'S A SNAPSHOT** — The printed page or saved PDF captures the issue exactly as it stands the moment you print it. It doesn't update later. If the issue changes, print it again to get a fresh copy.

> **WHAT YOU CAN PRINT, YOU COULD ALREADY SEE** — Printing doesn't reveal anything your role can't already view. The print page respects the same access rules as the issue itself — including private notes, which appear only to people allowed to see them.


## Your preferences

Select **Preferences** in the left navigation to open **Your preferences** — a short screen of personal defaults that make OpenTrack open the way *you* like it. These are yours alone: changing them affects only your own view and no one else's. There are two settings, and a **Save preferences** button; when you save, a green **Saved.** confirmation appears.

> _[Figure: The Preferences page with the Default project and Default sort dropdowns and the Save preferences button]_

| Setting | What it does | Default |
| --- | --- | --- |
| **Default project for new issues** | Pick one of your projects, and **Quick add** pre-selects it every time so you don't choose the project on each new report. A short note under the box reminds you: when set, Quick-add pre-selects this project. | **— none —** (you choose the project each time) |
| **Default sort on the issue list** | Choose the order the issue list uses when you open it without having picked a sort yourself. The choices are **Updated (newest)**, **Updated (oldest)**, **Created (newest)**, **Created (oldest)**, **Priority (high→low)**, **Severity (high→low)**, **Status**, **ID (newest)**, and **ID (oldest)**. The note under the box explains it applies only when the list address doesn't already specify a sort. | **— newest updated —** |

> **SET IT ONCE** — If you spend most of your time in a single project, set it as your **Default project** — from then on, **Quick add** already has it selected and filing a problem is one field shorter.

Your own account details — password, email, two-factor sign-in, passkeys, and so on — are managed separately under your name in the navigation (the **Account** area), not on this Preferences screen. Preferences is only about how the issue tracker itself behaves for you.


## Administration (Administrators only)

The **Administrator** is a *global* role — not a per-project one — usually held by the person who installed the server. Only an administrator sees the **Users** link in the navigation, and only an administrator can open the pages below. Most administration is occasional: set someone's role, or turn an account on or off.


### The Users screen

Select **Users** in the navigation to open **User Administration**. The top of the page is a table listing every account, and below it are two small forms for making changes. When you make a change, a colored banner confirms it (green) or explains what went wrong (red).

> _[Figure: The User Administration screen: a table of accounts above two forms — Set global role and Activate / deactivate]_

The account table has four columns:

| Column | What it shows |
| --- | --- |
| **User** | The person's username. |
| **Email** | The email address on the account. |
| **Global role** | Their instance-wide role — one of **Viewer**, **Reporter**, **Updater**, **Developer**, **Manager**, or **Administrator**. |
| **Status** | **Active** (can sign in) or **Deactivated** (blocked from signing in, but history preserved). |

Beneath the table are the two controls:

- **Set global role** — pick a user, pick a role from **Viewer**, **Reporter**, **Updater**, **Developer**, **Manager**, or **Administrator**, and click **Update role**. A note reminds you that you **cannot change your own role** (a safety catch so an administrator can't accidentally lock themselves out).
- **Activate / deactivate** — pick a user, choose **Active** or **Deactivated**, and click **Apply**. A **deactivated** user cannot sign in, but nothing they created is deleted — their issues, notes, and history remain. A note reminds you that you **cannot deactivate yourself**.

> **GLOBAL ROLE VS. PROJECT ROLE** — The role you set here is the person's **global** role across the whole instance. It's separate from the **per-project** role a project **Manager** grants on a project's **Members** screen (see the *People & Roles* chapter). Most day-to-day access is handled per project; the global role is the floor the administrator sets for the whole instance.

> **DEACTIVATE, DON'T DELETE** — When someone leaves, **deactivate** their account rather than trying to erase it. That immediately stops them signing in while keeping the issues, comments, and history they contributed intact and correctly attributed.


### Server-level settings (set once at install)

A few instance-wide options aren't buttons in the app — they're settings the administrator configures on the server, usually once, when standing the instance up. They're listed here so an administrator knows they exist and where to look.

| Setting | What it's for |
| --- | --- |
| **Set the first admin ahead of time** | Rather than relying on "first person to register becomes admin," the server can be told an administrator email and password before its first run, via the `OpenTrack*BootstrapAdmin*…` settings. That account starts as the Administrator. |
| **Turn on email** | Optional. Out of the box, OpenTrack sends no email and instead writes things like password-reset links to its own log. To send real email, fill in the `OpenTrack*Email*…` settings with your outgoing mail (SMTP — Simple Mail Transfer Protocol) server details. |
| **Require encryption (HTTPS)** | On a trusted home network OpenTrack can run over plain HTTP. If the server is reachable from outside that network, set `OpenTrack__RequireHttps=true` and supply a certificate so traffic is encrypted (HTTPS — the secure, padlocked form of a web address). |

> **ENCRYPT ANYTHING FACING THE INTERNET** — Plain HTTP is fine only on a network you fully trust. The moment the server can be reached from the wider internet, turn on **RequireHttps** and give it a certificate — otherwise sign-ins and issue contents travel unencrypted.


## Troubleshooting

- *The print page still shows menus and buttons.* You're on the normal issue page, not the print view. Open the issue's */print* address (or use the print link), then click **Print / Save as PDF** there.
- *There's no "Save as PDF" choice in the print dialog.* That option comes from your browser and operating system, not OpenTrack. Look for **Save as PDF** or **Microsoft Print to PDF** in the printer/destination list; if it's missing, print to a real printer or install a PDF printer.
- *A private note didn't appear in the printout.* Private notes print only for people allowed to see them. If you can't see it on the issue, it won't be on the paper either — that's the access rule working as intended.
- *I saved my preferences but the issue list didn't change.* Your **Default sort** applies only when you open the list without a sort already chosen. If the list address already specifies an order (for example from a saved link), that wins. Open a fresh issue list to see your default.
- *I don't see a "Users" link.* Only a global **Administrator** sees it. If you need it and don't have it, ask an existing administrator to raise your global role.
- *"Update role" or "Apply" won't act on my own account.* That's deliberate. You cannot change your own role or deactivate yourself — a safety catch against locking yourself out. Have another administrator make the change if it's truly needed.
- *No password-reset emails are arriving.* Unless an administrator has configured email, OpenTrack doesn't send any — it writes reset links to its log instead. Turn on email via the `OpenTrack*Email*…` settings to send real messages.
- *The browser warns the connection isn't secure.* The instance is running over plain HTTP. That's acceptable only on a trusted local network; if the server faces the internet, an administrator should set **RequireHttps** and add a certificate.


# 24. Keyboard Shortcuts & Quick Navigation

*Jump anywhere in OpenTrack without reaching for the mouse — the command palette and the left-navigation map.*

> **QUICK VERSION** — Press **Ctrl+K** (**⌘K** on a Mac) anywhere in OpenTrack to open the **command palette**. Type to search, type a number like **123** to jump straight to that issue, use the **↑/↓** arrows and **Enter** to pick, and **Esc** to close. Everything in the left navigation is also one click away.


## The command palette

OpenTrack has one keyboard shortcut worth building a habit around: the **command palette**. It's a search box that drops down over whatever you're looking at, lets you type where you want to go, and takes you there — all without touching the mouse. It works the same way in a desktop browser, on the installed app, and on the desktop version of OpenTrack, because it only ever *navigates* (changes which page you're on); it never changes your data, so there's nothing to undo and no risk in opening it to look.


### Opening and closing it

1. Press **Ctrl+K** on Windows or Linux, or **⌘K** (Command+K) on a Mac. A search box appears near the top of the screen, dimming the page behind it.
2. Start typing. The list below the box updates as you type.
3. Press **Enter** to go to the highlighted result, or click any result.
4. Press **Esc**, click outside the box, or press **Ctrl+K** / **⌘K** again to close it without going anywhere.

> _[Figure: The command palette open over the app: a search box reading “Type to search, #123 to jump to an issue, or a command…” with a list of destinations below it]_

The box shows a hint of everything it can do right in its placeholder text: *Type to search, #123 to jump to an issue, or a command…*. There are three things you can type.


### 1. Jump to a specific issue by number

If you know an issue's number, type it — with or without a leading *#*. As soon as the palette sees a number (for example `123` or `#123`), it offers **Go to issue #123** at the top of the list. Press **Enter** and you're on that issue. This is the fastest way to reach an issue you already know.


### 2. Run a command (jump to a section)

Type part of a section's name and the palette lists the matching destinations. You don't need the exact word — each command also answers to a few natural aliases, so "bugs" finds the issue list and "export" finds the backup page. The built-in commands are:

| Command | Goes to | Also try typing |
| --- | --- | --- |
| **Dashboard** | Your home dashboard | home, overview |
| **All issues** | The full issue list | issues, list, bugs |
| **Quick add a problem** | The quick-add form for a new issue | new, add, quick, report, create |
| **Projects** | Your projects | projects |
| **Notifications** | Your notifications | notifications, alerts |
| **Backup & export** | The backup, export, and import tools | backup, export, import, csv, json, mantis |


### 3. Search all issues for text

If you type words rather than a number, the palette also offers a **Search issues for "…"** option at the bottom of the list. Choose it and you land on the issue list already filtered to your search text — a one-step way to go from "I remember a word in the title" to the matching issues.

> **EMPTY BOX SHOWS EVERYTHING** — Open the palette and type nothing, and it simply lists all the built-in commands. It's a handy reminder of where you can go — you don't have to remember the destinations, just the **Ctrl+K** to summon the list.


## Every key the palette understands

Once the palette is open, it's driven entirely from the keyboard:

| Key | What it does |
| --- | --- |
| **Ctrl+K** / **⌘K** | Opens the palette from anywhere. Pressing it again while open closes it. |
| **Type text or a number** | Filters the list live — matching commands, a “Go to issue #…” option for a number, and a “Search issues for …” option for words. |
| **↓** (Down arrow) | Moves the highlight to the next result (wraps around to the top from the bottom). |
| **↑** (Up arrow) | Moves the highlight to the previous result (wraps around to the bottom from the top). |
| **Enter** | Goes to the highlighted result. |
| **Esc** | Closes the palette without going anywhere. |
| **Click a result** | Goes to that result (the mouse still works if you prefer it). |
| **Click outside the box** | Closes the palette, same as Esc. |

> **WHY ONE SHORTCUT?** — OpenTrack deliberately keeps a single, easy-to-remember shortcut instead of a thicket of key combinations you'd have to memorize. Learn **Ctrl+K**, and the palette gets you everywhere else. The buttons and links on each page do the rest — there are no hidden per-page hotkeys to trip over.


## The left-navigation map

Down the left side of every page is the **navigation menu** — the same set of links wherever you are. What you see depends on whether you're signed in and on your role, but the full menu for a signed-in user is below. A small reminder in the menu itself — *🔍 Ctrl+K to jump* — points you back to the palette.

| Menu item | Where it takes you |
| --- | --- |
| **Home** | The starting page / dashboard. |
| **Projects** | The list of projects you can see; the doorway to any single project and its settings. |
| **Issues** | The full issue list, with all the filters and search. |
| **Quick add** | The fast form for filing a new problem in a couple of fields. |
| **Reports** | Charts and summaries across your issues. |
| **SLA status** | How issues stand against their Service Level Agreement (SLA) targets — on track, at risk, or breached. |
| **Notifications** | Updates about issues you're involved in or monitoring. A red badge shows the unread count. |
| **Backup & export** | Download a backup or export your data (CSV, JSON, and more). |
| **Import** | Bring data in from a file or another tracker. |
| **Preferences** | Your personal defaults (default project and default sort). |
| **Users** | Administrators only: manage accounts, roles, and status. |
| **Your name (Account)** | Your own account settings — password, email, two-factor sign-in, passkeys. |
| **Logout** | Signs you out. |

> **THE MENU FITS YOUR ROLE** — You'll only see links you're allowed to use. **Users** appears only for a global **Administrator**; before you sign in, the menu shows just **Home**, **Register**, and **Login**. If a link named here is missing for you, it's your role — not a fault. See the *People & Roles* chapter.

> **ON A NARROW SCREEN** — On a phone or a small window the navigation collapses behind a menu button (the ☰ toggle) to save room. Tap it to slide the same list out. The **Ctrl+K** palette still works everywhere, and on a tablet with a keyboard it's often quicker than opening the menu.


## Troubleshooting

- *Ctrl+K does nothing.* Make sure the OpenTrack page itself has focus — click once on an empty part of the page, then try again. A few browsers reserve Ctrl+K for their own address bar; click into the page first so OpenTrack receives the key. On a Mac, use **⌘K**, not Ctrl+K.
- *The palette opened but typing a number didn't offer the issue.* Type just the digits (for example `42` or `#42`); extra letters turn it into a text search instead. The **Go to issue #…** option appears only when the box contains a number on its own.
- *“Go to issue #…” led to a “not found” or access-denied page.* The palette will happily try any number, but you still need permission to view that issue, and it has to exist. If your role doesn't allow it, OpenTrack won't show it — see *People & Roles*.
- *The arrow keys move the page instead of the list.* Click inside the palette's search box first so it has focus, then use **↑/↓**. Clicking outside the box closes the palette.
- *A menu link I read about here isn't in my sidebar.* It's almost always your role. **Users** is administrators-only, and some links depend on being signed in. Ask an administrator if you believe you should have access.
- *The sidebar is gone on my phone.* It collapsed to fit the screen. Tap the menu toggle (☰) at the top to slide it back out, or just press **Ctrl+K** to jump without it.


# 25. Glossary

*Plain-language definitions of every term and acronym used across OpenTrack and this manual.*


## How to read this glossary

OpenTrack borrows a fair amount of vocabulary from the world of bug tracking, and this manual uses those words because the app's own screens do too. This glossary is here so you never have to guess what one means. Skim it once, then come back whenever a word in another chapter — or a label on screen — leaves you unsure.

Definitions are short and practical, aimed at what the term means when you're actually using OpenTrack, not at textbook precision. Terms are listed alphabetically across the tables below. Acronyms are spelled out in full the first time they appear.


## Terms A through F

| Term | Meaning |
| --- | --- |
| **Administrator** | The most powerful role — and a *global* one, not tied to a single project. An administrator runs the whole instance: every project, every user account, and the server-level settings. Usually the person who installed OpenTrack. |
| **API** | Application Programming Interface. A doorway that lets another program talk to OpenTrack directly — read or create issues, for example — rather than a person clicking around the screens. Useful for scripts and custom integrations. |
| **At-risk** | An SLA state meaning a target (like a response or resolution deadline) is approaching but not yet missed — a heads-up to act before it becomes a breach. See *SLA* and *breach*. |
| **Automation** | Rules a project **Manager** sets up so OpenTrack does routine things by itself when an issue changes — for example, notify someone or set a field when an issue reaches a certain status. Configured on a project's **Automation** screen. |
| **Backup / export** | Saving a copy of your OpenTrack data to a file you keep — for safekeeping, or to move it elsewhere. Exports come in formats like CSV and JSON. Found under **Backup & export** in the navigation. |
| **Board (Kanban)** | A card-and-column view of a project's issues, where each column is a status and each card an issue you can drag from one column to the next. "Kanban" is the Japanese word for the signboard this style of board is named after. |
| **Breach** | An SLA target that has been missed — a deadline passed without the required response or resolution. The opposite of on-track; the step past *at-risk*. |
| **Category** | An optional label that sorts issues within a project by area — for example "UI," "Database," or "Docs." Categories are defined per project by a Manager. |
| **Changelog** | A per-version list of what was fixed or added, built from the issues tied to that version. Handy for telling users what changed in a release. See *Version* and *Roadmap*. |
| **Closed** | An issue **status** meaning the issue is finished and filed away — resolved and no longer needing attention. See *Status*. |
| **Commit** | A single saved change in a **Git** repository (a snapshot of edited code with a message). OpenTrack can link commits to issues so you can see the code that fixed a bug. See *Git*. |
| **CSV** | Comma-Separated Values. A plain-text spreadsheet format — rows of data with commas between the columns — that opens in Excel or any spreadsheet app. One of OpenTrack's export formats. |
| **Custom field** | An extra field a Manager adds to a project beyond the built-in ones, to capture information particular to that project. A custom field has a type — **Text**, **Number**, **Date**, or a fixed **list** of choices. See *Custom field type*. |
| **Custom field type** | The kind of data a custom field holds: **Text** (free words), **Number**, **Date** (a calendar date), or **Enum** (one value picked from a set list defined on the field). |
| **Developer** | A role: everything an Updater can do, plus being **assigned** issues and moving them through the workflow — the person who actually does the work of fixing an issue. |
| **Dashboard** | Your home screen. Across every project you can see, it shows how many issues are **open**, **overdue**, and **stale**, with links to jump straight to each group. |
| **Due date** | An optional date by which an issue should be handled. Issues past their due date count as **overdue** on the dashboard. |


## Terms G through N

| Term | Meaning |
| --- | --- |
| **Git** | The most common version-control system for source code — the tool that tracks every change to a codebase. OpenTrack can connect to a project's Git repository so commits mentioning an issue show up on it. See *Commit*, *Repository*. |
| **GPS** | Global Positioning System. The satellite system a phone or tablet uses to find where it is. On the New Issue form, **📍 Attach my location** uses GPS to stamp the issue with your current coordinates. See the *Mobile, Tablets & the Field* chapter. |
| **HTTP / HTTPS** | The language web pages travel in. **HTTPS** is the encrypted, padlocked form (the "S" is for Secure). OpenTrack can require HTTPS so sign-ins and issue contents aren't sent in the clear. |
| **Import** | Bringing data into OpenTrack from a file or another tracker (such as Mantis). The counterpart to *export*. Found under **Import** in the navigation. |
| **Issue** | The core record in OpenTrack — one bug, task, or request. Everything else (notes, tags, history, time logs, relationships) hangs off an issue. Also called a ticket or a bug. |
| **JSON** | JavaScript Object Notation. A structured text format that programs read easily. OpenTrack uses it for exports and for the full, detailed shape of a webhook message. See *Webhook*. |
| **Manager** | A role: everything a Developer can do, plus configuring the project — members, categories, versions, custom fields, automation, SLA targets, workflow, integrations, Git, and public intake. |
| **Markdown** | A simple way to add formatting to plain text using ordinary characters — `**bold**`, `` `code` ``, and ```` ``` ```` fenced blocks for stack traces. OpenTrack renders Markdown in descriptions and notes. |
| **Monitor** | To follow an issue you're not otherwise involved in, so you're notified when it changes. Click **Monitor** on an issue to start; updates show under **Notifications**. |
| **Note** | A comment on an issue. Notes carry the discussion and running history of the work. A note can be marked **private** so only the project team (not public reporters) sees it. |
| **Notification** | An in-app update telling you something changed on an issue you report, are assigned, or monitor. The bell in the navigation shows a red count of unread ones. |


## Terms O through R

| Term | Meaning |
| --- | --- |
| **Overdue** | An issue whose **due date** has passed while it's still open. Counted on the dashboard so nothing quietly slips by its deadline. |
| **PDF** | Portable Document Format. A "looks the same everywhere" document file. You can print an issue or save it as a PDF from its print page. See the *Printing, Preferences & Administration* chapter. |
| **Priority** | How *urgent* an issue is — how soon it should be dealt with. OpenTrack's levels run **None**, **Low**, **Normal**, **High**, **Urgent**, and **Immediate**. Contrast with *Severity* (how *bad* it is). |
| **Private issue** | An issue hidden from lower-access viewers (such as a public reporter) while staying visible to the project team. Marked with the **private** flag; use it for anything sensitive. |
| **Project** | A container for issues — one product, system, or area of work — with its own members, settings, categories, versions, and rules. Access is granted per project. |
| **Public intake** | A project setting that opens a public report form so anyone — no account needed — can file a problem, often by scanning a printed **QR code**. See the *Public Trouble-Ticket Intake & QR Posters* chapter. |
| **PWA** | Progressive Web App. A website built well enough to install and run like a regular app, with its own home-screen icon and a full-screen window. OpenTrack is a PWA, so you can install it on a phone or tablet with no app store. |
| **QR code** | Quick Response code. The square barcode you scan with a phone camera. A project can print one that opens its public report form instantly. See *Public intake*. |
| **Relationship** | A link between two issues that records how they relate: **related to**, **duplicate of** (shown as "has duplicate" from the other side), **parent of** / **child of**, or **blocks** / **blocked by**. |
| **Reporter** | A role: file new issues, and read and comment on the issues they're allowed to see. The entry-level working role. |
| **Reproducibility** | How reliably a bug can be made to happen again. OpenTrack's values: **Always**, **Sometimes**, **Random**, **Have not tried**, **Unable to reproduce**, and **Not applicable**. |
| **Repository (repo)** | The store of a project's source code that **Git** manages. OpenTrack links a project to its repository so commits can reference issues. See *Git*, *Commit*. |
| **Resolution** | *How* an issue ended, recorded when it's closed out: **Open**, **Fixed**, **Reopened**, **Unable to reproduce**, **Not fixable**, **Duplicate**, **Not a bug**, **Suspended**, or **Won't fix**. Different from *Status*, which is where it is in the flow. |
| **Roadmap** | A forward-looking view grouping a project's open issues by the **version** they're targeted for — what's planned for each upcoming release. See *Version*, *Changelog*. |


## Terms S through Z

| Term | Meaning |
| --- | --- |
| **Service worker** | A small background helper the PWA installs to remember pages you've opened, so they stay viewable if the network drops. It's what makes OpenTrack usable through brief connection gaps in the field. |
| **Severity** | How *serious* or damaging an issue is. OpenTrack's scale runs **Feature**, **Trivial**, **Text**, **Tweak**, **Minor**, **Major**, **Crash**, and **Block**. Contrast with *Priority* (how *soon* to act). |
| **SLA** | Service Level Agreement. A promise about how quickly issues get a response or a fix. OpenTrack tracks each issue against its SLA target and flags ones that are **at-risk** or in **breach**. See the *Service Level Agreements* chapter. |
| **Stale** | An open issue that hasn't been touched in a long time — effectively forgotten. The dashboard counts stale issues so they can be revisited before they rot. |
| **Status** | Where an issue sits in its life: **New**, **Feedback**, **Acknowledged**, **Confirmed**, **Assigned**, **Resolved**, or **Closed**. Contrast with *Resolution* (how it ended). |
| **Steps to reproduce** | The recipe for making a bug happen — the ordered actions someone can follow to see the problem for themselves. A field on the issue, and the single most useful thing a reporter can provide. |
| **Sticky** | A flag that pins an important issue to the top of the list so it doesn't scroll away. Set by someone with the right role. |
| **Tag** | A free-form label you attach to an issue to group or find it later — lighter and more flexible than a category. An issue can have many tags. |
| **Ticket** | Another word for an *issue* — especially one filed through public intake. Same record, different name. |
| **Time log** | A record of the minutes spent on an issue. Entries add up to a total shown on the issue and in reports, for anyone tracking effort. |
| **Updater** | A role: everything a Reporter can do, plus editing issues — changing fields, adding tags, and tidying details. |
| **Version** | A named release of a project's product. Issues can note the version they **affect** and the version they're **fixed** in, which feeds the roadmap and changelog. |
| **Viewer** | The lowest role: read-only access to the issues they're allowed to see, with no ability to file or change anything. |
| **Webhook** | An automatic message OpenTrack sends to another service when something happens on a project — shaped for **Slack**, **Discord**, or a **Generic** structured JSON payload. Set up by a Manager under integrations. See *JSON*. |
| **Workflow** | The set of rules governing which status an issue can move to next, and who may move it. Configured per project, it keeps issues from skipping steps in a way the team hasn't agreed to. |

> **SEVERITY VS. PRIORITY, ONE MORE TIME** — These two trip people up more than any other pair. **Severity** is how *bad* the problem is (a crash is severe). **Priority** is how *soon* you'll deal with it (a crash almost nobody hits might still be low priority). They're set independently on purpose.


# 26. Troubleshooting & Frequently Asked Questions

*Common problems and their fixes, and answers to the questions people ask most — grounded in how OpenTrack really behaves.*

> **START HERE** — Almost every "why can't I…" in OpenTrack comes down to one of two things: **your role** (what you're allowed to do) or **which optional features your administrator turned on** (AI, Git, SLAs, public intake, chat notifications). Check those two first — they explain most surprises before you go looking for a bug.


## The two things that shape what you see

OpenTrack shows each person a different app on purpose. Before treating something as broken, rule out the two reasons the screen legitimately differs from what a colleague — or this manual — describes:

- **Your role.** Roles run from **Viewer** (read-only) up through **Reporter**, **Updater**, **Developer**, **Manager**, and **Administrator**, and — except for Administrator — they're set *per project*. A button you don't see is usually a button your role doesn't grant. See the *People & Roles* chapter.
- **Optional features.** Several parts of OpenTrack are switches an administrator or Manager can leave off: the **AI assistant**, **Git** integration, **SLA** tracking, **public intake**, and **chat notifications** (Slack/Discord webhooks). If a whole feature seems absent, it may simply be turned off for you.

> **"MISSING" IS USUALLY "NOT ALLOWED" OR "NOT ENABLED"** — OpenTrack hides what you can't use rather than showing it grayed out and teasing you. So a missing link or button is far more often a role or a feature switch than a fault. When in doubt, ask a project **Manager** or an **Administrator**.


## Signing in

- *My password isn't accepted.* Check for caps lock and the right username or email. If you're sure it's right and still can't get in, your account may be **deactivated** (see below) or you may need a password reset.
- *I asked for a password reset but no email came.* Unless your administrator has configured email, OpenTrack **doesn't send any** — it writes reset links to the server log instead. Ask your administrator to either turn on email or read the reset link from the log for you. See *Printing, Preferences & Administration*.
- *"Your account is deactivated" (or sign-in is blocked).* An administrator has deactivated the account. This blocks sign-in without deleting any of your history. Only an administrator can reactivate it, on the **Users** screen.
- *I'm asked for a code after my password.* You've turned on two-factor sign-in. Enter the code from your authenticator app, or use a recovery code if you've lost the app. Manage this under your **Account** area.
- *The browser warns the connection isn't secure.* The instance is running over plain HTTP. That's fine on a trusted local network, but if the server faces the internet, an administrator should require HTTPS and add a certificate.


## "I can't see a project or an issue"

- *A project a colleague mentions isn't in my list.* Project access is granted **per project**. If you're not a member, you won't see it at all. Ask that project's **Manager** to add you on its **Members** screen with an appropriate role.
- *I was given a direct link to an issue but got "not found" or "access denied."* OpenTrack never reveals an issue — or even that it exists — to someone whose role doesn't allow it. A direct link is no back door. You need to be a project member with sufficient access.
- *An issue vanished from my list.* It may have been marked **private** (hidden from lower-access viewers), moved, closed and filtered out, or you may have a filter or search still applied. Clear filters and check whether your role covers private issues.
- *Dashboard totals don't match what a teammate sees.* Those numbers only count issues *you* can see. Two people with different project access will legitimately see different totals — that's the privacy rule working, not a miscount.


## "A button or field is missing"

- *There's no "Submit Issue" / I can't file a bug.* Filing needs at least the **Reporter** role on that project. A **Viewer** can read but not create.
- *I can't edit an issue's fields.* Editing needs **Updater** or above. Reporters can comment on issues but not change their fields.
- *I can't be assigned an issue or move it along the workflow.* Being assigned and advancing status needs the **Developer** role (or above).
- *I don't see project settings — Members, Categories, Versions, Custom Fields, Automation, SLA, Workflow, Integrations, Git, or Intake.* All of those are **Manager**-level (or Administrator). If you need them, ask to be made a Manager on that project.
- *There's no "Users" link in my sidebar.* That screen is for global **Administrators** only.
- *A field I expected on the issue form isn't there.* Some fields (like **categories**, **versions**, and **custom fields**) exist only if a Manager has set them up for that project. An empty category dropdown just means none are defined yet.
- *The "private" or "sticky" flag isn't offered when I edit.* Setting those needs the right role. Lower-access users can see the effects but not toggle the flags.


## "The AI buttons aren't there"

- *There's no "✨ Suggest with AI" button on the new-issue form.* The **AI assistant** is optional and off unless an administrator has configured an AI provider. When it's off, OpenTrack simply hides the AI buttons rather than showing broken ones. See the *AI Assistant* chapter.
- *"Suggest with AI" says it couldn't suggest anything.* Give it a **Title** first (it works from the title and description), and try again. If it keeps failing, the configured AI provider may be unreachable — an administrator can check the provider settings.
- *AI suggested tags but they didn't get added.* That's by design: suggested tags are shown for you to add on the issue *after* you create it — they aren't applied automatically.


## Git, SLA, intake, and chat features look absent

- *My project shows nothing about Git commits.* **Git** integration is per-project and off until a Manager connects the project to its repository. See the *Integrations* chapter.
- *There's no SLA status on my issues.* **SLA** tracking only appears once a Manager has defined SLA targets for the project. Without a policy there's nothing to measure against, so the at-risk/breach flags won't show.
- *My public QR poster / report form doesn't work.* **Public intake** is a per-project switch. If it's off, the public form is closed. A Manager turns it on in project settings; then the QR poster opens the form. See *Public Trouble-Ticket Intake & QR Posters*.
- *Slack or Discord isn't getting notified.* Chat notifications go out through a **webhook** a Manager configures per project, shaped for Slack, Discord, or a generic JSON payload. If none is set up, nothing is sent. See *Integrations*.


## The desktop app and connections

- *The desktop app can't connect / shows a connection error.* It talks to your OpenTrack **server**, so the server has to be running and reachable from your machine, and the address in the app's settings has to be right. Confirm the server is up and that you can reach it in a normal browser first.
- *The page says it lost its connection and is trying to reconnect.* OpenTrack's live pages keep a running link to the server; a brief network hiccup shows a reconnect message and usually recovers on its own. If it doesn't, reload the page.
- *The installed (PWA) app shows stale information.* When offline it shows the last version it cached while online. Reconnect and reload for fresh data. See *Mobile, Tablets & the Field*.
- *Ctrl+K (the command palette) doesn't open.* Click once on the page so it has focus, then try again; on a Mac use **⌘K**. A few browsers grab Ctrl+K for the address bar — clicking into the page first hands the key to OpenTrack. See *Keyboard Shortcuts & Quick Navigation*.


## In the field, printing, and your data

- *Offline checklist taps show "• pending sync."* That's normal offline behavior — the change is safely queued and will send itself when you're back online or when you reopen the checklist page. Don't clear the browser's data in the meantime.
- *Turning a failed check into an issue did nothing offline.* That step needs a connection. OpenTrack tells you it's waiting until you're back online; your **Fail** mark is still recorded.
- *"Attach my location" was denied or is wrong.* Allow location for OpenTrack in your device settings, capture it while standing at the problem (outdoors if you can), then tap the button again. See *Mobile, Tablets & the Field*.
- *The print page still shows menus.* You're on the normal issue page. Open the issue's */print* address, then click **Print / Save as PDF** there.
- *There's no "Save as PDF" option when printing.* That choice comes from your browser and operating system, not OpenTrack — look for **Save as PDF** or **Microsoft Print to PDF** in the destination list.
- *My saved default sort didn't apply.* It only applies when you open the issue list without a sort already chosen. If the list address specifies an order, that wins. Open a fresh issue list. See *Printing, Preferences & Administration*.
- *An import didn't bring everything in.* Check that the file is in a format OpenTrack imports (such as CSV or a supported tracker export) and that it's well-formed. See *Importing & Exporting Your Data*.


## Frequently asked questions

- *What's the difference between severity and priority?* **Severity** is how *bad* the problem is; **priority** is how *soon* you'll act on it. A rare crash can be high severity but low priority. They're set independently.
- *What's the difference between status and resolution?* **Status** is where the issue is in its life (New → … → Closed). **Resolution** is *how* it ended (Fixed, Duplicate, Won't fix, and so on). An issue gets a resolution when it's closed out.
- *Do I need an account to report a problem?* Not if the project has **public intake** turned on — then anyone can file a report through the public form, often by scanning the project's **QR code**. Everything else in OpenTrack needs an account.
- *Can I use OpenTrack on my phone?* Yes. It's a Progressive Web App (PWA) — install it from the browser with no app store, and it works on phones and tablets, including some offline use. See *Mobile, Tablets & the Field*.
- *How do I keep a copy of my data?* Use **Backup & export** to download your data (CSV, JSON, and more). It's also how you'd move data elsewhere.
- *Someone left the team — should I delete their account?* Deactivate it instead. That blocks sign-in immediately while keeping the issues, notes, and history they contributed intact and correctly attributed. Only an administrator can do this, on the **Users** screen.
- *Who can turn on features like AI or Git?* Instance-wide things (like the AI provider or requiring HTTPS) are for an **Administrator**; per-project things (Git, SLA targets, intake, webhooks) are for a project **Manager**.
- *I still can't explain what I'm seeing.* Note the exact page, what you did, and what happened, then ask a project **Manager** or your **Administrator** — they can see your role and which features are enabled, which is usually the whole answer.
