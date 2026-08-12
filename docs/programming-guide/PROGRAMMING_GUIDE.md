# OpenTrack Programming Guide

*How and why the code works — a maintainer's field guide, in plain language.*

*Generated August 12, 2026 · Markdown is the living source of truth.*


---


# 1. What OpenTrack Is, and How to Read This Book

*A plain introduction to the program this book explains — a self-hosted issue tracker written in C# on .NET 10 and licensed under the AGPL v3 — plus who the book is for, the What-then-Why-then-How rhythm every chapter follows, and the rules that keep the book trustworthy as the code keeps changing.*


## What This Is / What It Is For

*OpenTrack* is a bug and issue tracker: a shared place where a team writes down the things that are broken, the things that need doing, and the state of each one as it moves from reported to fixed. If you have ever used a to-do list where every item has an owner, a priority, and a history of who changed what — that is the shape of it. OpenTrack's own README states the goal plainly: it aims for "full feature parity" with MantisBT, a long-established tracker, "rebuilt on a modern .NET stack."

The word that matters most in the description is *self-hosted*. OpenTrack is not a service you rent from someone else's servers; it is a program you run on your own machine — a small office server, a Raspberry Pi, a spare mini-PC on the local network. You hold the database, you hold the data, and nobody else does. The README puts the promise this way: it is "yours to run and change — self-hosted, no vendor lock-in." This book is the guided tour of how that program is built on the inside, and — just as important — *why* it is built the way it is.

> **The one-sentence version** — OpenTrack is a self-hosted issue tracker written entirely in C# on .NET 10, delivered as a web app, a matching desktop app, and a network API — all sharing one codebase and one database — and released under the AGPL v3, a license that keeps every improvement open even when the program is run as a network service.


### What it is built on, and why that choice

OpenTrack is written in *C#* (the programming language) running on *.NET 10* (the platform that executes it). That single choice is deliberate and it echoes through the whole design. Because C# runs the browser screens (through a framework called *Blazor*), the behind-the-scenes service, and the desktop app alike, there is only one language to learn and one body of code to maintain. The README calls this "one language, top to bottom — C# for the backend, the API, and the UI (via Blazor). No separate JavaScript framework to maintain."

> **Jargon, in plain words** — A framework is a ready-made skeleton of code you build on top of, so you write only the parts unique to your program. Blazor is Microsoft's framework for building web-page screens in C# instead of JavaScript. The UI (user interface) is everything you see and click. An API (application programming interface) is a program's service door — a way for other programs, like the desktop app, to ask it to do things over a network. A database is the organized store where all the issues, projects, and users are kept on disk.

The pieces are all current, long-term-support versions, which the shared build file pins on purpose. Every project in the solution inherits the same settings from one file, `Directory.Build.props`, including that they all target .NET 10 and turn on the compiler's safety features:

```csharp
<PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TargetFramework Condition="'$(TargetFramework)' == ''">net10.0</TargetFramework>
</PropertyGroup>
```

That `<Nullable>enable</Nullable>` line is a small thing with a large payoff: it tells the C# compiler to warn whenever a value might be missing (a "null") where the code expects something real. It is a seatbelt for a whole category of crash, switched on for every project at once because it is set in the one shared file rather than repeated (and eventually forgotten) in each. You will see this pattern — decide once, in one place, apply everywhere — again and again; it is arguably the central habit of the entire codebase.


### What the license means: AGPL v3, from the real header

OpenTrack is released under the *GNU Affero General Public License, version 3* — AGPL v3 for short. The shared metadata file declares it (`<PackageLicenseExpression>AGPL-3.0-or-later</PackageLicenseExpression>`), and every single C# source file opens with the same ten-line notice so the terms travel with the code no matter where a file ends up:

```csharp
// OpenTrack — open-source issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version. This program is distributed WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License <https://www.gnu.org/licenses/> for
// more details.
```

> **Jargon, in plain words** — "Open source" means the source code — the human-readable instructions the program is made of — is published for anyone to read, use, and change. "Copyleft" is a stricter promise on top of that: anyone who passes the program on must pass the source along too, under the same terms, so it can never be quietly closed up again. The AGPL adds one extra clause aimed at network software specifically, covered next.

In everyday terms the license grants a lot and asks one thing in return. You are free to run OpenTrack, read its code, change it, and give your changes to others. The catch that makes the AGPL different from ordinary open-source licenses is the *A*, for Affero: if you run a modified version as a service that other people reach over a network, you must offer them your modified source too. The README states it directly: "if you distribute a modified version *or run one as a network service*, you must make your source code available under the same license." For a self-hosted tracker — a program whose whole point is to be reached over a network — that clause is the one that keeps the project and its improvements open for everyone.

> **Why this matters to a maintainer** — If you fork OpenTrack, add features, and stand it up for a team to use over the network, the AGPL asks you to make your version's source available to those users. Keeping the copyright header intact at the top of every file — and not stripping it when you copy code between files — is the simplest way to stay on the right side of the license. The header is load-bearing, not decoration.


### Who this book is for

This book is written for three kinds of reader at once, and the outline says so up front: "developers, future maintainers, and the curious — a non-programmer should be able to follow the story, while a programmer gets enough depth to work confidently." Those goals only sound like they are in tension. The way the book holds them together is that the *story* of each subsystem — what it does and why it was built that way — is told in plain language anyone can follow, while the *proof* of each claim is a real excerpt from the actual code, sitting right there for a programmer to check and build on.

So if you do not write code, read the prose and the analogies and skim past the code blocks; you will still come away understanding how OpenTrack works and why its author made the calls they did. If you do write code, the same chapters give you exact class names, file paths, and quoted source, so you can open the files yourself and start working. Nobody is asked to read a different book — the plain-language layer and the deep layer are stacked in the same pages.

There is a deeper purpose underneath all of it, stated in the outline's goal: "a complete picture of how and why the code was written, so the project can live on after the author." A working program is only half of what a lone author leaves behind; the other half is the reasoning — the dead ends avoided, the trade-offs accepted, the rules that must not be broken. Code shows the *what*. This book is where the *why* is written down so it is not lost.


### How to read this book: What, then Why, then How

Every chapter follows the same three-beat rhythm, and knowing it in advance makes the whole book easier to navigate. The outline fixes the standard: each section answers "*What it does -> Why it was built this way -> How it works*." The pattern is not decoration; it is the promise that you will never be shown a clever mechanism without first being told what problem it solves and why the obvious alternative was passed over.

- *What it does* — the plain-language purpose of the piece, before any code. If you only read these, you get an accurate mental map of the whole system.
- *Why it was built this way* — the reasoning and the trade-off. This is the part that usually gets lost when only code survives, and the part this book most wants to preserve.
- *How it works* — the real mechanism, grounded in exact excerpts copied from the source files, so the explanation can be checked against the truth and never drifts into fiction.

Two more conventions run throughout. Chapters open with a "*What This Is / What It Is For*" section and close with "*Why It Matters / Design Takeaways*," so you always know where you are: the opening tells you what you are about to learn, and the closing distills the handful of ideas worth carrying away — usually ending in a *maintainer's rule*, a single sentence naming the thing a future editor must not break. And the first time a technical word appears, a callout labeled "*Jargon, in plain words*" defines it in everyday terms, the way this chapter has already done for framework, database, and copyleft.

> **The callout boxes, decoded** — Colored boxes flag things worth pausing on. A note explains or defines. A tip offers a practical shortcut or a reason something helps. An important box marks a load-bearing idea you should not skim past. A warning marks a place where a careless change causes real damage. When you see one, slow down — it is there because the sentence matters more than the ones around it.


### How the book is maintained

A book about living code goes stale unless it is maintained with discipline, so OpenTrack's guide borrows the same rules its documentation uses everywhere. There are three, and they work together.

First, *Markdown is the living source of truth, and the Word document is generated from it*. The chapters are written as structured data files (the very `chapters/*.json` files this book is built from); a build script turns them into a readable Markdown document and, from the same source, into a styled Word `.docx` for people who prefer it. The outline states the pipeline directly: "`chapters/*.json` -> `PROGRAMMING_GUIDE.md`" plus "a styled Word `.docx` ... generated from the same chapter JSON." You never hand-edit the Word file, because it is an output; you edit the source, and both documents are regenerated. That way the two can never disagree — exactly the same anti-drift instinct the code itself is built on.

> **Jargon, in plain words** — Markdown is a plain-text way of writing formatted documents — a few simple marks for headings, bold, and lists — that stays readable as raw text and lives comfortably inside a code repository. "Source of truth" means the one authoritative copy that every other copy is generated from; when they differ, it wins. A .docx is a Microsoft Word file.

Second, *section numbers are stable, and improvements are added rather than renumbered*. Chapter 8 is always chapter 8. When the code grows a new subsystem or an old explanation needs correcting, the book does not shuffle every number down the line — which would break every cross-reference ever written. Instead it uses *dated amendments*. The outline spells out the mechanism: "Stable section numbers; dated amendments (`AMENDS §X`, `ADDS §Z`) with an Amendments Register — improvements are added, never renumbered."

- *AMENDS §X* — a dated note that revises or corrects what an existing section said, without moving it. The original number stays put; the amendment rides alongside it.
- *ADDS §Z* — a dated note introducing new material as a new section, appended rather than inserted, so nothing after it shifts.
- *The Amendments Register* — a running list of every AMENDS and ADDS, so the history of the book's own changes is visible in one place, the same way an issue's edit history is visible in the tracker.

Third, and underlying both, the book is kept *in the repository, beside the code it describes*. It is not a wiki on some other server that slowly falls behind; it lives in the same version-controlled home as the source, so a change to the code and the change to its explanation can travel together. The final, dedicated chapter of the book (§24) is about this maintenance discipline itself — a book that takes its own longevity seriously enough to document how it stays alive.


## Why It Matters / Design Takeaways

Everything in this opening chapter is really one idea seen from several angles: OpenTrack is built to *outlast any single person's memory of it*. The one-language stack keeps the whole thing learnable. The AGPL keeps it and its improvements open. The book keeps the reasoning — the why behind the what — from evaporating when the author moves on. Even the small choices, like the shared build file and the copyright header on every file, are bets that a decision made once and applied everywhere will survive better than a decision scattered and repeated.

As you read on, hold onto the three-beat rhythm — What, then Why, then How — because the rest of the book earns its keep in the middle beat. Plenty of documents can tell you what code does; you can often read that from the code itself. The rare and valuable thing, the thing worth preserving, is why it was done that way and what would break if you changed it. That is the promise this book is trying to keep on every page.

> **The maintainer's rule** — Treat the Markdown chapter sources as the one true copy: edit them, never the generated Word file, and regenerate both. When you improve a section, AMEND or ADD with a date and log it in the Register — never renumber. Keep the AGPL header at the top of every source file intact. The whole point of this book is to preserve the reasoning behind the code; a maintenance habit that lets the explanation drift from the code defeats it.


# 2. The Big Picture: Architecture at 10,000 Feet

*How three different front doors — a browser web app, a network API, and a native desktop app — are really one program: they share the same screens, talk to the same single database, and reach it through one narrow interface, so a feature written once appears everywhere and the three can never quietly disagree.*


## What This Is / What It Is For

*OpenTrack* can be reached three different ways. You can open it in a web browser. A separate service can be called over the network by other programs. And there is a native desktop app for Windows and macOS. Three front doors — but behind them there is only one building. The same rooms, the same filing cabinet, the same rules. This chapter is the view from ten thousand feet: how three surfaces that look different to a user are, underneath, one program sharing one set of screens and one database.

Picture a bank with a lobby, a drive-through window, and a phone line. A customer experiences three very different things, but there is one vault, one ledger, and one set of policies behind all of them. Nobody would build three separate ledgers and hope they stay in sync — the day they drift, money goes missing. OpenTrack is built on exactly that instinct: however you reach it, you are reaching the same data through the same rules. The whole architecture exists to make three front doors impossible to pull apart into three diverging programs.

> **The one-sentence version** — OpenTrack has three hosts — a Blazor Server web app, an ASP.NET Core minimal API, and a MAUI Blazor Hybrid desktop app — that all run the same shared UI components against one SQLite database, reached through one interface (IOpenTrackDataService); the web app fulfills that interface by touching the database directly, and the desktop app fulfills it by calling the API over HTTP.

> **Jargon, in plain words** — A host is a runnable program — a thing you can actually start. An interface (in C#) is a named list of operations with no implementation attached: a contract that says "whoever fills this in must provide these methods." HTTP is the ordinary language of the web — the same request-and-response one browser page uses, here spoken between the desktop app and the API. SQLite is a database that lives in a single ordinary file rather than needing its own separate server program.


### The three hosts, and what each is for

Each host is a real project in the solution with its own entry point, and each exists to answer a different need.

| Host | What it is | Who reaches it, and why |
| --- | --- | --- |
| OpenTrack.Web | A Blazor Server web app — the screens run on the server and stream to the browser | Anyone with a browser on the network; the always-on primary surface. It also owns background jobs (SLA scanning, backups) and public/webhook endpoints. |
| OpenTrack.API | An ASP.NET Core minimal API — no screens, just a service door returning data | Other programs — chiefly the desktop app — that need OpenTrack's data over the network with a token instead of a login cookie. |
| OpenTrack.Desktop | A .NET MAUI Blazor Hybrid app — a native Windows/macOS window hosting the same web screens | A user who wants an installed desktop app; it renders the shared UI locally and gets its data by calling the API. |

The key thing to notice is that only *one* of these three actually contains the screens and the database logic. The web app has the screens and touches the database. The API has the database logic but no screens. The desktop app has the screens but no database logic — it borrows the screens from the shared UI and gets its data from the API. Nothing important is written three times. That is the payoff the rest of the chapter unpacks.


### The unifying idea: one database, one UI, one seam between them

Three ingredients make the three hosts into one program. First, they all read and write *one shared database* — a single SQLite file. Second, the web app and the desktop app both render *one shared set of screens*, the components living in the `OpenTrack.UI` project. Third — and this is the hinge everything turns on — those shared screens never talk to the database directly. They talk to a single narrow interface called `IOpenTrackDataService`, and each host plugs its own answer in behind that interface.

> **Jargon, in plain words** — A "seam" is a deliberate joint in a program — a clean line where two parts meet through a defined contract, so either side can be swapped without disturbing the other. IOpenTrackDataService is OpenTrack's main seam: the screens sit on one side, the data-fetching sits on the other, and the interface is the joint they meet at. "DI" (dependency injection) is the machinery that, at startup, decides which real implementation gets plugged into each seam.

The interface's own summary comment states the design in one paragraph — worth reading slowly because the whole architecture is compressed into it:

```csharp
/// <summary>
/// The single data-access seam for OpenTrack's shared Blazor UI. The web app implements
/// this with direct EF Core access (DbOpenTrackDataService); the desktop app implements it
/// by calling OpenTrack.API over HTTP (HttpOpenTrackDataService). The CRUD pages depend
/// only on this interface, so the exact same components run in both hosts.
/// </summary>
public interface IOpenTrackDataService
{
    // Projects
    Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default);
    Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default);
    Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default);
    // ... many more operations, all shaped the same way ...
}
```

Read the promise in the last sentence of that comment: "the CRUD pages depend *only on this interface*, so the exact same components run in both hosts." A screen that lists issues does not know, and is not allowed to know, whether the issues came from a database on the same machine or from an API across the network. It asks the interface for issues and gets issues. That ignorance is a feature — it is exactly what lets one screen serve two hosts.

> **Jargon, in plain words** — CRUD is shorthand for Create, Read, Update, Delete — the four basic things you do to any record. "CRUD pages" are the ordinary screens for listing, viewing, adding, and editing issues and projects. A CancellationToken (the ct on every method) is a signal that lets a slow operation be called off early — for example when the user navigates away — so nothing keeps running that nobody is waiting for.


### Two implementations behind one doorway

The interface is a contract; something has to fulfill it. Two things do, one per host that shows screens, and they could hardly be more different inside — which is the whole point.

On the web, the fulfiller is `DbOpenTrackDataService`, and it reaches straight into the database with Entity Framework Core. On the desktop, the fulfiller is `HttpOpenTrackDataService`, and it reaches nothing locally — it turns each call into a web request to the API. Its summary comment describes just how thin it is: it "calls OpenTrack.API over HTTP (thin client). The API's DTOs are shaped to match the UI's view models, so most calls are near-passthrough deserialization." A typical method is a single line — ask the API, hand back what comes:

```csharp
public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default) =>
    await http.GetFromJsonAsync<List<ProjectRow>>("/api/projects", JsonOptions, ct) ?? [];
```

Which implementation a host uses is decided once, at startup, by dependency injection — a single registration line per host. The web host wires in the database-backed one:

```csharp
// OpenTrack.Web/Program.cs
// The shared UI's data seam, backed by direct EF Core access in the web app.
builder.Services.AddScoped<OpenTrack.UI.Services.IOpenTrackDataService,
    OpenTrack.Web.Services.DbOpenTrackDataService>();
```

```csharp
// OpenTrack.Desktop/MauiProgram.cs
// The shared UI's data seam, backed by HTTP calls to OpenTrack.API in the desktop app.
builder.Services.AddScoped<IOpenTrackDataService>(sp =>
    new HttpOpenTrackDataService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("OpenTrackApi")));
```

Two lines, in two files, are the entire difference between "talk to the database" and "talk over the network." Everything above the seam — every screen, every button, every list — is identical. This is the single most important structural fact about OpenTrack: the surface you see is shared, and only the plumbing underneath the seam changes per host.


### How a request travels, end to end

Trace one action — a user opening the list of projects — through each host, and the shapes become concrete.

*On the web app.* The path is short because the screen and the database live in the same process:

1. The shared Blazor screen (running on the server) calls `GetProjectsAsync` on `IOpenTrackDataService`.
2. Dependency injection has plugged in `DbOpenTrackDataService`, so that call lands there.
3. It opens a short-lived Entity Framework Core context, queries the SQLite database — filtered to what the signed-in user is allowed to see — and returns the rows.
4. Blazor Server streams the rendered list back down to the browser.

*On the desktop app.* The same screen, the same interface call — but the request now leaves the machine and comes back:

1. The identical shared Blazor screen calls the identical `GetProjectsAsync` on `IOpenTrackDataService`.
2. Here dependency injection has plugged in `HttpOpenTrackDataService`, so the call becomes an HTTP GET to `/api/projects` on the API server, with the user's bearer token attached automatically.
3. The API host receives the request, runs the very same database logic the web app would have run, and returns the projects as JSON.
4. `HttpOpenTrackDataService` deserializes that JSON straight into the same `ProjectRow` objects the screen expected, and the screen renders — none the wiser about the round trip.

Both journeys start and end in the same place with the same objects. The middle differs — one stays home, one crosses the network — but the screen at the top and the database logic at the bottom are shared. That is why a feature added to a screen shows up on web and desktop at once, and why a permission enforced in the database logic protects both surfaces without being written twice.


### One database, and the care taken to keep it one

The shared database is a single SQLite file. The choice pays off only if every host truly opens the *same* file — and there is a subtle way to get that wrong, which the code goes out of its way to prevent. All hosts resolve their connection string through one shared helper, `ResolveOpenTrackConnectionString`, whose comment names the exact trap it is closing:

```csharp
// OpenTrack.Infrastructure/DependencyInjection.cs
// ... every host falls back to the SAME absolute path — a single opentrack.db in a
// shared per-machine data folder — rather than each resolving "opentrack.db" relative
// to its own launch directory, which would silently create separate databases for the
// web app and the API.
var dataDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "OpenTrack");
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "opentrack.db");
return $"Data Source={dbPath};Cache=Shared";
```

The danger is quiet, not loud: if the web app opened `opentrack.db` in its own folder and the API opened `opentrack.db` in a different folder, nothing would crash — each would happily create its own file, and the two hosts would drift into two separate realities. The fix is to make every host compute one absolute path in one shared per-machine location, so "the same database" is guaranteed by construction rather than by luck. Both `OpenTrack.Web` and `OpenTrack.API` call this one helper, and a comment in the API's startup underlines the intent: its data layer uses the "shared SQLite database with OpenTrack.Web."

> **The recurring instinct: decide once, share the decision** — You have now seen the same reflex three times — one shared build file for compiler settings, one interface for data access, one helper for the database path. Each replaces something that could be written separately per host (and eventually diverge) with something written once and shared. When two hosts must agree about something, OpenTrack's answer is almost always to give them one thing to agree on rather than two things to keep in sync.


## Why It Matters / Design Takeaways

The architecture's whole ambition is to make three front doors behave as one program without ever being written three times. It gets there with three shared things: one database that all hosts open by the same absolute path; one set of UI screens that both display-hosts render; and one interface, `IOpenTrackDataService`, that the screens depend on so completely that they cannot tell a local database from a remote API. Behind that interface sit two very different implementations — direct EF Core and HTTP — but their difference is confined to two startup lines and never leaks upward.

If you are extending OpenTrack, the shape tells you exactly where new work goes. A new kind of data the screens need becomes a new method on the interface, implemented once against the database (for the web) and once as an API call (for the desktop) — and then every screen, on both hosts, can use it. You do not add a screen that reaches around the seam to the database, and you do not teach one host a data operation the other cannot perform, because that is precisely the drift the architecture was built to prevent.

> **The maintainer's rule** — Keep the shared screens talking only to IOpenTrackDataService — never to the database or the network directly. Every new data operation is a new method on that interface with both implementations supplied, so web and desktop stay in lockstep. And whenever two hosts must agree on something, give them one shared thing to depend on (one interface, one helper, one file) rather than two copies to keep synchronized. One program wearing three faces — never three programs.


# 3. The Solution Layout: Six Projects & Their Boundaries

*Why one program is split into six projects instead of one big pile of files — what each project holds, and the one rule that keeps the split meaningful: dependencies point inward toward a pure, dependency-free core, so the compiler itself refuses to let the layers tangle.*


## What This Is / What It Is For

A program the size of *OpenTrack* could, in principle, be one enormous project with every file thrown in together. It is not. It is deliberately split into six separate projects, each with a job, plus three test projects. This chapter is the map: what lives in each project, and — the part that actually matters — the rules about which project is allowed to lean on which. Those rules are the difference between a codebase that stays understandable for years and one that turns into a knot nobody dares touch.

Think of it as a well-run kitchen. The pantry holds raw ingredients and knows nothing about any particular dish. The prep station takes ingredients and does the real work. The dining room plates and serves. Each station depends on the ones deeper in the kitchen, never the other way around — the pantry does not need to know what is on tonight's menu. OpenTrack's six projects are arranged exactly like that: the deepest one knows nothing about the others, and the ones near the surface lean inward on the deeper ones.

> **The one-sentence version** — OpenTrack is six projects arranged in layers — a pure Core at the bottom that depends on nothing, Infrastructure and UI in the middle, and three runnable hosts (Web, API, Desktop) on top — with all dependency arrows pointing inward toward Core, a rule the compiler enforces through the ProjectReference lines so the layers physically cannot tangle.

> **Jargon, in plain words** — A project (a .csproj file) is one buildable unit of code that produces one output — a library other projects use, or a runnable program. A "dependency" means one project needs another to compile; in C# that need is declared by a ProjectReference line inside the .csproj. A "layer" is a project's rank in the stack: deeper layers are more general and know less about the outside world; higher layers are more specific and pull the deeper ones together.


### Why split it up at all

The reason to split is not tidiness for its own sake — it is to make certain mistakes *impossible instead of merely discouraged*. If everything lived in one project, any file could reach any other file. Nothing would stop a screen from reaching straight into the database, or the core business rules from accidentally depending on the web framework. You could write a comment asking people not to, but comments do not stop anyone. Separate projects with controlled references turn those requests into laws the compiler enforces: if Core is not allowed to know about the web, then code in Core that tries to use the web simply will not build.

OpenTrack's solution file lists exactly six source projects and three test projects, grouped into two folders:

```csharp
<Solution>
  <Folder Name="/src/">
    <Project Path="src/OpenTrack.API/OpenTrack.API.csproj" />
    <Project Path="src/OpenTrack.Core/OpenTrack.Core.csproj" />
    <Project Path="src/OpenTrack.Desktop/OpenTrack.Desktop.csproj" />
    <Project Path="src/OpenTrack.Infrastructure/OpenTrack.Infrastructure.csproj" />
    <Project Path="src/OpenTrack.UI/OpenTrack.UI.csproj" />
    <Project Path="src/OpenTrack.Web/OpenTrack.Web.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/OpenTrack.API.Tests/OpenTrack.API.Tests.csproj" />
    <Project Path="tests/OpenTrack.Core.Tests/OpenTrack.Core.Tests.csproj" />
    <Project Path="tests/OpenTrack.Web.Tests/OpenTrack.Web.Tests.csproj" />
  </Folder>
</Solution>
```


### The six projects, from the bottom up

Read them from the deepest layer upward — the same way the dependencies point — and each one's job follows naturally from the layer below it.

| Project | Layer | What it holds |
| --- | --- | --- |
| OpenTrack.Core | The pure core (bottom) | The domain itself: entities (Issue, Note, Project, User, ProjectMembership, ...), the fixed menus (Enums like UserRole and IssueStatus), and pure rules — Authorization, Sla math, Validation, Querying types. No database, no web. |
| OpenTrack.Infrastructure | Data & services (middle) | Everything that talks to the outside world: the EF Core database (Data), Migrations, plus the per-feature operations (Issues, Sla, Automation, Notifications, Webhooks, Git, Queries, and more). Depends on Core. |
| OpenTrack.UI | Shared screens (middle) | The Blazor components and pages both display-hosts render, and the IOpenTrackDataService seam they depend on. Pages, Shared, Services. Depends only on Core. |
| OpenTrack.Web | Host (top) | The Blazor Server web app: Program.cs, Components, Endpoints, and the DbOpenTrackDataService that fulfills the seam with direct database access. |
| OpenTrack.API | Host (top) | The minimal API for the desktop app: Program.cs, Endpoints, Contracts (the JSON shapes), and the API-side access helpers. |
| OpenTrack.Desktop | Host (top) | The MAUI Blazor Hybrid desktop app: MauiProgram.cs, its Services (HttpOpenTrackDataService, auth/token handling), and platform glue. |

Notice how each project's contents match its rank. Core holds ideas that are true regardless of how OpenTrack is run — what an issue *is*, who is allowed to see one — and so it holds no database and no web code. The hosts at the top hold the opposite: nothing general, just the specific wiring that turns the shared pieces into one runnable program. Infrastructure and UI sit between, each serving a different kind of consumer — Infrastructure serves the two data-touching hosts, UI serves the two screen-showing hosts.


### The dependency arrows: which way they point

The rule that makes the layers real is the direction of the arrows: *every reference points inward, toward Core, and nothing points back out*. You can read the entire dependency graph straight off the `ProjectReference` lines in the six `.csproj` files. Start at the bottom.

*Core references nothing internal at all.* Its `.csproj` pulls in one small identity package and stops; there is not a single `ProjectReference` to another OpenTrack project. That emptiness is the most important fact in the whole layout, and the next section is about why. *Infrastructure references only Core:*

```csharp
<!-- OpenTrack.Infrastructure.csproj -->
<ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
```

*UI also references only Core* — notably *not* Infrastructure. The shared screens depend on the domain types and the interface, but they are kept ignorant of the database entirely; that ignorance is what lets the desktop app render them without ever touching EF Core:

```csharp
<!-- OpenTrack.UI.csproj -->
<ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
```

Then the three hosts on top each reference exactly what they need and nothing more. The web host, which shows screens *and* touches the database, references all three lower projects:

```csharp
<!-- OpenTrack.Web.csproj -->
<ProjectReference Include="..\OpenTrack.Infrastructure\OpenTrack.Infrastructure.csproj" />
<ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
<ProjectReference Include="..\OpenTrack.UI\OpenTrack.UI.csproj" />
```

The API, which touches the database but shows no screens, references Core and Infrastructure but *not* UI. The desktop app, which shows screens but never touches the database directly, references UI and Core but *not* Infrastructure:

```csharp
<!-- OpenTrack.API.csproj -->
<ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
<ProjectReference Include="..\OpenTrack.Infrastructure\OpenTrack.Infrastructure.csproj" />

<!-- OpenTrack.Desktop.csproj -->
<ProjectReference Include="..\OpenTrack.UI\OpenTrack.UI.csproj" />
<ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
```

The absences say as much as the presences. The API does not reference the UI (it has no screens to show). The desktop does not reference Infrastructure (it has no database to touch — it goes through the API instead). A comment in the desktop's own startup makes this deliberate poverty explicit: it registers its role policies inline precisely because "desktop is a thin client and doesn't reference Infrastructure." Each host carries the minimum, and the minimum is a design statement about what that host is allowed to do.

> **The arrows, drawn out** — Infrastructure -> Core. UI -> Core. Web -> Core, Infrastructure, UI. API -> Core, Infrastructure. Desktop -> Core, UI. Every arrow points down toward Core; not one points up or sideways in a way that would let a lower layer depend on a higher one. Core is the hub everything reaches for and that reaches for nothing.


### The pure core at the bottom, and why it is the linchpin

The reason all arrows aim at Core is that Core is *pure*: it has no dependency on a database, a web framework, or a network, so anything can safely depend on it without dragging that machinery along. This is the same purity the calibration chapter (§8) celebrated in `AccessContext` — the permission rules live in Core precisely so both the web app and the API can call the identical rules and never drift apart. Purity at the bottom is what makes sharing at the top possible.

Purity buys three concrete things. It makes Core *trivially testable* — you can exercise its rules with no server and no database running, which is exactly what the Core test project does. It makes Core *shareable* — because it drags nothing heavy behind it, both a full web server and a thin desktop client can reference it freely. And it makes Core *stable* — the deepest layer changes least, so the thing everything else depends on is the thing least likely to shift underfoot. Keep the bottom pure and the whole tower stays sound.

> **The one move that would rot the layout** — The fastest way to wreck this design is to add a database or web dependency to OpenTrack.Core — a stray ProjectReference to Infrastructure, or an EF Core package. The instant Core depends on the database, it stops being pure: its rules can no longer be tested without a database, and the clean inward-pointing graph gains its first outward arrow. If you ever feel tempted to reach from Core into Infrastructure, the answer is to pass the fact in as a parameter instead — the same trick §8's AccessContext uses to stay pure.


### Why these boundaries are laws, not suggestions

Here is the quiet strength of splitting into projects: the boundaries are enforced by the compiler, not by anyone's good intentions. Because `OpenTrack.UI` does not reference `OpenTrack.Infrastructure`, a screen physically *cannot* call the database — the types are not even visible to it; the code would fail to compile. Because `OpenTrack.Core` references no other project, nothing in Core can accidentally reach the web. You do not need a reviewer to catch these mistakes, because the mistakes cannot be written down and built in the first place.

That is the difference between architecture as a wish and architecture as a fact. A single-project version of OpenTrack could hold the exact same files and the exact same intentions, and it would slowly erode the first time someone in a hurry reached across a boundary that nothing was stopping them from crossing. The six-project split makes the intended shape the *only* shape that compiles. The `ProjectReference` lines are the load-bearing walls.


### The three test projects

Alongside the six source projects sit three test projects, and the choice of which three is itself informative. Each references exactly the one source project it exercises:

| Test project | References | What it guards |
| --- | --- | --- |
| OpenTrack.Core.Tests | OpenTrack.Core | The pure rules — permissions, SLA math, validation — proven with no database or server, thanks to Core's purity. |
| OpenTrack.API.Tests | OpenTrack.API | The API host: its endpoints and the access checks on the network path the desktop app relies on. |
| OpenTrack.Web.Tests | OpenTrack.Web | The web host: its endpoints and services, including the database-backed data service. |

That the tests concentrate on Core, API, and Web is no accident. Core is where the rules live, so it earns the most direct scrutiny. Web and API are the two hosts that touch the database and enforce access, so each gets its own suite. The shared UI and the thin desktop client, by design, add little logic of their own — they render and forward — so the meaningful behavior they might otherwise be tested for is already tested where it actually lives, one layer down. The test layout mirrors the source layout: guard the depth where the logic is, not the surface where it is merely displayed.


## Why It Matters / Design Takeaways

The six-project split is not bureaucracy; it is the mechanism that makes every promise in the previous chapter enforceable. One shared UI can serve two hosts only because UI depends on Core and not on any host. Two hosts can share one set of rules only because those rules live in a pure Core that both can reference without conflict. And none of it can quietly erode, because the boundaries are `ProjectReference` lines the compiler obeys — the intended architecture is the only one that builds.

When you extend OpenTrack, let the layers tell you where your code belongs. A rule that is true no matter how the app runs goes in Core, pure. Something that talks to the database or an outside service goes in Infrastructure. A screen goes in UI. Wiring that turns the shared pieces into a runnable program goes in a host. If a piece of code seems to want to live in two layers at once, that is usually a sign it is really two pieces — split it along the seam rather than smearing a dependency across a boundary.

> **The maintainer's rule** — Keep every dependency arrow pointing inward toward Core, and keep Core pure — no database, no web, no network references, ever. Before adding a ProjectReference, ask whether it points the right way; a reference that makes a lower layer depend on a higher one, or that gives Core a database, is the seam beginning to fail. The layout only protects you for as long as those arrows all point home.


# 4. Issues as Data: the Entity Model

*The handful of plain C# classes that describe everything OpenTrack remembers — the issue at the center, the project and people around it, and the notes, attachments, tags, relationships, versions, and custom fields that hang off it — and the deliberate way they point at one another.*


## What This Is / What It Is For

*OpenTrack* is an issue tracker — a shared list of problems, bugs, and tasks that a team works through. Everything the program remembers has to be written down somewhere in a definite shape: an issue has a title and a status, it belongs to a project, it was filed by a person, it might have notes and file attachments. This chapter is about those shapes. In the code they are called *entities*, and they are the vocabulary the rest of the book is written in.

Think of the entity model as the set of blank forms the whole system uses. There is a form for an issue, a form for a project, a form for a person, a form for a note, and so on. Each form has named boxes to fill in, and some boxes say "see form number 12" — that is how an issue points at the project it belongs to. Get these forms right and everything above them — the screens, the rules, the reports — has solid ground to stand on. Get them wrong and no amount of clever code above will save you. That is why they live in the deepest, simplest project in the whole solution, `OpenTrack.Core`, with no database code and no web code anywhere near them.

> **Jargon, in plain words** — An entity is one kind of thing the app stores, written as a C# class — a named bundle of fields. Each entity usually becomes one table in the database, and each stored item becomes one row. A property is one named box on the form (Title, Status, CreatedAt). A foreign key is a box that holds the id number of a row in another table — the way one form references another. A navigation property is the same link seen from the code's side: instead of an id number, it hands you the whole related object.

> **The one-sentence version** — OpenTrack stores its whole world as a small set of plain C# classes — Issue at the center, surrounded by Project, User, and the membership that joins them, plus the notes, attachments, tags, relationships, versions, and custom fields that decorate an issue — and every link between them is just an id number in one direction and a convenience object in the other.


### The Issue: the center of gravity

Nearly everything in OpenTrack exists to describe, organize, or attach to an *issue*. So the `Issue` class is the largest and most important form in the system. Read it top to bottom and you can see the life of a bug: what it is, how bad it is, who owns it, which version it affects, and when things happened.

The first boxes are the human description — the words a person types:

```csharp
public int Id { get; set; }

public string Title { get; set; } = string.Empty;
public string Description { get; set; } = string.Empty;
public string? StepsToReproduce { get; set; }
public string? ExpectedBehavior { get; set; }
public string? ActualBehavior { get; set; }
```

Notice a small but deliberate distinction hiding in the punctuation. `Title` and `Description` are `string` and start out as `string.Empty` — the design says every issue always has a title and a description, even if only an empty one; they are never "missing." But `StepsToReproduce`, `ExpectedBehavior`, and `ActualBehavior` are written `string?` with a question mark, which means they are allowed to be genuinely absent. That is the whole grammar of nullability: a plain type is a promise the value is always there; a `?` is permission for it to be nothing.

> **Jargon, in plain words** — 'Nullable' — the `?` after a type — means the box is allowed to be empty (hold 'nothing', which in C# is called null). Without the `?`, the code is declaring the box should always hold a real value. This isn't decoration: it tells every screen and rule above whether it must handle the 'no value' case. `Id` is the row's unique number, assigned by the database; a brand-new, unsaved issue has Id 0.

Next come the five classifications — the fixed-menu fields that let the team sort and prioritize. Each is an *enum* (a fixed menu of named choices, covered in full in the next chapter), and each is given a sensible starting value so a half-filled new issue is never in a nonsense state:

```csharp
public IssueStatus Status { get; set; } = IssueStatus.New;
public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;
public IssuePriority Priority { get; set; } = IssuePriority.Normal;
public IssueReproducibility Reproducibility { get; set; } = IssueReproducibility.HaveNotTried;
public IssueResolution Resolution { get; set; } = IssueResolution.Open;

public bool IsPrivate { get; set; }
public bool IsSticky { get; set; }
```

The two `bool` flags below them are quietly important. `IsPrivate` is the single switch that the entire access-control system reads to decide whether an issue is hidden from most people (it is the star of §8). `IsSticky` marks an issue as pinned to the top of a list. A `bool` defaults to `false`, so an issue is public and un-pinned unless someone deliberately says otherwise — a safe default in both cases.


### How an Issue points at everything around it

An issue does not live alone. It belongs to a project, sits in a category, was filed by one person and maybe assigned to another, and may name the versions it affects and fixes. Every one of those links is written the same way — a paired *foreign key and navigation property* — and the pattern is worth learning once because it repeats across every entity in the system.

```csharp
// Project / categorisation
public int ProjectId { get; set; }
public Project Project { get; set; } = null!;
public int? CategoryId { get; set; }
public Category? Category { get; set; }

// People
public int ReporterId { get; set; }
public User Reporter { get; set; } = null!;
public int? AssigneeId { get; set; }
public User? Assignee { get; set; }

// Versions
public int? AffectsVersionId { get; set; }
public ProjectVersion? AffectsVersion { get; set; }
public int? FixVersionId { get; set; }
public ProjectVersion? FixVersion { get; set; }
```

Read the pairs and the design decisions are visible in the nullability. `ProjectId` is a plain `int` and `ReporterId` is a plain `int`: an issue *must* belong to a project and *must* have been filed by someone — there is no such thing as a project-less or author-less issue. But `CategoryId`, `AssigneeId`, `AffectsVersionId`, and `FixVersionId` are all `int?`: an issue can be uncategorized, unassigned, and silent about versions. Those are real, ordinary states — a freshly filed bug that nobody has picked up yet has no assignee — and the `?` is how the model admits it.

> **Why every link is stored as an id, plus a matching object** — The id (ProjectId) is what actually lives in the database — a plain number pointing at a row in another table. The object (Project) is a convenience for the code: when the data layer loads it, you can write issue.Project.Name instead of looking the project up yourself. The `= null!;` on the object is the author telling the compiler 'trust me, the data layer fills this in' — it isn't stored, it's populated on load. Store the number; travel to the object when you need it.

The `Reporter`/`Assignee` split is a good example of why the model bothers with two separate links to the same `User` table. One person filed the issue (the reporter, fixed forever); a possibly-different person is responsible for it now (the assignee, which changes as work moves around). Both are users, but they answer different questions, so they are different boxes on the form — and this same reporter-or-assignee distinction is exactly what the private-issue rule keys on in §8.


### Timestamps, geography, and the housekeeping fields

The rest of the `Issue` form is dates and a set of small operational fields, each solving one concrete problem. The timestamps record the issue's life; the location fields exist for field and mobile reports; and a scatter of nullable fields quietly support imports, service-level tracking, and public intake:

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
public DateTime? DueDate { get; set; }

public double? Latitude { get; set; }
public double? Longitude { get; set; }

public DateTime? SlaBreachNotifiedAt { get; set; }
public int? ImportedMantisId { get; set; }
public string? ImportedExternalKey { get; set; }
public string? IntakeName { get; set; }
public string? IntakeEmail { get; set; }
```

You do not have to memorize these, but the pattern is instructive: they are *all nullable except the two creation/update stamps*, because every one of them describes an optional circumstance. An issue only has a due date if someone set one. It only has coordinates if the reporter chose to attach their location — the source comment says the fields are "Null unless the reporter chose to attach their location." `SlaBreachNotifiedAt` is null until the background scanner has sent a breach warning, and its whole job is to make sure that warning goes out once rather than "every tick." `ImportedMantisId` and `ImportedExternalKey` remember where an imported issue came from so that re-running the same import "skip[s] issues already brought in, instead of duplicating them." And `IntakeName`/`IntakeEmail` hold the name and email of someone who filed a ticket through the public page without an account.

One field deserves special attention because it protects your data from a subtle disaster:

```csharp
public Guid RowVersion { get; set; } = Guid.NewGuid();
```

This is the *optimistic-concurrency token*. Imagine two people open the same issue and both start editing. Person A saves; then Person B saves a few seconds later, unaware, and silently erases Person A's change — a "lost update." `RowVersion` prevents it: it is a random value that is reassigned on every save, and if the value a client loaded no longer matches when they try to save, the database layer "raises DbUpdateConcurrencyException instead of silently overwriting a concurrent edit." The same guard appears on `Project`. It is a tiny field that turns a data-corruption bug into a polite "someone else changed this, please reload."

> **Jargon, in plain words** — A Guid is a 'globally unique identifier' — a long random value practically guaranteed never to repeat. 'Optimistic concurrency' means the system optimistically assumes two people rarely edit the same row at once, so it doesn't lock anything up front; instead it checks the RowVersion at save time and only complains if a clash actually happened. UTC is Coordinated Universal Time — timestamps are stored in one worldwide zone so a Mac in California and a server in Virginia agree on when things happened.


### The satellites: what hangs off an issue

The bottom of the `Issue` form is a set of *navigation collections* — lists pointing at the smaller entities that decorate an issue. An issue does not store its notes inside itself; instead, each note stores the id of its issue, and this list is the convenient view back the other way:

```csharp
// Navigation
public ICollection<IssueNote> Notes { get; set; } = [];
public ICollection<IssueAttachment> Attachments { get; set; } = [];
public ICollection<IssueHistory> History { get; set; } = [];
public ICollection<IssueTag> IssueTags { get; set; } = [];
public ICollection<CustomFieldValue> CustomFieldValues { get; set; } = [];
```

Each of these is its own small form. `IssueNote` is a comment: it belongs to one issue and one author, carries its `Text`, and — echoing the issue itself — has its own `IsPrivate` flag so a note can be hidden even on a visible issue. `IssueAttachment` is an uploaded file: it records the `FileName`, the `FilePath` on disk, the `FileSize`, the `ContentType`, and who uploaded it. Notice attachments store a *path*, not the file's bytes — the database keeps the small facts about the file, the file itself lives on disk.

`IssueHistory` is the audit trail, and its comment states its job in one line: "one row per field change on an issue." Every time a field changes, a row is written recording which field, its `OldValue`, its `NewValue`, who changed it, and when — so an issue's whole past can be reconstructed:

```csharp
public string FieldChanged { get; set; } = string.Empty;
public string? OldValue { get; set; }
public string? NewValue { get; set; }
public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
```


### Tags and relationships: the many-to-many links

Two of the satellites are trickier because they connect *many things to many things*, and that always needs a small extra table in the middle. A `Tag` (a free-form label like "regression" or "ui") can be on many issues, and an issue can have many tags. You cannot store that with a single id on either side, so the model adds a tiny *join entity*, `IssueTag`, whose only job is to hold one issue-id and one tag-id — one row per pairing:

```csharp
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<IssueTag> IssueTags { get; set; } = [];
}

public class IssueTag
{
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
```

Tags are deliberately *global* — shared across all projects, MantisBT-style — because a tag name is not sensitive. The `Tag` comment is careful to note the security consequence: "Which issues a tag reveals is still governed by each issue's access control." Sharing the label does not share the issues; you still only see the tagged issues you are allowed to see.

> **Jargon, in plain words** — A 'many-to-many' relationship is one where each side can link to many of the other — many issues, many tags. Databases can't express that with a single reference, so you add a 'join entity' (also called a join table) in the middle whose rows are just pairs: this issue with that tag. IssueTag and ProjectMembership are both join entities.

`IssueRelationship` connects an issue to another issue — "duplicate of," "blocks," "parent of," "related to." A subtle design choice lives in its comment: the link is "stored once" as a directed row from a `SourceIssue` to a `TargetIssue`, and "the reciprocal is derived for display." In other words, if issue #10 blocks issue #12, the system stores that one fact once; when you look at #12 it computes the mirror label "blocked by" on the fly rather than storing a second row. That keeps the two sides from ever contradicting each other — there is only one row to keep honest.

```csharp
public int SourceIssueId { get; set; }
public Issue SourceIssue { get; set; } = null!;

public int TargetIssueId { get; set; }
public Issue TargetIssue { get; set; } = null!;

public IssueRelationshipType Type { get; set; }
```


### Project, User, and the membership that binds them

Above the issue sit the two big organizing entities. A `Project` is a container for issues and everything project-scoped — its categories, versions, custom fields, and members. Most of its form is navigation collections pointing down at the things it owns, plus a couple of flags that shape behavior:

```csharp
public string Name { get; set; } = string.Empty;
public string? Description { get; set; }
public bool IsPublic { get; set; } = true;

public bool PublicIntakeEnabled { get; set; }

public int OwnerId { get; set; }
public User Owner { get; set; } = null!;
```

`IsPublic` defaults to `true` and is the flag the access rules read to decide whether everyone signed in can see the project or only its members. `PublicIntakeEnabled` is off by default and, when a Manager turns it on, lets anyone with no account at all file a trouble ticket through the public "Report a problem" page. Every project also has an `Owner` — a required `User` link.

A `User` is an account. It is unusual among the entities because it does not define its own `Id`, `Email`, or password fields — it inherits them by building on ASP.NET Core Identity, the framework's ready-made login system:

```csharp
public class User : IdentityUser<int>
{
    /// <summary>Default/global role. Per-project roles live on <see cref="ProjectMembership"/>.</summary>
    public UserRole Role { get; set; } = UserRole.Reporter;

    public bool IsActive { get; set; } = true;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
```

> **Jargon, in plain words** — `class User : IdentityUser<int>` means User 'inherits from' — builds on top of — a class the framework already provides. All the login machinery (unique Id, UserName, Email, PasswordHash, SecurityStamp) comes from that base class for free, and OpenTrack only adds the fields it cares about, like the global Role. Not reinventing accounts is why the auth boxes aren't written here.

The single most important field on `User` is that one-line `Role` — the account's *global* rank. But OpenTrack also lets a person have a different rank on a specific project, and that is where the third entity comes in. `ProjectMembership` is a join entity, like `IssueTag`, but it carries an extra piece of information — a role — which is the whole point of it:

```csharp
/// <summary>Join entity giving a user a role within a specific project.
/// Composite key (UserId, ProjectId) is configured in AppDbContext.</summary>
public class ProjectMembership
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.Reporter;
}
```

This little three-field class is the linchpin of the entire permission system. A user has one global `Role`; each `ProjectMembership` row gives that user a possibly-different `Role` on one project. §8 combines the two into an "effective role" — the higher of the two — but the raw materials are exactly these two `Role` fields, one on `User` and one here. The comment notes the key is *composite*: a person can be a member of a project only once, because the pair (UserId, ProjectId) has to be unique.


### Categories, versions, custom fields, and checklists

The remaining entities are all *project-scoped* — each one belongs to a project and helps organize its issues. `Category` is a simple label an issue can sit in ("Backend," "Docs"). `ProjectVersion` is a release of the product; it is named `ProjectVersion` rather than just "Version" for a mundane but real reason stated in its comment — to avoid "clashing with System.Version," a type the framework already owns.

Custom fields let a Manager add their own boxes to a project's issue form, and they take *two* entities to do it — a lesson in separating a definition from its values. `CustomFieldDefinition` is the box itself: its name, its type (text, number, date, or a fixed list), whether it is required, and its sort order. `CustomFieldValue` is one issue's answer to one such box:

```csharp
public class CustomFieldValue
{
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;

    public int CustomFieldDefinitionId { get; set; }
    public CustomFieldDefinition Definition { get; set; } = null!;

    public string? Value { get; set; }
}
```

The comment captures the design in one sentence: the value is "Keyed by (IssueId, DefinitionId) so an issue has at most one value per field," and "The stored form is always text; the definition's type governs how it was validated and how it renders." That last part is a recurring OpenTrack theme — store the simple thing (text), and let a separate rule decide what it means. (The rule that validates those values is pure Core logic, and we meet it in §6.)

Finally, `ChecklistItem` supports the project's bug-hunt checklist — a list of things to verify, each marked Pass, Fail, or Not-Applicable, with a Fail typically "turned into a linked issue" via its nullable `LinkedIssueId`. Its comment is careful about scope: the checklist "lives alongside — not inside — the normal issue list, so any unlisted problem is still logged as an ordinary issue." It is a companion to the issue list, not a replacement for it.


### Reading the map: how it all connects

Step back and the shape of the whole model is simple to hold in your head. A short table of the main links makes the structure plain:

| This entity | points at | via (required?) |
| --- | --- | --- |
| Issue | Project | ProjectId — required |
| Issue | User (reporter) | ReporterId — required |
| Issue | User (assignee) | AssigneeId — optional |
| Issue | Category / versions | CategoryId, AffectsVersionId, FixVersionId — all optional |
| IssueNote / IssueAttachment / IssueHistory | Issue | IssueId — required |
| IssueTag | Issue + Tag | the many-to-many join |
| IssueRelationship | Issue + Issue | SourceIssueId + TargetIssueId |
| ProjectMembership | User + Project (+ a Role) | the join that grants project roles |
| Category / ProjectVersion / CustomFieldDefinition | Project | ProjectId — required |
| CustomFieldValue | Issue + CustomFieldDefinition | keyed by both |

Everything funnels toward `Issue` at the center, with `Project` and `User` as the two big anchors it hangs from, and a ring of smaller entities decorating each issue. There are no exotic structures here — no inheritance trees of issue subtypes, no clever tricks — just plain classes, plain id links, and a consistent rule about which links are required and which may be empty. That plainness is the feature.


## Why It Matters / Design Takeaways

The entity model is the foundation the entire building sits on, and its virtues are quiet ones. It is *plain*: every entity is an ordinary C# class with named fields, no framework attributes cluttering it, no database code mixed in — which is exactly why it can live in dependency-free `OpenTrack.Core` and be understood on its own. It is *consistent*: every link is the same foreign-key-plus-navigation pattern, so once you have read `Issue` you can read any of the others at a glance. And it is *honest about absence*: the difference between `int` and `int?`, between `string` and `string?`, is used carefully to say precisely which facts an issue must have and which it may lack.

The details that must not erode: required links (a project, a reporter) stay non-nullable so the system can never hold an orphaned issue; optional links stay nullable so real states like 'unassigned' are representable without lying; the `RowVersion` guards stay in place so concurrent edits fail loudly instead of losing data; and the privacy flags on `Issue` and `IssueNote` remain the single fields the access rules read, rather than sprouting copies. When you add a new kind of thing to OpenTrack, add it here as an entity in the same shape — an id, its own fields, foreign keys for what it belongs to, navigation properties for convenience — and the rest of the system will already know how to hold it.

> **The maintainer's rule** — Before adding a field, decide two things and let the type say them: is this value always present (plain type) or sometimes absent (add the `?`), and does it point at another entity (then add both the id and the navigation object, and choose whether that link is required). Keep entities free of database and web code so they stay in Core. The model is the contract every layer above depends on — change it deliberately, and let nullability carry the meaning.


# 5. The Fixed Menus: Enums & the Issue Vocabulary

*The small set of fixed-choice menus — Status, Severity, Priority, Reproducibility, Resolution, the user-role ladder, and a few others — that give every issue a shared, unambiguous vocabulary, and why the order of the choices is quietly load-bearing.*


## What This Is / What It Is For

When someone files a bug, a lot of it is free text — the title, the description, what they expected to happen. But some of it needs to be a *choice from a fixed menu*: how bad is it? what is its status? how urgent? If those were free text, one person would type "critical," another "Critical," another "crit," another "showstopper," and the software could never reliably sort, filter, or report on them. So OpenTrack pins each of those questions to a fixed list of allowed answers. In C# a fixed list of named choices is called an *enum*, and this chapter is a tour of all of them — the shared vocabulary every issue is described in.

Picture a restaurant with a printed menu versus one where you shout whatever you want at the kitchen. The printed menu is the enum: everyone orders from the same short list, the kitchen always understands, and the bill adds up. OpenTrack's enums are that printed menu for issues. And, as we will see, the menus are printed in a deliberate *order* — mild choices near the top, severe ones near the bottom — because a surprising amount of the program's logic works by asking "is this choice at least as far down the list as that one?"

> **Jargon, in plain words** — An enum (short for 'enumeration') is a type whose value must be one of a fixed set of named options — like IssueStatus.New or IssueSeverity.Crash. Behind each name sits a whole number the code can compare. Because the choices are fixed at compile time, the program can never end up with a status of 'banana': the only legal values are the ones on the menu.

> **The one-sentence version** — OpenTrack's enums are the fixed menus that give every issue an unambiguous, sortable vocabulary — and several of them (especially UserRole and IssueStatus) are ordered ladders whose numeric values are chosen so that 'higher means more' and rules can be written as simple greater-than comparisons.


### Why fixed menus, and why the numbers behind them matter

Every enum value has two faces: the *name* that humans read (`Major`, `Urgent`, `Closed`) and a *number* underneath it that the computer stores and compares. Most programming languages will assign those numbers for you automatically — 0, 1, 2, and so on — but OpenTrack writes the numbers out by hand, on purpose. That single decision is the theme of this whole chapter, so it is worth seeing why up front.

There are two reasons to control the numbers. First, *stability*: the number is what actually gets written to the database. If the numbers were automatic and someone later inserted a new choice in the middle of the list, every existing issue's stored number would suddenly mean something different — a silent data-corruption bug. Pinning the numbers means the menu can grow without disturbing what is already saved. Second, and more interestingly, *ordering*: by choosing the numbers so that milder options are lower and more serious ones are higher, the menu becomes a *ladder*, and any rule about "more serious than" or "at least this rank" becomes a plain number comparison.

OpenTrack follows the numbering convention of MantisBT, the long-running tracker it is modeled on. That is why you will see values like 10, 20, 30 rather than 1, 2, 3 — the gaps are deliberate, and we will come back to why at the end.


### IssueStatus: where an issue is in its life

The most fundamental menu is *status* — where an issue stands in its journey from just-filed to done. It is a genuine ladder: the numbers climb as the issue moves forward through its life.

```csharp
public enum IssueStatus
{
    New = 10,
    Feedback = 20,
    Acknowledged = 30,
    Confirmed = 40,
    Assigned = 50,
    Resolved = 80,
    Closed = 90
}
```

| Value | Number | What it means in plain words |
| --- | --- | --- |
| New | 10 | Just filed; nobody has looked at it yet. |
| Feedback | 20 | Waiting on more information from the reporter. |
| Acknowledged | 30 | A maintainer has seen it and agrees it's worth tracking. |
| Confirmed | 40 | Reproduced/verified — it's a real issue. |
| Assigned | 50 | Someone is now responsible for working it. |
| Resolved | 80 | A fix (or decision) is in; awaiting final closure. |
| Closed | 90 | Done and put to bed. |

The jump from `Assigned = 50` to `Resolved = 80` is not an accident, and it is the clearest example in the codebase of ordering doing real work. Everything below 80 is an issue that is still *open* — being worked on; everything from 80 up is finished. The SLA (service-level agreement) subsystem relies on exactly that boundary. In `SlaCalculator`, the definition of "still open" is one line that reads the status number against the `Resolved` rung:

```csharp
/// <summary>True for statuses whose SLA clock is still running (below Resolved).</summary>
public static bool IsOpen(IssueStatus status) => (int)status < (int)IssueStatus.Resolved;
```

Read that slowly: `(int)status` turns the menu choice into its number, and the rule says an issue's response clock is running as long as that number is *below* `Resolved`'s 80. New, Feedback, Acknowledged, Confirmed, Assigned — all below 80, all "open," all still on the clock. Resolved and Closed are at or above 80, so their clock has stopped. Because the ladder was numbered with that gap, a whole category ("open vs. done") is expressible as a single less-than. Change the ladder's order carelessly and this one line quietly changes meaning — which is exactly why the numbers are written down and left alone.


### Severity and Priority: how bad, and how soon

Two menus that people often confuse are *severity* and *priority*, and OpenTrack keeps them separate on purpose because they answer different questions. Severity asks "how much damage does this do?" Priority asks "how soon should we deal with it?" A cosmetic typo on the front page might be low severity but high priority; a rare crash in an unused corner might be high severity but low priority. Both are ordered ladders, mild at the top, severe at the bottom.

```csharp
public enum IssueSeverity
{
    Feature = 10,
    Trivial = 20,
    Text = 30,
    Tweak = 40,
    Minor = 50,
    Major = 60,
    Crash = 70,
    Block = 80
}
```

Read as a scale of impact: `Feature` is a request for something new (no damage at all), `Trivial` and `Text` are cosmetic or wording problems, `Tweak` is a small adjustment, `Minor` and `Major` are ordinary bugs of growing seriousness, `Crash` means the program falls over, and `Block` is the worst — something that blocks work entirely. Because it is ordered, a report or filter can say "show me everything Major or worse" as a simple "number ≥ 60." (`Minor` is the default a new issue starts at, from the previous chapter.)

```csharp
public enum IssuePriority
{
    None = 10,
    Low = 20,
    Normal = 30,
    High = 40,
    Urgent = 50,
    Immediate = 60
}
```

Priority is the urgency dial, climbing from `None` through `Low`, `Normal` (the default), `High`, `Urgent`, up to `Immediate` ("drop everything"). Priority is also the field the SLA system keys its targets on — a policy might give `Immediate` issues a four-hour resolution target and `Low` ones a week — so this ordered menu feeds directly into the deadline math we saw a moment ago.


### Reproducibility and Resolution: can we repeat it, and how did it end

The last two issue-classification menus round out the picture. *Reproducibility* records how reliably the problem can be made to happen again — crucial information for anyone trying to fix it:

```csharp
public enum IssueReproducibility
{
    Always = 10,
    Sometimes = 30,
    Random = 50,
    HaveNotTried = 70,
    UnableToReproduce = 90,
    NotApplicable = 100
}
```

This one is ordered more by "how much of a problem the unpredictability is" than by pure severity: an `Always`-reproducible bug is the easiest to fix, `Sometimes` and `Random` are progressively harder, `HaveNotTried` means nobody has checked yet (the default for a new issue), `UnableToReproduce` means someone tried and could not, and `NotApplicable` covers issues where the question does not fit (a feature request, say). The exact numeric order matters less here than for status or role — this menu is mostly used for display and filtering, not comparison logic.

*Resolution* records how an issue ultimately ended — the reason it was closed. It is best read as a plain list of outcomes rather than a strict ladder:

```csharp
public enum IssueResolution
{
    Open = 10,
    Fixed = 20,
    Reopened = 30,
    UnableToReproduce = 40,
    NotFixable = 50,
    Duplicate = 60,
    NotABug = 70,
    Suspended = 80,
    WontFix = 90
}
```

Every issue starts `Open`. From there it can end `Fixed`, be `Reopened` if the fix did not hold, or be closed for one of the "we are not fixing this" reasons: `UnableToReproduce`, `NotFixable`, `Duplicate` (already tracked elsewhere), `NotABug` (working as intended), `Suspended` (parked for now), or `WontFix` (a deliberate decision to leave it). Unlike status, these are categories of outcome rather than rungs you climb, so nothing in the code does math on their order — the numbers here mainly exist for stable storage and a sensible default sort.


### UserRole: the ladder the whole security system stands on

One enum matters more than all the others combined, because the entire access-control system (§8) is written in its terms. `UserRole` is the ranking of how much authority an account has, and it is the purest ordered ladder in the codebase — its own comment says so: "Access levels, ascending in privilege."

```csharp
/// <summary>Access levels, ascending in privilege (Mantis-style numeric values).</summary>
public enum UserRole
{
    Viewer = 10,
    Reporter = 25,
    Updater = 40,
    Developer = 55,
    Manager = 70,
    Administrator = 90
}
```

| Role | Number | What this person may broadly do |
| --- | --- | --- |
| Viewer | 10 | Look at issues they're allowed to see; no changes. |
| Reporter | 25 | File new issues and add notes. |
| Updater | 40 | Edit existing issues' fields. |
| Developer | 55 | Be assigned work; see private issues; set an issue private. |
| Manager | 70 | Manage the project — categories, versions, members, settings. |
| Administrator | 90 | Everything, everywhere; bypasses per-project scoping. |

The whole reason these numbers ascend is so that permission questions can be asked as "is your rank at least this high?" You saw the payoff in §8's `AccessContext`: rules like `CanAssignIssue() => AtLeast(UserRole.Developer)` are literally a number comparison — is your role's number ≥ Developer's 55? Because the ladder climbs, "Manager can do anything a Developer can, and more" falls out for free, with no rule ever having to list every role explicitly. Get this ladder's order right once and every access rule in the system reads like plain English.

> **Reordering this menu is not a cosmetic change** — Because access control asks 'is your role's number at least X', the safety of the whole app depends on these values ascending in privilege. Swapping two roles' numbers, or slotting a new role in with a number that puts it out of privilege order, silently rewrites who can do what. A new role must be given a number that places it at the correct rung of authority — its position on the ladder IS its power.


### The smaller menus

A few more enums exist to keep other parts of the system unambiguous. They are shorter, and most start their numbering at 0 (rather than 10) because they are plain lists of kinds, not privilege or severity ladders — nothing does 'greater-than' math on them, so there is no ladder to preserve.

`IssueRelationshipType` names the ways one issue can relate to another, and it comes with a small helper that turns the stored direction into a human label:

```csharp
public enum IssueRelationshipType
{
    RelatedTo = 0,
    DuplicateOf = 1,
    ParentOf = 2,
    Blocks = 3,
}
```

As §4 noted, each relationship is stored once as a direction (source → target), and the mirror label is computed for the other side. That computation lives right next to the enum in a `RelationshipLabels.Describe` method: given the type and whether you are looking from the source's side, it returns "duplicate of" versus "has duplicate," "parent of" versus "child of," "blocks" versus "blocked by" — while `RelatedTo` reads the same from both sides because it is symmetric. Keeping the labels beside the enum means the vocabulary and its human wording never drift apart.

`CustomFieldType` is the menu of data types a Manager can give a custom field, and its comment states the design cleanly: "Values are always stored as text; the type governs how a value is validated on input and how it is rendered/edited in the UI."

```csharp
public enum CustomFieldType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Enum = 3,
}
```

So a custom field can be free text, a number, a calendar date, or a one-from-a-list choice — and which one it is decides how OpenTrack checks and displays the value (the pure rule that does that checking is in §6). `ChecklistItemStatus` is the tiny menu for a bug-hunt checklist item — `Pending`, `Pass`, `Fail`, `NotApplicable` — mirroring the pass/fail/N-A choices you work through when verifying a list. And `WebhookFormat` records how an outgoing webhook's message should be shaped for its destination:

```csharp
/// <summary>How a project webhook's payload is shaped for its destination.</summary>
public enum WebhookFormat
{
    Generic = 0,   // Full structured JSON
    Slack = 1,     // a { "text": "…" } message
    Discord = 2,   // a { "content": "…" } message
}
```

`Generic` sends full structured data; `Slack` and `Discord` shape the message into the exact little format each of those services expects. It is a plain list of destinations, so its numbers are just labels — the order carries no meaning, and the values simply need to stay stable so saved webhook settings keep pointing at the right format.


### Why the gaps between the numbers?

You will have noticed that the ladders count 10, 20, 30 — or 10, 25, 40, 55 — never 1, 2, 3. Those gaps are a deliberate piece of forward-thinking inherited from MantisBT. Because the numbers are what the database stores, and because they are ordered, you sometimes want to insert a brand-new choice *between* two existing ones — a new status that sits after Confirmed but before Assigned, say. If the values were 3 and 4 with no gap, there would be no whole number to give the newcomer without renumbering everything after it (and rewriting every stored issue). With Confirmed at 40 and Assigned at 50, you can drop a new rung in at 45 and it lands in exactly the right place on the ladder, no existing data disturbed.

> **The gaps are room to grow** — Leaving space between the numbers means a future maintainer can add a new choice at the correct point in an ordered menu just by picking a number between its neighbors. The menu stays sorted, every already-saved issue keeps its meaning, and no migration is needed. It's a small habit that pays off years later.


## Why It Matters / Design Takeaways

The enums look like the most trivial files in the project — a handful of names and numbers each — but they carry two heavy responsibilities. They are the *shared vocabulary*: because status, severity, priority, and role are fixed menus rather than free text, every screen, filter, report, and rule speaks the same language and can never be tripped up by a typo or a synonym. And several of them are *ordered ladders whose numeric order is logic*: the status ladder is what lets one line decide 'open vs. done,' and the role ladder is what lets the whole access system be written as 'at least this rank.'

The rules that must not erode: keep the numbers explicit and stable so stored data never shifts meaning underneath you; keep the privilege and severity ladders in ascending order so the greater-than comparisons that depend on them stay correct; and keep the gaps so the menus have room to grow in place. When you add a value, you are not just adding a name — you are choosing where it sits on a ladder that other code is quietly measuring against.

> **The maintainer's rule** — Never renumber an existing enum value, and never reorder a ladder to make it 'read nicer' — the numbers are stored in the database and compared by the code. To add a choice, give it a new number that places it at the correct rung (use the gaps), and if it belongs to an ordered ladder (status, severity, priority, role), double-check that its number puts it in the right spot relative to its neighbors. The name is for humans; the number is the contract.


# 6. Pure Rules in Core: Why some logic lives with no database

*A principle, not just a place: OpenTrack keeps its trickiest decisions — who may see a thing, whether an SLA has been breached, which automation rules fire, whether a custom-field value is valid — as pure functions in Core that touch no database and no network, so the same rule can be shared everywhere and proven correct by a test in milliseconds.*


## What This Is / What It Is For

Some of the decisions OpenTrack makes are fiddly and consequential. Is this person allowed to see this private issue? Has this issue blown past its promised response time? When a new bug comes in, which of the project's automation rules should fire, and what should they do? Is the value someone typed into a custom "Due date" field actually a date? Getting any of these subtly wrong is the kind of mistake that ships a security hole or a silent miscalculation. This chapter is about a design principle OpenTrack uses to make those decisions trustworthy: it keeps the *rules themselves* as *pure* code in `OpenTrack.Core`, with no database and no web anywhere near them.

The idea is worth stating as a principle because it recurs all over the codebase. A pure rule is like a *pocket calculator*: you hand it some numbers, it hands back an answer, and it has no memory, no network cable, and no opinions about where the numbers came from. Because it does nothing but compute, you can check that a calculator is correct by punching in cases and reading off answers — no server to start, no database to seed, no login to fake. OpenTrack pushes as many of its important decisions as it can into that calculator-shaped form, and this chapter shows the pattern through its flagship example and several smaller ones.

> **Jargon, in plain words** — A 'pure function' is code that only looks at the values you pass in and only produces a returned answer — it reads no file, no database, no network, and changes nothing outside itself. Give it the same inputs and it always returns the same output. 'I/O' (input/output) means talking to the outside world — disk, database, network — and pure code deliberately does none of it. A 'unit test' is a tiny program that calls one function with known inputs and checks the answer; pure functions are the easiest possible thing to unit-test.

> **The one-sentence version** — OpenTrack keeps its hardest decisions as pure, database-free functions in Core — permission checks, SLA math, automation matching, value validation — so each rule lives in exactly one place, can be reused by every part of the app without dragging a database along, and can be proven correct by fast unit tests instead of by running the whole system.


### The principle: judgment is one job, fetching data is another

The heart of the idea is a separation of two jobs that are easy to tangle together. One job is *fetching* — going to the database to find out whether a project is public, when an issue was created, what the SLA target is for High priority. The other job is *judging* — given those facts, deciding the answer. OpenTrack insists these are different jobs done by different code. The messy fetching lives in the data layer (§7 onward). The clean judging lives in Core as pure functions that are simply *handed* the facts as plain arguments.

Why go to the trouble? Three payoffs, and they compound. First, *no drift*: if the rule lives in one pure place, the web app and the desktop-serving API both call that one place, and they cannot slowly diverge into two subtly different versions of the rule (this is the whole spine of §8). Second, *testability*: a pure rule can be exercised across every case in a test that runs in milliseconds, with no server. Third, *clarity*: a function that takes plain facts and returns an answer can be read and understood on its own, without also understanding the database schema, the web framework, and the network.

> **Why 'facts in, answer out' is the whole trick** — None of these pure rules ever look anything up. AccessContext doesn't ask 'is this project public?' — it is told, as a parameter. SlaCalculator doesn't read the clock — you pass it 'now'. This is exactly what keeps them pure: the layer that owns the database does the looking-up and then hands the facts over. Judgment and data-fetching are kept apart on purpose, and that single discipline is what makes everything in this chapter testable.


### The flagship: AccessContext, permission rules as pure code

The clearest and most important example of the principle is the access-control authority, `AccessContext`. It gets a full chapter of its own (§8), so here we only look at it as the model citizen of the 'pure rules in Core' idea. Its own summary comment is the mission statement for this entire chapter — the rules are kept "deliberately pure (no EF, no HTTP)" precisely so both front doors call the same rules and "the whole matrix is unit-testable."

See how it takes only plain facts and returns only a yes/no — never touching a database to answer:

```csharp
public bool CanViewIssue(bool projectIsPublic, bool issueIsPrivate, int reporterId, int? assigneeId) =>
    CanViewProject(projectIsPublic)
    && (!issueIsPrivate
        || reporterId == UserId
        || assigneeId == UserId
        || AtLeast(UserRole.Developer));
```

Every input — whether the project is public, whether the issue is private, who reported it, who it is assigned to — arrives as an argument. The method does not go and find any of that; it is handed the facts and it judges. That is why a single test file can drive this rule through every combination of role and privacy with no server running, and it is why the browser and the desktop app can never disagree about who may see an issue. The permission logic is the flagship, but it is one instance of a pattern OpenTrack repeats wherever a decision is important enough to get right once. The full deep dive — effective roles, the list-filter twin, the two front doors — is §8.


### SlaCalculator: deadline math without a clock or a server

The second example makes the payoff vivid. A *service-level agreement* (SLA) is a promise about response time — "we will resolve High-priority issues within eight hours." OpenTrack has to decide, for each open issue, whether it is comfortably on track, getting close to its deadline, or already breached. That decision involves time, which is normally the enemy of testable code: anything that reads the real clock behaves differently every second. `SlaCalculator` sidesteps that entirely by being pure — you pass it the current time as an argument.

```csharp
public static SlaAssessment Evaluate(
    IssuePriority priority, DateTime createdAtUtc, bool isOpen, DateTime nowUtc, int? targetHours)
{
    // No target for this priority, or the clock has stopped → nothing to track.
    if (!isOpen || targetHours is not > 0) return SlaAssessment.NotTracked;

    var dueUtc = createdAtUtc.AddHours(targetHours.Value);
    var atRiskUtc = createdAtUtc.AddHours(targetHours.Value * SlaDefaults.AtRiskFraction);

    var status = nowUtc >= dueUtc ? SlaStatus.Breached
        : nowUtc >= atRiskUtc ? SlaStatus.AtRisk
        : SlaStatus.OnTrack;
    return new SlaAssessment(status, dueUtc);
}
```

Read it as plain arithmetic. If the issue is not open, or its priority has no target, there is nothing to track. Otherwise the deadline (`dueUtc`) is simply the creation time plus the target hours, and the "getting close" threshold (`atRiskUtc`) is the same but at 80% of the way there — that fraction is a named constant, `SlaDefaults.AtRiskFraction = 0.8`, so there is no mystery number floating in the math. Then the verdict is three plain comparisons: past the deadline is `Breached`, past 80% is `AtRisk`, otherwise `OnTrack`.

Because `nowUtc` is passed in rather than read from the system clock, a test can hand this function a creation time of noon, a target of four hours, and a "now" of 3pm and assert — deterministically, forever — that the answer is `AtRisk`. The rule's own comment names exactly the audiences this purity serves: it is "Kept in Core so the background scanner, the data layer, and the tests all agree." The background job that scans for breaches, the code that shows an issue's SLA badge, and the test suite all call this one function, so all three can never disagree about what "breached" means.

> **Pass in the clock, don't read it** — Reading the real time inside a function makes it untestable — its answer changes every second and you can't check a specific case. OpenTrack's fix is the small, powerful habit of taking 'now' (nowUtc) as a parameter. The one caller that has the real clock passes it in; everyone else, including every test, can pass whatever moment they want to examine.


### AutomationEvaluator: 'when it looks like this, do that', purely

The third example handles automation. A project can define rules like "when a new issue's text contains 'password', set its severity to Major and tag it security." When a fresh issue arrives, OpenTrack has to work out which rules match and what their combined effect is. That is exactly the kind of branchy logic where bugs hide — so, again, the deciding is a pure function, `AutomationEvaluator`, deliberately "decoupled from the EF entity so the evaluator is a pure function the infrastructure and tests can both drive."

The trick is that the evaluator does not work on database rows at all. The rules and the incoming issue are re-expressed as plain, immutable *records* — `AutomationRuleDef` for a rule's when/then shape and `AutomationInput` for the issue's as-created state — so the evaluator never sees Entity Framework. Its result is a third record, `AutomationOutcome`, describing which fields to change and which tags to add:

```csharp
public static AutomationOutcome Evaluate(AutomationInput input, IEnumerable<AutomationRuleDef> rules)
{
    IssueSeverity? severity = null;
    IssuePriority? priority = null;
    IssueStatus? status = null;
    int? assignee = null;
    var tags = new List<string>();
    var applied = new List<string>();

    foreach (var r in rules)
    {
        if (!Matches(input, r)) continue;
        applied.Add(r.Name);

        if (r.SetSeverity is { } s) severity = s;
        if (r.SetPriority is { } p) priority = p;
        if (r.SetStatus is { } st) status = st;
        if (r.AssignToUserId is { } a) assignee = a;
        ...
    }

    return new AutomationOutcome(severity, priority, status, assignee, tags, applied);
}
```

The function's comment pins down two subtle decisions that matter for predictability: every rule's conditions are "tested against the ORIGINAL issue state (not the running result), so the outcome doesn't depend on subtle action/condition interplay," and "For scalar actions the last matching rule wins; tags accumulate." Those are precisely the kind of ordering-and-interaction rules that are hard to reason about in your head and easy to verify with a test — you feed the evaluator a known issue and a known list of rules and assert the exact outcome. Because it is pure, the code that actually writes the changes to the database is a separate, thin layer; the tricky decision of *what* to do is isolated and provable.


### The smaller pure helpers

The principle shows up in several smaller places, and it is worth seeing that it is a habit, not a one-off. Each of these is a pure, dependency-free function that some data-layer code calls but that can be tested entirely on its own.

*Custom-field validation* (`CustomFieldValidation`) decides whether the value someone typed into a project's custom field is acceptable, and what canonical form to store. It is explicitly "Kept free of EF/HTTP so the whole matrix is unit-testable," and it returns a small result that is either an error message or a normalized value:

```csharp
case CustomFieldType.Number:
    return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var num)
        ? new ValueResult(null, num.ToString(CultureInfo.InvariantCulture))
        : new ValueResult("Value must be a number.", null);

case CustomFieldType.Date:
    return DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
        ? new ValueResult(null, d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
        : new ValueResult("Value must be a date (YYYY-MM-DD).", null);
```

This is the pure counterpart to the `CustomFieldType` menu from §5: the menu says what type a field is; this function is the rule that enforces it — a number must parse as a number, a date must parse and is stored in one canonical ISO form, an enum value must match one of the defined options. Whether the value ultimately gets saved is the data layer's job; whether it is *valid* is decided here, where a test can check every type and every bad input without a database in sight.

*Duplicate detection* (`IssueSimilarity`) is a pair of pure helpers that reduce an issue title to its "significant words" and score how many two titles share — enough to surface likely duplicates when someone files a new bug. It is "Deliberately simple and dependency-free," and the split of labor is telling: "The persistence layer uses SignificantWords to build the candidate query and Overlap to rank." The database does the fetching; these pure functions do the judging of what counts as a word and how much two titles overlap.

*Git-reference parsing* (`GitRefParser`) reads a commit message and pulls out the issue numbers it mentions — plain mentions like `#123` and closing references like `fixes #123` — so a commit can automatically comment on or close an issue. It is "Kept in Core so the webhook receiver and the tests share one definition," and it captures one nice subtlety in a comment: "If the same issue is mentioned both ways, the closing intent wins." A gnarly bit of text parsing becomes a pure function that the incoming-webhook code and the test suite both drive from the same place.

> **The tell-tale phrase** — Grep the Core project for the word 'pure' and you keep landing on the same sentence: kept pure / free of EF/HTTP so it is shared and unit-testable. That repeated phrase is the design principle announcing itself — every time a decision was important enough to get exactly right, its author lifted the decision out of the database code and into a pure Core function.


### The shape they all share

Look across the examples and one silhouette repeats. Each pure rule takes *plain facts in* — booleans, ids, times, small records — and returns *a plain answer out* — a yes/no, an assessment, an outcome, a validation result. None of them opens a database, makes a web request, reads a file, or looks at the system clock. Whatever messy fetching those facts required happened in a different layer that then handed the facts over. And each one is, as a direct result, driven by a focused set of unit tests that run in the blink of an eye.

This is also why these rules can live in `OpenTrack.Core`, the project at the very bottom of the dependency stack that is allowed to depend on almost nothing. A rule that needed the database would have to live higher up, near the database, and could not be shared cleanly by both the web host and the API host. By keeping the rule pure, OpenTrack earns the right to put it in Core, and putting it in Core is what lets everyone share it. The purity and the placement reinforce each other.


## Why It Matters / Design Takeaways

The lesson of this chapter is a mindset more than a mechanism: when a decision is important enough to get exactly right, separate the *judging* from the *fetching* and put the judging in a pure Core function. OpenTrack does this for its permission checks, its SLA deadlines, its automation matching, its value validation, its duplicate scoring, and its commit parsing — every place where a subtle mistake would be expensive. The reward each time is the same trio: one authoritative copy of the rule, reuse by every host without dragging a database along, and a fast test that proves the rule across all its cases.

The rules that must not erode: pure logic stays free of database and web code so it stays shareable and testable; facts are passed in as arguments rather than fetched inside (including 'now' — pass the clock, don't read it); and each important decision has exactly one pure home that every surface calls, so there is never a second, drifting copy. When you find yourself about to embed a tricky decision inside data-layer or endpoint code, that is the signal to lift it out into a pure Core function instead.

> **The maintainer's rule** — If a new piece of logic is a decision — 'is this allowed / valid / breached / a match?' — write it as a pure function in OpenTrack.Core that takes plain facts and returns a plain answer, and back it with unit tests. Do the database fetching in the layer that owns the database, then hand the facts to the pure rule. Keep judgment and I/O apart, and the hard parts of OpenTrack stay provable.


# 7. Entity Framework Core & the One Shared Database

*How OpenTrack's whole world of data — projects, issues, notes, users, and every feature table — is described once in plain C#, stored in a single SQLite file, and reached the same way by the web app and the API, so the two front doors can never end up reading from two different databases.*


## What This Is / What It Is For

Everything OpenTrack remembers — every project, every issue, every note, comment, tag, attachment, and user account — lives in a database. This chapter is about the layer that owns that database: how the shape of all the data is described in ordinary C# code, how it is stored in one small file on disk, and how both of OpenTrack's front doors (the website and the API the desktop app talks to) are wired to reach that one file and never accidentally split into two.

Think of it as the building's records room. There is exactly one records room, one filing system, and one set of rules for how folders are laid out and cross-referenced. Whether a clerk walks in from the front lobby (the web app) or the loading dock (the API), they open the same cabinets and read the same folders. The whole point of this layer is to guarantee that there is only ever one records room — not a second, shadow copy that quietly drifts out of sync.

> **Jargon, in plain words** — Entity Framework Core (EF Core) is Microsoft's library that lets you work with a database using normal C# objects instead of hand-written SQL — you say `db.Issues`, it writes the SQL for you. SQLite is a database that is just a single ordinary file on disk (here, opentrack.db) with no separate server to install or run. A DbContext is the C# object that represents one live connection-and-session to that database. A DbSet is one table exposed as a list you can query (`db.Issues` is the Issues table).

> **The one-sentence version** — AppDbContext is the single C# description of the whole database; every host reaches it through the same connection string so they all share one opentrack.db; and the schema is built and upgraded automatically by migrations that run when the app starts.


### The map of the whole database: AppDbContext

The class `AppDbContext` is the one place that lists every table OpenTrack has. Each table is exposed as a `DbSet` — a queryable list of one kind of thing. Reading the top of the class is like reading the index of the records room:

```csharp
public DbSet<Project> Projects => Set<Project>();
public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
public DbSet<Category> Categories => Set<Category>();
public DbSet<ProjectVersion> Versions => Set<ProjectVersion>();
public DbSet<Issue> Issues => Set<Issue>();
public DbSet<IssueNote> IssueNotes => Set<IssueNote>();
public DbSet<IssueHistory> IssueHistories => Set<IssueHistory>();
public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();
```

Further down the file the list continues with the tables that back OpenTrack's later features — `Tags`, `IssueTags`, `IssueMonitors`, `Notifications`, `CustomFieldDefinitions`, `CustomFieldValues`, `SavedFilters`, `UserPreferences`, `ProjectWebhooks`, `TimeLogs`, `WorkflowTransitions`, `AutomationRules`, `SlaPolicies`, `GitIntegrations`, and more. Each `DbSet` corresponds to one entity class from `OpenTrack.Core` (the plain data shapes covered in Part II). One class, one master list: if a table is not here, OpenTrack does not have it.

One detail at the very top of the class is worth calling out, because it explains why the user table is not in that list. `AppDbContext` does not start from scratch — it inherits from ASP.NET Identity's user-store context:

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<User, int>(options)
```

By deriving from `IdentityUserContext<User, int>`, OpenTrack gets the `Users` table (and the sign-in login/token and passkey tables) for free from the sign-in framework, and adds its own tables on top. The code's own comment explains a deliberate choice: it uses `IdentityUserContext`, not the fuller `IdentityDbContext`, because "OpenTrack authorization is driven entirely by the custom UserRole enum (User.Role) and ProjectMembership, never ASP.NET Identity's role store, so we omit the AspNetRoles/AspNetUserRoles/AspNetRoleClaims tables." In other words, OpenTrack keeps the parts of the sign-in framework it uses (accounts, passwords, passkeys) and skips the built-in role tables, because — as Chapter 8 showed — its permission system is its own.


### One place that declares every table's shape

Listing the tables is only half the job. The other half is describing each table in detail: how long a name can be, which columns are required, which tables point at which, and what happens to child rows when a parent is deleted. All of that lives in a single method, `OnModelCreating`, which EF Core calls once to learn the model. It is long, but it is long the way a table of contents is long — every entity gets a short, readable block.

Here is the `Project` block. Read it as a sentence: a project has a required name capped at a set length, an optional description, a concurrency token, and an owner that must exist and cannot be deleted out from under it:

```csharp
b.Entity<Project>(e =>
{
    e.Property(p => p.Name).HasMaxLength(FieldLimits.ProjectName).IsRequired();
    e.Property(p => p.Description).HasMaxLength(FieldLimits.Description);
    e.Property(p => p.RowVersion).IsConcurrencyToken();
    e.HasOne(p => p.Owner)
        .WithMany()
        .HasForeignKey(p => p.OwnerId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

> **Jargon, in plain words** — A foreign key is a column that points at a row in another table — an issue's `ProjectId` points at the project it belongs to. A cascade delete means 'when the parent goes, its children go too' (delete a project and its issues vanish with it). Restrict means the opposite — 'refuse to delete the parent while children still point at it.' A concurrency token (`RowVersion`) is a stamp that changes on every save, so if two people edit the same issue at once, the second save is caught instead of silently overwriting the first.

The lengths are never bare numbers — they come from a shared `FieldLimits` constants class (`FieldLimits.ProjectName`, `FieldLimits.Description`, and so on), so the same limit is used by the database, the validation code, and the UI. Notice too the deliberate choice of delete behavior. A project's issues are set to `OnDelete(DeleteBehavior.Cascade)` — remove the project, remove its issues — while a project's owner is `Restrict` — you cannot delete a user who still owns a project. Each relationship is a small, considered decision about what should happen when something is removed.

A few of these decisions carry long comments because they encode a real trap. The `IssueRelationship` block, which links one issue to another, spells out why one side cascades and the other restricts:

```csharp
// Source cascades, Target restricts: two cascade FKs to the same Issue table would be
// "multiple cascade paths" on SQL Server, so only one cascades. Nothing deletes issues or
// projects through the UI today. WHEN a delete feature is added, it must first clear
// relationships where the doomed issue is the TARGET ...
e.HasOne(r => r.SourceIssue)
    .WithMany()
    .HasForeignKey(r => r.SourceIssueId)
    .OnDelete(DeleteBehavior.Cascade);
e.HasOne(r => r.TargetIssue)
    .WithMany()
    .HasForeignKey(r => r.TargetIssueId)
    .OnDelete(DeleteBehavior.Restrict);
```

The value of putting all this in one method is that the database's shape is never a mystery scattered across the codebase. Want to know whether tag names must be unique, or whether deleting an issue deletes its notes? It is all in `OnModelCreating`, in the block named for that entity. The model also declares indexes here — for example `e.HasIndex(i => i.Status)` and `e.HasIndex(i => i.ProjectId)` on issues — which are the database's shortcuts for fast lookups, again decided once and in the open.


### Why the web app and the API get the context differently

Now the subtle part, and the reason this chapter exists as its own topic. A `DbContext` is a short-lived working object: you open one, do a unit of work, and let it go. It is explicitly not safe to share one across several things happening at the same time. That collides head-on with how the web app is built, and the collision is what shapes the registration code.

> **Jargon, in plain words** — OpenTrack's website is a Blazor Server app. Blazor Server keeps a long-lived connection open to your browser — called a circuit — for as long as you have the page open, and runs the page's code on the server over that connection. 'Scoped' is a lifetime rule in .NET: normally one shared instance per web request. The trouble is that a Blazor circuit is not one quick request — it can live for many minutes while you click around, and several things on the page can run at the same moment.

Put those two facts together and the danger is clear: if the whole Blazor circuit shared a single `DbContext`, then two parts of the page doing database work at the same instant would be using the same non-thread-safe object at once — which throws. The registration method `AddOpenTrackInfrastructure` solves this with a two-part setup, and its summary comment lays out the reasoning in full:

```csharp
public static IServiceCollection AddOpenTrackInfrastructure(
    this IServiceCollection services, string connectionString)
{
    services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
    services.AddScoped<AppDbContext>(sp =>
        sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
    return services;
}
```

The first line registers a factory — a little machine that hands out a brand-new, short-lived `AppDbContext` every time you ask. The Blazor web code uses that factory to create a fresh context for each operation and dispose it right after, so two simultaneous actions on the same page never touch the same context. The second line adds a scoped shim: a plain `AppDbContext` that other code can still ask for directly, which the shim quietly builds from the same factory. The comment explains exactly who each is for:

- *The factory* exists because "a single scoped context lives for the entire SignalR circuit and DbContext is not thread-safe, so concurrent component operations on a shared instance throw." The web data service creates a short-lived context per operation.
- *The scoped shim* exists "so ASP.NET Identity and the API endpoints can still inject AppDbContext directly (they run per-HTTP-request, where a request-scoped context is short-lived and safe)."

So the same registration serves both worlds. The API is a normal request-in, response-out program — each request is quick and gets its own context, which is exactly the classic "scoped" pattern, so injecting `AppDbContext` directly is safe there. The web app's long-lived, concurrent circuits are the exception, and they use the factory. One method, `AddOpenTrackInfrastructure`, sets up both, and both hosts call it at startup.


### The one shared file: resolving the connection string

A database is only "one shared database" if every program actually opens the same file. This is easy to get wrong in a way that fails silently: SQLite will happily create a new empty file if you point at a path that does not exist yet. If the web app resolved `opentrack.db` relative to its own folder and the API resolved `opentrack.db` relative to a different folder, each would create its own database and neither would ever see the other's data — with no error to warn you. OpenTrack closes that trap in one shared helper, `ResolveOpenTrackConnectionString`:

```csharp
public static string ResolveOpenTrackConnectionString(this IConfiguration configuration)
{
    var configured = configuration.GetConnectionString("Default");
    if (!string.IsNullOrWhiteSpace(configured))
        return configured;

    var dataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenTrack");
    Directory.CreateDirectory(dataDir);
    var dbPath = Path.Combine(dataDir, "opentrack.db");
    return $"Data Source={dbPath};Cache=Shared";
}
```

The logic is deliberately boring, which is the point. First, if the deployment has set an explicit connection string in configuration, use it exactly as given — this is the hook the real Beelink deployment uses to point the database at the D: drive. Otherwise, every host falls back to the *same absolute path*: a single `opentrack.db` inside a per-machine `OpenTrack` folder under the operating system's local-application-data location. The comment states the goal plainly — fall back to the same absolute path "rather than each resolving 'opentrack.db' relative to its own launch directory, which would silently create separate databases for the web app and the API."

Both hosts wire this up identically at startup, so both end up at the same file. In the web host's `Program.cs`:

```csharp
var connectionString = builder.Configuration.ResolveOpenTrackConnectionString();
builder.Services.AddOpenTrackInfrastructure(connectionString);
```

The API host's `Program.cs` contains the exact same two lines. Because the resolution logic lives in one shared method and both hosts call it, the website and the API cannot end up looking at different databases unless someone deliberately configures them apart. (A third caller, the web app's backup scheduler, uses the same helper too, so backups always target the live file.)


### How the database gets built and stays current: migrations

The `OnModelCreating` method describes what the database should look like. But the actual file on disk has to be created to match, and when the model changes — a new feature adds a table or a column — the existing file has to be updated without losing anyone's data. That is what migrations are for.

> **Jargon, in plain words** — A migration is a small, ordered, dated step that changes the database's structure — 'add this table', 'add this column'. Each one is a C# file with an `Up` method (apply the change) and a `Down` method (undo it). Run every migration in order against an empty file and you get the current schema; run only the new ones against an existing file and it catches up. They live in the Migrations folder, each stamped with a timestamp so their order is fixed.

OpenTrack's `Migrations` folder holds the whole history, from `InitialCreate` through steps like `AddConcurrencyTokens`, `AddTags`, `AddCustomFields`, `AddSlaPolicies`, and `AddGitIntegration` — one per feature that touched the schema, each a readable record of exactly what changed. A single migration is small and legible. `AddConcurrencyTokens`, for example, just adds the `RowVersion` stamp column to two tables:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<Guid>(
        name: "RowVersion",
        table: "Projects",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

    migrationBuilder.AddColumn<Guid>(
        name: "RowVersion",
        table: "Issues",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
}
```

The operator never has to run these by hand. Each host applies any pending migrations automatically the moment it starts. In the web host's `Program.cs`, right after the app is built:

```csharp
// Apply any pending EF Core migrations at startup so the SQLite database (and the
// Identity tables) are created/updated automatically when the app runs.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await OpenTrackSeeder.EnsureBootstrapAdminAsync(scope.ServiceProvider, app.Configuration);
}
```

The single call `db.Database.Migrate()` does the whole job: if the database file does not exist, it is created and every migration is applied to bring it up to the current shape; if it already exists, only the migrations it has not seen yet are run. This is why a fresh install "just works" — start the app and the schema builds itself — and why upgrading is equally quiet: start the new version and the file catches up. The API host runs the identical `db.Database.Migrate()` at its own startup, so whichever host launches first prepares the shared file for both.


### One more safeguard: the design-time factory

There is a fiddly edge case worth understanding, because it caused a real bug that a comment now guards against. When a developer generates a new migration with the `dotnet ef` tooling, that tooling has to build an `AppDbContext` without the full web app running. OpenTrack provides a special `AppDbContextFactory` for exactly that moment — and it is written to build the context the *same way the running hosts do*, not with a bare `new AppDbContext(...)`.

```csharp
public AppDbContext CreateDbContext(string[] args)
{
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddDataProtection();
    services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

    services.AddOpenTrackInfrastructure("Data Source=opentrack-designtime.db");
    services.AddOpenTrackIdentity();

    var provider = services.BuildServiceProvider();
    return provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
}
```

Why bother going through the same registrations? Because building the context any other way quietly produces a *different* model. The factory's comment records the exact failure: a bare construction "has no application service provider, so Identity's schema version never applies and the model silently OMITS the .NET 10 AspNetUserPasskeys table — which made every scaffolded migration try to DROP that table (audit finding D1), and passkeys are a live feature here." In plain terms: if the tool that writes migrations sees a slightly different picture of the database than the running app does, it writes migrations that would delete a table the app actually needs. Forcing the design-time factory through `AddOpenTrackInfrastructure` and `AddOpenTrackIdentity` — the very same setup the real hosts use — guarantees the migration tool and the running app always agree on what the database looks like.


## Why It Matters / Design Takeaways

The shape to preserve here is "one description, one file, one path to it." The entire database is described in a single class, `AppDbContext`, with every table's rules gathered in `OnModelCreating`. There is one physical file, `opentrack.db`, and one shared helper that every host calls to find it, so the website and the API cannot drift onto separate databases. And the schema builds and upgrades itself through dated migrations applied at startup, so nobody has to run database commands by hand for an install or an upgrade to work.

The reason for the factory-plus-shim split is equally worth keeping in mind: the web app's long-lived, concurrent Blazor circuits need a fresh short-lived context per operation, while the API's quick per-request lifetime is safe with a directly-injected scoped context. Both are served by one registration method. And the design-time factory must always build the context exactly as the app does, or the migration tooling and the app will disagree about reality — the kind of mismatch that writes a migration to drop a table you depend on.

> **The maintainer's rule** — When you change the data model, change it in one place — add or edit the entity, wire it in OnModelCreating, then generate a migration through the shared AppDbContextFactory and let startup apply it. Never point a host at a database path of its own, never construct AppDbContext by hand for tooling, and never assume a schema change is done until a migration exists for it. One description, one file, one path — keep it that way and the two front doors stay in perfect agreement about the data beneath them.


# 8. The Access-Control Authority

*The single set of rules that decides who may see and touch each project and issue — written once, in pure code with no database, and consulted identically by both the web app and the Web API so the two can never disagree about who is allowed to do what.*


## What This Is / What It Is For

*OpenTrack* is an issue tracker that many people share. Some projects are open to everyone signed in; some are private to a handful of members. Some individual issues are marked private inside an otherwise-visible project. Every time the software shows a list of issues, opens one issue, or lets someone edit or assign one, it has to answer a question first: *is this person allowed?* The access-control authority is the one place in the whole codebase that answers that question.

Picture it as the building's security desk. Every request to see or change something — open project #4, list my issues, edit issue #812, read this private note — walks past the same desk and gets a yes or a no from the same rulebook. There is not one rulebook for the website and a different one for the phone app. There is one rulebook, and this chapter is about how it is written, why it was written as one shared thing, and how the code physically routes both the web app and the Application Programming Interface (API — the behind-the-scenes service the desktop app talks to) through it.

> **The one-sentence version** — The permission rules live in one small, pure class (AccessContext) that knows nothing about databases or the web, so the web app and the API both call the identical rules and cannot drift apart — and the rules are arranged so a private thing is hidden unless you are explicitly entitled to see it.


### Why one pure rulebook, consulted by two surfaces

OpenTrack has two front doors. The *web app* (what you open in a browser) reads and writes the database directly. The *API* (what the Windows and Mac desktop apps call over the network) is a separate program. That is two completely different pieces of code that must nonetheless enforce the exact same permissions. If they were written separately, the day would come when someone tightened a rule on the website and forgot the API — and a private issue would quietly leak to the desktop app. The whole design exists to make that impossible.

The fix is to write the rules in one place that neither front door owns. That place is `AccessContext`, and it lives in the deepest, most dependency-free project, `OpenTrack.Core`. Its own summary comment states the intent directly: it is "the single source of truth for every per-project access decision," kept "deliberately pure (no EF, no HTTP) so both the web/EF data service and the Web API endpoints call the exact same rules — they cannot drift — and so the whole matrix is unit-testable."

> **Jargon, in plain words** — A class is a bundle of related data and the operations on it. 'Pure' here means the class does no input/output — it never reads a file, a database, or the network; you hand it plain facts and it returns an answer, which makes it trivial to test. EF (Entity Framework) is the library that turns C# code into database queries. An enum is a fixed menu of named choices (here, the roles). A record struct is a small, immutable value holder: once built, its fields never change.


### The one idea: your effective role

Everything rests on a single concept: your *effective role* on a given project. A person has a *global role* (their account-wide rank) and, if they have been added to a project, a *per-project role* on it. The effective role is simply the higher of the two. This is the classic MantisBT model: a global Developer is a Developer everywhere, while someone whose global role is only Reporter can still be a Manager on the one project they were made a Manager of.

`AccessContext` is a three-field value — the user's id, their global role, and their optional project role — and the effective-role rule is one line:

```csharp
public readonly record struct AccessContext(int UserId, UserRole GlobalRole, UserRole? ProjectRole)
{
    /// <summary>The higher of the global role and the (optional) per-project role.</summary>
    public UserRole EffectiveRole =>
        ProjectRole is { } pr && (int)pr > (int)GlobalRole ? pr : GlobalRole;

    /// <summary>Global administrators bypass per-project scoping entirely.</summary>
    public bool IsGlobalAdmin => (int)GlobalRole >= (int)UserRole.Administrator;

    /// <summary>True when the user is an explicit member of the project in question.</summary>
    public bool HasProjectMembership => ProjectRole is not null;

    private bool AtLeast(UserRole role) => (int)EffectiveRole >= (int)role;
}
```

The roles are an ordered ladder — an enum whose numeric values climb from Reporter up through Updater, Developer, Manager, to Administrator. Because they are ordered, every rule can be written as a plain comparison: `AtLeast(UserRole.Developer)` just asks "is your effective role at least Developer?" That one tiny helper, `AtLeast`, is the vocabulary the entire rulebook is written in. Get the ladder right once, and every rule below reads like English.


### The rules themselves

With `EffectiveRole` and `AtLeast` in hand, the actual permissions are a list of small, named questions. Each is one line, each returns a clean yes/no, and each is named for exactly what it decides. A few of the project-level and issue-level ones:

```csharp
// ---- Project ----
public bool CanViewProject(bool projectIsPublic) =>
    IsGlobalAdmin || projectIsPublic || HasProjectMembership;

public bool CanManageProject() => AtLeast(UserRole.Manager);   // categories, versions, members

// ---- Issue ----
public bool CanCreateIssue(bool projectIsPublic) =>
    CanViewProject(projectIsPublic) && AtLeast(UserRole.Reporter);

public bool CanEditIssue()   => AtLeast(UserRole.Updater);
public bool CanAssignIssue() => AtLeast(UserRole.Developer);
```

Read them and the design philosophy is visible in the shape: viewing a project is generous (public, or a member, or an admin), while every action that changes something demands a minimum rank. Nothing here reaches for a database or a web request; you hand each method plain facts — is the project public? — and it returns a boolean. That purity is exactly what lets a single unit test drive the entire permission matrix through every role, with no server running.

> **Why booleans in, boolean out** — AccessContext never asks 'is this project public?' by looking it up — it is told, as a parameter. That is the whole trick behind its purity. The messy job of fetching the project's public flag, the issue's reporter, and so on belongs to the layer that has the database; AccessContext only judges. Judgment and data-fetching are kept apart on purpose.


### Closed by default: the private-issue rule

The most safety-critical rule is who may see a *private* issue. The rule is written so that privacy is the strong default: a private issue is hidden from everyone except the small set of people who are explicitly entitled to it.

```csharp
/// <summary>
/// A private issue is visible only to its reporter, its assignee, and users with Developer+
/// authority on the project (in addition to the project itself being viewable).
/// </summary>
public bool CanViewIssue(bool projectIsPublic, bool issueIsPrivate, int reporterId, int? assigneeId) =>
    CanViewProject(projectIsPublic)
    && (!issueIsPrivate
        || reporterId == UserId
        || assigneeId == UserId
        || AtLeast(UserRole.Developer));
```

Notice the structure. First you must be able to see the project at all. Then, if the issue is private, you are allowed through only if one of three specific things is true: you filed it, it is assigned to you, or your effective role is Developer or higher. If none of those hold, the private issue does not exist as far as you are concerned. The default answer — the answer you get unless you clear a specific bar — is no. The matching rule for private notes (`CanViewNote`) follows the identical shape: author, or Developer+, or it stays hidden.


### Two shapes of the same rules

There is a practical wrinkle. `AccessContext.CanViewIssue` judges *one* issue when you already hold its facts — perfect for opening a single issue. But a list screen may match ten thousand issues, and you cannot load ten thousand rows into memory just to ask `CanViewIssue` about each one; that would be slow and would defeat the database. Lists need the same rules expressed as a *filter* the database itself can apply. So the same policy exists in two shapes, and keeping them agreeing is the subtle heart of this chapter.

The second shape needs the user's whole access picture at once — not one project's role, but every project they belong to. That picture is `AccessSnapshot`, loaded a single time per request. From the raw membership rows it precomputes two id lists that the filters will need: every project the user is a member of, and the subset where their role alone is Developer-or-higher (so they may see private issues there).

```csharp
public sealed class AccessSnapshot
{
    /// <summary>Ids of every project the user is a member of (any role).</summary>
    public IReadOnlyList<int> MemberProjectIds { get; }

    /// <summary>Ids of projects where the user's membership role alone grants Developer+ (private issues).</summary>
    public IReadOnlyList<int> DeveloperProjectIds { get; }

    /// <summary>Builds the pure per-project decision context for one project.</summary>
    public AccessContext For(int projectId) =>
        new(UserId, GlobalRole, _projectRoles.TryGetValue(projectId, out var role) ? role : null);
}
```

That `For(projectId)` method is the bridge between the two shapes: from the loaded snapshot it hands back a pure `AccessContext` for any single project, so single-item checks and list filters are fed from the very same source of truth. Load once, judge many.


### Why the list filter runs inside the database

The list shape lives in `VisibilityQueries`, a set of filters you attach to a query. Its summary says the two surfaces "enforce identical row-level security," and — the performance-critical part — the predicates are "written entirely in terms of local id lists so EF Core translates them to SQL rather than pulling rows into memory to filter."

> **Jargon, in plain words** — An IQueryable is a query you are still building — it has not run yet. A predicate is the 'where' condition inside it. EF Core reads that condition and translates it into real database SQL, so the filtering happens in the database engine and only the rows you are allowed to see ever travel back. 'Row-level security' just means the rules decide which individual rows (issues) you can see, not merely which tables.

```csharp
public static IQueryable<Issue> WhereVisibleTo(this IQueryable<Issue> query, AccessSnapshot access)
{
    if (access.IsGlobalAdmin)
        return query;

    var memberIds = access.MemberProjectIds;
    var developerProjectIds = access.DeveloperProjectIds;
    var globalDeveloperPlus = access.GlobalAtLeast(UserRole.Developer);
    var userId = access.UserId;

    return query.Where(i =>
        (i.Project.IsPublic || memberIds.Contains(i.ProjectId))
        && (!i.IsPrivate
            || globalDeveloperPlus
            || i.ReporterId == userId
            || i.AssigneeId == userId
            || developerProjectIds.Contains(i.ProjectId)));
}
```

Read the `Where` condition beside `AccessContext.CanViewIssue` from earlier and you will see they are the same rule, twice: the project must be public or one you belong to, and a private issue is allowed only if you are a global Developer+, its reporter, its assignee, or a Developer+ member of that project. One is phrased for a single row you already hold; the other is phrased so the database can apply it to millions. They must stay in lockstep — and because both are grounded in the same roles ladder and the same `AccessSnapshot`, changing the policy means changing this shared understanding in one small neighborhood of code, not hunting across the app.


### How both front doors reach the same rules

Now the anti-drift payoff. On the web side, `DbOpenTrackDataService` builds an `AccessSnapshot` for the signed-in user and calls `WhereVisibleTo` on its queries and `AccessContext` on its single-item checks. On the API side, a tiny helper does the identical load, pointedly documented as using "the exact same rules as the web/EF path":

```csharp
// OpenTrack.API/ApiAuthorization.cs
public static class ApiAccess
{
    public static async Task<AccessSnapshot?> LoadAsync(
        ClaimsPrincipal user, AppDbContext db, CancellationToken ct = default)
    {
        var identity = user.GetAccessIdentity();
        return identity is null ? null : await AccessSnapshot.LoadAsync(db, identity.Value, ct);
    }
}
```

Both hosts load the same `AccessSnapshot` from the same `ProjectMemberships` table and pass it to the same `VisibilityQueries` and `AccessContext`. Neither host contains a single hand-written permission rule of its own; they only gather the caller's identity and then defer to the shared authority. That is the mechanism that makes drift not merely unlikely but structurally hard: there is nowhere for a second, divergent copy of the rules to live.

> **The global-admin fast path, stated plainly** — Every layer short-circuits for a global Administrator: AccessContext.IsGlobalAdmin returns true, and both WhereVisibleTo filters simply return the unfiltered query. Admins bypass per-project scoping by design. This is deliberate and load-bearing — it is also the one rule to be most careful around when editing, because it is the widest grant in the system.


## Why It Matters / Design Takeaways

If a future maintainer preserves only one thing about this subsystem, preserve the shape: the rules are pure, they live in Core with no database or web dependency, and every surface is fed by them rather than reimplementing them. The per-item `AccessContext` and the list-level `VisibilityQueries` are two expressions of one policy, kept honest by a shared roles ladder and a shared `AccessSnapshot`. That is what lets a single test suite prove the whole matrix, and what keeps the browser and the desktop app from ever disagreeing about who can see an issue.

The rules that must never erode: permissions stay pure and database-free (so they stay testable and shareable); a private thing is hidden unless the viewer is explicitly entitled (closed by default); list filters stay expressed in local id lists so they run as SQL, not in memory; and no host grows its own private copy of a permission check. Add a feature that exposes data someday and the safe move is not to write a fresh access check in your endpoint — it is to route it through the authority that already exists.

> **The maintainer's rule** — If you ever find yourself writing a fresh 'is this user allowed?' check inside a page or an endpoint, stop. That is the exact scattering this design was built to prevent. Add the rule to AccessContext (for a single item) or VisibilityQueries (for a list), keep the two in agreement, and call them from both hosts — one rulebook, always, closed until the rules prove open.


# 9. The Operations Pattern: one place per action, so Web and API never drift

*The recurring habit that keeps OpenTrack honest — every meaningful action (tag an issue, change a status rule, set an SLA target, link two issues, apply a change to a hundred issues at once) is written exactly once in a static 'Operations' class, permission checks and all, and called identically by both the website and the API so the two surfaces can never enforce different rules.*


## What This Is / What It Is For

OpenTrack does a lot of specific things: it tags an issue, removes a tag, defines which status changes a project allows, sets how many hours a high-priority issue may sit before it breaches its service target, links two issues as duplicates, and applies one change to a whole batch of issues at once. Each of those is an *action* — a small unit of real work with its own rules about who may do it and what counts as valid. This chapter is about the single habit OpenTrack uses to keep every one of those actions correct: write it once, in a plainly named place, and have everyone call that one copy.

The name of the habit is the Operations pattern. For each cluster of related actions there is a class named for it — `TagOperations`, `WorkflowOperations`, `SlaPolicyOperations`, `RelationshipOperations`, `BulkOperations`, `AutomationRuleOperations`, and more — and inside each class the actions are ordinary methods. The website calls them. The API calls the same ones. Neither writes its own version. It is the same anti-drift motive that drives the access-control authority in Chapter 8, applied one level up: not just "who is allowed?" written once, but the entire action — validation, permission check, database change, and all — written once.

> **Jargon, in plain words** — A static class is a class you never create an instance of — it is just a home for a set of related functions you call directly, like TagOperations.AddAsync(...). 'Drift' is when two copies of the same logic slowly stop matching because someone changed one and forgot the other. An ACL (access-control list) is the set of rules about who may see or change something. 'Idempotent' means doing the same thing twice has the same effect as doing it once — tagging an already-tagged issue changes nothing and reports success.

> **The one-sentence version** — Every action lives once in a static Operations class — permission checks, validation, and database work together — and both the web app and the API call that single copy, so the two front doors can never enforce different rules for the same action.


### The drift problem, and why one copy fixes it

Recall from Chapter 8 that OpenTrack has two front doors: the Blazor Server website, which talks to the database directly, and the API, a separate program the desktop app calls over the network. Both must let a user tag an issue — but with the same rules: you may only tag an issue you can see, and only if you have edit rights on its project. If the website and the API each wrote their own tagging code, the day would come when someone tightened the rule in one and forgot the other, and suddenly the desktop app would let a user tag an issue the website would have refused.

The Operations pattern removes the second copy entirely. There is exactly one `TagOperations.AddAsync`, and it contains the whole action — check the issue is visible, check the caller may edit it, find-or-create the tag, link it, save. The website calls it; the API calls it. There is nowhere for a divergent second version to live. `TagOperations`' own summary states the intent directly: it holds "ACL-aware tag operations on issues, shared by both the Web API and the web/EF data service so the authorization logic exists once."


### The common shape every Operations class shares

Before looking at individual classes, it helps to see the mold they are all cast from. Almost every method across these classes has the same first three ingredients in its signature, and the same handful of habits in its body. Once you have read one, you can read them all.

- *It takes the database and the caller's access.* Nearly every method's first two parameters are `AppDbContext db` and `AccessSnapshot access` — the database to work in, and the caller's full permission picture (the load-once snapshot from Chapter 8).
- *It checks permission before it acts.* It asks `access.For(projectId)` for the pure per-project decision context and calls a named rule like `CanEditIssue()` or `CanManageProject()` — the same rules from the access-control authority, never a fresh hand-written check.
- *A read that you're not allowed returns empty.* A list method a user may not see returns `[]` (an empty list), not an error — you simply see nothing.
- *A write you're not allowed throws.* A create/update/delete a user may not perform throws `UnauthorizedAccessException` with a plain-English reason.
- *A validation problem returns a message string.* Many write methods return `Task<string?>`: `null` means success, and a non-null string is the human-readable reason it was rejected (a bad name, a duplicate). This cleanly separates 'you are not allowed' (an exception) from 'that input was invalid' (a returned message).
- *It carries a CancellationToken.* Every async method threads `CancellationToken ct` through, so a request that is abandoned can stop cleanly.

> **Why 'return empty' for reads but 'throw' for writes** — The split is deliberate. Showing a list is a soft operation — if you may not see something, the honest, quiet answer is an empty list, which also avoids revealing that anything existed to hide. Changing data is a hard operation — if you try to change something you have no right to, that is a genuine error the calling code must handle loudly, so it throws. Reads stay quiet; writes are strict.


### TagOperations: read vs. edit, and create-on-assign

Tags are the simplest full example. Viewing an issue's tags needs only view access to the issue; adding or removing one needs edit access (Updater or higher). Both checks route through the same access-control authority via a tiny private helper at the bottom of the class:

```csharp
private static bool CanView(AccessSnapshot access, Issue issue) =>
    access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId);
```

The `AddAsync` method shows the full shape in miniature — view gate, edit gate, validation, then the actual work — and it also handles a subtle real-world wrinkle: tags are created the first time they are used ("create-on-assign"), and two people might create the same tag at the same instant. Watch how it opens:

```csharp
public static async Task<string?> AddAsync(AppDbContext db, AccessSnapshot access, int issueId, string tagName, CancellationToken ct = default)
{
    var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
    if (issue is null || !CanView(access, issue)) return "Issue not found.";
    if (!access.For(issue.ProjectId).CanEditIssue())
        throw new UnauthorizedAccessException("Tagging an issue requires the Updater role on its project.");

    var name = tagName.Trim();
    if (name.Length == 0) return "Tag name is required.";
    if (name.Length > FieldLimits.TagName) return $"Tag name must be {FieldLimits.TagName} characters or fewer.";
```

Every habit is visible in those few lines: it may-I-see (return a soft "not found" if not — never revealing whether the issue exists), then may-I-edit (throw if not), then validate the input (return a message if the name is empty or too long). Only after all three does it touch the database. And when it does, it guards the create-on-assign race explicitly — if a unique-index clash means someone else just created the same tag, it recovers instead of failing:

```csharp
tag = new Tag { Name = name };
db.Tags.Add(tag);
try { await db.SaveChangesAsync(ct); }
catch (DbUpdateException) // possibly the unique-index race: another request created it
{
    db.Entry(tag).State = EntityState.Detached;
    // If the tag now exists it WAS the race; otherwise this was a different failure — rethrow.
    tag = await db.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == lowered, ct);
    if (tag is null) throw;
}
```

The important design point is that this careful logic — the permission gates, the case-insensitive reuse, the race recovery, the idempotent "already tagged" check — exists exactly once. When the API's tag endpoint and the website's tag button both call `TagOperations.AddAsync`, they inherit every bit of that care for free, identically.


### WorkflowOperations and SlaPolicyOperations: manager-gated settings

Some actions are not about a single issue but about a project's *configuration* — and those raise the bar to the Manager role. Two examples that mirror each other almost exactly show how consistent the pattern is.

`WorkflowOperations` governs which status changes a project allows (defining any transitions makes the project's workflow "restricted"; defining none leaves it open). Managing those rules is manager-only, and the class states it: "Managing rules is Manager-only; the allow-check is used by every issue-status write path so the API, web, board, and bulk actions all enforce the same workflow." Its `AddAsync` gates on `CanManageProject()`:

```csharp
public static async Task<string?> AddAsync(
    AppDbContext db, AccessSnapshot access, int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
{
    if (!access.For(projectId).CanManageProject())
        throw new UnauthorizedAccessException("Managing the workflow requires the Manager role on this project.");
    if (from == to) return "Pick two different statuses.";
    if (await db.WorkflowTransitions.AnyAsync(w => w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct))
        return "That transition is already allowed.";
    db.WorkflowTransitions.Add(new WorkflowTransition { ProjectId = projectId, FromStatus = from, ToStatus = to });
    await db.SaveChangesAsync(ct);
    return null;
}
```

That class also exposes a second kind of method — not "change a setting" but "answer a question the rest of the app needs": `IsAllowedAsync`, which every status-changing code path consults to ask "is moving this issue from this status to that status permitted?" Because that one method is the single authority on the answer, the API, the website, the status board, and bulk edits all obey the identical workflow. (We will see `BulkOperations` call it below.)

```csharp
public static async Task<bool> IsAllowedAsync(
    AppDbContext db, int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
{
    if (from == to) return true;
    var any = await db.WorkflowTransitions.AsNoTracking().AnyAsync(w => w.ProjectId == projectId, ct);
    if (!any) return true; // no workflow defined => open
    return await db.WorkflowTransitions.AsNoTracking()
        .AnyAsync(w => w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct);
}
```

`SlaPolicyOperations` is cut from the identical cloth for service-target settings (how many hours an issue of a given priority may sit before it breaches). Its summary even names the shared template: it "Mirrors the other per-project operations classes: reads return an empty list for a non-manager; writes throw." Its list method is the reads-return-empty habit in one line:

```csharp
public static async Task<IReadOnlyList<SlaPolicy>> ListForProjectAsync(
    Data.AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
{
    if (!access.For(projectId).CanManageProject()) return [];
    return await db.SlaPolicies.AsNoTracking()
        .Where(p => p.ProjectId == projectId)
        .OrderBy(p => p.Priority)
        .ToListAsync(ct);
}
```

It even folds in a small safety cap of its own — `MaxTargetHours = 100_000` ("≈ 11 years"), "a sanity cap so a fat-fingered target can't overflow date math." `AutomationRuleOperations` follows the exact same manager-gated create/update/delete shape, right down to a `Validate` helper that returns a rejection message. Once you have read two of these, the third reads itself — which is precisely the benefit of a consistent pattern.


### RelationshipOperations: keeping a private issue's existence secret

Linking issues to one another is where the pattern earns its keep on a genuinely tricky security question. When you view an issue's related issues, the list must never reveal a related issue you are not allowed to see — not even the fact that it exists. `RelationshipOperations` handles this in one place, and its summary spells out the stakes: "the related-issue list only ever includes issues the viewer may see (so a private issue's existence isn't leaked)."

The read method loads every link touching this issue, then filters each partner through the same `CanView` gate before including it — silently dropping the ones you may not see:

```csharp
foreach (var r in rels)
{
    var viewerIsSource = r.SourceIssueId == issueId;
    var other = viewerIsSource ? r.TargetIssue : r.SourceIssue;
    if (!CanView(access, other)) continue; // never leak a private issue's existence
    items.Add(new RelationshipItem(r.Id, other.Id, other.Title, other.Project.Name,
        RelationshipLabels.Describe(r.Type, viewerIsSource)));
}
```

The same discretion runs through the writes. When adding a link, if the target issue is one you cannot see, the method reports it as simply "not found" rather than "access denied" — because "access denied" would itself confirm the issue exists:

```csharp
var target = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == targetIssueId, ct);
// Report "not found" (not "no access") so a private issue's existence is never revealed.
if (target is null || !CanView(access, target)) return $"Issue #{targetIssueId} not found.";
```

And removal applies the same no-existence-signal rule to relationship ids themselves: if you can view neither end of a link, the delete returns a plain `false` ("treat as not-found") so "a denied delete is indistinguishable from a nonexistent id." This is delicate, closed-by-default reasoning — exactly the sort of thing you want written once, tested once, and reused everywhere, rather than re-derived in each endpoint that happens to touch relationships.


### BulkOperations: the same per-item rules, a thousand times

The most persuasive demonstration of the pattern is bulk editing — applying one action (set status, close, assign, add a tag) to many issues at once. The tempting shortcut would be to check permission once for the batch and then blast the change across every id. `BulkOperations` refuses that shortcut on purpose. Its summary is emphatic: it enforces "the SAME per-issue authorization as a single edit: an issue the caller can't see, or can't perform this action on, is silently SKIPPED (never a partial leak or a blanket allow)."

So it loops one issue at a time and re-runs every gate for each. It first bounds the batch (`MaxBatch = 1000`, "so a crafted API request can't submit an unbounded id list"), then, per issue, checks visibility and the action-specific permission before making any change:

```csharp
var issue = await db.Issues.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id, ct);
if (issue is null) { skipped++; continue; }
var ctx = access.For(issue.ProjectId);
if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
{ skipped++; continue; }
```

Then each action type applies its own required permission — and, crucially, reuses the *other* Operations classes rather than reinventing their rules. A status change consults `WorkflowOperations.IsAllowedAsync`; a tag add calls straight into `TagOperations.AddAsync`, inheriting its edit-check and idempotency:

```csharp
case BulkActionType.SetStatus when action.Status is { } s:
    if (!ctx.CanEditIssue()) { skipped++; continue; }
    if (!await WorkflowOperations.IsAllowedAsync(db, issue.ProjectId, issue.Status, s, ct)) { skipped++; continue; }
    issue.Status = s; summary = $"status set to {s}";
    break;
// ...
case BulkActionType.AddTag when !string.IsNullOrWhiteSpace(action.Tag):
    if (!ctx.CanEditIssue()) { skipped++; continue; }
    // Reuse the tag ACL/create-on-assign logic; it re-checks edit and is idempotent.
    try { await TagOperations.AddAsync(db, access, id, action.Tag, ct); }
    catch (UnauthorizedAccessException) { skipped++; continue; }
    updated++;
    continue;
```

This is the pattern feeding on itself, in the best way: `BulkOperations` does not know or duplicate the tagging rules or the workflow rules — it *calls* the classes that own them. The result is that a bulk change can never do something a one-at-a-time change would have refused, because under the hood a bulk change *is* a series of one-at-a-time changes through the very same code. Assigning even re-checks that the new assignee is actually a member of the issue's project, exactly as a single assign would. Each changed issue also records its history and fires a notification, "exactly like an individual edit," and the method returns a tidy count of how many were updated versus skipped.


### The unwritten contract these classes all follow

Step back and the whole family shares one contract, even though no interface forces it. Every Operations class is static and stateless; every method takes the database and the caller's `AccessSnapshot`; every method checks permission through the shared access-control authority before acting; reads return empty when unauthorized, writes throw, and invalid input comes back as a message; nothing reveals the existence of something you may not see; and higher-level actions reuse lower-level Operations rather than copying them. Learn that contract once and every class in the family is predictable.

This is also why the pattern scales. OpenTrack has many of these — tags, workflow, SLA, automation, relationships, bulk, custom fields, checklists, saved filters, preferences, webhooks, time logs, Git integration, public intake — and adding the next feature does not mean inventing a new structure. You write one more `...Operations` class in the same mold, and both hosts pick it up by calling it. The shape is the documentation.


## Why It Matters / Design Takeaways

If Chapter 8 was "write the permission *rules* once," this chapter is "write the whole *action* once." The Operations classes are where a feature's real behavior lives — its validation, its permission gates, its database work, its careful handling of races and secrecy — and because that behavior exists in exactly one static method per action, the website and the API cannot enforce different versions of it. There is no second copy to fall out of step.

The habits that must not erode: keep each action in one shared Operations method; always take the caller's access and check it through the access-control authority, never with a fresh inline rule; keep the reads-return-empty / writes-throw / invalid-returns-a-message split so callers can react correctly; never leak the existence of something a user may not see; and when a new action naturally builds on an existing one, call the existing Operations class instead of re-deriving its rules. A bulk action that quietly reuses the single-edit rules is the pattern working as intended.

> **The maintainer's rule** — When you add an action, do not write its logic inside a page or an endpoint. Create (or extend) a static ...Operations class that takes AppDbContext and AccessSnapshot, gate it through the access-control authority, return empty for forbidden reads and throw for forbidden writes, and then call that one method from both hosts. If you ever find the same action implemented twice, you have found the exact drift this pattern exists to prevent — collapse it back to one.


# 10. Queries & Row-Level Security

*How every list, dashboard, report, roadmap, and duplicate-finder in OpenTrack starts from the same visibility filter — so the rules about which issues you may see are applied inside the database itself, in SQL, before a single forbidden row ever travels back to the screen.*


## What This Is / What It Is For

A screen full of issues, a dashboard of tallies, a report chart, a project roadmap, a "you might be filing a duplicate" hint — these are all *lists* built from many rows at once. And every one of them faces the same danger: it must show you only the issues you are allowed to see, and it must do so without accidentally counting, or even briefly loading, the ones you are not. This chapter is about the query classes that build those lists, and the single shared filter they all lean on to stay safe.

Chapter 8 introduced the two shapes of OpenTrack's permission rules: a per-item yes/no for opening one issue, and a database filter for lists. This chapter is where that list filter goes to work. The query classes here — `IssueQueries`, `SimilarIssueQuery`, `RoadmapQuery`, `DashboardQuery`, `ReportQuery` — all begin from the same starting line, `WhereVisibleTo`, and only then do their own particular job. Get that starting line right and every list in the app is secure by construction.

> **Jargon, in plain words** — Row-level security means the rules decide which individual rows (which issues) you may see, not merely which tables — issue #5 might be visible to you and issue #6 hidden, in the same list. A query is a request for data you are still assembling; it does not run until you ask for the results. When the query runs 'in SQL', the database engine does the filtering and sends back only the allowed rows. When it runs 'in memory', the app pulls every row into itself first and then throws some away — slower, and briefly holding data it should never have loaded.

> **The one-sentence version** — Every list query starts by attaching WhereVisibleTo, which the database turns into a SQL filter, so only the issues you are allowed to see are ever fetched — and each query then adds its own filtering and sorting on top of that already-safe foundation.


### Why the filter has to run inside the database

Imagine a search that matches ten thousand issues, of which you are allowed to see a few hundred. The naive approach would be to load all ten thousand into the app, then loop through and keep only the ones you may see. That is wrong on two counts. It is slow — you have hauled ten thousand rows across for nothing. And it is unsafe in spirit — for a moment, the application is holding thousands of issues the user has no right to, trusting itself not to slip and show one.

The right approach is to hand the visibility rule to the database as part of the query, so the database never sends back the forbidden rows at all. That is exactly what `WhereVisibleTo` (from Chapter 8) is built for. Its summary states the performance-critical promise: its predicates "are written entirely in terms of local id lists (from AccessSnapshot) so EF Core translates them to SQL rather than pulling rows into memory to filter." Because the filter is phrased only in terms of simple id lists and plain comparisons — never anything the database cannot understand — EF Core can express the whole rule as a SQL `WHERE` clause. Here is the issue filter again, the shared engine every list in this chapter runs on:

```csharp
public static IQueryable<Issue> WhereVisibleTo(this IQueryable<Issue> query, AccessSnapshot access)
{
    if (access.IsGlobalAdmin)
        return query;

    var memberIds = access.MemberProjectIds;
    var developerProjectIds = access.DeveloperProjectIds;
    var globalDeveloperPlus = access.GlobalAtLeast(UserRole.Developer);
    var userId = access.UserId;

    return query.Where(i =>
        (i.Project.IsPublic || memberIds.Contains(i.ProjectId))
        && (!i.IsPrivate
            || globalDeveloperPlus
            || i.ReporterId == userId
            || i.AssigneeId == userId
            || developerProjectIds.Contains(i.ProjectId)));
}
```

The rest of this chapter is really one idea shown five times: each query class attaches this filter first, and builds its own result on top of a foundation that is already trimmed to exactly what the user may see.


### The golden order: security, then filter, then sort

`IssueQueries` builds the main issue list from a search form — project, status, severity, assignee, tag, a text search, and a sort order. The important thing about it is not any single filter but the *order* in which the pieces stack, which its summary states as a rule: "Call it AFTER WhereVisibleTo so filtering only ever narrows the rows a user may already see."

> **Why order is the whole game** — Because a query is assembled in layers and only runs at the end, each `.Where(...)` you add can only shrink the set further — it can never add rows back. So if the very first layer is WhereVisibleTo, everything stacked after it is trapped inside the visible set. Put the visibility filter first and no later search condition — however it is written — can ever surface an issue the user may not see. The safety comes from the sequence, not from remembering to re-check.

The `ApplyFilter` method itself is a tidy list of optional narrowings — each one only added if the user asked for it — followed by the sort. Its search-text handling shows the same care about not leaking through a side channel:

```csharp
if (!string.IsNullOrWhiteSpace(filter.Text))
{
    // Case-insensitive contains: EF translates string.Contains to a case-SENSITIVE match on
    // SQLite (instr), so lower-case both sides to get the search behaviour users expect. Also
    // match PUBLIC note text — private notes are deliberately excluded so search can't reveal
    // that a note the searcher can't read contains their term.
    var text = filter.Text.Trim().ToLower();
    query = query.Where(i =>
        i.Title.ToLower().Contains(text)
        || i.Description.ToLower().Contains(text)
        || i.Notes.Any(n => !n.IsPrivate && n.Text.ToLower().Contains(text)));
}
```

Notice the `!n.IsPrivate` guard on the note search: text search will match a public note but deliberately not a private one, "so search can't reveal that a note the searcher can't read contains their term." This is the same closed-by-default instinct as everywhere else — a search must not become a way to peek at hidden content. After all the narrowing comes the sort, which always pins sticky issues first and then applies the requested order. Both hosts call `ApplyFilter`, so "search behaves identically on both surfaces and can't drift."


### The dashboard: even the totals only count what you may see

A list is the obvious place to worry about visibility, but a dashboard is sneakier: it shows *numbers* — how many issues are open, how many overdue, a breakdown by severity — and a number can leak just as surely as a row. If the "open issues" count included issues in a private project you are not a member of, the count itself would betray that those issues exist. `DashboardQuery` prevents that by starting from the visible set and computing every tally from it. Its summary: "Everything is scoped through VisibilityQueries first, so a user never sees a tally or a row for an issue they couldn't open directly."

The very first line establishes the safe foundation, and every count afterward is built from that `visible` starting point — never from the raw table:

```csharp
var visible = db.Issues.AsNoTracking().WhereVisibleTo(access);
var open = visible.Where(i => (int)i.Status < (int)IssueStatus.Resolved);
var staleBefore = nowUtc.AddDays(-IssueDefaults.StaleDays);
var totalStale = await open.CountAsync(i => i.UpdatedAt < staleBefore, ct);

var projects = await open
    .GroupBy(i => new { i.ProjectId, ProjectName = i.Project.Name })
    .Select(g => new DashboardProjectTally(
        g.Key.ProjectId,
        g.Key.ProjectName,
        g.Count(),
        g.Count(i => i.DueDate != null && i.DueDate < nowUtc)))
    .ToListAsync(ct);
```

Because `open` is derived from `visible`, and every tally — the stale count, the per-project grouping, the severity breakdown, the ten most-recent rows — is derived from `open` or `visible` in turn, there is no path by which a hidden issue can affect a single number on the dashboard. The counting and grouping also happen in the database (`CountAsync`, `GroupBy`), so the app receives finished tallies rather than raw rows. `ReportQuery` follows the same discipline for its charts, opening with `var q = db.Issues.AsNoTracking().WhereVisibleTo(access);` and computing every figure — totals, created-per-month, open-by-status, open-by-severity — over that visible set. Its summary names the habit outright: "over the issues a user may see (ACL first)."


### Roadmap and duplicate-finder: ACL first, no exceptions

The pattern holds even for features that seem far from a plain issue list. `RoadmapQuery` builds a project's roadmap and changelog from its versions and the issues targeted at each. It guards at two levels — first the project itself, then the issues within it — and both guards route through the access-control authority. It refuses outright if you cannot view the project:

```csharp
var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return [];

var versions = await db.Versions.AsNoTracking()
    .Where(v => v.ProjectId == projectId)
    .Select(v => new { v.Id, v.Name, v.IsReleased, v.ReleaseDate })
    .ToListAsync(ct);
if (versions.Count == 0) return [];

var issues = await db.Issues.AsNoTracking().WhereVisibleTo(access)
    .Where(i => i.ProjectId == projectId && i.FixVersionId != null)
    .Select(...)
    .ToListAsync(ct);
```

So the roadmap's progress bars and changelog counts are computed only from issues the caller may see — "only issues the caller may see are counted or listed, and a project the caller can't view yields nothing." A private issue you cannot see never nudges a version's "12 of 20 done" figure.

`SimilarIssueQuery`, the helper that warns "this looks like a duplicate" while you type a new title, is perhaps the most tempting place to cut a corner — it only wants a few candidate matches — yet it applies the filter first too. Its summary: "ACL first (only issues the user could open are ever considered), then a coarse word-overlap match ranked in memory." The base query it searches from is visibility-filtered before any word-matching happens:

```csharp
var baseQuery = db.Issues.AsNoTracking().WhereVisibleTo(access);
if (projectId is { } pid) baseQuery = baseQuery.Where(i => i.ProjectId == pid);
if (excludeIssueId is { } ex) baseQuery = baseQuery.Where(i => i.Id != ex);

// Union a "title contains this word" query per significant word (EF translates each to a LIKE).
var candidates = words
    .Select(w => baseQuery.Where(i => i.Title.ToLower().Contains(w)))
    .Aggregate((a, b) => a.Union(b));
```

This matters because a duplicate-finder that ignored the ACL would be an information leak dressed up as a convenience: type a guess at a private issue's title and it would helpfully confirm the match. Building every candidate query on top of `WhereVisibleTo` means the suggestion box can only ever point you at issues you were already allowed to open.

> **A fair split of labor** — Notice that these queries do the security and the heavy narrowing in the database (WhereVisibleTo and the word LIKE-matches run as SQL), then do a little final ranking in memory (scoring by how many words overlap). That is a sensible division: the database is the right place to enforce who-may-see and to cut millions of rows down to a handful, and once you hold only that safe handful, a touch of in-memory scoring is cheap and clear. The security is never the part left to memory.


### One filter, many faces

Lay the five classes side by side and the shared spine is unmistakable. The issue list, the dashboard, the reports, the roadmap, and the duplicate-finder each begin by attaching the same visibility filter (`WhereVisibleTo`, or its per-project sibling `CanViewProject`), and only then add their own logic — a search form, a set of tallies, a chart, a version rollup, a fuzzy title match. None of them re-implements "who may see this issue." They all borrow it from the one place that defines it, and they all borrow it *first*.

That is what makes the whole surface trustworthy without heroics. A maintainer adding a sixth list next year does not need to reason freshly about privacy; they need to remember one habit — start from `WhereVisibleTo` — and the closed-by-default guarantee comes along for free, applied in SQL, before any forbidden row is ever fetched.


## Why It Matters / Design Takeaways

The shape to preserve is "security is the first layer of every list, and it runs in the database." Because a query is assembled in layers that can only narrow the result, putting `WhereVisibleTo` at the bottom means nothing stacked above it — no search term, no grouping, no ranking — can ever surface an issue the user may not see. And because that filter is written in plain id lists and comparisons, it translates to SQL, so forbidden rows are never even fetched, let alone counted or displayed. Lists stay fast and closed by default at the same time.

The rules that must not erode: every list, tally, chart, and suggestion starts from `WhereVisibleTo` (or the project-level view check) before it does anything else; visibility filtering stays expressed in local id lists so it runs as SQL, never as an in-memory sweep; searches must not leak through side channels (the private-note exclusion is the model to copy); and numbers are as sensitive as rows — count only over the visible set. When you add a new way to list or summarize issues, do not write a fresh visibility check; attach the shared one, and attach it first.

> **The maintainer's rule** — Any query that reads more than one issue must begin with WhereVisibleTo, and begin with it before adding filters, groupings, sorts, or ranking. If you ever find yourself filtering for visibility in memory after loading rows — or counting a total off the raw table instead of the visible set — stop and move the filter into the query, first. Security is the foundation layer, it lives in SQL, and every list in the app is only as safe as its first line.


# 11. The IOpenTrackDataService Seam

*One interface that the shared screens talk to, with two completely different bodies behind it — the web app fills it in by reaching straight into the database, the desktop app fills it in by making network calls — so the very same Blazor pages run, unchanged, in both places.*


## What This Is / What It Is For

*OpenTrack* has one set of screens — the pages you click through to list projects, open an issue, add a note, manage members. Those screens live in a single project, `OpenTrack.UI`, and they are shared: the web app in your browser and the desktop app on your Windows or Mac both show the exact same pages. That raises an obvious question. The web app has a database sitting right next to it; the desktop app does not — it is a small program on your laptop that has to reach a server over the network. So how can one set of screens work in two such different worlds?

The answer is a *seam*: `IOpenTrackDataService`. It is a single interface — a written list of every data operation a screen could ever ask for ("get the projects," "create an issue," "add a note") — with no actual code behind the operations. The screens are written to talk only to this list. Each host then supplies its own body for the list: the web app's body reaches into the database; the desktop app's body makes network calls. The screens never know which body they got. That is the whole trick, and this chapter is about how it is written and why it is shaped this way.

> **Jargon, in plain words** — An interface is a contract: a list of named operations with their inputs and outputs, but no working code. A class that 'implements' the interface promises to provide a real body for every operation on the list. A 'seam' is a deliberate line drawn through a program where you can swap what is on one side without disturbing the other. 'Dependency injection' (DI) is the mechanism that, at startup, decides which real class fills in an interface and hands it to whatever asked for the interface — so the screens receive whichever body the host chose.

> **The one-sentence version** — The shared screens depend on one interface, IOpenTrackDataService, and never on a concrete data source; the web app implements it with direct database access and the desktop app implements it with HTTP calls, so the identical pages run in both hosts and neither the pages nor the interface know or care which body is behind them.


### Why write the screens against a list they don't own

The plain reason is reuse. Writing an issue tracker's screens is the bulk of the work — the forms, the validation, the layout, the little confirmations. Doing that twice, once for the web and once for the desktop, would mean two copies drifting apart forever. So OpenTrack writes the screens once and makes them depend on nothing more specific than "something that can fetch and change OpenTrack data." That "something" is the interface. Whoever is hosting the screens plugs in the appropriate "something" at startup.

The interface's own summary comment states this outright — the pages depend only on the interface, so the same components run in both hosts:

```csharp
/// <summary>
/// The single data-access seam for OpenTrack's shared Blazor UI. The web app implements
/// this with direct EF Core access (DbOpenTrackDataService); the desktop app implements it
/// by calling OpenTrack.API over HTTP (HttpOpenTrackDataService). The CRUD pages depend
/// only on this interface, so the exact same components run in both hosts.
/// ...
/// </summary>
public interface IOpenTrackDataService
```

> **Jargon, in plain words** — CRUD is shorthand for the four everyday things software does to stored records: Create, Read, Update, Delete. A 'CRUD page' is just a screen that creates, shows, edits, or removes something — an issue, a project, a note. EF Core (Entity Framework Core) is the library the web app uses to turn C# into database queries; you will meet it in depth in the Infrastructure chapters.


### The interface itself: one flat menu of operations

`IOpenTrackDataService` is long but not complicated — it is a menu. Each line names one operation, says what it needs, and says what it hands back. There is no logic here, only promises. A representative slice:

```csharp
// Projects
Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default);
Task<ProjectDetail?> GetProjectAsync(int id, CancellationToken ct = default);
Task<int> CreateProjectAsync(CreateProjectInput input, CancellationToken ct = default);
Task UpdateProjectAsync(int id, UpdateProjectInput input, CancellationToken ct = default);

// Issues
Task<IReadOnlyList<IssueRow>> GetIssuesAsync(IssueFilter filter, CancellationToken ct = default);
Task<IssueDetail?> GetIssueAsync(int id, CancellationToken ct = default);
Task<int> CreateIssueAsync(int projectId, CreateIssueInput input, CancellationToken ct = default);
Task AddIssueNoteAsync(int issueId, string text, bool isPrivate = false, CancellationToken ct = default);
```

A few things are worth reading off these lines. Every operation returns a `Task` — it is asynchronous, meaning the screen can ask for the data and stay responsive while it arrives, rather than freezing. Every operation takes a `CancellationToken` — a little 'never mind' handle, so if you navigate away mid-load the work can be dropped. And the inputs and outputs are plain view models (`ProjectRow`, `IssueDetail`, `CreateIssueInput`) that carry no hint of a database or a network. The menu describes *what* the screens want, never *how* it is fetched.

> **Jargon, in plain words** — 'Asynchronous' (async) work is work you start now and collect later, without standing still while it runs. A Task is C#'s receipt for such work — 'the answer will be here shortly.' A 'view model' is a small, plain data holder shaped for a screen to display, with no behavior of its own. The trailing 'Async' on method names is a naming convention that flags them as asynchronous.


### The rule that keeps the pages identity-agnostic

There is one design decision on this interface easy to miss and important to preserve: *no operation is told who is asking.* Look again — `GetProjectsAsync` takes only a cancellation token; `GetIssueAsync` takes only an id. Nowhere does a page pass in "and by the way, the current user is Jim." That is deliberate, and the interface comment explains why:

```csharp
/// The current-user identity is NOT passed in per-call: each implementation resolves it
/// from its own context (the web app from the authenticated ClaimsPrincipal; the desktop
/// app from the signed-in API session). This keeps the pages identity-agnostic.
```

Why go to that trouble? Because "who is signed in" is answered very differently on the two hosts. In the browser, the web app knows the user from an authentication cookie; on the desktop, the user is whoever holds the bearer token from the API login. If every page had to fetch the identity and thread it into every call, each page would have to understand both mechanisms — and the screens would stop being neutral. Instead, each host's body figures out the current user on its own, from its own context, and the pages simply ask for data as if identity were somebody else's job. It is.


### The web body: reach into the database, guarded

The web app's body is `DbOpenTrackDataService`. Its job is to answer each menu item using direct database access — but never blindly. Every read is filtered to what the signed-in user may see, and every write is checked against the shared access rules first (the very rules from the access-control chapter). Its summary says both halves plainly:

```csharp
/// EF Core-backed implementation of IOpenTrackDataService for the web app.
/// Creates a short-lived AppDbContext per operation via the factory ... and enforces the
/// same per-project access rules as the Web API by calling the shared AccessContext /
/// VisibilityQueries from OpenTrack.Core/Infrastructure. Reads the current user from
/// the authenticated Blazor Server circuit.
```

Notice how it resolves identity on its own. A small private helper opens a fresh database context and, in the same breath, loads the caller's access snapshot — so every operation begins already knowing both the database and who is asking, without the page ever supplying either:

```csharp
private async Task<(AppDbContext Db, AccessSnapshot Access)> OpenAsync(CancellationToken ct)
{
    var identity = await RequireIdentityAsync();
    var db = await dbFactory.CreateDbContextAsync(ct);
    var access = await AccessSnapshot.LoadAsync(db, identity, ct);
    return (db, access);
}
```

With that in hand, a menu item like "get the projects" is a database query that filters to the visible set before it ever builds a result:

```csharp
public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default)
{
    var (db, access) = await OpenAsync(ct);
    await using var _ = db;
    return await db.Projects.AsNoTracking()
        .WhereVisibleTo(access)
        .OrderBy(p => p.Name)
        .Select(p => new ProjectRow(...))
        .ToListAsync(ct);
}
```

> **Why a fresh database context each time** — The comment notes that a single long-lived context 'would live for the whole Blazor Server circuit and is not thread-safe.' A Blazor Server page stays connected for as long as it is open, and several actions can overlap. Sharing one database handle across all of them invites corruption, so each operation opens its own short-lived one and disposes it at the end (the `await using var _ = db;` line). Cheap, safe, and simple.

Reads and writes handle a denial differently, and on purpose. A read the user isn't entitled to returns nothing — null or an empty list — so the page renders "not found" rather than admitting the thing exists. A forbidden write throws `UnauthorizedAccessException`. The interface comment frames this as defense in depth: the screens already hide controls the user can't use, so a throw here is the backstop for a request that should never have been made.


### The desktop body: the same menu, turned into HTTP calls

The desktop app's body is `HttpOpenTrackDataService`. It answers the exact same menu, but it owns no database. Each operation becomes a network call to `OpenTrack.API`, the separate service the desktop talks to. Its summary calls it a thin client — the API already returns data shaped to match the UI's view models, so most methods are barely more than send-request-and-deserialize:

```csharp
/// HTTP-backed implementation of IOpenTrackDataService for the desktop app.
/// Calls OpenTrack.API over HTTP (thin client). The API's DTOs are shaped to match the
/// UI's view models, so most calls are near-passthrough deserialization. The authenticated
/// bearer token is attached by AuthTokenHandler on the underlying HttpClient,
/// so this class stays identity-agnostic just like the DB-backed version.
```

Put the two bodies of the same menu item side by side and the parallel is the whole point. "Get the projects" on the web ran a filtered database query; on the desktop it is one line that asks the server for the already-filtered list:

```csharp
public async Task<IReadOnlyList<ProjectRow>> GetProjectsAsync(CancellationToken ct = default) =>
    await http.GetFromJsonAsync<List<ProjectRow>>("/api/projects", JsonOptions, ct) ?? [];
```

And notice the desktop body is identity-agnostic in the same way the web one is. It never attaches the user's token itself; a separate piece, `AuthTokenHandler`, clips the bearer token onto every outgoing request automatically (Chapter 14 covers it). So this class, like its web twin, does its job without ever being handed "who is asking."

> **Jargon, in plain words** — HTTP is the request-and-response language browsers and apps use to talk to servers. A DTO (Data Transfer Object) is a plain data shape used to carry information across a network boundary. 'Deserialize' means turning the text a server sends back (here, JSON) into real C# objects. 'Bearer token' is a signed pass the desktop app got at login and shows on each request to prove who it is — like a wristband at an event.


### Agreeing on failure, not just success

A subtle but essential detail: for the shared pages to behave the same on both hosts, the two bodies must agree not only on what a successful call returns but on how a failure surfaces. The web body throws `UnauthorizedAccessException` when an action is forbidden. The API, being a network service, can't throw an exception across the wire — it returns an HTTP 403 status. So the desktop body translates that status back into the very same exception the web body would have thrown, and a shared helper makes the translation explicit:

```csharp
// Translate a 403 into the same exception the web/EF path throws, so the shared razor pages
// handle a denied action identically on both hosts.
private static void ThrowIfForbidden(HttpResponseMessage resp, string message)
{
    if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        throw new UnauthorizedAccessException(message);
}
```

The same care applies to edit conflicts. When two people edit the same issue at once, the web body catches the database's concurrency failure and throws `ConcurrencyConflictException`; the desktop body sees the API's HTTP 409 status and throws that identical exception. Either way, the shared page catches one exception type and shows one "someone else changed this" message. The pages are written once; both hosts make the world look the way those pages expect.


## Why It Matters / Design Takeaways

This seam is the hinge the entire three-host design swings on. Because the screens depend on one interface and nothing more, the web app can be a database-backed Blazor Server app, the desktop app can be a MAUI window talking to a network API, and both can present pixel-for-pixel the same experience without a second copy of the UI. Everything in the next three chapters — the web host, the API host, the desktop app — exists to fill in, or to serve, this one interface.

The properties that must not erode: the interface stays free of any hint of database or network, so it can host either body; no operation gets handed the caller's identity, so the pages stay neutral and each host resolves "who is asking" its own way; and the two bodies stay in step not just on results but on failures, so a denied or conflicting action looks the same everywhere. Add a new screen feature and the discipline is to add one operation to this menu and give it a body in both places — never to let a page reach around the seam to a database or a URL directly.

> **The maintainer's rule** — When you add a data operation, add it to IOpenTrackDataService and implement it in BOTH bodies — DbOpenTrackDataService (database) and HttpOpenTrackDataService (HTTP) — and make them agree on success shape AND failure behavior (a forbidden action throws UnauthorizedAccessException on both; a conflict throws ConcurrencyConflictException on both). Never let a shared page import a DbContext or a raw URL; the moment it does, the seam has leaked and the desktop app quietly breaks.


# 12. The Web Host: Blazor Server, Identity & Endpoints

*How the browser-facing app boots up — how it renders the shared screens live over a connection, how it signs people in with a cookie, how it registers the shared access rules, and the handful of extra 'endpoints' it maps for jobs the screens can't do alone, including the inbound Git webhook.*


## What This Is / What It Is For

A running program needs a *host* — a startup file that assembles all the parts, wires them together, and presses go. `OpenTrack.Web` is the host for the browser experience. Its `Program.cs` is the recipe: it registers every service the app needs, decides how the shared screens are rendered, sets up sign-in, applies any database updates, and finally maps the web addresses the app answers. When you open OpenTrack in a browser, this file is what booted the thing you are looking at.

Think of it as the stage manager for a play. The actors (the shared UI screens, the data seam, the access rules) already exist. `Program.cs` is what sets up the stage, hands each actor what it needs, arranges the entrances, and raises the curtain. It contains almost no business logic of its own — its whole job is assembly. This chapter walks that assembly top to bottom and then looks at the small set of side doors (endpoints) the web host opens for tasks the live screens can't handle by themselves.

> **The one-sentence version** — OpenTrack.Web boots a Blazor Server app that renders the shared UI live over a persistent connection, signs users in with an ASP.NET Core Identity cookie, plugs the shared data seam into its database-backed body, registers the same four role policies as every other host, and maps a handful of minimal-API endpoints — including a signature-verified inbound Git webhook — for the stream-and-form jobs the screens can't do inline.


### Blazor Server: the shared screens, rendered live

The first thing the recipe sets up is how the shared pages are drawn. OpenTrack's web app uses *Blazor Server*, and the very first registration says so:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

> **Jargon, in plain words** — Blazor is Microsoft's framework for building interactive web screens in C# instead of JavaScript. In the 'Server' flavor, the page's logic runs on the server, and a live connection to the browser ships button clicks up and screen updates down in real time. A 'Razor component' is one reusable piece of such a screen. The upshot: the browser shows the page, but the C# driving it runs on the server — which is exactly why the web host can reach the database directly.

This is the reason the web body of the data seam (Chapter 11) can talk straight to the database: the code behind each page is running on the server, right next to the data. Later in the file the components are actually mapped, and — importantly — the shared UI assembly is pulled in so the same pages the desktop uses are the pages the web serves:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(OpenTrack.UI.AssemblyMarker).Assembly);
```

That `AddAdditionalAssemblies(...OpenTrack.UI...)` line is the seam paying off: the web host renders components that physically live in the shared `OpenTrack.UI` project, not in a web-only copy.


### Identity: how the web app knows who you are

Next the recipe sets up sign-in, using *ASP.NET Core Identity* — Microsoft's built-in user-accounts system. It handles the account records, password hashing, the login and registration screens, and the sign-in cookie your browser carries so you don't log in on every click. OpenTrack registers its data layer and Identity together:

```csharp
var connectionString = builder.Configuration.ResolveOpenTrackConnectionString();
builder.Services.AddOpenTrackInfrastructure(connectionString);
// ...
builder.Services.AddOpenTrackIdentity();
```

> **Jargon, in plain words** — Identity here means the accounts system: who can sign in and how their password is stored. A 'cookie' is a small token the browser holds and sends back on each request, so the server recognizes you after you log in. A 'claim' is a labeled fact about the signed-in user carried alongside that cookie (their id, their name, their role) so the app doesn't have to re-read the database to answer 'who is this and what may they do?' on every click.

One registration deserves a spotlight because it ties directly into the access rules: on sign-in, OpenTrack stamps the user's role onto their identity as a claim, so permission checks can read it instantly without a database round trip:

```csharp
// Adds the user's OpenTrack.Role as a claim on sign-in so [Authorize(Policy = "...")] and
// AuthorizeView can check it without an extra DB round trip on every request.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<OpenTrack.Core.Entities.User>, RoleClaimsPrincipalFactory>();
```


### The four role policies, registered inline

OpenTrack expresses its coarse role gates as four named *authorization policies* — `RequireUpdater`, `RequireDeveloper`, `RequireManager`, `RequireAdministrator`. Each simply asks: is the signed-in user's role at least this rank? The web host registers all four right here:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Updater)))
    .AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Developer)))
    .AddPolicy("RequireManager", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Manager)))
    .AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Administrator)));

static bool WebRoleCheck(...AuthorizationHandlerContext ctx, UserRole minimum)
{
    var roleClaim = ctx.User.FindFirst("OpenTrack.Role")?.Value;
    return roleClaim is not null
        && Enum.TryParse<UserRole>(roleClaim, out var role)
        && (int)role >= (int)minimum;
}
```

The comment above them notes they are "shared with OpenTrack.API so both surfaces enforce identical access rules," and that they are "registered inline (rather than via a shared helper) because the DI extension lives in ASP.NET Core packages." In other words, all three hosts define the same four policies the same way — you will see the near-identical block in the API host (Chapter 13) and the desktop app (Chapter 14). These four are the *broad* gates on top of the *fine*, per-project rules from the access-control chapter; both layers are consulted.

> **Two layers of check, not one** — The role policies here answer 'is this user at least a Manager, anywhere?' — a coarse yes/no on a screen or endpoint. The per-project AccessContext / AccessSnapshot rules from the access-control chapter answer the finer 'may this user do this to THIS project or issue?'. A sensitive action passes both: the policy is the outer door, the per-project rule is the inner one.


### Plugging the seam into its database body

Now the payoff line for Chapter 11. The web host picks which body fills in the shared data seam — and it chooses the database-backed one:

```csharp
// The shared UI's data seam, backed by direct EF Core access in the web app.
builder.Services.AddScoped<OpenTrack.UI.Services.IOpenTrackDataService, OpenTrack.Web.Services.DbOpenTrackDataService>();
```

That single registration is the entire difference, on the data side, between the web app and the desktop app. The shared pages ask for `IOpenTrackDataService`; here the web host says "give them the `DbOpenTrackDataService`." The desktop host will say "give them the HTTP one." Same screens, different body, decided in one line.

The web host also registers a few things that are genuinely web-only — a background job that scans for overdue SLAs, scheduled database backups, an admin surface for global user management, and rate limiting on the public intake routes so spam can't flood them:

```csharp
// Background SLA-breach escalation (the always-on web host owns this periodic job).
builder.Services.AddHostedService<OpenTrack.Web.Services.SlaScanner>();
// Scheduled database backups (opt-in via OpenTrack:Backup).
builder.Services.AddHostedService<OpenTrack.Web.Services.BackupScheduler>();
```

These live on the web host because it is the always-on server — the natural home for periodic background work. The desktop app, which comes and goes with the user, is the wrong place for a breach scanner.


### Build, then migrate on startup

After all the registrations, the recipe builds the app and does one housekeeping step before serving traffic: it applies any pending database migrations and makes sure a bootstrap administrator exists, so a fresh install comes up with a working database and a way to log in:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await OpenTrackSeeder.EnsureBootstrapAdminAsync(scope.ServiceProvider, app.Configuration);
}
```

> **Jargon, in plain words** — A 'migration' is a recorded change to the database's shape (a new table, a new column). 'Migrate on startup' means the app checks the database against the migrations it knows about and applies any missing ones automatically, so nobody has to run database scripts by hand when they update OpenTrack. The 'bootstrap admin' is the first administrator account, seeded so a brand-new install isn't locked out of itself.

There is also a deliberate deployment choice tucked in here: HTTPS is off by default. OpenTrack is built to run on a trusted home or office LAN over plain HTTP, and the comment warns that forcing an HTTPS redirect with no HTTPS endpoint configured "would otherwise break access on a plain-HTTP LAN deployment." A single setting, `OpenTrack:RequireHttps`, turns on the redirect (and HSTS) for anyone exposing the server beyond that trusted network.


### The side doors: minimal-API endpoints

Most of what a user does happens through the live Blazor screens. But a few jobs don't fit the live-screen model — sending a file up, streaming a download, receiving a push from GitHub. For those, the web host maps a handful of *minimal-API endpoints*: plain web addresses that take a request and return a response, no live component involved. They are mapped at the end of `Program.cs`:

```csharp
// Cookie-authenticated attachment upload/download/delete for the web host.
OpenTrack.Web.Endpoints.AttachmentWebEndpoints.MapAttachmentWebEndpoints(app);
// Cookie-authenticated CSV/JSON export downloads for the web host.
OpenTrack.Web.Endpoints.ExportWebEndpoints.MapExportWebEndpoints(app);
// Cookie-authenticated MantisBT XML import upload for the web host.
OpenTrack.Web.Endpoints.ImportWebEndpoints.MapImportWebEndpoints(app);
// Cookie-authenticated change-token for smart-poll auto-refresh.
OpenTrack.Web.Endpoints.ActivityWebEndpoints.MapActivityWebEndpoints(app);
// Cookie-authenticated checklist status endpoint (offline check-off replay target).
OpenTrack.Web.Endpoints.ChecklistWebEndpoints.MapChecklistWebEndpoints(app);
// Public (unauthenticated, rate-limited) trouble-ticket intake endpoints.
OpenTrack.Web.Endpoints.PublicIntakeWebEndpoints.MapPublicIntakeWebEndpoints(app);
// Inbound Git webhook (unauthenticated but HMAC-verified, rate-limited).
OpenTrack.Web.Endpoints.GitWebhookEndpoints.MapGitWebhookEndpoints(app);
```

> **Jargon, in plain words** — A 'minimal API' is a lightweight way to answer a web address with a short function — request in, response out — without the ceremony of controllers. 'Cookie-authenticated' means the endpoint trusts the same sign-in cookie the browser already carries. 'Multipart form' is the format a browser uses to upload a file. These endpoints sit alongside the live pages; the pages link or post to them for the streaming jobs.

The attachment endpoints show the pattern well. Uploading a file is a plain multipart form post (static rendering has no live interactivity to stream bytes through), and downloading is a link — but each one re-checks the same per-project access rules the screens use, before touching a file:

```csharp
var access = await LoadAccess(http, db, ct);
if (access is null) return Results.Unauthorized();
var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
if (issue is null) return Results.NotFound();
var ctx = access.For(issue.ProjectId);
if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
    return Results.NotFound();
if (!ctx.CanAddNote()) return Results.Forbid();
```

That is the same `AccessSnapshot` / `AccessContext` machinery from the access-control chapter, called from an endpoint instead of a page. The endpoints are a different doorway into the app, but they consult the identical rulebook — no endpoint invents its own permission logic. Downloads add one extra guard: a `nosniff` header so an uploaded HTML or SVG file can't try to execute in the browser when viewed.


### The inbound Git webhook: open door, verified caller

The most interesting endpoint is the Git webhook receiver, because it must accept requests from GitHub, which has no OpenTrack account and can't carry a cookie. It is therefore unauthenticated in the usual sense — but it is far from unguarded. Every request is verified with an *HMAC signature* against the project's stored secret before a single byte of the payload is trusted:

```csharp
var config = await db.GitIntegrations.AsNoTracking().FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
if (config is null || !config.Enabled)
    return Results.NotFound(); // don't reveal whether the project exists / is configured

var signature = request.Headers["X-Hub-Signature-256"].ToString();
if (!GitSignature.IsValid(config.WebhookSecret, body, signature))
    return Results.NotFound(); // same response as unconfigured — don't reveal which projects have Git enabled
```

> **Jargon, in plain words** — A 'webhook' is a way for one system to call another the moment something happens — here, GitHub calls OpenTrack whenever code is pushed. HMAC is a signature made by mixing the message with a shared secret; only someone who knows the secret can produce a matching signature, so OpenTrack can confirm the request genuinely came from the configured GitHub project and wasn't forged or altered.

Two details show the security mindset. First, the signature is checked over the *exact raw bytes* of the body, read once up front — because even a trivial reformat would change the signature. Second, a request that fails the check gets the same `NotFound` as a project with no Git configured: the endpoint refuses to reveal which projects even have Git enabled. Only after the signature passes does it parse the commits and hand them to the shared `GitIntegrationOperations` to link and auto-resolve issues. The route is also rate-limited (`RequireRateLimiting("intake")`) so a flood of forged calls can't hammer the server.


## Why It Matters / Design Takeaways

`OpenTrack.Web` is assembly, not logic — and that is its strength. It renders the shared screens with Blazor Server, knows the user through Identity's cookie, plugs the data seam into its database body in one line, registers the same four role policies every host registers, and opens a small, well-guarded set of endpoints for the jobs live screens can't do. Nothing here reimplements a permission rule or a domain operation; it reaches for the shared pieces the rest of the book describes.

The properties to protect: the host stays a wiring file, deferring real work to the shared UI, the shared seam, and the shared rules; every endpoint — even an unauthenticated one — passes through the same access checks or a cryptographic verification before acting; and web-only concerns (background scans, backups, admin) stay on the always-on web host and out of the portable desktop app. When you add an endpoint, copy the attachment pattern: load the access snapshot, check the per-project rule, then act.

> **The maintainer's rule** — Treat Program.cs as assembly only — register services and map endpoints, never bury business logic here. Any new endpoint must authenticate (a cookie) or authorize (a signature/secret) AND consult the shared AccessContext / AccessSnapshot before it reads or writes; an endpoint that answers a URL without one of those is a hole. And keep the four role policies byte-for-byte in step with the API and desktop hosts — if you change one here, change all three.


# 13. The API Host: a Minimal API for the Desktop App

*How OpenTrack.API boots as a separate network service the desktop app calls — signing people in with bearer tokens instead of cookies, registering the same four role policies as every host, checking the same per-project access rules at every endpoint, and grouping its many routes by topic.*


## What This Is / What It Is For

The desktop app has no database and can't run the shared screens' server-side logic on the user's laptop. It needs something to call over the network — a service that holds the data, enforces the rules, and answers requests. That service is `OpenTrack.API`, and it is a separate program with its own startup file, `Program.cs`, booted independently of the web app.

If the web host (Chapter 12) is a stage that both renders the play and stores the script, the API host is a librarian at a service window: it holds the same books (the same database), it checks your credentials, and it hands back exactly what you're entitled to — but it draws no scenery of its own. It returns data, not pages. The desktop app is its customer. This chapter walks the API's boot sequence, shows how it proves who you are without a browser cookie, and shows that every one of its endpoints leans on the very same access rules the web app uses.

> **The one-sentence version** — OpenTrack.API is a standalone minimal-API service over the same database as the web app; it authenticates callers with bearer tokens instead of cookies, registers the identical four role policies, and — through the tiny ApiAccess and ApiRoleCheck helpers — re-checks the same per-project AccessSnapshot / AccessContext rules at every endpoint, so the desktop app is held to exactly the same permissions as the browser.


### Bearer tokens: the same accounts, proven differently

The web app knows you by a cookie your browser carries. A desktop app has no browser cookie jar, so the API proves identity a different way: *bearer tokens*. You log in once, the API hands back a token, and the desktop app shows that token on every later request. Crucially, it is the *same accounts* — the same users, in the same database — just a different way of presenting them. The registration says exactly that:

```csharp
// Bearer-token Identity for the API (desktop/thin-client consumers) — separate auth
// scheme from OpenTrack.Web's cookie-based Identity, but backed by the same AppDbContext
// and User type, so an account works identically whether signing in via the web app or
// the desktop app.
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<AppDbContext>();
```

> **Jargon, in plain words** — A 'bearer token' is a pass issued at login: whoever bears (holds) it is treated as that user, so the app attaches it to each request instead of logging in again. An 'auth scheme' is simply the method used to prove identity — cookie here, bearer token there. Both schemes read the same user table, so your one OpenTrack account works in the browser and in the desktop app alike.

The API shares one more thing with the web host: the role-claim factory that stamps the user's OpenTrack role onto their identity, so permission checks can read it without a database trip. Same class, registered in both hosts:

```csharp
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, RoleClaimsPrincipalFactory>();
```

And it points at the same database via the same helper the web app uses, which is what makes "one OpenTrack, two front doors" literally true — there is one set of data underneath both:

```csharp
// OpenTrack data layer (shared SQLite database with OpenTrack.Web).
var connectionString = builder.Configuration.ResolveOpenTrackConnectionString();
builder.Services.AddOpenTrackInfrastructure(connectionString);
```


### The same four role policies, once more

Here is the block you saw in the web host, near-identical, in the API host — the same four coarse role gates:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Updater)))
    .AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Developer)))
    .AddPolicy("RequireManager", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Manager)))
    .AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Administrator)));
```

The comment beside it explains the "why again": the policies are "shared with OpenTrack.Web so both surfaces enforce identical access rules," and are "kept inline here so the API doesn't need to reference the Blazor UI project." This is a deliberate trade. The four policies are so tiny — parse the role claim, compare to a minimum — that copying the definition into each host is cheaper and cleaner than forcing the API to depend on the web/UI projects just to borrow four lines. The names, the ranks, and the logic are identical; only the enclosing method name differs (`ApiRoleCheck.HasRoleAtLeast` here, `WebRoleCheck` there, `DesktopRoleCheck` in the desktop).

> **Why copy instead of share, just this once** — Everywhere else, OpenTrack shares rules ruthlessly to prevent drift. The role policies are the one spot it copies — because sharing them would drag a UI-framework dependency into a plain data service. The safety net is that the shared, weighty rules (the per-project AccessContext) are NOT copied; those four one-line policies are the only duplicated authorization code, and their names are constants (AuthorizationPolicies) so a typo can't silently create a fifth policy.


### The tiny bridge: ApiAccess and ApiRoleCheck

The API never writes its own permission logic for the important decisions — the per-project ones. It routes into the shared authority through two small helpers in `ApiAuthorization.cs`. The first, `ApiAccess`, loads the caller's whole access picture — the same `AccessSnapshot` the web app builds — and its comment is explicit that it uses the very same rules:

```csharp
/// <summary>Loads the caller's per-project access snapshot for endpoint authorization,
/// using the exact same rules as the web/EF path.</summary>
public static class ApiAccess
{
    public static async Task<AccessSnapshot?> LoadAsync(ClaimsPrincipal user, AppDbContext db, CancellationToken ct = default)
    {
        var identity = user.GetAccessIdentity();
        return identity is null ? null : await AccessSnapshot.LoadAsync(db, identity.Value, ct);
    }
}
```

The second, `ApiRoleCheck`, is the one-line role-threshold test the four policies above call. It reads the role claim and compares it to the required minimum — the same shape as the web host's `WebRoleCheck`:

```csharp
public static class ApiRoleCheck
{
    public static bool HasRoleAtLeast(AuthorizationHandlerContext ctx, UserRole minimum)
    {
        var roleClaim = ctx.User.FindFirst("OpenTrack.Role")?.Value;
        return roleClaim is not null
            && Enum.TryParse<UserRole>(roleClaim, out var role)
            && (int)role >= (int)minimum;
    }
}
```

Between them, these two little classes are the API's entire relationship to authorization. `ApiRoleCheck` powers the coarse policy gates; `ApiAccess` loads the snapshot that the fine, per-project checks run against. Neither contains a rule of its own — they are couplings to the shared machinery, not new machinery.


### Every endpoint re-checks the per-project rules

The API's endpoints are grouped by topic (projects, issues, and so on) and mapped near the end of `Program.cs`. Each group requires authentication, and then each endpoint inside it loads the access snapshot and consults the per-project rule for the thing being touched — exactly as the web body did. Compare a project endpoint with its web-app twin and they read the same:

```csharp
group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
{
    var access = await ApiAccess.LoadAsync(user, db, ct);
    if (access is null) return Results.Unauthorized();

    var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    if (project is null || !access.For(project.Id).CanViewProject(project.IsPublic))
        return Results.NotFound();

    return Results.Ok(new ProjectDetailDto(...));
});
```

That `access.For(project.Id).CanViewProject(...)` is the identical per-project decision the web app makes; the only difference is the wrapper. And where the web body throws or returns null, the API returns HTTP statuses — `Results.Unauthorized()`, `Results.NotFound()`, `Results.Forbid()` — which, as Chapter 11 showed, the desktop's HTTP body translates right back into the same exceptions the web body would have thrown. A forbidden write returns a clean 403:

```csharp
group.MapPut("/{id:int}/public-intake", async (int id, SetPublicIntakeRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
{
    var access = await ApiAccess.LoadAsync(user, db, ct);
    if (access is null) return Results.Unauthorized();
    if (!access.For(id).CanManageProject()) return Results.Forbid();
    // ...
});
```

> **The anti-drift payoff, restated for the API** — The API contains no hand-written per-project permission rule. Every endpoint calls ApiAccess.LoadAsync to get the shared AccessSnapshot and then the shared AccessContext methods (CanViewProject, CanManageProject, CanViewIssue, ...). So the desktop app, reaching OpenTrack through the API, is held to precisely the same rules as a browser reaching it through the web app. There is nowhere for a second, looser copy of the rules to live.

Some endpoints add the coarse policy on top as a fast outer gate — for example, creating a project (which isn't scoped to an existing project) both requires the Manager policy and re-checks a global-Manager rank inside, belt and suspenders:

```csharp
group.MapPost("/", async (CreateProjectRequest req, ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
{
    var access = await ApiAccess.LoadAsync(user, db, ct);
    if (access is null) return Results.Unauthorized();
    if (!access.GlobalAtLeast(UserRole.Manager)) return Results.Forbid();
    // ...
}).RequireAuthorization(AuthorizationPolicies.RequireManager);
```


### The endpoint groups, and the login the desktop needs

The bottom of `Program.cs` reads like a table of contents for the whole API — each line maps one topical group of routes:

```csharp
app.MapProjectEndpoints();
app.MapProjectSettingsEndpoints();
app.MapIssueEndpoints();
app.MapAttachmentEndpoints();
app.MapNotificationEndpoints();
app.MapDashboardEndpoints();
app.MapChecklistEndpoints();
app.MapSavedFilterEndpoints();
app.MapPreferenceEndpoints();
app.MapReportEndpoints();
app.MapAiEndpoints();
app.MapAutomationEndpoints();
app.MapSlaEndpoints();
app.MapGitEndpoints();
```

Each of those groups mirrors a slice of the `IOpenTrackDataService` menu — the desktop's HTTP body calls into exactly these routes. Two auth-related routes sit above them. The Identity login/register/refresh endpoints are mapped under `/api/auth`, and then a small custom route, `/api/auth/me`, answers a question the desktop specifically needs after signing in:

```csharp
// "Who am I" — returns the signed-in user's id, name, and OpenTrack role. The desktop client
// calls this right after login (the Identity /login token is opaque, so the client can't read
// the role from the token itself). Requires a valid bearer token.
app.MapGet("/api/auth/me", (System.Security.Claims.ClaimsPrincipal user) =>
{
    var id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var name = user.Identity?.Name ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var role = user.FindFirst("OpenTrack.Role")?.Value;
    return Results.Ok(new { id, name, role });
}).RequireAuthorization();
```

This exists because the token the API hands out at login is *opaque* — it is an encrypted blob, not something the desktop app can read the role out of. So right after logging in, the desktop calls `/api/auth/me` to learn its own id, name, and role, which it then feeds into the role policies on its side (Chapter 14). It is a small but telling seam between the two hosts: the API is the authority on who you are and what rank you hold, and it tells the desktop so the desktop's UI can light up the right controls.

> **Same HTTPS-off-by-default posture** — Like the web host, the API defaults HTTPS off for trusted-LAN plain-HTTP deployments and turns on a redirect only when OpenTrack:RequireHttps is set. The comment adds a matching instruction for the desktop: when you enable HTTPS on the server, point the desktop client's ApiBaseUrl at the https:// address so tokens are never sent in the clear.


## Why It Matters / Design Takeaways

The API host proves that a completely separate program, reached over the network with a different authentication scheme, can nonetheless enforce byte-for-byte the same permissions as the browser app — because it borrows those permissions instead of rewriting them. Bearer tokens replace the cookie; the four role policies are copied verbatim; and every meaningful decision defers to the shared `AccessSnapshot` and `AccessContext` through the two-line `ApiAccess` and `ApiRoleCheck` bridges. One database, one rulebook, two doors.

The properties to protect: the API stays a data service — it returns DTOs, never pages, and holds no UI dependency; every endpoint loads the shared snapshot and checks the per-project rule before acting, returning honest HTTP statuses the desktop can translate; and the only duplicated authorization is the four tiny role policies, whose names live in the `AuthorizationPolicies` constants so they can't fork by accident. Add an endpoint and the recipe is fixed: require auth on the group, load `ApiAccess`, check the per-project rule, then act.

> **The maintainer's rule** — A new API endpoint must call ApiAccess.LoadAsync and consult the shared AccessContext (CanView.../CanManage...) for the exact item it touches — never invent a permission check here, and never trust the desktop to have checked. Return Forbid/NotFound/Unauthorized so the desktop's HTTP body can mirror the web body's behavior. Keep the four role policies identical to the web and desktop hosts; if the ranks ever need to change, change all three in the same commit.


# 14. The Desktop App: MAUI Blazor Hybrid over HTTP

*How OpenTrack.Desktop hosts the exact same shared screens inside a native Windows or Mac window, points them at the API over the network, attaches the login token to every request automatically, lets the user change the server address without recompiling, and bridges a token-based session into Blazor's authorization system.*


## What This Is / What It Is For

`OpenTrack.Desktop` is the native application a user installs on Windows or Mac. Here is the surprising part: it contains almost none of the issue-tracker screens itself. It is a native window whose entire content is the *same shared Blazor UI* the web app renders — the identical pages, from the identical `OpenTrack.UI` project. The desktop app's real job is to be a good host for those pages on a machine that has no database: give them a window to live in, point them at the API over the network, and make sign-in work without a browser.

Picture the shared screens as a stage play and the desktop app as a touring venue. The web host staged the same play in a theater that owns the script library (the database). The desktop app stages that identical play in a small local venue that has no library of its own — so it phones the library (the API) for every fact it needs. The audience sees the same show. This chapter is about the venue: how `MauiProgram.cs` wires the shared pages to the HTTP body, how the token gets onto every call, and how a network login is made to look, to the shared pages, exactly like a browser login.

> **Jargon, in plain words** — .NET MAUI is Microsoft's toolkit for building native desktop and mobile apps from one C# codebase. A 'Blazor Hybrid' app runs Blazor web components inside a native window using an embedded browser control (a WebView) — so the same web UI shows up as a real application, no browser tab required. 'Over HTTP' means those components fetch their data from a network service (the API) rather than a local database.

> **The one-sentence version** — OpenTrack.Desktop is a MAUI Blazor Hybrid app that renders the shared UI in a native window, plugs the data seam into its HTTP body pointed at a user-configurable API address, attaches the login bearer token to every request via a message handler, and bridges its token-based session into Blazor's authorization so the shared pages' [Authorize] and role policies work completely unchanged.


### The window that hosts the shared pages

The native shell is tiny. A MAUI window contains a single `BlazorWebView`, and that view's root component is the shared UI's router — so the whole application surface is the shared pages:

```csharp
<BlazorWebView x:Name="blazorWebView" HostPage="wwwroot/index.html">
    <BlazorWebView.RootComponents>
        <RootComponent Selector="#app" ComponentType="{x:Type components:Routes}" />
    </BlazorWebView.RootComponents>
</BlazorWebView>
```

Everything else lives in `MauiProgram.cs`, the desktop equivalent of the web and API `Program.cs` files — the recipe that registers services and presses go. It opens by turning on Blazor for MAUI:

```csharp
builder.Services.AddMauiBlazorWebView();
```


### Plugging the seam into its HTTP body

This is the counterpart to the web host's one-line seam decision. Where the web host said "give the pages the database body," the desktop host says "give the pages the HTTP body" — and hands it a network client to talk through:

```csharp
// The shared UI's data seam, backed by HTTP calls to OpenTrack.API in the desktop app.
builder.Services.AddScoped<IOpenTrackDataService>(sp =>
    new HttpOpenTrackDataService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("OpenTrackApi")));
```

That single registration is the entire data-side difference between this app and the web app. The shared pages ask for `IOpenTrackDataService`; here they receive `HttpOpenTrackDataService`, which — as Chapter 11 showed — turns every menu item into a call to the API. The pages never know. They ask for projects; on the web that was a database query, here it is an HTTP GET, and the screen renders identically either way.


### The configurable server address

A desktop app can't hard-code where the server is — one user runs it against `localhost` while developing, another against a little Beelink box on the office LAN. So the API address is layered: a bundled default that ships with the app, which the user can then override in an in-app Settings screen, remembered per machine. The startup code reads the bundled default out of an embedded config file first:

```csharp
string defaultApiBaseUrl = "http://localhost:5003";
using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("OpenTrack.Desktop.wwwroot.appsettings.json"))
{
    if (stream is not null)
    {
        var cfg = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var configured = cfg["ApiBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
            defaultApiBaseUrl = configured;
    }
}
var savedApiBaseUrl = Preferences.Default.Get(ApiEndpoint.PreferenceKey, defaultApiBaseUrl);
builder.Services.AddSingleton(new ApiEndpoint(savedApiBaseUrl));
```

The bundled file itself is trivial — just the address — and it is included as an embedded resource so it is readable at startup on every platform:

```csharp
{
  "ApiBaseUrl": "http://localhost:5003"
}
```

The chosen address is held in a small mutable object, `ApiEndpoint`, and every network client reads it *at the moment the client is created*, so a change on the Settings screen takes effect on the next request with no app restart. Its comment states the design directly:

```csharp
/// Holds the current OpenTrack.API base address for the desktop client. Every named HttpClient reads
/// its base address from here at creation time, so changing it (via the in-app Settings page) takes
/// effect on the next request without an app restart. The value is seeded at startup from the saved
/// user preference, falling back to the bundled appsettings default.
public sealed class ApiEndpoint(string baseUrl)
{
    public const string PreferenceKey = "ApiBaseUrl";
    public string BaseUrl { get; set; } = baseUrl;
}
```

> **Jargon, in plain words** — An 'embedded resource' is a file baked into the app's executable, so it is always present regardless of where the app is installed. 'Preferences' is MAUI's per-machine settings store — a small place to remember a user's choice between runs. A 'singleton' is a service the app creates once and shares everywhere, which is why one ApiEndpoint object can hold the address the whole app reads.


### Two HTTP clients: one signed-in, one anonymous

The desktop registers two named network clients, and the split matters. One, `OpenTrackApi`, is the authenticated client used for all data once you're signed in — it has the token-attaching handler wired into it. The other, `OpenTrackApiAnon`, is a plain client with no token, used only to perform the login itself (you can't attach a token you don't have yet). Both read their base address from `ApiEndpoint` at creation:

```csharp
builder.Services.AddHttpClient("OpenTrackApi", (sp, client) =>
    {
        client.BaseAddress = new Uri(sp.GetRequiredService<ApiEndpoint>().BaseUrl);
    })
    .AddHttpMessageHandler<AuthTokenHandler>();

// A plain (unauthenticated) client that DesktopAuthState uses to perform login itself.
builder.Services.AddHttpClient("OpenTrackApiAnon", (sp, client) =>
    {
        client.BaseAddress = new Uri(sp.GetRequiredService<ApiEndpoint>().BaseUrl);
    });
```


### AuthTokenHandler: the token on every request, automatically

This is the piece that lets `HttpOpenTrackDataService` stay identity-agnostic (Chapter 11). The data body never attaches a token; instead a *message handler* sits in the pipeline of the authenticated client and clips the bearer token onto every outgoing request as it passes through:

```csharp
public class AuthTokenHandler(DesktopAuthState auth) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(auth.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return base.SendAsync(request, cancellationToken);
    }
}
```

> **Jargon, in plain words** — A 'delegating handler' is a checkpoint in the outgoing-request pipeline: every request passes through it before leaving, so it is the perfect place to add something to all of them at once. Here it adds the Authorization header carrying the bearer token. Because it is automatic, no individual data call has to remember to include the token — which is exactly why the HTTP data body could be written without ever mentioning identity.


### DesktopAuthState and the opaque-token problem

The token is held by `DesktopAuthState`, which drives login and logout and keeps the session in memory. Login posts to the API's anonymous endpoint and stores the returned tokens. But then it hits the wrinkle you met in Chapter 13: the token the API returns is *opaque* — encrypted, not readable — so the desktop can't learn the user's role from it. Its solution is to immediately call `/api/auth/me` and capture the id, name, and role the API reports back:

```csharp
AccessToken = tokens.accessToken;
RefreshToken = tokens.refreshToken;

try
{
    using var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
    meReq.Headers.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
    var meResp = await http.SendAsync(meReq, ct);
    if (meResp.IsSuccessStatusCode)
    {
        var me = await meResp.Content.ReadFromJsonAsync<MeResponse>(cancellationToken: ct);
        if (me is not null)
        {
            UserName = me.name;
            Role = me.role;
            UserId = int.TryParse(me.id, out var uid) ? uid : null;
        }
    }
}
catch
{
    // If /me fails, we're still logged in; role-gated features just won't light up.
}
```

A nice touch: login uses a fresh anonymous client per attempt, precisely so it always uses the *current* server address — the user may have just changed it on the Settings page before signing in. The tokens are kept in memory only; signing out simply clears them. This little class is the desktop's whole notion of "who is signed in."


### Bridging the session into Blazor's authorization

The shared pages use Blazor's standard authorization — `[Authorize]` attributes, `<AuthorizeView>` blocks, and the four role policies. Those expect an `AuthenticationStateProvider`. So the desktop provides one, `DesktopAuthenticationStateProvider`, that turns the token session into the claims-based identity Blazor understands — marking the user authenticated when a token is present and carrying the role as a claim:

```csharp
if (auth.IsSignedIn)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, auth.UserName ?? "user"),
        new(ClaimTypes.NameIdentifier, auth.UserId?.ToString() ?? "0"),
    };
    if (!string.IsNullOrEmpty(auth.Role))
        claims.Add(new Claim("OpenTrack.Role", auth.Role));

    identity = new ClaimsIdentity(claims, authenticationType: "opentrack-desktop");
}
```

Its comment names the reason plainly: the Identity login tokens are "OPAQUE (encrypted), not decodable JWTs, so we can't read claims out of the token itself" — hence the app carries the role/name it captured from `/api/auth/me` instead. And note the claim it sets: `OpenTrack.Role`, the very same claim name the web and API hosts stamp on. That is what lets the desktop register the *same four role policies*, checked the same way:

```csharp
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Updater)));
    options.AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Developer)));
    options.AddPolicy("RequireManager", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Manager)));
    options.AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Administrator)));
});
```

The comment beside it notes desktop is "a thin client and doesn't reference Infrastructure," so — like the API — it registers these four inline rather than sharing them. `DesktopRoleCheck` is the same parse-and-compare as `WebRoleCheck` and `ApiRoleCheck`. Finally, the session's `Changed` event is wired to the provider so the UI re-evaluates authorization the instant someone signs in or out:

```csharp
var authState = app.Services.GetRequiredService<DesktopAuthState>();
var authProvider = app.Services.GetRequiredService<DesktopAuthenticationStateProvider>();
authState.Changed += authProvider.NotifyChanged;
```

> **The desktop's role check is a convenience, not the gate** — The desktop carries the role and runs the four policies only to light up the right controls locally — hide a button the user can't use, show an admin link to an admin. It is NOT the real security boundary. Every request still goes to the API, which independently re-checks the role policy AND the per-project rules (Chapter 13). A tampered desktop that lied about its role would still be refused by the API. The UI check is courtesy; the server check is law.


### Which platforms, and why not Linux

The project file targets Windows and Mac (Mac Catalyst) only, and the comment explains the reasoning candidly. Android and iOS from the default MAUI template were removed — this is a desktop client, not a mobile app — and Linux is deliberately not a MAUI target because Microsoft doesn't support MAUI on Linux desktop:

```csharp
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">net10.0-windows10.0.19041.0</TargetFrameworks>
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('osx'))">net10.0-maccatalyst</TargetFrameworks>
```

The comment's closing line is the elegant part of the story: "Linux users run the fully cross-platform web app instead." Because the screens are shared, a platform the native app can't reach is not a platform without OpenTrack — a Linux user simply opens the web app in a browser and sees the same UI. Each OS also builds only its own head (a Windows machine builds the Windows head, a Mac the Mac Catalyst head), so neither developer needs the other's workload. Notably, the desktop project references only `OpenTrack.UI` and `OpenTrack.Core` — not Infrastructure — which is exactly why it re-declares the four role policies inline and owns no database code.


## Why It Matters / Design Takeaways

The desktop app is the clearest demonstration of what the seam bought. A native window, a token-based login, no database, a configurable server address — and yet the actual application the user interacts with is the same `OpenTrack.UI` the browser shows, unchanged. All the desktop-specific machinery (`ApiEndpoint`, `AuthTokenHandler`, `DesktopAuthState`, `DesktopAuthenticationStateProvider`) exists to make the network world underneath those shared pages look, to the pages, just like the web world: a data service to call, an authenticated user, and the four familiar role policies.

The properties to protect: the desktop stays a thin host — it references only UI and Core, owns no rules, and defers every real decision to the API; the token is attached centrally by the handler so the data body stays identity-agnostic; the server address stays user-configurable and read-at-creation so it can change without a restart; and the local role check is understood as cosmetic, with the API as the true gate. Add a desktop-only concern and the discipline is to keep it in the host plumbing, never in a screen — the screens must remain the shared ones.

> **The maintainer's rule** — Keep OpenTrack.Desktop a host, not a fork of the UI: it renders OpenTrack.UI's pages and nothing else, references only UI and Core, and lets the API enforce security. Attach the token only through AuthTokenHandler (never per-call), read the server address from ApiEndpoint at client-creation time, and keep the OpenTrack.Role claim name and the four role policies identical to the web and API hosts. If a feature seems to need a desktop-only screen, that is a signal it belongs in the shared UI instead.


# 15. Issues End to End

*Following one bug report from the New Issue form, through the shared data-service seam, into the database — then the edits and status changes that follow — and how each project's own workflow rules decide which status moves are allowed.*


## What This Is / What It Is For

An *issue* is the single most important thing OpenTrack holds: one report of one problem — a bug, a task, a request. Everything else in the product exists to help you file one, find one, work one, and close one. This chapter follows a single issue on its whole journey: born in the *New Issue* form, carried across the shared data-service seam, written into the database, and then edited and moved through a series of *statuses* (New, Acknowledged, Resolved, and so on) until it is done.

Think of an issue like a paper work-order on a clipboard. When it is first created, some boxes are filled in by the person reporting the problem (the title, what happened, how bad it is) and some are stamped by the system itself (who filed it, when, and a starting status of *New*). As the work-order moves around the shop, people write on it — but only in the boxes they are allowed to, and it can only move to the next station if the shop's posted routing chart permits that move. This chapter is about where each of those rules physically lives in the code.

> **The one-sentence version** — Creating and editing an issue always runs through one shared data-service method that checks permission, scrubs any cross-project ids, records the change in history, and saves atomically — and any change of status is additionally gated by the project's own workflow, a check both the web app and the API call so they can never disagree about which moves are legal.


### The issue, as data

Before the journey, the destination. An issue is a plain C# class, `Issue`, whose properties are the columns it will occupy in the database. The human-entered fields sit at the top; the machine-managed ones lower down. A few of them:

```csharp
public class Issue
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? StepsToReproduce { get; set; }

    public IssueStatus Status { get; set; } = IssueStatus.New;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;
    public IssuePriority Priority { get; set; } = IssuePriority.Normal;

    public bool IsPrivate { get; set; }
    public bool IsSticky { get; set; }

    public int ProjectId { get; set; }
    public int ReporterId { get; set; }
    public int? AssigneeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

> **Jargon, in plain words** — An entity is a C# class whose objects map one-to-one to rows in a database table — an Issue object is a row in the Issues table. An enum (short for enumeration) is a fixed menu of named choices; IssueStatus, IssueSeverity, and IssuePriority are all enums, so a status can only ever be one of a small, known set of values, never a free-typed string. A property is one named box on the object (Title, Status). The int? with a question mark means the value is optional — an issue may have no assignee, so AssigneeId can be empty.

Notice `Status`, `Severity`, and `Priority` are not free text — each is one of those fixed menus. That is deliberate and load-bearing: because a status is always one of a known handful of values, the workflow rules later in this chapter can talk about moves between them with total confidence. There is no such thing as a mistyped status.


### The New Issue form: what you fill in, what the software fills in

The journey starts in `Create.razor`, the *New Issue* page. It is an ordinary web form: a title, a description (with a note that Markdown is supported for stack traces), steps to reproduce, and dropdowns for severity, priority, category, versions, and an optional due date. There is even an optional *Attach my location* button for a problem out in the field. Crucially, the form does *not* ask for a status, a reporter, or a created-date — those are the system's to stamp, not yours.

When you press *Submit Issue*, the page does not touch the database itself. It gathers the boxes you filled into a small input record and hands them to the shared data service — the one seam both the web app and the desktop app go through:

```csharp
var id = await Data.CreateIssueAsync(ProjectId, new CreateIssueInput(
    model.Title, model.Description, model.StepsToReproduce,
    model.ExpectedBehavior, model.ActualBehavior, model.CategoryId,
    model.Severity, model.Priority, model.Reproducibility, model.DueDate,
    model.AffectsVersionId, model.FixVersionId,
    model.Latitude, model.Longitude));
Nav.NavigateTo($"issues/{id}");
```

The page's whole responsibility is to collect input and, if the service refuses (throws), to show the message. It carries no knowledge of who may create an issue or how the row is written. That knowledge lives one layer down, in the data service — which is where the real work happens.


### The create path, step by step

`CreateIssueAsync` in `DbOpenTrackDataService` is where a form submission becomes a database row. Read it as a short checklist it runs top to bottom. First it loads the target project and asks the access authority (from Chapter 8) two questions — may this user even see the project, and are they at least a Reporter on it?

```csharp
var ctx = access.For(projectId);
if (!ctx.CanViewProject(project.IsPublic))
    throw new UnauthorizedAccessException("You do not have access to this project.");
if (!ctx.CanCreateIssue(project.IsPublic))
    throw new UnauthorizedAccessException("Reporting an issue requires the Reporter role on this project.");
```

Only after permission is settled does it build the row. And before trusting the category and version ids the form sent, it scrubs them — because a hand-crafted request could try to attach this issue to a category that belongs to a *different* project:

```csharp
// Drop any category/version ids that don't belong to this project (cross-project id smuggling).
var (catId, avId, fvId) = await IssueScope.SanitizeAsync(
    db, projectId, input.CategoryId, input.AffectsVersionId, input.FixVersionId, ct);
```

> **Jargon, in plain words** — "Id smuggling" is a common attack shape: a form legitimately shows you a dropdown of this project's categories, but the raw request underneath is just numbers, and a malicious client can send a number pointing at another project's category. Sanitizing means the server re-checks that every id actually belongs where it claims to, and quietly drops any that do not — so it never trusts the shape of the request over the facts in the database.

Now it stamps the fields that are the system's to own — the reporter is the signed-in user, the status is `New`, and both timestamps are now — and then does the subtle, important part: it adds the *first history row* to the issue's own history collection before saving, so the issue and the record that it was created are written in a single database save.

```csharp
issue.ReporterId = access.UserId; issue.Status = IssueStatus.New;
issue.CreatedAt = DateTime.UtcNow; issue.UpdatedAt = DateTime.UtcNow;

// Add the creation-history row via the navigation collection so the issue and its history
// persist in a SINGLE SaveChanges — atomic, no half-written issue with no history.
issue.History.Add(new IssueHistory
{
    UserId = access.UserId, FieldChanged = "Status",
    OldValue = null, NewValue = IssueStatus.New.ToString(), ChangedAt = DateTime.UtcNow
});
db.Issues.Add(issue);
await db.SaveChangesAsync(ct);
```

> **Why 'atomic' matters here** — "Atomic" means all-or-nothing: because the issue and its first history entry are handed to one SaveChanges call, the database writes both or neither. There is never a moment where an issue exists with no record of how it came to be. If the save fails, nothing is written at all. This is the same discipline you want any time two facts must be true together.

Two final touches run after the row exists and has an id: the project's *automation rules* get a chance to auto-tag or auto-assign the brand-new issue (Chapter 17), and interested people are notified. Then the new issue's id is returned, and the page redirects you to it. The clipboard has been created, stamped, filed, and pinned to the right board.


### Editing an issue: who may change what

Editing lives in `Edit.razor` and, underneath, `UpdateIssueAsync`. The page pre-loads the current values into a form (only once, guarded by a hidden `Loaded` flag, so your in-progress typing is not clobbered on a re-render) and adds the controls a creator does not get: *Status*, *Resolution*, *Assignee*, and checkboxes for *Sticky* and *Private*. On save it hands the values to the same seam and catches the three ways an edit can be refused.

The data service's `UpdateIssueAsync` is stricter than create, because editing is more powerful. It first re-checks that the user may even see this issue (returning silently — not-found — if not) and then that they are at least an Updater. But the interesting design is what happens with the *privileged* fields. Ordinary fields (title, description, severity) are simply overwritten. Assignment and the privacy/sticky flags, though, are only applied if the caller actually holds that right — otherwise the existing value silently stands:

```csharp
// Privileged fields: silently keep the existing value if the caller lacks the right, so a
// crafted request can never escalate (the UI already hides these controls by role).
if (ctx.CanAssignIssue() && await IsAssignableAsync(db, issue.ProjectId, input.AssigneeId, ct))
    issue.AssigneeId = input.AssigneeId;
if (ctx.CanSetIssuePrivacy())
    issue.IsPrivate = input.IsPrivate;
if (ctx.CanSetIssueSticky())
    issue.IsSticky = input.IsSticky;
```

This is defense in depth. The web UI already hides the assignee dropdown from someone who cannot assign — but the server does not *trust* that the control was hidden. If a crafted request tries to set an assignee it has no right to set, the line simply does not run and the old value remains. The UI hiding is a convenience; this server check is the actual security. And, as with create, any change to status or assignee appends a history row, and a meaningful change (not every trivial edit) fires a notification — so a wall of email does not follow every typo fix.


### Statuses, and the workflow that fences them

Here is the heart of the chapter. An issue's status is drawn from the fixed `IssueStatus` menu, and the menu is *ordered* — its numbers climb, which lets the rest of the system reason about "open" versus "done" with a single comparison:

```csharp
public enum IssueStatus
{
    New = 10,
    Feedback = 20,
    Acknowledged = 30,
    Confirmed = 40,
    Assigned = 50,
    Resolved = 80,
    Closed = 90
}
```

By default an issue can move from any status to any other — a small team may not want ceremony. But many teams want discipline: no jumping straight from *New* to *Closed* without acknowledging it first, for instance. That is what a *workflow* is: a per-project list of the specific status moves that are allowed. Define even one, and the project's workflow becomes "restricted" — from then on *only* the listed moves are permitted. Define none, and it stays open. The whole feature is one small entity and one class of rules, `WorkflowOperations`, whose own summary states the intent:

```csharp
public readonly record struct WorkflowTransitionItem(int Id, IssueStatus FromStatus, IssueStatus ToStatus);

/// Per-project workflow rules: the set of allowed status transitions. Defining any turns the project's
/// workflow "restricted"; defining none leaves it open (the default). Managing rules is Manager-only;
/// the allow-check is used by every issue-status write path so the API, web, board, and bulk actions all
/// enforce the same workflow.
```

> **Jargon, in plain words** — A transition is one arrow on a routing chart: a move from one status (the "from") to another (the "to"), e.g. Acknowledged → Resolved. A workflow is the whole set of allowed arrows for a project. "Restricted" simply means the chart is now the law — a move with no matching arrow is refused. A record struct is a small, immutable value that just bundles a few fields together; here, the id of one allowed arrow plus its from and to.

Managing the arrows is Manager-only — a project Manager adds and removes them on the project's *Settings* page, which calls `AddWorkflowTransitionAsync` / `DeleteWorkflowTransitionAsync`. Each add refuses two obvious mistakes (a status to itself, or a duplicate arrow) and refuses anyone below Manager outright:

```csharp
if (!access.For(projectId).CanManageProject())
    throw new UnauthorizedAccessException("Managing the workflow requires the Manager role on this project.");
if (from == to) return "Pick two different statuses.";
if (await db.WorkflowTransitions.AnyAsync(w =>
        w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct))
    return "That transition is already allowed.";
```


### The one gate every status change passes through

The rule that actually decides whether a move is legal is a single method, `IsAllowedAsync`. It is written to be generous in exactly two cases — when the status is not changing at all, and when the project has defined no workflow — and otherwise to permit only a move that appears in the list:

```csharp
public static async Task<bool> IsAllowedAsync(
    AppDbContext db, int projectId, IssueStatus from, IssueStatus to, CancellationToken ct = default)
{
    if (from == to) return true;
    var any = await db.WorkflowTransitions.AsNoTracking().AnyAsync(w => w.ProjectId == projectId, ct);
    if (!any) return true; // no workflow defined => open
    return await db.WorkflowTransitions.AsNoTracking()
        .AnyAsync(w => w.ProjectId == projectId && w.FromStatus == from && w.ToStatus == to, ct);
}
```

In the edit path, this gate is consulted the moment a status change is detected, and a disallowed move is rejected before any field is written:

```csharp
// Enforce the project's workflow (if it defines one) before accepting a status change.
if (input.Status != originalStatus &&
    !await WorkflowOperations.IsAllowedAsync(db, issue.ProjectId, originalStatus, input.Status, ct))
    throw new InvalidOperationException(
        $"Changing status from {originalStatus} to {input.Status} isn't allowed by this project's workflow.");
```

Now the payoff, and the reason the gate is a shared method rather than code pasted into the edit page. An issue's status can be changed from more than one place: the web edit form, the desktop app's API, a Git commit that auto-resolves an issue, and a bulk action that moves many issues at once. Every one of them calls the very same `IsAllowedAsync`. The API host's issue endpoint uses the identical line the web path does:

```csharp
// OpenTrack.API/Endpoints/IssueEndpoints.cs
if (req.Status != originalStatus &&
    !await WorkflowOperations.IsAllowedAsync(db, issue.ProjectId, originalStatus, req.Status, ct))
    return Results.BadRequest(
        $"Changing status from {originalStatus} to {req.Status} isn't allowed by this project's workflow.");
```

And so do `BulkOperations` (skipping any issue whose requested move is not allowed, rather than failing the whole batch) and `GitIntegrationOperations` (a commit only auto-resolves an issue if a move to *Resolved* is a legal transition). There is exactly one definition of "is this status move allowed?" in the whole codebase, and five callers lean on it. That is the same anti-drift design you saw with permissions: write the rule once, in the shared layer, and let every surface defer to it.


## Why It Matters / Design Takeaways

An issue's life is a chain of small, deliberate seams. The form collects but does not decide. The data service decides — permission first, then a scrub of untrusted ids, then a single atomic save that records history alongside the row. Editing overwrites the ordinary fields but silently holds the privileged ones unless the caller truly holds the right, so a hidden UI control is never mistaken for a security boundary. And every status change, from whichever of the five entry points, squeezes through one shared workflow gate.

The rules that must not erode: statuses stay a fixed, ordered menu (so "open", "done", and "is this move legal" are simple, reliable comparisons); the create and edit paths stay in the shared data service rather than in a page, so both hosts get the same checks; untrusted ids are always re-scoped to the project on the server; and the workflow check stays a single method that every write path calls. Add a sixth way to change a status someday and the safe move is not to write a fresh allow-check — it is to call `IsAllowedAsync` like everyone else.

> **The maintainer's rule** — If you add any new path that changes an issue's status — a new endpoint, a new automation, a new integration — route it through WorkflowOperations.IsAllowedAsync, and route the write itself through the shared data service so it inherits the permission checks, the id-scrubbing, and the history record. Never let a page or an endpoint grow its own private copy of these rules; that is precisely the drift this layering was built to prevent.


# 16. Service-Level Agreements & Escalation

*How a project sets a resolve-by target for each priority, how the software works out whether an open issue is on-track, at-risk, or already breached, and how a quiet background scanner escalates a breach exactly once so nobody is nagged and nobody is missed.*


## What This Is / What It Is For

A *service-level agreement* — SLA for short — is a promise about speed: "we will resolve a high-priority issue within eight hours, a normal one within three days." OpenTrack lets a project Manager set such a target for each priority, then does two things with it. It shows a live *status board* of everything that has missed, or is about to miss, its target; and it runs a quiet background job that, when an issue actually blows past its deadline, notifies the right people — once.

Picture a kitchen during a dinner rush. Every order has a ticket, and every ticket has a promised time. A good expediter does two jobs: at a glance they can see which tickets are running late (the board), and when one goes truly overdue they call it out to the cook and the manager — but they call it out *once*, not every thirty seconds, because a constant alarm is an ignored alarm. This chapter is those two jobs, in code.

> **The one-sentence version** — SLA targets are stored per project and per priority; a single pure calculator turns an issue's created-time plus its target into on-track / at-risk / breached; the status board runs that calculator over only the issues you may see; and a background scanner escalates each real breach exactly once by stamping the issue after it notifies.


### Where the targets live, and who may set them

A target is a plain number of hours attached to one project and one priority: "in project 4, an Urgent issue should be resolved within 8 hours." Setting them is Manager-only and follows the same pattern as every other per-project setting — reads return an empty list for a non-manager; writes throw. `SlaPolicyOperations.SetAsync` both sets and clears a target: passing null or zero hours removes that priority's row entirely.

```csharp
public static async Task<string?> SetAsync(
    Data.AppDbContext db, AccessSnapshot access, int projectId, IssuePriority priority, int? hours, CancellationToken ct = default)
{
    if (!access.For(projectId).CanManageProject())
        throw new UnauthorizedAccessException("Managing SLA targets requires the Manager role on this project.");
    if (hours > MaxTargetHours) return $"Target must be {MaxTargetHours} hours or fewer.";

    var existing = await db.SlaPolicies.FirstOrDefaultAsync(p => p.ProjectId == projectId && p.Priority == priority, ct);
    if (hours is null or <= 0)
    {
        if (existing is not null) { db.SlaPolicies.Remove(existing); await db.SaveChangesAsync(ct); }
        return null;
    }
    ...
}
```

> **Jargon, in plain words** — A priority is how urgent an issue is (Low, Normal, High, Urgent…), one of the fixed enum menus from Chapter 5. "Per project and per priority" means the targets are a small grid: each project has at most one target row per priority. AccessSnapshot is the loaded picture of what the current user is allowed to do (Chapter 8); CanManageProject() asks whether they are a Manager here. The odd-looking hours is null or <= 0 is C# pattern-matching for "no target or a nonsense one" — the signal to clear the row.

There is one small piece of defensive engineering worth noticing: a `MaxTargetHours` cap of 100,000 hours (about eleven years). Its own comment explains why — it is a "sanity cap so a fat-fingered target can't overflow date math." A target is used later to compute a due-date by adding hours to a timestamp, and an absurd number could overflow that arithmetic; the cap makes the impossible input simply impossible.


### The pure clock: on-track, at-risk, or breached

The actual judgement — is this issue fine, getting close, or late? — is not tangled up with the database at all. It lives in `SlaCalculator`, a pure class in `OpenTrack.Core` with no database and no network, so the board, the background scanner, and the tests all compute the answer the exact same way. It is handed plain facts and returns a verdict. First, the four possible verdicts:

```csharp
public enum SlaStatus
{
    NotTracked = 0, // no target applies, or the clock has stopped
    OnTrack = 1,    // comfortably within target
    AtRisk = 2,     // past the at-risk threshold but not yet past the deadline
    Breached = 3,   // past the deadline and still open
}
```

And the calculation itself. Give it the issue's priority, when it was created, whether it is still open, the current time, and the target hours for that priority; it hands back the verdict and the due-by instant:

```csharp
public static SlaAssessment Evaluate(
    IssuePriority priority, DateTime createdAtUtc, bool isOpen, DateTime nowUtc, int? targetHours)
{
    // No target for this priority, or the clock has stopped -> nothing to track.
    if (!isOpen || targetHours is not > 0) return SlaAssessment.NotTracked;

    var dueUtc = createdAtUtc.AddHours(targetHours.Value);
    var atRiskUtc = createdAtUtc.AddHours(targetHours.Value * SlaDefaults.AtRiskFraction);

    var status = nowUtc >= dueUtc ? SlaStatus.Breached
        : nowUtc >= atRiskUtc ? SlaStatus.AtRisk
        : SlaStatus.OnTrack;
    return new SlaAssessment(status, dueUtc);
}
```

Read it plainly. If the issue is closed or has no target, there is nothing to track — done. Otherwise the deadline is created-time plus the target, and the *at-risk* moment is created-time plus a fraction of the target. Then it is a two-step comparison against the clock: past the deadline is *Breached*, past the at-risk mark is *AtRisk*, and anything earlier is *OnTrack*. The whole model is: only *open* issues have a running clock — resolving an issue stops it. That is why the calculator also exposes a tiny helper, `IsOpen`, defining "open" as any status below `Resolved`.

> **The 80% threshold, named once** — The at-risk line is not a magic number sprinkled through the code — it is one named constant, SlaDefaults.AtRiskFraction = 0.8. "At risk" therefore means 80% of the promised time has elapsed and the issue is still open: the yellow zone before the red. Because it is defined in exactly one place, a team that wants an earlier warning (say 0.7) changes it once and every screen and scanner agrees. That is the point of naming a number instead of typing it.


### The status board: the same clock, safely scoped

The board is the expediter's at-a-glance view: every open issue that is at-risk or breached, breached first, each list ordered most-overdue-first. It is built by `SlaBoard.BuildAsync`, and its single most important design choice is the *order* of its steps: it applies the access filter *first*, before anything else, so the board can only ever show issues the caller is already allowed to see.

```csharp
// Only open issues have a running clock.
var open = await db.Issues.AsNoTracking()
    .WhereVisibleTo(access)
    .Where(i => (int)i.Status < (int)IssueStatus.Resolved)
    .Select(i => new { i.Id, i.ProjectId, ProjectName = i.Project.Name, i.Title, i.Priority,
        AssigneeName = i.Assignee != null ? i.Assignee.UserName : null, i.CreatedAt })
    .ToListAsync(ct);
```

That `WhereVisibleTo(access)` is the row-level security filter from Chapter 8, running inside the database. So even though the board is a cross-project view, a user never sees a late issue in a project they have no business seeing. With the visible open issues in hand, the board loads only the targets for the projects actually in play, then runs the same pure calculator over each one and keeps only the ones that are at-risk or breached:

```csharp
targets.TryGetValue((i.ProjectId, i.Priority), out var hours);
var a = SlaCalculator.Evaluate(i.Priority, i.CreatedAt, isOpen: true, nowUtc, hours == 0 ? null : hours);
if (a.Status is SlaStatus.AtRisk or SlaStatus.Breached && a.DueUtc is { } due)
    rows.Add(new SlaBoardRow(i.Id, i.ProjectId, i.ProjectName ?? "", i.Title, i.Priority,
        i.AssigneeName, i.CreatedAt, due, a.Status));
```

The due-date math — created plus target hours — is done here in memory rather than in the database, and the class comment says why plainly: it "isn't worth expressing in SQL." The *filtering* that must be efficient (which issues can you see) runs in the database; the small *arithmetic* over the already-narrowed list runs in C#. The `Sla.razor` page then simply renders the two lists, with a friendly "Nothing is breaching or at risk right now" when both are empty.


### The background scanner: escalation with no one at the keyboard

The board is passive — it only shows you something when you open it. But a breach that nobody is looking at still needs to raise its hand. That is the job of `SlaScanner`, a *background service* that runs on a timer inside the web host, with no signed-in user at all. It is registered once at startup and then runs for the life of the app:

```csharp
// OpenTrack.Web/Program.cs
builder.Services.AddHostedService<OpenTrack.Web.Services.SlaScanner>();
```

> **Jargon, in plain words** — A background service (or hosted service) is code that runs on its own schedule inside the app, separate from any web request — like a night-shift worker who keeps going after the customers leave. Because there is no logged-in user driving it, it is a "trusted system actor": it queries the database directly without the per-user access filter, precisely because there is no user to filter for. A DbContextFactory hands it a fresh, short-lived database connection for each tick so its long-running loop never clings to one stale connection.

It wakes on a fifteen-minute interval (after a one-minute startup delay to let migrations settle), and each tick is best-effort: any error is logged and the loop simply continues to the next interval — a single bad scan never kills the scanner. On each tick it loads the SLA targets, and if no project has any, it does nothing at all. Then it gathers *candidates*: open issues, in SLA-enabled projects, that have *not already been escalated*:

```csharp
// Candidate open issues in SLA-enabled projects that haven't been escalated yet.
var candidates = await db.Issues.AsNoTracking()
    .Where(i => projectIds.Contains(i.ProjectId)
                && (int)i.Status < (int)IssueStatus.Resolved
                && i.SlaBreachNotifiedAt == null)
    .Select(i => new { i.Id, i.ProjectId, i.Priority, i.CreatedAt, i.AssigneeId })
    .ToListAsync(ct);
```

That last condition, `SlaBreachNotifiedAt == null`, is the whole "notify once" mechanism. `SlaBreachNotifiedAt` is a timestamp field on the issue itself; while it is empty, the issue is a candidate; once the scanner escalates, it will be stamped, and from then on the candidate query skips it. The scanner then runs the same pure `SlaCalculator.Evaluate` over each candidate and keeps only the truly breached ones.


### Notify once — and the case where it deliberately does not stamp

For each breached issue, the scanner assembles the recipients — the assignee, plus every Manager of the project — writes each of them one notification, and stamps the issue with the current time so the next tick will pass it by:

```csharp
foreach (var uid in recipients)
    db.Notifications.Add(new Notification
    {
        UserId = uid, IssueId = issue.Id,
        Text = "SLA breached — this issue has passed its resolution target.",
        CreatedAt = now,
    });
issue.SlaBreachNotifiedAt = now;
```

But there is a subtle, considerate exception, and it is the most instructive line in the whole file. What if a breached issue has *nobody* to escalate to yet — no assignee, and the project has no Managers? If the scanner stamped it anyway, it would be marked "already notified" forever, and a Manager added tomorrow would never learn the issue had breached. So in that one case it deliberately leaves the stamp empty, letting the issue come back around next tick:

```csharp
// If there's nobody to escalate to yet (no assignee and no managers), leave the issue
// un-stamped so it's re-evaluated next tick — otherwise a later-added assignee/manager would
// never be notified about an already-breached issue.
if (recipients.Count == 0) continue;
```

This is the difference between "notify once" and "notify once someone is actually there to be notified." The stamp does not mean "we tried"; it means "we successfully told someone." Getting that distinction right is what keeps the feature from silently dropping the very breaches that most need a human — the orphaned ones.


## Why It Matters / Design Takeaways

The SLA subsystem is a clean split between judgement and plumbing. The judgement — on-track, at-risk, breached — is one pure calculator in Core, so the passive board and the active scanner can never disagree about what "breached" means, and a single unit test can prove the whole thing with no server running. The plumbing differs by context: the board scopes its data with the per-user access filter because a user is present, while the scanner runs with system authority because none is. And the one number that defines "at-risk" lives in exactly one named constant.

The rules that must not erode: the SLA verdict stays pure and database-free (so it stays shared and testable); the board keeps applying the visibility filter before it computes anything; the at-risk fraction stays a single named constant, never a scattered literal; and escalation stays idempotent — driven by the `SlaBreachNotifiedAt` stamp — while never stamping a breach that reached no one. Change the cadence, add a channel, tune the threshold: do it in these small, single-purpose places and every surface follows.

> **The maintainer's rule** — If you extend SLA behavior, compute the verdict only through SlaCalculator — never re-derive "is it breached?" inline — and if you touch the scanner, preserve the meaning of SlaBreachNotifiedAt: stamp it only after a real recipient was notified, and leave it null when there was no one to tell. A stamp is a promise that a human heard the alarm, not merely that the alarm fired.


# 17. Automation Rules

*How a project says "when a new issue matches these conditions, do these things" — where those rules are stored, how a pure evaluator decides which fire and in what order, and how the engine applies the result to a brand-new issue the instant it is created.*


## What This Is / What It Is For

An *automation rule* is a small standing instruction a project Manager writes once so the software does the routine triage for them: "when a new issue's title mentions *crash*, tag it *crash* and set its severity to Major," or "anything filed in the Billing category, assign to Dana." Every time a new issue is created, the project's rules get a look at it and quietly apply whatever matches. It is the difference between a Manager hand-sorting every incoming ticket and the mailroom sorting itself.

Think of it like the mail-sorting rules you might set in an email program: a list of "if the message looks like this, then file it there / flag it / forward it." You write the rules; the program runs them on each new arrival. OpenTrack's automation is exactly that idea, aimed at newly-created issues, with a fixed little menu of things a rule can look at and a fixed little menu of things it can do.

> **The one-sentence version** — Each project holds an ordered list of "match these conditions, apply these actions" rules on single flat rows; a pure evaluator tests every rule against the issue's original state and combines the matches (last writer wins for fields, tags accumulate); and one engine, called from the exact same spot in both hosts' create paths, applies the result to the just-saved issue — running on creation only, so a rule can never re-trigger itself.


### A rule, as data

A rule is one row: `AutomationRule`. Its own summary states the shape and the two governing conventions — conditions are ANDed, and null means "don't care":

```csharp
/// A per-project automation rule: "when a new issue matches these conditions, apply these actions."
/// Conditions are ANDed; a null condition means "don't care", so a rule with no conditions matches every
/// new issue. Actions are applied only when the rule matches; a null action does nothing. All conditions
/// and actions live on the one row (no child tables) to keep the schema and the editor simple.
```

The fields divide cleanly into three groups — the housekeeping, the conditions (the "when"), and the actions (the "then"):

```csharp
public bool IsEnabled { get; set; } = true;
public int SortOrder { get; set; }        // rules run in ascending order

// ---- Conditions (all must match; null = ignore) ----
public string? WhenTextContains { get; set; }
public IssueSeverity? WhenSeverity { get; set; }
public IssuePriority? WhenPriority { get; set; }
public int? WhenCategoryId { get; set; }

// ---- Actions (applied on match; null = no-op) ----
public IssueSeverity? SetSeverity { get; set; }
public IssuePriority? SetPriority { get; set; }
public IssueStatus? SetStatus { get; set; }
public int? AssignToUserId { get; set; }
public string? AddTag { get; set; }
```

> **Jargon, in plain words** — "ANDed" means every stated condition must be true together for the rule to match — title contains 'crash' AND priority is High, not either-or. The question marks (string?, int?) mark each field optional; a null there is the code's way of saying "this condition is not set — ignore it" or "this action is not set — do nothing." A rule with all conditions null therefore matches every new issue, which is a feature: it is how you write a blanket rule. "No child tables" means the whole rule fits on one database row, so the editor is a single simple form.

Because everything sits on one row, the rule editor (`Automation.razor`) is one flat form, and managing rules follows the same Manager-only pattern as every other per-project setting. `AutomationRuleOperations` handles create/update/delete, refusing non-Managers and validating the input — for instance, it re-checks that a category condition actually belongs to this project, "defense in depth against a spoofed id," the same id-scoping caution seen throughout the codebase.


### The pure evaluator: which rules fire, and how they combine

The decision of *which* rules match and *what* they collectively want to do is made by a pure function, `AutomationEvaluator.Evaluate`, kept in `OpenTrack.Core` with no database in sight. It is handed the new issue's facts and the list of rule definitions, and it returns an *outcome* — the net effect. Keeping it pure means the same logic the app runs is the logic the tests drive, with nothing mocked.

Three combining conventions, stated in its summary, make the result predictable:

```csharp
/// Rules are considered in the order given; every rule's conditions are tested against the ORIGINAL
/// issue state (not the running result), so the outcome doesn't depend on subtle action/condition
/// interplay. For scalar actions the last matching rule wins; tags accumulate (de-duplicated,
/// case-insensitive).
```

That middle point is the important one. Every rule is judged against the issue *as it was created*, never against the half-changed result of earlier rules. So if rule 1 sets priority to High and rule 2's condition is "when priority is Low," rule 2 does *not* suddenly stop matching just because rule 1 ran first — because rule 2 is tested against the original Low, not the freshly-set High. This removes a whole category of order-dependent surprises. The loop makes it concrete:

```csharp
foreach (var r in rules)
{
    if (!Matches(input, r)) continue;
    applied.Add(r.Name);

    if (r.SetSeverity is { } s) severity = s;
    if (r.SetPriority is { } p) priority = p;
    if (r.SetStatus is { } st) status = st;
    if (r.AssignToUserId is { } a) assignee = a;
    if (!string.IsNullOrWhiteSpace(r.AddTag))
    {
        var tag = r.AddTag.Trim();
        if (!tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
            tags.Add(tag);
    }
}
```

See the two combining behaviors side by side. For a *scalar* field like severity, each matching rule simply overwrites the running choice, so the *last* matching rule wins — which is exactly why `SortOrder` exists and why later rules "override earlier scalar actions." For *tags*, nothing is overwritten; each new tag is added unless it is already present (case-insensitively), so tags *accumulate*. One issue can collect five tags but end with one severity. And `Matches` is just the plain reading of the conditions — a null condition is skipped, a set one must agree:

```csharp
private static bool Matches(AutomationInput i, AutomationRuleDef r)
{
    if (!string.IsNullOrWhiteSpace(r.WhenTextContains))
    {
        var needle = r.WhenTextContains.Trim();
        var inTitle = i.Title?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true;
        var inDesc = i.Description?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true;
        if (!inTitle && !inDesc) return false;
    }
    if (r.WhenSeverity is { } sev && i.Severity != sev) return false;
    if (r.WhenPriority is { } pri && i.Priority != pri) return false;
    if (r.WhenCategoryId is { } cat && i.CategoryId != cat) return false;
    return true;
}
```

> **Jargon, in plain words** — "Scalar" just means a single-value field — an issue has exactly one severity, one priority, one status — so a later rule setting it replaces the earlier choice. A tag is different: an issue can wear many, so tags add up instead of replacing. "De-duplicated, case-insensitive" means adding 'Crash' when 'crash' is already there does nothing. A pure function is one that only reads its inputs and returns a value — it touches no database and changes nothing outside itself, which is what makes it trivial to test exhaustively.


### The engine: applying the outcome, with safety rails

The evaluator decides; it does not touch the database. Turning its outcome into real changes on the real issue is the job of `AutomationEngine.RunOnCreateAsync`, in the Infrastructure layer. It loads the project's enabled rules in order, runs the pure evaluator, and — only if the outcome actually changes something — applies the mutations to the tracked issue and saves:

```csharp
var outcome = AutomationEvaluator.Evaluate(
    new AutomationInput(issue.Title, issue.Description, issue.Severity, issue.Priority, issue.CategoryId),
    rules.Select(ToDef));
if (!outcome.AnyEffect) return outcome.AppliedRuleNames;

var changed = false;
if (outcome.Severity is { } sev && issue.Severity != sev) { issue.Severity = sev; changed = true; }
if (outcome.Priority is { } pri && issue.Priority != pri) { issue.Priority = pri; changed = true; }
if (outcome.Status is { } st && issue.Status != st) { issue.Status = st; changed = true; }
```

Two safety rails are worth calling out, because they show the engine being careful with power. Automation runs with *system authority* — the rules were, after all, authored by a Manager — so it does not re-check the acting user's edit rights. But it will not do something a Manager themselves could not sensibly want. First: it will only auto-assign to someone who is *actually still a member* of the project, because a member named in an old rule may have been removed since:

```csharp
if (outcome.AssignToUserId is { } uid && issue.AssigneeId != uid)
{
    // Only auto-assign to an actual member of the project (a member may have been removed since
    // the rule was written — don't assign to someone who can no longer see the project).
    if (await db.ProjectMemberships.AnyAsync(m => m.ProjectId == issue.ProjectId && m.UserId == uid, ct))
    {
        issue.AssigneeId = uid;
        changed = true;
    }
}
```

Second: adding a tag is done idempotently — get-or-create the tag by name, then link it only if it is not already linked — and even survives two issues racing to create the same brand-new tag at once, catching the unique-index collision and re-reading the winner's row rather than failing. These are the unglamorous details that keep a convenience feature from becoming a source of corruption.


### Creation only — and the single shared call site

The most important design decision is stated right in the class summary: automation "runs on creation only, so a rule's own action can never re-trigger the engine." Consider the alternative: if rules also ran on every edit, a rule that sets a status could trip a rule that watches that status, which changes a field a third rule watches, and so on — an infinite cascade, or at best an unpredictable one. By firing *only* when an issue is first born, and judging against its original state, the whole hall-of-mirrors problem simply cannot arise.

And, exactly as with the workflow gate in Chapter 15, the engine is invoked from *one* place in each host, right after the new issue's first save (so it already has an id), on the same database context the create used. The web data service calls it:

```csharp
db.Issues.Add(issue);
await db.SaveChangesAsync(ct);
// Apply any project automation rules to the just-created issue (auto-tag/assign/set fields).
await AutomationEngine.RunOnCreateAsync(db, issue, ct);
```

The API host's create endpoint calls the identical `AutomationEngine.RunOnCreateAsync(db, issue, ct)` at the same point in its own flow. Both front doors run the same rules, in the same order, with the same safety rails — there is no second copy of automation logic that could drift. The engine's summary names this on purpose: it is "called from BOTH hosts' create paths … so the rules and their authorization can't drift (mirrors `WorkflowOperations`)."


## Why It Matters / Design Takeaways

Automation is a textbook example of the codebase's favorite split: a pure decision-maker and a thin do-er. `AutomationEvaluator` decides which rules match and combines them by clear, order-stable conventions, with no database anywhere near it — so it is exhaustively testable. `AutomationEngine` takes that verdict and touches the world, adding just the guardrails a database-touching layer must (assign only to real members, tag idempotently). And the feature is deliberately scoped to creation-time, which trades away edit-time automation for the far more valuable property that rules can never chain into themselves.

The rules that must not erode: keep the evaluator pure and matching against the *original* issue state (that is what makes order predictable and testing honest); keep scalar-last-wins and tags-accumulate as the two combining rules; keep the safety rails in the engine (membership-checked assignment, idempotent tagging); and keep automation firing from one shared call site per host, on creation only. If someone ever asks for edit-time automation, that is not a small tweak — it is a decision to re-open the re-triggering problem this design closed, and it must be made deliberately, not slipped in.

> **The maintainer's rule** — Put any new matching or combining logic in the pure AutomationEvaluator and cover it with tests; put anything that touches the database — a new action type, a new safety check — in AutomationEngine. Never add a second place that runs the rules, and do not quietly extend automation to fire on edits: that reopens the self-triggering cascade the creation-only rule was built to prevent.


# 18. Notifications, Webhooks & Two-Way Git

*The three ways OpenTrack talks to the outside world when an issue changes — a private note inside the app, a message pushed out to a chat room, and a signed message pushed in from GitHub that links commits to issues and can close them — each built so a slow, dead, or hostile other end can never break the change that triggered it.*


## What This Is / What It Is For

An issue tracker is not an island. When something changes on an issue — it gets assigned, resolved, commented on — people who care want to hear about it, and other systems may want to know too. And sometimes the change comes from *outside*: a developer pushes code to GitHub that fixes a bug, and it would be tedious to then walk back into OpenTrack and mark that bug resolved by hand. This chapter covers the three doors between OpenTrack and the outside world that all open when an issue changes.

Think of it as the tracker's mail room. One clerk drops a note in each interested person's in-tray inside the building (*in-app notifications*). A second clerk faxes an announcement to the chat rooms the project asked to be kept in the loop (*outgoing webhooks* to Slack or Discord). And a third clerk stands at the loading dock receiving deliveries from GitHub — but only after checking the delivery's tamper-proof seal — and files each commit against the issue it mentions (*inbound Git webhooks*). Three clerks, one rule they all obey: nothing they do to the outside world may ever slow down or break the change that set them off.

> **The one-sentence version** — When an issue changes, OpenTrack notifies people in-app (re-checking each recipient's current access so a title can't leak), pushes a best-effort message out to the project's chat webhooks, and — for pushes coming the other way from GitHub — verifies a cryptographic signature before it trusts a single byte, then links commits to issues and can auto-resolve them; every one of these is wrapped so a failure out there never fails the work in here.

> **Jargon, in plain words** — A webhook is just a URL that one system POSTs a little message to when something happens — 'when an issue changes, phone this number.' 'Outgoing' means OpenTrack does the phoning (telling Slack); 'inbound' means OpenTrack is the one being phoned (GitHub telling it about a push). 'Best-effort' means we try, and if it fails we log it and move on rather than throwing an error up to the user. An HMAC signature is a fingerprint computed from a message plus a shared secret; only someone who knows the secret can produce the matching fingerprint, which is how the receiver knows the message is genuine.


## In-app notifications: the clerk who checks access twice


### What it does

`NotificationDispatch` is the piece that, given 'issue #812 just changed, here's a one-line summary,' writes a notification row for each person who should hear about it — the reporter, the assignee, and anyone explicitly monitoring the issue — and, if a mail server is configured, sends each of them an email too. The person who made the change is never notified of their own change.


### Why it was built this way

There is a subtle trap here, and the class was designed specifically to avoid it. A notification's text includes the issue *title*. But access to an issue can change over time: an issue can be flipped to private, or someone can be removed from a private project. A person who was monitoring an issue last week might no longer be allowed to see it today. If the dispatcher blindly mailed every monitor, it would leak the title of a now-private issue to someone who has lost the right to read it. So the class re-checks each candidate's *current* view access before adding them to the recipient list. Its own summary states the rule plainly: a monitor who has since lost access "is dropped — the notification text contains the issue title and must never leak to someone who can no longer see it."


### How it works

The whole operation is wrapped so it can never fail the change that triggered it. The public method is a try/catch around the real work, and a failure is logged, not thrown:

```csharp
public async Task NotifyIssueChangedAsync(AppDbContext db, int actorUserId, int issueId, string summary, CancellationToken ct = default)
{
    try
    {
        await DispatchAsync(db, actorUserId, issueId, summary, ct);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to dispatch notifications for issue {IssueId}.", issueId);
    }
}
```

Inside, it gathers the candidates — the reporter, the assignee if there is one, and every user monitoring the issue — into a set, then removes the actor so no one is told about their own edit. Then comes the access re-check. For each candidate it builds the same pure `AccessContext` from Chapter 8 and asks the identical `CanViewIssue` question the rest of the app uses:

```csharp
var recipients = new List<(int Id, string? Email)>();
foreach (var u in users)
{
    var ctx = new AccessContext(u.Id, u.Role, memberRoles.TryGetValue(u.Id, out var role) ? role : (UserRole?)null);
    if (ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
        recipients.Add((u.Id, u.Email));
}
```

This is the anti-drift design from Chapter 8 paying off again: the notifier does not invent its own idea of who may see an issue — it calls the one authority everything else calls. Only the survivors get a notification row and, if email is configured, a message. And the email step is itself defensive: `email.IsConfigured` is checked first, and the class summary notes a mail failure "is logged, never thrown."

> **The leak this design prevents** — It is tempting, when adding a new kind of notification, to just look up who is monitoring an issue and message them. Don't. Membership and privacy drift over time. Always route the recipient list through CanViewIssue (or the equivalent) as NotificationDispatch does, or you will eventually mail the title of a private issue to someone who was quietly removed from the project.


## Outgoing webhooks: announcing to the chat rooms


### What it does

A project can register outgoing webhooks — URLs that OpenTrack POSTs a short message to whenever one of the project's issues changes. `WebhookDispatch` is the piece that does the POSTing. It supports three shapes of message: Slack's, Discord's, and a generic JSON format for anything else (a custom script, another tracker, an automation tool).

Notice how the notifier and the webhook dispatcher are wired together. `NotificationDispatch` holds an optional `WebhookDispatch` and fires the project's webhooks as part of dispatching — but it does so *regardless* of whether anyone is subscribed in-app, because a chat room caring about a project is independent of any individual watching the issue:

```csharp
// Fire project webhooks regardless of who (if anyone) is subscribed in-app. Best-effort and
// non-blocking; it only awaits a quick DB read for the active hooks.
if (webhooks is not null)
    await webhooks.DispatchAsync(db, issue.ProjectId, issue.Project.Name, issue.Id, issue.Title, issue.Status.ToString(), summary, ct);
```


### Why it was built this way

The dominant design worry with an outgoing webhook is that the other end is out of your control. A Slack endpoint could be slow, down, or hanging. If OpenTrack waited for that POST to finish before completing the user's edit, one dead webhook would freeze the whole edit for everyone. So the dispatcher is deliberately *fire-and-forget*: it does one quick database read to find the active hooks, then launches the HTTP POSTs without awaiting them. The class summary is explicit that this is so "a slow or dead webhook can never delay or fail the edit that triggered it."


### How it works

Each hook stores a URL and a format. The dispatcher builds the right payload shape per format with a simple switch — Slack wants `{ text: ... }`, Discord wants `{ content: ... }`, and everything else gets a structured JSON object with the event, ids, title, status, and a UTC timestamp:

```csharp
object payload = h.Format switch
{
    WebhookFormat.Slack => new { text = message },
    WebhookFormat.Discord => new { content = message },
    _ => new
    {
        @event = eventText,
        projectId,
        projectName,
        issueId,
        title = issueTitle,
        status,
        occurredAtUtc = DateTime.UtcNow,
    },
};
_ = SendAsync(h.Url, payload); // fire-and-forget; must not delay the caller
```

That leading `_ =` is the fire-and-forget in the flesh: the task is started and intentionally not awaited. The `SendAsync` helper wraps its own POST in try/catch and only logs failures, so a bad hook is a log line, never an exception the user sees. Each POST is also bounded by the shared `HttpClient` timeout, so even a hook that accepts the connection and then hangs forever is eventually abandoned.


### Why managing a webhook needs the Manager role

Configuring the hooks — listing, adding, deleting them — lives in `WebhookOperations`, and both reading and writing them require the Manager role on the project. That is stricter than you might expect for a mere list of URLs, and there is a specific reason: a Slack or Discord webhook URL *is a secret* — anyone holding it can post into that chat room. The class summary spells it out: "because a webhook URL can carry a secret token (Slack/Discord URLs do), reading and managing them both require the Manager role." Adding a hook also validates the URL is a well-formed absolute `http`/`https` address, with a pointed comment about not aiming a webhook at an internal address you don't control (a class of attack called SSRF, server-side request forgery).

> **Jargon, in plain words** — SSRF (server-side request forgery) is a trick where an attacker gets a server to make a request on their behalf to somewhere they couldn't reach directly — for example, an internal admin page that only the server can see. Because OpenTrack's webhooks are configured by a trusted Manager on a self-hosted box, the code treats the URL field as a trust boundary rather than a full sandbox, but it still insists on a proper public-shaped http(s) URL.


## Inbound Git webhooks: deliveries from GitHub


### What it does

This is the two-way part, and the most interesting. GitHub can be told to POST to OpenTrack every time code is pushed to a repository. OpenTrack receives that push, reads the commit messages, finds any that mention an issue (`#123`) or say they fix one (`fixes #45`), records a link between the commit and the issue, and — if the project opted in — automatically marks 'fixes' issues as Resolved. A developer fixes a bug in code and the tracker updates itself.


### Why the signature check comes first

GitHub cannot log into OpenTrack — it has no account, no session, no cookie. So the receiving endpoint is *unauthenticated* in the usual sense. That would be alarming if anyone could POST a fake 'push' claiming commits fix issues. The defense is a cryptographic signature. When you set up the webhook, you paste a shared secret into GitHub. GitHub then signs every delivery with that secret, and OpenTrack recomputes the same signature and compares. No valid signature, no trust — and the check happens before a single byte of the payload is acted on.

`GitSignature` does the verifying. It expects GitHub's `X-Hub-Signature-256` header — the literal text `sha256=` followed by the HMAC-SHA256 of the exact request body, keyed by the project's secret — and compares in constant time:

```csharp
using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
var expected = Convert.ToHexStringLower(hmac.ComputeHash(body));

// Constant-time compare of the two hex strings.
var a = Encoding.ASCII.GetBytes(expected);
var b = Encoding.ASCII.GetBytes(provided);
return CryptographicOperations.FixedTimeEquals(a, b);
```

> **Why the comparison is 'constant-time'** — A naive string comparison bails out at the first character that differs, so it returns a hair faster for a wrong guess that shares more leading characters with the real answer. An attacker measuring those tiny timing differences could, over many tries, reconstruct the secret fingerprint one character at a time. FixedTimeEquals always takes the same amount of time regardless of where the values differ, closing that side channel. It is a small habit that matters everywhere secrets are compared.

There is a matching subtlety on the storage side. When a Manager saves the webhook secret, the code stores it *exactly as entered* — no trimming — with a comment explaining why: "GitHub signs over the exact bytes you paste into its webhook Secret field, so trimming here would silently break verification for a padded secret." A stray trailing space in the secret must survive on both ends or the fingerprints will never match.


### How the receiver behaves

The endpoint reads the raw body once (the signature must be computed over the exact bytes GitHub sent), looks up the project's Git config, and then makes a careful series of decisions. Two of them are about not leaking information: if the project has no Git integration or it is disabled, and again if the signature is invalid, it returns the *same* `404 Not Found` — never revealing whether the project even exists or has Git enabled:

```csharp
var config = await db.GitIntegrations.AsNoTracking().FirstOrDefaultAsync(g => g.ProjectId == projectId, ct);
if (config is null || !config.Enabled)
    return Results.NotFound(); // don't reveal whether the project exists / is configured

var signature = request.Headers["X-Hub-Signature-256"].ToString();
if (!GitSignature.IsValid(config.WebhookSecret, body, signature))
    return Results.NotFound(); // same response as unconfigured — don't reveal which projects have Git enabled
```

Only past those gates does it inspect the event type. A GitHub 'ping' (sent when you first save the webhook) gets a friendly `pong`; anything that isn't a 'push' is politely accepted and ignored; a real push has its commits parsed out of the JSON and handed to the processing step. The whole route is also rate-limited under the shared `intake` policy — the same throttle the public ticket form uses — so even a valid-looking flood is bounded.


### Finding issue references: the pure parser

The job of reading a commit message like "fix login crash, closes #42 and touches #7" and pulling out the issue numbers lives in `GitRefParser`, deliberately placed in `OpenTrack.Core` with no dependencies so the webhook receiver and the tests share one definition. It recognizes plain mentions (`#123`) and GitHub's closing keywords (`fix`, `fixes`, `fixed`, `close`, `closes`, `resolve`, `resolved`, and so on). If the same issue is mentioned both plainly and with a closing keyword, closing intent wins — the reference is treated as intending to resolve it:

```csharp
var closing = m.Groups["kw"].Success;
byIssue[id] = byIssue.TryGetValue(id, out var existing) ? existing || closing : closing;
```

The regular expression that finds these is carefully anchored so it won't match a `#` glued to a preceding word — `abc#12` and `v1.2#3` are not issue references, only a `#number` standing on its own. Keeping the parser pure and in Core means its tricky behavior can be unit-tested exhaustively without a database or a web server in sight.


### Linking and auto-resolving — safely

`GitIntegrationOperations.ProcessPushAsync` takes the parsed commits and does the real work. It is a *trusted system action*: the signature already proved the request genuine, so there is no signed-in user, and the resulting history and notifications are attributed to the project owner and the 'system' actor id 0. But 'trusted' does not mean careless — the method guards against several ways a push could misbehave, whether by accident or by a crafted payload.

First, the referenced issue must belong to *this* project; a commit can't reach across projects. Second, duplicate links are prevented even within a single push. A commit can appear twice in one payload, and blindly inserting both would violate the database's unique index and abort the entire push. So the code tracks what it has already seen in this batch as well as what is already in the database:

```csharp
var key = (issue.Id, c.Sha);
var alreadyLinked = seen.Contains(key)
    || await db.IssueCommitLinks.AnyAsync(l => l.IssueId == issue.Id && l.Sha == c.Sha, ct);
```

Third, and most carefully, auto-resolve is gated on the link being *newly* created. This stops a replayed push — GitHub retrying, or someone re-sending an old delivery — from re-resolving an issue that a human has since deliberately reopened. The condition reads like a checklist of everything that must be true: a closing reference, newly linked, the project opted in, the issue not already resolved, and the workflow actually permits the move to Resolved:

```csharp
if (r.Closing && newlyLinked && autoResolve && (int)issue.Status < (int)IssueStatus.Resolved
    && !resolvedIssueIds.Contains(issue.Id)
    && await WorkflowOperations.IsAllowedAsync(db, projectId, issue.Status, IssueStatus.Resolved, ct))
{
    var old = issue.Status;
    issue.Status = IssueStatus.Resolved;
    issue.UpdatedAt = now;
    issue.History.Add(new IssueHistory
    {
        UserId = project.OwnerId, FieldChanged = "Status",
        OldValue = old.ToString(), NewValue = IssueStatus.Resolved.ToString(), ChangedAt = now,
    });
    resolvedIssueIds.Add(issue.Id);
}
```

Notice that even an automated resolve still respects the project's own workflow rules (Chapter 15) via `WorkflowOperations.IsAllowedAsync` — Git integration is not a back door around the state machine. When the dust settles, each auto-resolved issue is announced through the very same `NotificationDispatch` we started with, using actor id 0 so that everyone who cares — including the owner — is notified. The three clerks are connected: a delivery at the loading dock ends with a note in everyone's in-tray.

> **How the two directions meet** — Follow one commit all the way through: GitHub POSTs a push → GitSignature proves it's genuine → GitRefParser reads 'fixes #42' → ProcessPushAsync links the commit, checks the workflow, and sets #42 to Resolved as the system actor → NotificationDispatch tells #42's watchers, re-checking each one's access → WebhookDispatch announces it to the project's Slack room. Inbound Git and outgoing chat are two ends of the same pipe.


## Why It Matters / Design Takeaways

The unifying idea across all three subsystems is *containment of the outside world*. Every interaction with something OpenTrack does not control — a user's mail server, a project's Slack room, a push from GitHub — is treated as capable of being slow, absent, or hostile, and is wrapped so that its failure or malice stays contained. Notifications are best-effort and never throw. Webhooks are fired without waiting. Inbound Git is distrusted until a signature proves it genuine, then still guarded against replays and cross-project reach.

The second idea is *reuse of the one authority*. The notifier does not reimplement 'who may see this issue' — it calls `CanViewIssue`. Auto-resolve does not bypass the workflow — it calls `WorkflowOperations`. The Git parser lives in Core so tests and the receiver share one definition. Each new door to the outside world leans on the rules that already exist rather than growing a private copy — the same anti-drift discipline that runs through the whole codebase.

> **The maintainer's rule** — When you add anything that reacts to a change or accepts a message from outside, obey both halves of this chapter. Contain the outside end: wrap it best-effort, never let it block or fail the triggering change, and bound it with a timeout or rate limit. And trust the inside authorities: re-check access before you put an issue's text in front of someone, verify a signature before you act on an inbound request, and route state changes through the workflow — never around it.


# 19. Public Intake, QR & the Guardrails

*The one door into OpenTrack that needs no account — a public 'Report a problem' form anyone can fill in — and the layered guardrails (an off-by-default project gate, a hidden honeypot, per-IP rate limiting, and hard length caps) that keep that open door from becoming an open drain, plus the QR poster that puts the door on a wall and the email-gated status lookup that lets a submitter check just their own ticket.*


## What This Is / What It Is For

Everywhere else in OpenTrack, creating or reading an issue requires an account and the access rules of Chapter 8. Public intake is the deliberate exception. Its whole reason to exist is the person who is *not* a member of your team: a customer who hit a bug, a visitor to a building who spotted a broken door, a volunteer at an event. You want them to be able to tell you, and you do not want to make them create an account first. So OpenTrack offers a public 'Report a problem' page that anyone can open and submit — no login, no password.

An unlocked front door is convenient and dangerous in equal measure. The moment a form can create records with no account behind it, it becomes a target for spam bots, abusive floods, and giant junk submissions. So the interesting engineering here is not the form — forms are easy — it is the *guardrails* that let the door stay open without becoming a way to fill your database with garbage. Picture a suggestion box bolted to the wall outside a shop: anyone can drop a note in, but the slot is narrow (you can't stuff a phone book through it), there's a limit to how fast one person can post, and there's an invisible trip-wire that catches the vandals.

> **The one-sentence version** — Public intake is the only issue-creation path with no signed-in user; it is fenced by four layers — a per-project on/off flag that defaults to off, a hidden honeypot field that silently drops bots, a per-IP rate limiter, and hard length caps on every field — and it pairs with an email-gated status lookup so a submitter can read their own ticket and only their own.


## The intake path: one door, plainly marked


### What it does

The core logic lives in `PublicIntakeOperations`, whose summary names exactly what makes it special: it is "the one issue-creation path that is NOT authenticated." A submission becomes an ordinary issue — it shows up in the normal issue list, gets worked like any other — but it is attributed to the project's owner rather than to a user account, because the real submitter has no account. Their name and email are captured on the issue so the team knows who reported it and so the submitter can later look up its status.


### Why it was built this way

The design keeps the two concerns in separate layers, matching the whole codebase's habit. The domain rules that must always hold — the project must actually be accepting submissions, and every field has a maximum length — live in `PublicIntakeOperations` in Infrastructure, where they cannot be skipped no matter which host calls them. The web-only defenses that depend on the HTTP request — the honeypot field and the rate limiter — live up in the web endpoint. The class summary states this split directly: "Length caps and the project gate live here; the web layer adds rate-limiting and a honeypot."


### How it works

The first thing `SubmitAsync` does is check the project gate, and refuse if the project is not accepting public reports:

```csharp
var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
if (project is null || !project.PublicIntakeEnabled)
    return new IntakeResult(null, "This project isn't accepting public submissions.");
```

`PublicIntakeEnabled` is off by default; a Manager must deliberately turn it on for a project. This is the outermost guardrail: intake does not exist for a project until someone with authority opts that project in. Then every field is trimmed and truncated to a hard cap, and the reason is spelled out in a comment aimed squarely at the anonymous nature of the endpoint:

```csharp
title = title?.Trim() ?? "";
if (title.Length == 0) return new IntakeResult(null, "Please describe the problem in the summary.");
title = Truncate(title, FieldLimits.IssueTitle);

name = Blank(name) ? null : Truncate(name!.Trim(), FieldLimits.IntakeName);
// Cap the free-text description — this is an UNAUTHENTICATED endpoint, so an anonymous submitter
// could otherwise store a multi-megabyte body (bounded only by Kestrel's request limit).
description = Blank(description) ? null : Truncate(description!, FieldLimits.IntakeDescription);
email = Blank(email) ? null : email!.Trim();
if (email is not null && (!email.Contains('@') || email.Length > FieldLimits.IntakeEmail))
    return new IntakeResult(null, "Enter a valid email address, or leave it blank.");
```

The created issue is a normal `Issue` with sensible defaults (status New, severity Minor, priority Normal), reported by the project owner, but with the submitter's `IntakeName` and `IntakeEmail` stored on it. A history entry is written just as for any new issue. The submitter's details are also woven into the description so the team sees at a glance that this came from the public form and who sent it:

```csharp
var parts = new List<string> { "*Submitted via the public \u201cReport a problem\u201d form.*" };
var who = string.Join(" ", new[] { name, email is null ? null : $"<{email}>" }.Where(s => !string.IsNullOrEmpty(s)));
if (who.Length > 0) parts.Add($"Reported by: {who}");
```

> **Jargon, in plain words** — Kestrel is the built-in web server that runs an ASP.NET app; it has its own outer limit on how big a request body may be. The comment above notes that without the description cap, an anonymous body would be bounded only by that server-wide limit — which could still be many megabytes. Capping each field in the domain layer means the smallest, tightest limit wins, and it applies no matter which host serves the form.


## The four guardrails, layer by layer

It helps to see the defenses as concentric rings, from the broadest gate down to the narrowest field. Each ring catches a different kind of abuse, and together they let the door stay unlocked.

| Guardrail | Where it lives | What it stops |
| --- | --- | --- |
| Project gate (PublicIntakeEnabled) | PublicIntakeOperations (domain) | Intake existing at all for a project nobody opted in |
| Honeypot 'website' field | PublicIntakeWebEndpoints (web) | Automated bots that fill every field |
| Per-IP 'intake' rate limiter | Program.cs / RequireRateLimiting | One client flooding the form |
| Length caps on every field | PublicIntakeOperations (domain) | Giant junk bodies from an anonymous poster |


### The honeypot: a field only a robot loves

A honeypot is a form field that real people never see and never fill, but automated bots — which fill every field they find — do. The public report page renders a hidden `website` field positioned far off-screen and marked to be ignored by assistive tech:

```csharp
@* Honeypot: real people never see or fill this; bots do, and we silently drop those. *@
<div style="position:absolute; left:-9999px; top:-9999px;" aria-hidden="true">
    <label>Leave this field empty<input type="text" name="website" tabindex="-1" autocomplete="off" /></label>
</div>
```

On the server, if that field comes back with anything in it, the submission is treated as a bot and dropped — but the crucial detail is *how* it is dropped. The endpoint pretends success, redirecting exactly as a real submission would, rather than showing an error:

```csharp
// Honeypot: bots fill the hidden "website" field. Pretend success and drop it silently.
if (!string.IsNullOrWhiteSpace(form["website"]))
    return Results.Redirect($"/report/{projectId}?ref=0");
```

> **Why fake success beats a visible rejection** — If the honeypot returned an obvious error, a bot author would notice their submissions were being blocked and adjust. By returning the same happy redirect a real user gets (with ref=0), the bot 'succeeds' into a black hole and has no signal that anything was filtered. Silence is the point — the trap works best when the vandal never learns it exists.


### Rate limiting: the same throttle the Git webhook uses

Both public routes — the submit and the status lookup — are tagged with `.RequireRateLimiting("intake")`. That names a policy defined once at startup in the web host. It is a fixed-window limiter partitioned by client IP address: each IP gets a fixed number of permits per time window, and requests beyond that are rejected:

```csharp
options.AddPolicy("intake", http =>
    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
        {
            PermitLimit = 8,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
        }));
```

Eight requests per five minutes per IP, with no queue — an over-limit request is turned away with a 429, not held. This is the same `intake` policy the inbound Git webhook of Chapter 18 attaches to. One named throttle protects every unauthenticated entry point, so there is a single place to tune the tolerance for the whole 'anyone can call this' surface of the app.

> **Jargon, in plain words** — A 'fixed-window' rate limiter counts requests inside a repeating clock window — here, five minutes — and resets the count when the window rolls over. 'Partitioned by IP' means each client address gets its own independent count, so one abusive address can't use up everyone else's allowance. A 429 is the HTTP status code for 'Too Many Requests.'


## Status lookup: reading your own ticket, and only yours


### What it does

A submitter who left an email gets a reference number back ("your reference is #42"). The public status page lets them come back later, enter that number plus the email they used, and see the current status of their ticket. No account, but also no browsing of anyone else's tickets.


### Why it was built this way

The obvious naive version — look up an issue by its number — would let anyone read any public-intake ticket just by guessing sequential numbers. The fix is to require *both* the reference and the exact email the ticket was submitted with, treating the email as a shared secret between the submitter and their own ticket. The lookup only ever returns issues that actually came through public intake:

```csharp
if (Blank(email)) return null;
var e = email!.Trim();
var issue = await db.Issues.AsNoTracking()
    .FirstOrDefaultAsync(i => i.Id == reference && i.IntakeEmail != null && i.IntakeEmail.ToLower() == e.ToLower(), ct);
return issue is null ? null : new IntakeStatus(issue.Id, issue.Title, issue.Status);
```

The method summary states the guarantee: it "requires the reference AND the exact email used, so one submitter can't read another's ticket, and it only ever returns public-intake issues." A mismatch and a not-found both come back as the same 'no matching ticket' — no hint about whether the number exists. Only a submission with a stored `IntakeEmail` can ever be found this way; a normal internal issue, which has no intake email, is invisible to this lookup entirely.


### How the pages fit together

The two public pages, `PublicReport.razor` and `TicketStatus.razor`, are both marked `[AllowAnonymous]` and are plain server-rendered forms that POST to the endpoints above. They are intentionally simple HTML forms rather than interactive components, so they work with nothing but a browser — no JavaScript, no live connection required. The report page even switches to a friendly 'this project isn't accepting submissions' message when the gate is off, reading the same `PublicIntakeEnabled` flag the domain layer enforces:

```csharp
@if (!enabled)
{
    <h1 class="h3 mt-4">Report a problem</h1>
    <div class="alert alert-info mt-3">This project isn't accepting public submissions right now.</div>
}
```


## The QR poster: the door on a wall


### What it does

`IntakePoster.razor` turns a project's public report page into something physical: a printable poster with a big QR code. Point a phone camera at it and the report form opens. It is meant to be printed and stuck up where problems happen — a lobby, a workshop, an event venue — so someone can report an issue on the spot without knowing any URL.


### Why it was built this way, and how

Unlike the public pages, the poster page is *not* anonymous — it is `[Authorize]`, and it further requires the Manager role, because generating and printing the intake link is a project-administration act. It builds the same public report URL a phone would visit and renders a QR code for it as inline SVG, using the approved `QRCoder` package:

```csharp
intakeUrl = $"{Nav.BaseUri}report/{Id}";
qrSvg = BuildQrSvg(intakeUrl);

// ...

using var generator = new QRCodeGenerator();
using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
return new SvgQRCode(data).GetGraphic(6);
```

The poster is print-first: a Print button calls the browser's own print dialog, print-only CSS scales the QR code up for the page, and controls tagged `no-print` disappear on paper. There is one honest bit of user-guarding, too — if public intake is currently off for the project, the poster shows a warning that the QR code won't accept reports yet and points the Manager to the setting. That closes the loop with the domain gate: printing a poster that leads to a closed door would only frustrate people, so the page says so up front.

> **Rendering a QR code as SVG** — A QR code is just a grid of black and white squares. Rendering it as inline SVG (vector shapes) rather than a bitmap image means it stays razor-sharp at any print size — a poster blown up to A3 has no fuzzy pixels — and it needs no separate image file to load. QRCoder is the small library that produces that SVG from a piece of text, here the public report URL.


## Why It Matters / Design Takeaways

Public intake is a study in opening a door safely. The feature everyone asks for — 'let people report problems without an account' — is trivial to build and easy to build badly. What makes OpenTrack's version sound is not the form but the discipline around it: an off-by-default gate so no project is exposed by accident, a silent honeypot so bots defeat themselves, a shared rate limiter so one client can't flood the works, and hard length caps in the domain layer so the anonymous nature of the endpoint can't be turned into a storage attack.

The placement of those defenses is the lesson worth keeping. The rules that must never be skipped — the gate and the length caps — live down in `PublicIntakeOperations` where every caller inherits them. The web-shaped defenses — the honeypot and the rate limiter — live in the web layer where the HTTP request actually is. And reading your own ticket back is gated by a secret you already hold (your email), not by an account you never made.

> **The maintainer's rule** — If you ever add another way for an anonymous stranger to reach the database, copy this shape exactly: gate it behind an explicit, off-by-default opt-in; cap every field in the domain layer, not just the UI; put it behind the shared 'intake' rate limiter; and if it returns anyone's data, require a secret the requester already holds. An unauthenticated endpoint with none of these is not a feature — it is a spam funnel waiting to be found.


# 20. The AI Assistant: one provider seam, three helpers

*The optional, opt-in AI layer — a single interface with two interchangeable implementations behind it (Anthropic's Claude, or any OpenAI-compatible endpoint including a local Ollama that never leaves your machine) — powering three conveniences: suggesting a triage for a new issue, turning plain-English into a search filter, and summarizing a long thread; all server-side, all best-effort, and every one of them a suggestion a human still owns.*


## What This Is / What It Is For

Some jobs in an issue tracker are tedious in a way a language model is genuinely good at: guessing how severe a new bug is, translating a vague request like 'crashes nobody has touched in a month' into concrete filter settings, or reading a forty-comment thread and telling you where it stands. OpenTrack offers all three as *optional* AI helpers. The key word is optional: the AI is off by default, must be deliberately configured by the server operator, and never does anything a human didn't ask for and can't override.

Think of the AI as a smart intern who whispers suggestions. When switched on, the intern can propose a severity, sketch a search from a sentence, or summarize a long conversation. But the intern never files anything, never changes a record, and never sees anything the person they're helping couldn't already see. And the intern is entirely replaceable: you can hire a cloud one (Anthropic's Claude, or OpenAI), or a local one that works inside your own building and tells no one outside anything. The rest of OpenTrack neither knows nor cares which — it only knows there's an intern, or there isn't.

> **The one-sentence version** — A single IAiAssistant interface hides which provider is in use (Anthropic or any OpenAI-compatible endpoint, cloud or local); it exposes exactly three helpers — triage, search-interpretation, and summarize — each opt-in, server-side, best-effort (returns null on any failure so nothing is ever blocked), and each producing only a suggestion or a filter a human still controls.

> **Jargon, in plain words** — An interface is a promise about what methods exist, with no code behind them — a socket that any matching plug can fill. 'Opt-in' means it does nothing unless someone deliberately turns it on. 'Server-side' means the calls happen on OpenTrack's own machine, never in the visitor's browser, so an API key is never exposed. 'Best-effort' means if the AI is off or the call fails, the method quietly returns null and the app carries on as if the feature weren't there. An OpenAI-compatible endpoint is any service that speaks the same request/response shape OpenAI uses — many do, including local engines like Ollama.


## One interface, three promises


### What it does

`IAiAssistant` is the seam the whole feature hangs on. It declares one property and three methods — is the assistant enabled, and the three helpers — and nothing else. Every method returns a nullable value, and that is a design decision, not an accident: null is the universal 'not available' answer that lets every caller degrade gracefully.

```csharp
public interface IAiAssistant
{
    bool IsEnabled { get; }

    Task<TriageSuggestion?> SuggestTriageAsync(
        string title, string? description, IReadOnlyList<string> categories, CancellationToken ct = default);

    Task<SearchCriteria?> InterpretSearchAsync(
        string query, IReadOnlyList<string> projectNames, CancellationToken ct = default);

    Task<string?> SummarizeIssueAsync(
        string title, string? description, IReadOnlyList<string> notes, CancellationToken ct = default);
}
```


### Why it was built this way

Two forces shaped this interface. First, the AI is genuinely optional, so 'off' must be a first-class state, not an error — hence `IsEnabled` and the nullable returns. The interface summary is explicit: when disabled or unconfigured, "`IsEnabled` is false and the methods return null, so callers degrade gracefully." Second, an AI call is over the network to someone else's model and can fail in a dozen ways; a failure to suggest a severity must never stop you from filing a bug. So the contract itself bakes in 'this might just not answer,' and every caller is written to shrug and move on.

There is also a privacy note built into the very type of a suggestion. `TriageSuggestion` — a severity, priority, category, and tags — carries a comment that governs how the whole feature behaves: "Every field is a suggestion for a human to accept or change — never applied automatically." The AI proposes; the person disposes.


## Two providers behind the seam


### What it does

Behind the interface sit two implementations. `AnthropicAiAssistant` talks to Anthropic's Messages API. `OpenAiAssistant` talks to any OpenAI-*compatible* Chat Completions API — and that one class covers a surprising range: OpenAI itself, Azure OpenAI, Groq, OpenRouter, and, importantly, local engines like Ollama and LM Studio running on your own hardware. The only real difference between those is a base URL and whether a key is needed.


### Why two, and why this split

Supporting both a first-class Claude integration and 'anything OpenAI-shaped' covers essentially the whole market with two classes instead of a dozen. The local-engine case is the one that matters most for a self-hosted tracker: an operator uneasy about sending issue text to a cloud can point OpenTrack at Ollama on `localhost` and keep every word on their own machine. The configuration class, `AiOptions`, captures all of this in settings that come only from the server's configuration — "appsettings / environment / user-secrets — never the database or the browser":

```csharp
/// <summary>"anthropic" (Claude, default) or "openai" (any OpenAI-compatible Chat Completions API).</summary>
public string Provider { get; set; } = "anthropic";

public string? ApiKey { get; set; }

/// <summary>Model id. Defaults to a fast, inexpensive Claude model; set this to match your provider
/// (e.g. "gpt-4o-mini" for OpenAI, or a local model name like "llama3.1" for Ollama).</summary>
public string Model { get; set; } = "claude-haiku-4-5-20251001";
```

Note the small but real cleverness in `HasCredentials`: a cloud provider needs a key, but a *local* OpenAI-compatible engine (identified by a custom `BaseUrl`) needs none — so the code treats 'has a key' OR 'is a local openai endpoint' as usable. That's why a laptop running Ollama with no API key at all can still power the assistant.


### How the right one is chosen

Which implementation you get is decided once, at startup, in dependency injection. Both concrete classes are registered as typed HTTP clients (with different timeouts — a local model can be slow to produce its first token, so it gets longer), and then `IAiAssistant` is resolved to whichever one the configured provider names:

```csharp
services.AddHttpClient<Ai.AnthropicAiAssistant>(c => c.Timeout = TimeSpan.FromSeconds(30));
services.AddHttpClient<Ai.OpenAiAssistant>(c => c.Timeout = TimeSpan.FromSeconds(60));
services.AddScoped<Ai.IAiAssistant>(sp => options.IsOpenAi
    ? sp.GetRequiredService<Ai.OpenAiAssistant>()
    : sp.GetRequiredService<Ai.AnthropicAiAssistant>());
```

Everything downstream — the web data service, the API endpoints, the pages — only ever asks for `IAiAssistant`. They are blissfully unaware of which provider answered. Swap the provider in configuration and not one line of the feature code changes. This is the same seam pattern the whole app uses for its data service (Chapter 11), applied to the AI.


## The provider-agnostic middle: shared prompts and schemas


### What it does

Here is the part that keeps the two providers honest. If the Anthropic path and the OpenAI path each wrote their own prompts and their own idea of the answer's shape, they would slowly diverge — the same issue could get a different triage depending on which provider was configured. So the wording of every prompt, the JSON schema of every answer, and the mapping from the model's reply back to a C# value all live in provider-neutral helper classes: `AiTriage`, `AiSearch`, and `AiSummary`. Both providers call into these.

`AiTriage`'s summary states the intent: these pieces are "shared by the Anthropic and OpenAI-compatible assistants so both speak the same triage contract and stay in lock-step." The prompt, the tool schema, and the parsing are written once. The only thing each provider class does differently is the network mechanics — how to wrap that shared prompt and schema in the request its particular API expects.


### How it works: strict answers via tool-use

For triage and search, OpenTrack does not just ask the model for prose and hope to parse it. It uses *tool-use* (also called function-calling): the model is told about a single 'tool' with a strict JSON schema, and instructed to answer only by calling that tool. That forces the reply into a fixed shape the code can map reliably. The schema is one shared object usable by both providers, because Anthropic's `input_schema` and OpenAI's function `parameters` are both JSON-schema:

```csharp
public static object BuildInputSchema() => new
{
    type = "object",
    properties = new Dictionary<string, object>
    {
        ["severity"] = new { type = "string", @enum = Enum.GetNames<IssueSeverity>() },
        ["priority"] = new { type = "string", @enum = Enum.GetNames<IssuePriority>() },
        ["category"] = new { type = "string" },
        ["tags"] = new { type = "array", items = new { type = "string" } },
    },
};
```

The two provider classes then differ only in packaging. The Anthropic version puts the tool under `tools` with a `tool_choice` forcing that tool; the OpenAI version wraps the same name/description/schema as a `function`. Compare them side by side and the shared pieces (`toolName`, `toolDesc`, `schema`, the same `prompt`) are identical — only the envelope changes:

```csharp
// Anthropic
tools = new object[] { new { name = toolName, description = toolDesc, input_schema = schema } },
tool_choice = new { type = "tool", name = toolName },

// OpenAI-compatible
tools = new object[]
{
    new { type = "function", function = new { name = toolName, description = toolDesc, parameters = schema } },
},
tool_choice = new { type = "function", function = new { name = toolName } },
```

Parsing back is equally shared and equally defensive. `AiTriage.FromInput` is "tolerant of missing/unknown values: an unrecognized enum becomes null, and a category is accepted only if it exists on the project." So even a model that hallucinates a category name it invented cannot slip it through — the code only accepts a category that really exists:

```csharp
if (input.TryGetProperty("category", out var c) && c.ValueKind == JsonValueKind.String)
{
    var raw = c.GetString();
    // Only accept a category that actually exists on the project.
    cat = categories.FirstOrDefault(x => string.Equals(x, raw, StringComparison.OrdinalIgnoreCase));
}
```

> **Jargon, in plain words** — Tool-use / function-calling is a way of asking a model to answer by 'calling a function' whose arguments must match a schema you supply, instead of replying in free text. You get back structured data (fields you named) rather than a paragraph you'd have to parse. JSON schema is just a description of the allowed shape — which fields, what types, which values are permitted. Both Anthropic and OpenAI accept the same schema shape, which is exactly why one definition can serve both.

One more shared guard lives in `AiText.Cap`: every piece of user text is truncated before it is put in a prompt, so a caller "can't drive an arbitrarily large (billable) request to the AI provider." The title is capped at 500 characters, the description at 4000, a search query at 500. Cost and abuse are bounded at the prompt, once, for both providers.


## The three helpers in action


### Helper 1 — smart triage: a suggestion, never a decision

On the new-issue screen, when the assistant is enabled, a 'Suggest with AI' button appears. Pressing it does not create the issue — it *pre-fills* the form. The page's own comment says so, and the handler treats every returned field as optional, applying only what came back and leaving the rest untouched:

```csharp
// The "Suggest with AI" button pre-fills the form instead of creating the issue.
if (model.Action == "ai")
{
    var s = await Data.SuggestTriageAsync(ProjectId, model.Title, model.Description);
    if (s is null) { error = "AI couldn't suggest a triage right now."; }
    else
    {
        if (s.Severity is { } sv) model.Severity = sv;
        if (s.Priority is { } pr) model.Priority = pr;
        if (s.Category is { } cat)
            model.CategoryId = categories.FirstOrDefault(c => string.Equals(c.Name, cat, StringComparison.OrdinalIgnoreCase))?.Id;
        aiSuggestedTags = s.Tags;
    }
}
```

The user still reviews and submits. If the AI is off or the call fails, `s` is null and the form is simply left as the human typed it — the feature's absence is invisible, exactly the graceful degradation the interface promised.


### Helper 2 — plain-English search, with no new powers

The second helper turns a sentence into a set of filter fields. Its result type, `SearchCriteria`, is carefully scoped, and its summary explains the safety property in one breath: these fields "map one-to-one onto the issue list's existing query-string filter, so natural-language search can only ever produce a filter the user could have built by hand — no new query power, no ACL bypass." The AI is a nicer way to drive the existing search, not a new search that skips the rules.

The prompt teaches the model the app's own vocabulary — for instance that 'stale', 'idle', 'untouched', or 'nobody has touched' all mean the existing `stale=true` filter — and, crucially, the *project* it may match is constrained to a list of names the caller can actually see. That list is built with the same `WhereVisibleTo` filter from Chapter 8, so the model is never even shown a project the user isn't allowed to know exists:

```csharp
// Constrain any project match to the caller's visible projects.
var projectNames = await db.Projects.AsNoTracking()
    .WhereVisibleTo(access).OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(ct);
var c = await ai.InterpretSearchAsync(req.Query, projectNames, ct);
```


### Helper 3 — thread summary, fed only what you may read

The third helper summarizes an issue thread. This one is the most privacy-sensitive, because it hands the model the issue's comments — and some comments may be private. The guard is that the summarizer is only ever given notes the caller is *already* allowed to see; the AI cannot become a way to launder a private note into a summary. In the web data service the summary reuses the same access-checked loader the detail page uses, and the interface itself documents that the notes passed in are 'already-ACL-filtered'.

The API endpoint makes the filtering visible line by line: it loads the issue, refuses (as a plain not-found, so it doesn't even leak that a private issue exists) if the caller can't view it, and then feeds the model only the notes that pass `CanViewNote`:

```csharp
var ctx = access.For(issue.ProjectId);
if (!ctx.CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId))
    return Results.NotFound(); // don't leak existence of a private issue

// Only feed the model notes this caller may see.
var notes = issue.Notes.Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
    .OrderBy(n => n.CreatedAt)
    .Select(n => $"{n.Author.UserName ?? "unknown"}: {n.Text}")
    .ToList();
var summary = await ai.SummarizeIssueAsync(issue.Title, issue.Description, notes, ct);
```

Unlike triage and search, the summary asks for plain prose, not a tool call — so `AiSummary.BuildPrompt` just requests a few plain-language sentences and the provider classes return the model's text. The access discipline, though, is identical to everything else: the AI sees exactly what the human calling it sees, and not one note more.


### Reached the same way from both hosts

Because everything hangs off `IAiAssistant`, both front doors reach the three helpers through the same seam. The web app's `DbOpenTrackDataService` implements `IsAiEnabledAsync`, `SuggestTriageAsync`, `InterpretIssueSearchAsync`, and `SummarizeIssueAsync` by calling the injected `ai`. The desktop app reaches identical logic through the API's `/api/ai` endpoints, which are grouped behind `RequireAuthorization()` — the AI helpers are never anonymous — and repeat the same access checks before calling the assistant. Two hosts, one interface, one set of rules.


## Why It Matters / Design Takeaways

The AI layer is a model of how to add a powerful-but-risky capability without letting it corrode the rest of the system. It is fenced by three consistent rules: *opt-in* (off by default, configured only by the operator, never touching the database or browser for its secrets), *server-side* (calls and keys stay on OpenTrack's machine, and a local engine can keep the data there too), and *best-effort* (every method returns null on any failure, so the AI's absence or malfunction is always survivable).

Two structural choices make it maintainable. The provider seam means adding or swapping a model is a configuration change, not a code change — and the provider-agnostic prompt/schema/parse helpers mean the two providers can never quietly disagree. And the safety choices reuse the authorities that already exist: search can only build a filter the user could build by hand, and the summarizer is fed only notes the caller may already read. The AI got no special powers and no private access; it was bolted onto the existing rails, not around them.

> **The maintainer's rule** — If you add a fourth AI helper, keep the four invariants: put its prompt, schema, and parsing in a shared provider-agnostic class so both providers stay in lock-step; cap the user text you send; return null on any failure so callers degrade; and feed the model only data the calling user is already entitled to — reuse AccessContext and WhereVisibleTo, never work around them. The AI is an intern that whispers suggestions; never let it become one that files them or reads what it shouldn't.


# 21. The Blazor UI Layer: components, PWA & offline

*The one set of screens both the browser and the desktop app show — a shared Razor class library that talks only to the data-service seam, never to a database — plus the four browser-side pieces written in plain JavaScript that make it feel like an app you installed: a service worker for offline, an installable Progressive Web App manifest, a Ctrl/Cmd+K command palette, and an offline-capable bug-hunt checklist that queues your taps and replays them when you're back online.*


## What This Is / What It Is For

OpenTrack has two ways to run: a website you open in a browser, and a desktop app for Windows and Mac. It would be a quiet disaster to build the screens twice — the issue list, the create form, the dashboard — because the two copies would drift apart, and every feature would cost double. So the screens are built *once*, in a shared library called `OpenTrack.UI`, and both hosts display the very same components. This chapter is about how that shared UI is organized and about the handful of small browser-side scripts that make the web version feel less like a web page and more like an installed app.

Picture the UI library as a set of stage scenery that two different theaters can wheel in. The scenery — the backdrops, the props, the layout — is identical; each theater just supplies its own stagehands (its own way of fetching the data behind the scenery). Then, layered on top of the web theater, are four little machines that don't exist on paper at all: one that lets you keep watching the show when the power flickers (offline caching), one that lets you install the theater on your phone's home screen, one that's a keyboard shortcut to jump anywhere, and one that lets you keep ticking off a checklist even with no signal and syncs it up later.

> **The one-sentence version** — The screens live once in the shared OpenTrack.UI library and talk only to the IOpenTrackDataService seam, so the web and desktop hosts render identical UI over different plumbing; and four vanilla-JavaScript files bolted onto the web host — a service worker, a PWA manifest, a Ctrl/Cmd+K command palette, and an offline checklist queue — turn that shared UI into something installable and usable offline.

> **Jargon, in plain words** — Blazor is Microsoft's framework for building web UI in C# instead of JavaScript; a '.razor' component is one reusable piece of screen (a page or a widget). A Razor class library is a bundle of those components that other projects can share. A PWA (Progressive Web App) is an ordinary website that a browser can 'install' so it opens in its own window and works offline, like a native app. A service worker is a small script the browser keeps running in the background to intercept network requests — the piece that makes offline possible. 'Vanilla JavaScript' just means plain JavaScript with no framework or library involved.


## One shared library of screens


### What it does

`OpenTrack.UI` holds essentially every screen in the product. Under `Pages` you'll find the dashboard, the issue screens (`Index`, `Details`, `Create`, `Edit`, `Print`, `QuickCapture`), the project screens (settings, members, board, roadmap, checklist, automation, Git integration, the intake poster, and more), plus reports, SLA, notifications, preferences, backup, and import. Under `Shared` sit reusable widgets like `BarChart`. This one library is what both hosts point at.


### Why it was built this way

The library is kept deliberately thin and unaware of infrastructure. Its project file is a Razor class library that references only `OpenTrack.Core` — not the database project, not Entity Framework, not the web host — and pulls in just the Blazor component packages plus `QRCoder` for the intake poster's QR code:

```csharp
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <!-- ... -->
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="10.0.10" />
    <PackageReference Include="QRCoder" Version="1.6.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\OpenTrack.Core\OpenTrack.Core.csproj" />
  </ItemGroup>
</Project>
```

The reason the same components can run on two very different hosts is the seam from Chapter 11. A page never reaches for a database; it asks for an injected `IOpenTrackDataService` and calls methods on it. On the web host that interface is implemented by `DbOpenTrackDataService` (which talks straight to the database); on the desktop it's implemented by `HttpOpenTrackDataService` (which calls the API over HTTP). The page cannot tell the difference, and doesn't try to. That single indirection is what lets the screens be written once.

Each host then just tells its Blazor router to include the shared library's components. In the web host's `App.razor`, that's one line adding the UI assembly to the render:

```csharp
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(OpenTrack.UI.AssemblyMarker).Assembly);
```


### The house style, in one small component

`BarChart` is worth a look because it captures the UI layer's whole aesthetic in forty lines. There is no charting library. The bar chart is drawn by hand as inline SVG built into a string, and it is careful about two things the whole UI cares about: theming and safety. Bars use the Bootstrap primary color via a CSS variable and text uses the current text color, so the same chart reads correctly in light mode, dark mode, and on either host; and every label is HTML-encoded, so a maliciously named category can't inject markup:

```csharp
sb.Append($"<rect x=\"{labelW}\" y=\"{y + 5}\" width=\"{w}\" height=\"15\" rx=\"3\" fill=\"var(--bs-primary, #0d6efd)\"></rect>");
// ...
private static string Enc(string s) => WebUtility.HtmlEncode(s);
```

That preference — draw it ourselves with SVG and CSS variables rather than add a dependency — recurs across the UI (the far-field-style plots, the QR poster, this chart) and keeps the shared library light enough to run comfortably in a desktop WebView as well as a browser.

> **Jargon, in plain words** — SVG (Scalable Vector Graphics) describes a picture as shapes — rectangles, lines, text — rather than a grid of pixels, so it stays sharp at any size and can be built as plain text. A CSS variable like var(--bs-primary) is a named color the page defines once; using it means the chart automatically follows the current theme instead of hard-coding a color. 'HTML-encoding' converts characters like < and > into harmless equivalents so text can never be mistaken for markup.


## The browser-side helpers: why plain JavaScript

Four capabilities can't be expressed as Blazor C# components because they live below the app, in the browser itself: intercepting network requests, being installable, a global keyboard shortcut, and surviving a lost connection. These are written as small vanilla-JavaScript files in the web host's `wwwroot` and wired up in `App.razor`. They are plain JS on purpose — with no framework — so the same file behaves identically on the static-rendered web host and inside the desktop WebView. The web host's page registers them directly:

```csharp
<script>
    if ("serviceWorker" in navigator) {
        window.addEventListener("load", function () { navigator.serviceWorker.register("/sw.js").catch(function () { }); });
    }
</script>
<!-- ... -->
<script src="command-palette.js"></script>
<script src="auto-refresh.js"></script>
<script src="checklist-offline.js"></script>
```


### The service worker: what 'offline' really means

`sw.js` is the service worker — the background script that sits between the app and the network and can answer requests from a cache when the network is gone. Its strategy is stated plainly in its own header: static assets are cached when it installs, and page navigations are 'network-first with a cache fallback,' so "pages you've opened while online remain viewable offline (e.g. a bug-hunt checklist next to you on a tablet)."

The rules it enforces are deliberately conservative. Only GET requests are ever cached — any change (a POST) always goes to the network, never served stale. It ignores anything cross-origin, and it pointedly stays out of Blazor's way by not touching Blazor's own framework paths:

```csharp
self.addEventListener("fetch", (e) => {
    const req = e.request;
    if (req.method !== "GET") return; // never cache mutations
    const url = new URL(req.url);
    if (url.origin !== location.origin) return; // don't touch cross-origin
    if (url.pathname.startsWith("/_framework") || url.pathname.startsWith("/_blazor")) return; // let Blazor manage its own
```

When the network is reachable it fetches fresh, caches successful navigations and known assets, and returns them; when a fetch throws (you're offline), it falls back to whatever is cached, and for a navigation with nothing cached it serves the app's home page as a last resort. The cache is named with a version (`ot-cache-v1`), and the header notes this suits a personal or trusted device — a caution the maintainer left in the code itself, since a cache persists across sign-out.


### The manifest: making it installable

A service worker makes a site work offline; a *web manifest* makes it installable. `manifest.webmanifest` gives the browser the metadata it needs to offer 'Install OpenTrack' and then launch it in its own standalone window like a native app — a name, colors, a start URL, and an icon:

```csharp
{
  "name": "OpenTrack — Bug & Issue Tracker",
  "short_name": "OpenTrack",
  "start_url": "/",
  "scope": "/",
  "display": "standalone",
  "theme_color": "#0d6efd",
  "icons": [
    { "src": "/icon.svg", "sizes": "any", "type": "image/svg+xml", "purpose": "any" },
    { "src": "/icon.svg", "sizes": "any", "type": "image/svg+xml", "purpose": "maskable" }
  ]
}
```

`"display": "standalone"` is what drops the browser chrome so the installed app opens in its own frame; the SVG icon (with both a normal and a 'maskable' purpose) keeps the home-screen icon crisp on any device. The manifest is linked from the app's head alongside the theme color and Apple-specific tags, so the same app installs cleanly on desktop and on iOS.


### The command palette: Ctrl/Cmd+K to go anywhere

`command-palette.js` adds the now-familiar Ctrl/Cmd+K overlay: a search box that jumps you to a screen, an issue, or a search. Its header states the two design constraints that keep it simple and portable — it is "pure vanilla JS so it works the same on the static-SSR web host and the desktop WebView," and "it only ever navigates (changes the URL); no server interactivity." It never edits data; the worst it can do is take you somewhere.

Beyond a fixed list of destinations (dashboard, all issues, quick-add, projects, notifications, backup), it has one clever input rule: type a number and it offers to jump straight to that issue; type words and it offers a full-text issue search — so `#812` and `login crash` both do the natural thing:

```csharp
var m = q.match(/^#?(\d+)$/);
if (m) out.push({ label: "Go to issue #" + m[1], href: "issues/" + m[1] });
// ...
if (q && !m) out.push({ label: 'Search issues for "' + q + '"', href: "issues?Text=" + encodeURIComponent(q) });
```

Arrow keys move the selection, Enter navigates, Escape closes — all handled by one keydown listener. Because it only manipulates the URL, it needs nothing from the server and can't get out of sync with application state.


### The offline checklist: the cleverest of the four

`checklist-offline.js` is the most ambitious browser-side piece, and it exists for a very concrete scenario: someone running a bug-hunt checklist on a tablet, physically walking around, where the Wi-Fi comes and goes. Online, it does nothing special — the normal form posts as usual. Offline, it intercepts each Pass/Fail/N-A tap, records the change locally, updates the card immediately so the user sees progress, and replays the queued changes when the connection returns.

The queue lives in `localStorage` (which survives a page reload and a dead connection), and each item overwrites any earlier pending change for the same checklist item, so 'last write wins':

```csharp
function enqueue(pid, item, status) {
    var items = readQueue().filter(function (x) { return !(x.p === pid && x.i === item); }); // last write wins per item
    items.push({ p: pid, i: item, s: status });
    writeQueue(items);
}
```

The interception is careful to only act when it must. If you're online it steps aside entirely and lets the normal form submit; only offline does it prevent the default, queue the change, and update the card optimistically:

```csharp
if (!STATUS[verb] || isNaN(item)) return;
if (navigator.onLine) return; // online → let the normal form submit proceed
e.preventDefault();
enqueue(pid, item, STATUS[verb]);
optimistic(item, STATUS[verb]);
```

When the device comes back — on the next page load, or the moment the browser fires its `online` event — the queue is flushed to the same-origin checklist endpoint, carrying the anti-forgery token, and any items that fail to sync are kept for the next attempt. If anything did sync, the page reloads to show the true, server-confirmed state rather than the optimistic guess. One honest edge case is handled out loud: creating an issue from a failed check genuinely needs a connection, so that action, when offline, is blocked with a plain-language explanation instead of being silently queued. And a deliberate note in the header explains a portability choice — it avoids the Background Sync API precisely so it works on an iPad, where Safari lacks it.

> **Optimistic UI, honestly labeled** — Updating the card the instant you tap — before the server has heard about it — is called optimistic UI: assume it'll succeed and show that, then reconcile later. The risk is lying to the user if the sync fails. This code manages that honestly: it marks the change 'pending sync' while it's only local, keeps unsynced items in the queue, and reloads to the real state once the server confirms — so the optimistic display is always eventually replaced by the truth.


## Why It Matters / Design Takeaways

The UI layer earns its keep through one structural rule and one stylistic one. The structural rule is that the screens live once and talk only to the `IOpenTrackDataService` seam — so the web and desktop apps can never present different UI, and a new screen automatically works on both hosts the day it's written. The stylistic rule is dependency restraint: charts, QR codes, and the four browser helpers are hand-built with SVG, CSS variables, and vanilla JavaScript rather than pulled-in libraries, which keeps the shared library light enough to run in a browser and a desktop WebView alike.

The browser-side quartet shows a consistent instinct too: each does the smallest possible thing and stays out of the framework's way. The service worker caches only GETs and leaves Blazor's own paths alone; the command palette only navigates; the offline queue only intervenes when actually offline and always reconciles to the server's truth. None of them owns application state — they enhance the shared UI without competing with it.

> **The maintainer's rule** — When you add a screen, put it in OpenTrack.UI and reach for data only through IOpenTrackDataService — never touch the database or an HttpClient from a component, or you'll break one of the two hosts. When you add browser behavior, prefer a small vanilla-JS file that enhances the page over anything that owns state: keep the service worker caching only safe GETs, keep the palette navigation-only, and if you queue anything offline, always reconcile to the server's confirmed state rather than trusting the optimistic guess.


# 22. Testing: the xUnit Suite and How to Add to It

*The automated test suite that proves the rules still hold — what it covers and why those particular things, the arrange-act-assert shape every test follows, and how to add your own, whether the thing you're testing is a pure calculation or needs a throwaway database.*


## What This Is / What It Is For

A *test suite* is a second, smaller program whose only job is to run the real program in tiny pieces and check that each piece still does what it is supposed to. You run it, and in a few seconds it either says "all green" or points at the exact line where something broke. *OpenTrack* ships with 184 such checks, spread across three test projects, and this chapter is about what they guard, how they are written, and how to add one when you change or add a feature.

Think of it as a set of tripwires strung across the parts of the code most likely to hurt someone if they silently changed. Who can see a private issue? Does the service-level-agreement clock turn red at the right moment? Does an imported file map its fields correctly? Each tripwire is a small, fast, repeatable check. When a future edit trips one, you learn immediately, at your desk, instead of a user learning later, in production.

> **The one-sentence version** — The suite concentrates on the code where a quiet mistake would be expensive — the access-control matrix, the row-level visibility filter, the SLA math, the workflow gate, the importers, and the git webhook — and it splits into pure tests (hand it facts, check the answer) and database tests (spin up a throwaway SQLite database, act, check the rows), all written in xUnit's simple arrange-act-assert shape.

> **Jargon, in plain words** — xUnit is the testing library (the framework) that finds and runs the tests and reports pass/fail. A test is just a normal method that xUnit runs. An assertion is the line that states what must be true — if it isn't, the test fails. A regression is a bug that sneaks back into code that used to work; a regression test is a tripwire placed exactly where a past bug lived so it can never return unnoticed.


### Why test these things, and not everything

You could try to test every line of a program, but that is a poor use of effort — much of an app is plumbing that either obviously works or obviously doesn't the moment you run it. The suite instead aims at the places where a mistake is both *easy to make* and *hard to notice*. A permission rule that accidentally flips from "no" to "yes" doesn't crash; it just leaks. An SLA threshold that's off by a hair doesn't error; it just quietly mis-colors every ticket. That silent-but-serious category is exactly where automated checks earn their keep.

That is why the heaviest testing sits on the security and correctness core: the access-control matrix (`AccessControlTests`), the shared row-level visibility filter (`VisibilityQueryTests`), the pure SLA calculator (`SlaCalculatorTests`), the per-project workflow gate (`WorkflowTests`), the file importers (`MantisImportTests`, `CsvIssueImportTests`, `GitHubIssueImportTests`), and the git webhook's signature and reference parsing (`GitSignatureTests`, `GitRefParserTests`). These are the subsystems where the rest of this book has said, again and again, "this must be right." The tests are how "must be right" is kept honest over time.


### The three test projects and the line between them

The suite is split into three projects, and the split is not arbitrary — it mirrors the layering of the app itself. Each test project references exactly the part of the app it is allowed to test, so a test can only reach for the layers that layer is allowed to reach for.

| Test project | References | What it tests |
| --- | --- | --- |
| OpenTrack.Core.Tests | OpenTrack.Core | Pure logic with no database: the ACL matrix, SLA math, importers, git-ref parsing, markdown safety, custom-field validation |
| OpenTrack.API.Tests | OpenTrack.API (and through it, Infrastructure) | Behavior that needs a real database: the visibility filter, workflow, tags, bulk, notifications, the git signature check |
| OpenTrack.Web.Tests | OpenTrack.Web | The web host's model wiring: the EF runtime model and the design-time factory |

The reason the pure tests live in `OpenTrack.Core.Tests` and the database tests in `OpenTrack.API.Tests` is the same reason the whole codebase keeps pure rules in Core (Chapter 6): a test with no database is trivially fast and needs nothing running. Notice what the `OpenTrack.Core.Tests` project file pulls in — just the test framework, no database at all:

```csharp
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.4" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
  <PackageReference Include="xunit" Version="2.9.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\..\src\OpenTrack.Core\OpenTrack.Core.csproj" />
</ItemGroup>
```

The `OpenTrack.API.Tests` project file adds one thing the Core tests never need — the SQLite database driver — because its tests actually create databases:

```csharp
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />
```


### The two kinds of test

Almost every test in the suite is one of two shapes, and learning to tell them apart is most of what you need to write your own.

- *Pure tests* — you hand a function some plain facts and check the answer it returns. No database, no network, nothing to set up or clean up. These are the fastest and the ones to reach for whenever the logic you're testing doesn't actually need stored data.
- *Database-backed tests* — you build a tiny, throwaway database in memory, put a few rows in it, run a real operation against it, and then check what changed. Slower, but the only honest way to test code whose whole job is to query or modify stored rows.

The design goal that keeps the pure category large is the same one from Chapter 8: the rules that matter most were written *pure* on purpose, precisely so they could be tested without a server. The more logic lives in pure form, the more of the suite gets to be the fast, simple kind.


### A pure test, read line by line

Here is a complete pure test from `SlaCalculatorTests`. The SLA calculator decides whether a ticket is on track, at risk, or breached, purely from a target time and how much time has elapsed — no database in sight. Watch the shape: a small helper sets up the inputs, then each test states one expectation.

```csharp
private static readonly DateTime Created = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

private static SlaAssessment Eval(int? targetHours, double hoursElapsed, bool isOpen = true) =>
    SlaCalculator.Evaluate(IssuePriority.High, Created, isOpen, Created.AddHours(hoursElapsed), targetHours);

[Fact]
public void OnTrack_BeforeAtRiskThreshold()
{
    // 10h target -> at-risk at 8h. At 5h we're comfortably on track.
    var a = Eval(10, 5);
    Assert.Equal(SlaStatus.OnTrack, a.Status);
    Assert.Equal(Created.AddHours(10), a.DueUtc);
}
```

Read it as three beats — the pattern every test in the suite follows, whether it says so out loud or not:

1. *Arrange* — set up the inputs. Here that's `Eval(10, 5)`: a ten-hour target, five hours elapsed. The `Eval` helper exists only to spare each test from restating the fixed bits (the creation time, the priority).
2. *Act* — run the thing under test. `SlaCalculator.Evaluate(...)` runs inside `Eval` and hands back an assessment.
3. *Assert* — state what must be true. `Assert.Equal(SlaStatus.OnTrack, a.Status)` says the status must be OnTrack; if the calculator returned anything else, the line fails and names the mismatch.

> **Jargon, in plain words** — [Fact] marks a method as a test that takes no inputs — a single, self-contained case. Assert.Equal(expected, actual) checks that two values match; there are siblings like Assert.True, Assert.False, Assert.Null, Assert.Empty, Assert.Single (exactly one item), and Assert.Throws (the code must throw a specific error). 'Arrange, act, assert' is just the habit of writing every test in those three beats, in that order.

Notice what makes this test trustworthy: it depends on nothing but its own inputs. Run it on any machine, in any order, a thousand times, and it gives the same answer, in milliseconds. That reliability is the direct reward for keeping `SlaCalculator` pure. The same file also pins the boundaries that are easy to get wrong — that the clock stops when a ticket is closed, and that a missing target means "not tracked" rather than "instantly breached":

```csharp
[Fact]
public void ClosedIssue_IsNotTracked_EvenIfPastDeadline()
{
    // 24h target, 100h elapsed, but the issue is no longer open -> clock stopped.
    Assert.Equal(SlaStatus.NotTracked, Eval(24, 100, isOpen: false).Status);
}
```


### One test, many cases: [Theory]

Sometimes you want to check the same rule against a table of inputs. Writing ten near-identical `[Fact]` methods would be noise. xUnit's `[Theory]` solves this: you write the test once with parameters, then feed it rows with `[InlineData]`. The access-control tests lean on this to sweep a rule across every role in one place. Here is the private-issue-visibility idea expressed as a theory, from `AccessControlTests`:

```csharp
[Theory]
[InlineData(UserRole.Reporter, false)]
[InlineData(UserRole.Updater, true)]
[InlineData(UserRole.Developer, true)]
public void EditIssue_RequiresUpdater(UserRole effective, bool allowed)
    => Assert.Equal(allowed, Ctx(effective).CanEditIssue());
```

That is three tests in four lines. Each `[InlineData]` row becomes its own independently-reported case: a Reporter is refused, an Updater and a Developer are allowed. If someone later loosened `CanEditIssue` so a Reporter could edit, the first row — and only the first row — would light up red, telling you precisely which case broke. This is the same file that pins the security-critical rules the whole app rests on, each named for exactly what it protects:

```csharp
[Fact]
public void PrivateIssue_IsHidden_FromOrdinaryProjectMembers()
{
    // A Reporter member of a public project cannot see a private issue they neither reported nor own.
    var ctx = Ctx(UserRole.Reporter, UserRole.Reporter);
    Assert.False(ctx.CanViewIssue(projectIsPublic: true, issueIsPrivate: true, reporterId: Someone, assigneeId: Someone));
}
```

> **Read the summary comment first** — Every test class opens with a /// <summary> that states, in one or two sentences, exactly what it guards — for example AccessControlTests notes it holds 'regression tests for audit findings C1, C2, and H3.' When you touch a subsystem, read that summary before its tests: it tells you what promises you must not break, and which past bugs must never come back.


### A database-backed test, read line by line

Some behavior cannot be tested with plain inputs, because the behavior *is* a database query. The row-level visibility filter from Chapter 8 is the prime example: its entire job is to translate an access snapshot into a SQL `WHERE` clause and return only the rows a user may see. To test that honestly you need real rows in a real relational database. `VisibilityQueryTests` builds one — in memory, from scratch, for the duration of the test class.

The setup runs once when the test class is constructed. It opens an in-memory SQLite connection, tells Entity Framework to create the tables, and seeds a deliberately tricky little world: a public project and a private one, four users, and four issues covering every combination of public/private issue in public/private project.

```csharp
public VisibilityQueryTests()
{
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();
    _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

    using var db = new AppDbContext(_options);
    db.Database.EnsureCreated();

    db.Users.AddRange(/* owner1, owner2, a reporter member, an outsider */);
    db.Projects.AddRange(
        new Project { Id = PublicProject, Name = "Public", IsPublic = true, OwnerId = U_Owner1 },
        new Project { Id = PrivateProject, Name = "Private", IsPublic = false, OwnerId = U_Owner2 });
    // ...memberships and four issues (public/private x public/private)...
    db.SaveChanges();
}
```

> **Jargon, in plain words** — 'In-memory SQLite' is a genuine SQLite database that lives entirely in RAM instead of a file — 'DataSource=:memory:' is the address that means 'nowhere on disk.' It behaves like the real database (real tables, real SQL, real foreign keys) but vanishes when the connection closes. EnsureCreated() builds the tables from the entity model. The class constructor runs fresh for every test method, so each test gets its own clean, seeded database — no test can pollute another.

With the world seeded, a small helper runs the actual thing under test — the shared `WhereVisibleTo` filter — as a given user, and returns the ids that survived the filter:

```csharp
private async Task<(int[] projects, int[] issues)> VisibleTo(int userId, UserRole globalRole)
{
    await using var db = new AppDbContext(_options);
    var access = await AccessSnapshot.LoadAsync(db, new AccessIdentity(userId, globalRole));
    var projects = await db.Projects.WhereVisibleTo(access).Select(p => p.Id).OrderBy(i => i).ToArrayAsync();
    var issues = await db.Issues.WhereVisibleTo(access).Select(i => i.Id).OrderBy(i => i).ToArrayAsync();
    return (projects, issues);
}
```

And each test is one crisp claim about who sees what. This one pins the whole point of the private-issue rule — an outsider gets the public project and only its public issue, and nothing more:

```csharp
[Fact]
public async Task Outsider_SeesOnlyPublicProjectAndItsPublicIssues()
{
    var (projects, issues) = await VisibleTo(U_Outsider, UserRole.Reporter);
    Assert.Equal(new[] { PublicProject }, projects);
    // NOT the private issue in the public project, and nothing in the private project.
    Assert.Equal(new[] { I_PublicInPublic }, issues);
}
```

The arrange-act-assert shape is still here, just larger: *arrange* was the seeded database in the constructor, *act* is `VisibleTo(...)` running the real filter, *assert* is the two `Assert.Equal` checks on the surviving ids. Because the filter runs against a real SQLite database, the test proves the rule survives translation into actual SQL — not just that a C# method returns the right boolean. That is the whole reason this test earns the extra cost of a database.


### Cleaning up: IDisposable

A test that opens a database connection must close it, or connections would pile up across a run. xUnit's convention is that if a test class implements `IDisposable`, its `Dispose` method runs after each test. `VisibilityQueryTests` uses exactly that to drop the in-memory database:

```csharp
public sealed class VisibilityQueryTests : IDisposable
{
    // ...constructor seeds, tests run...
    public void Dispose() => _connection.Dispose();
}
```

When the connection closes, the whole in-memory database evaporates — no files to delete, no state carried to the next test. Combined with the constructor running fresh each time, this gives every database test a clean room to work in. The `WorkflowTests` class follows the same pattern (seed a project and an issue in the constructor, `Dispose` the connection at the end), which is worth reading next if you want a second worked example that both seeds data and runs a real operation against it.


### How to add a test

When you change or add a feature, adding a matching test is a short, mechanical process. The only real decision is which of the two shapes you need — and that follows from a single question: does the thing you're testing touch stored data?

1. Decide the shape. If your logic takes plain inputs and returns an answer (a calculation, a parser, a permission rule), write a *pure* test in `OpenTrack.Core.Tests`. If it queries or changes rows, write a *database* test in `OpenTrack.API.Tests`.
2. For a pure test: add a class named `<Thing>Tests`, open it with a one-line `/// <summary>` of what it guards, then write `[Fact]` methods (or a `[Theory]` with `[InlineData]` rows when the same rule spans several inputs). Arrange the inputs, call the function, assert the result.
3. For a database test: implement `IDisposable`, open an in-memory SQLite connection in the constructor, `EnsureCreated()`, seed just the rows your case needs, run the real operation, assert on what changed, and `Dispose` the connection.
4. Name each test for the behavior it protects, not the method it calls — `PrivateIssue_IsHidden_FromOrdinaryProjectMembers` tells a future reader what breaks if it goes red, where `TestCanViewIssue3` tells them nothing.
5. Run the suite (`dotnet test`) and watch it go green. If you're fixing a bug, write the failing test first, watch it go red, then fix the code and watch it turn green — that proves the test actually catches the bug.

> **A test that never fails is worse than no test** — A test only protects you if it would actually fail when the behavior breaks. Before trusting a new test, make it fail on purpose once — break the code, or invert the assertion — and confirm it goes red. A test that stays green no matter what is a false comfort; it looks like coverage but guards nothing.


## Why It Matters / Design Takeaways

The suite is not trying to test everything. It is trying to test the things where a silent mistake would be expensive — permissions, visibility, SLA timing, workflow gates, imports, and the git webhook — and to do so in a form that stays fast and repeatable. Purity is what makes that possible: because the highest-stakes rules were written with no database, most of the highest-stakes tests are the fast, simple kind, and the database tests are reserved for behavior that genuinely lives in SQL.

The payoff compounds over the life of the project. Every regression test is a lesson from a past bug, frozen so the bug cannot come back unnoticed. The access-control tests literally carry audit finding codes in their summaries, so the fixes for those findings can never quietly regress. Years from now, a maintainer who has never met the original author can change code with confidence, because the tripwires will tell them the instant they break a promise the app has always kept.

> **The maintainer's rule** — Every change to guarded behavior gets a test in the same commit — a new rule gets a test that proves it, a bug fix gets a test that fails before the fix and passes after. Keep pure logic in Core so its tests can stay database-free, reach for in-memory SQLite only when the behavior truly is a query, and name each test for the promise it protects. The suite is only as trustworthy as the discipline that keeps it complete.


# 23. How This Codebase Is Meant to Grow

*The single most important chapter for anyone extending OpenTrack: how a new feature travels from a database table all the way to the screen, following the grain the codebase already runs in — and the handful of load-bearing rules that must never erode, no matter how the app grows.*


## What This Is / What It Is For

Every long-lived program eventually gets a feature its author never imagined. The question that decides whether the program ages gracefully or rots is not *what* that feature is, but *how* it gets added. If each new feature is bolted on wherever was convenient, the code slowly turns into a thicket where nothing can be changed safely. If instead every feature follows the same well-worn path, the code stays legible for decades. This chapter describes that path for *OpenTrack*, so the next person to extend it can add with the grain instead of against it.

The earlier chapters each described one layer. This chapter is the synthesis: it walks a single imaginary feature through all of them in order, and then names the rules that hold the whole shape together. Read it as a map you keep beside you the first few times you add something — not because the steps are hard, but because doing them in the right order, in the right place, is the entire difference between growth and decay.

> **The one-sentence version** — A new feature travels a fixed path — entity, then migration, then an Operations class that holds the logic once, then the shared data-service seam, then both hosts, then the UI, then a test — and four rules keep that path honest: pure rules live in Core, each action has exactly one Operations home, every access decision routes through the one authority, and Web and API are always fed by shared code, never divergent copies.


### The grain of the codebase

Before the steps, the shape. OpenTrack is built as a stack of layers, each allowed to depend only on the ones beneath it (Chapter 3). Wood has a grain; so does this code. When you plane wood along the grain, it goes smoothly; across it, it splinters. The layers are the grain here, and a feature added *along* them slots in cleanly:

- *OpenTrack.Core* — the entities (plain data shapes like `Issue`, `Project`, `Tag`) and the pure rules (permissions, SLA math). No database, no web. This is the bedrock everything sits on.
- *OpenTrack.Infrastructure* — the database (`AppDbContext`), the migrations, and the *Operations* classes that actually read and write rows while checking permissions.
- *OpenTrack.UI* — the shared screens, and the `IOpenTrackDataService` interface: the single seam both hosts plug into.
- *OpenTrack.Web* and *OpenTrack.API* — the two hosts (the browser app and the desktop-facing service), each a thin adapter that wires the shared code to its own front door.
- *OpenTrack.Desktop* — the desktop app, which reaches the API over HTTP through the same seam.

Every feature already in the app — tags, workflow, SLA policies, automation rules, relationships, webhooks, git integration — was built by pouring it down this stack in the same order. The proof is in the folder listing: `OpenTrack.Infrastructure` holds `TagOperations`, `WorkflowOperations`, `SlaPolicyOperations`, `AutomationRuleOperations`, `RelationshipOperations`, `WebhookOperations`, `GitIntegrationOperations`, and more — one Operations class per subsystem, each following the identical shape. Your new feature is simply the next one in that list.


### Adding a feature, end to end

Here is the whole journey as an ordered checklist. To make it concrete, imagine a small new feature — say, letting a project define a list of *milestones* an issue can be tagged to. The steps are the same for anything larger.

1. *Entity first.* Add a plain data class in `OpenTrack.Core/Entities/` (a `Milestone` with an id, a project id, a name). This is just the shape of the data — no behavior. Wire it into `AppDbContext` as a `DbSet` so Entity Framework knows about the table.
2. *Migration next.* Generate a migration so the database gains the new table. The schema change is recorded as code in `OpenTrack.Infrastructure/Migrations`, so every existing database can be upgraded in place and every new one is built correctly. Never hand-edit the live database; let the migration be the single record of the change.
3. *Pure rules, if any, in Core.* If the feature needs a decision that is really just logic — who may edit a milestone, or how a due date is computed — write it as a pure method in Core (an `AccessContext` permission, or a small calculator), so it can be unit-tested with no database, exactly like the SLA calculator.
4. *Operations class — the logic's one home.* Add `MilestoneOperations` in `OpenTrack.Infrastructure/Milestones/`. This is where the reading and writing of rows lives, and where every action first asks the access authority whether the caller is allowed. This class is the feature's brain; the hosts will only call into it.
5. *Data-service seam.* Add the feature's methods to the `IOpenTrackDataService` interface in `OpenTrack.UI/Services/`. This is the single doorway both hosts share. Implement it once against the database (`DbOpenTrackDataService`, calling your `MilestoneOperations`) and once over HTTP (`HttpOpenTrackDataService`, calling the API endpoint).
6. *Both hosts.* Expose the feature at each front door: an endpoint in `OpenTrack.API/Endpoints/` for the desktop app, and the Web host's wiring. Each host only gathers the caller's identity and delegates to the shared Operations code — it writes no rules of its own.
7. *UI.* Build the screen in `OpenTrack.UI` once. Because it talks only to `IOpenTrackDataService`, the exact same screen runs in the browser and in the desktop app, each backed by its own implementation of the seam.
8. *Test.* Add a test in the same commit — a pure test if the logic is pure, an in-memory-SQLite test if it touches rows — proving the new rule holds and the access checks bite.

> **Jargon, in plain words** — An entity is a plain C# class that mirrors one database table — a row's worth of fields. A migration is a recorded, replayable description of a schema change (add this table, add that column), stored as code so any database can be brought up to date. A seam is a deliberate joint in the code — here, an interface — where two implementations can be swapped without the callers noticing. An endpoint is one addressable operation on a host (a URL the desktop app can call).


### Why entity first, then migration

The order is not a style preference; it is causality. The entity defines the shape of the data, and the migration is the record of teaching the database that shape. If you wrote logic before the data existed, you'd be building on air; if you changed the live database by hand before recording a migration, the next person's fresh database wouldn't match yours and the app would break for them in ways that never reproduce on your machine.

Doing it in order — entity, then migration — means the schema change is captured *as code*, reviewable in the same pull request as the feature, and replayable on every deployment. The database is never a mystery that drifted out of sync with the source; it is always exactly what the migrations say it is. This is the discipline that lets a second developer clone the repo, run the app, and get a database identical to yours without a single manual step.


### The Operations class: one place per action

The single most important habit in this codebase is that *each action has exactly one home*, and that home is an Operations class in `OpenTrack.Infrastructure`. The logic for adding a tag lives in `TagOperations` and nowhere else; the logic for a workflow transition lives in `WorkflowOperations` and nowhere else. Read the summary comment on `TagOperations` and the intent is stated outright — the operations are "shared by both the Web API and the web/EF data service so the authorization logic exists once."

Look at the shape of an Operations method and you see the pattern every one of them follows: load what you need, check access through the authority, then act. From `TagOperations.AddAsync`:

```csharp
public static async Task<string?> AddAsync(AppDbContext db, AccessSnapshot access, int issueId, string tagName, CancellationToken ct = default)
{
    var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
    if (issue is null || !CanView(access, issue)) return "Issue not found.";
    if (!access.For(issue.ProjectId).CanEditIssue()) return "...";
    // ...only now does it create/reuse the tag and save...
}
```

Every Operations method takes the `AccessSnapshot` as an argument and asks it — through `access.For(projectId)` — whether the caller may do the thing, before doing it. This is the reason the codebase can promise that Web and API never diverge: the rule and the action live together, once, and both hosts call that one place. If instead each host reimplemented "add a tag," the day would come when one enforced edit-access and the other forgot. Keeping the logic in a single Operations class makes that class of bug impossible by construction, the same way Chapter 8 makes permission-drift impossible.

> **The smell that means you're doing it wrong** — If you find yourself writing the same query or the same 'is this allowed?' check in both a Web endpoint and an API endpoint, stop — you are about to grow a second, divergent copy of a rule. The correct move is to put that logic in one Operations class and have both hosts call it. Two copies of a rule is not two features; it is one future bug waiting for someone to update only one of them.


### The data-service seam: one feature, both hosts

The bridge that lets a single feature light up in both the browser and the desktop app is the `IOpenTrackDataService` interface in `OpenTrack.UI/Services/`. It is a list of everything the shared UI can ask for, with no hint of *how* the answer is fetched. There are exactly two implementations of it, and they are the two hosts' whole personalities:

- `DbOpenTrackDataService` (in `OpenTrack.Web`) answers by going straight to the database — it builds an `AccessSnapshot` and calls the Operations classes directly.
- `HttpOpenTrackDataService` (in `OpenTrack.Desktop`) answers by calling the API over HTTP — the API host, on the other end, builds its own `AccessSnapshot` and calls the very same Operations classes.

Because the shared screens depend only on the interface, they neither know nor care which implementation is behind it. Add your feature's methods to `IOpenTrackDataService`, implement them in both places, and the identical UI works in both apps. This is the seam that turns "build it once" from an aspiration into a mechanical fact: the UI is written once, the rules live once in Operations, and only the thin fetch-the-data adapters differ between hosts.


### The four rules that must never erode

If the whole chapter compressed to four sentences, these are them. They are the load-bearing walls. You may add rooms, but you may not knock these out — every one of them, removed, reintroduces a class of bug the architecture was specifically built to prevent.

| The rule | What it prevents | Where it lives |
| --- | --- | --- |
| Pure rules stay in Core, database-free | Untestable logic; rules you can't verify without a running server | OpenTrack.Core — AccessContext, SlaCalculator |
| One Operations place per action | The same action drifting apart between hosts | OpenTrack.Infrastructure — *Operations.cs |
| Every access decision routes through the one authority | Scattered, inconsistent permission checks that leak | AccessContext / AccessSnapshot / VisibilityQueries |
| Web and API are fed by shared code, never divergent copies | Two front doors slowly enforcing different behavior | IOpenTrackDataService + shared Operations |

These four are not independent good ideas that happen to coexist; they reinforce each other. Pure rules in Core are what make the single authority testable. The single authority is what the one Operations place calls. The one Operations place is what both hosts share through the seam. Pull any one thread and the others start to fray — remove the seam and the Operations classes sprout host-specific copies; scatter the access checks and the pure authority stops being the single source of truth. Preserve them together, or not at all.

> **Where each rule is proved elsewhere in this book** — These are not new claims — they are the recurring spine of the guide. The pure-rules principle is Chapter 6; the access authority is Chapter 8; the Operations pattern is Chapter 9; the data-service seam is Chapter 11; and the tests that keep them all honest are Chapter 22. This chapter only gathers them into the single workflow you follow when you add something new.


### How to tell you're fighting the design

You rarely break the architecture on purpose. It erodes through small, reasonable-seeming shortcuts under deadline pressure. The defense is to recognize the warning signs early — each one is the codebase telling you that you have wandered across the grain:

- You're writing an `if (user.Role >= ...)` check inside a page or an endpoint. *Stop* — that rule belongs in `AccessContext`, and the endpoint should call it.
- You're copying a query from the Web host into the API host (or vice versa). *Stop* — that logic belongs in one Operations class both hosts call.
- You're reaching for the database from inside a Core class. *Stop* — Core is pure by design; the data-fetching belongs one layer up, in Infrastructure.
- You changed a table by hand and the app works on your machine but not a colleague's. *Stop* — you skipped a migration; the schema change must be recorded as code.
- You built a screen that only works in the browser, or only in the desktop app. *Stop* — a screen that talks to `IOpenTrackDataService` works in both; if yours doesn't, it reached around the seam.

None of these is a catastrophe caught early. Each is simply a nudge back onto the path. The architecture is forgiving as long as you notice the nudge and follow it; it becomes unforgiving only when a hundred un-noticed nudges have accumulated into a codebase where the rules live everywhere and nowhere.


## Why It Matters / Design Takeaways

The goal of this whole design is not elegance for its own sake. It is that the tenth feature should be as easy to add as the first, and the maintainer who arrives after the author is gone should be able to extend the app confidently by pattern-matching against what already exists. Every subsystem in the app was built by walking the same steps — entity, migration, Operations, seam, both hosts, UI, test — which means every subsystem is also a worked example you can copy. `TagOperations` shows you how the next Operations class should look; `VisibilityQueryTests` shows you how to test one.

Growth, done this way, does not increase risk in proportion to size. Because the rules live in one place each, the app can double in features without doubling in danger: there is still one authority to reason about, still one Operations home per action, still one seam feeding both hosts. That is the quiet promise the architecture makes to the future — you can keep building, for years, without the ground shifting under you, as long as you build along the grain.

> **The maintainer's rule** — When you add a feature, find the closest existing feature and copy its shape end to end — entity, migration, Operations class, seam method, both hosts, one UI, one test. Never introduce a rule that lives in two places, never let a host grow its own copy of logic, and never route an access decision around the one authority. If your change makes the app easier to reason about, you followed the grain; if it adds a place where the truth could disagree with itself, you crossed it.


# 24. How This Book Is Maintained + Amendments Register

*The rules that keep this guide trustworthy as the code moves underneath it: stable section numbers that never shuffle, dated amendments instead of renumbering, Markdown as the living source with the Word document generated from it, and a simple way to spot which chapters a code change may have made stale.*


## What This Is / What It Is For

A guide to a living codebase has a problem a novel does not: the thing it describes keeps changing. Code gets added, rules get refined, a subsystem gets rebuilt. If the book is not maintained with the same care as the code, it slowly drifts from truth until no one trusts it — and an untrusted guide is worse than none, because it misleads with an air of authority. This short chapter sets out the small set of rules that keep this guide honest over years, and closes with the register where every change to the book will be logged.

The approach is deliberately boring, because boring is what survives. There are no clever tools to learn. There is a numbering rule, an amendment rule, a source-of-truth rule, and a freshness habit. Follow those four and the book stays as reliable at version 5.0 as it was on the day it was written.

> **The one-sentence version** — Section numbers are permanent and never reshuffled; changes are recorded as dated amendments (AMENDS an existing section, or ADDS a new one) rather than by renumbering; the Markdown chapters are the single source of truth and the Word document is generated from them; and each chapter is anchored to the source files it describes, so a change to that code flags the chapter for review.


### Stable section numbers

The first rule is the simplest and the most important: *a section number, once assigned, never changes*. Chapter 8 is the Access-Control Authority today, and it will be Chapter 8 forever. If a new topic belongs conceptually between 8 and 9, it does not push everything down — it becomes a new, appended section with its own fresh number.

The reason is that numbers are how people, other documents, and commit messages point at parts of this book. Someone writes "see §8" in a code comment, a colleague bookmarks §16, the amendments register below refers to sections by number. The moment renumbering is allowed, every one of those references silently rots — "§8" now points at the wrong chapter, and no one can tell. Permanent numbers make every reference durable. It is the same instinct as the code's migrations: you record change as an addition to a stable history, never by rewriting what the past pointed at.

> **Jargon, in plain words** — A 'stable identifier' is a name or number that is promised never to change, so other things can safely point at it. Street addresses work this way — the city adds new house numbers rather than renumbering the whole street every time a house is built, because your friends memorized where you live. Section numbers here are the book's street addresses.


### Amendments, not rewrites

When the code changes in a way the book must reflect, you do not quietly edit a chapter and move on. You record the change as a dated *amendment*, using one of two verbs, and you log it in the register at the end of this chapter.

- *AMENDS §X* — you changed something within an existing section. The section keeps its number; the amendment notes what changed and when, so a reader can see the section has evolved and when it last did.
- *ADDS §Z* — you added an entirely new section for a new subsystem or topic. It gets the next available number and slots in without disturbing anything already numbered.

This mirrors, on purpose, how the codebase itself grows (Chapter 23) and how its database migrations work (Chapter 7): change is layered on as a dated, traceable addition, never as an invisible rewrite of history. The payoff is accountability. Anyone can open the register and see the book's whole history of change at a glance — what moved, when, and in which section — without diffing the entire document line by line. A reader who last studied §16 a year ago can check the register and know in seconds whether it has changed since.

> **Small fixes still get logged** — The register is not only for big revisions. A corrected code excerpt, a clarified callout, a fixed class name — all are amendments too. The point of the discipline is that no change to the book is invisible: if a reader could notice it, the register records it. When in doubt, log it; the cost is one row, and the benefit is a book no one has to take on faith.


### One source of truth: Markdown in, Word out

This guide exists in two forms — an in-repository set of chapter files, and a styled Word document (the navy-and-gold `.docx` that matches the User Manual and Installation Guide). It is essential to understand that these are not two documents to keep in sync by hand. There is *one* source, and the other form is generated from it.

The source of truth is the set of validated chapter files under `docs/programming-guide/chapters/` (this very chapter is one of them), which the build pipeline turns into both the Markdown `PROGRAMMING_GUIDE.md` and the Word document. You never edit the generated Word file directly — any change made there would be erased the next time the document is regenerated. You edit a chapter's source, and the document is rebuilt from it.

> **Never edit the generated document** — The Word .docx and the assembled Markdown are outputs, like a compiled program. Editing them directly is like editing a program's binary instead of its source code — the change looks real until the next build silently overwrites it. All edits go into the chapter source files; the documents are then regenerated. If you see a mistake in the Word file, fix it in the source and rebuild.

This single-source rule is why the whole guide can be trusted to render consistently: every chapter reuses the same shared block types — headings, paragraphs, callouts, code blocks, tables — so the book reads and looks the same throughout, and a fix to the rendering improves every chapter at once. It is the documentation equivalent of the code's shared seam: write once, render everywhere, no divergent copies.


### A freshness idea: anchor chapters to their source

The subtle way a code book goes stale is not obvious errors — it is a chapter quietly describing code that has since moved on. The defense is a habit borrowed from how the codebase itself is organized: *each chapter is anchored to the specific source files it documents*. The outline already records these anchors (Chapter 8 points at `AccessContext`, `AccessSnapshot`, `VisibilityQueries`, and `ApiAuthorization`; Chapter 22 points at the `*.Tests` projects; and so on).

That mapping turns staleness from a guessing game into a mechanical check. When a code change touches a file, the anchors tell you which chapter claims to describe that file — and therefore which chapter to review. In practice: a maintainer changing `AccessContext` can look up which chapter is anchored to it (§8), and knows before merging that §8 may now need an amendment. The version-control history of the source and the anchors together answer the question "which chapters did this change put at risk?" without anyone having to reread the whole book.

> **Jargon, in plain words** — An 'anchor' here is just a recorded link between a chapter and the source files it explains. A 'diff' is the list of files a change touched. Put the two together and you can cross-reference: this change touched these files, these files are anchored to these chapters, so these chapters need a fresh read. It is a low-tech smoke alarm for documentation drift — it doesn't fix the book, it just tells you where to look.

This is only an idea, not an automated gate, and that is by design — the judgment of whether a code change actually invalidates a chapter belongs to a human. But even as a manual habit it is powerful: it converts "is the book still accurate?", an impossible question to answer all at once, into "did this specific change affect its anchored chapter?", a question anyone can answer in a minute.


### The Amendments Register

Below is the register itself, empty and ready for its first entry. Every future change to this guide gets one row: the date it was made, whether it amended an existing section or added a new one (with the section number), and a short note on what changed. Read top to bottom, it will one day be the complete, honest history of how this book kept pace with the code it describes.

| Date | Amends / Adds | Section | Change |
| --- | --- | --- | --- |

> **How to add a row** — Use an ISO date (YYYY-MM-DD). In the second column write AMENDS or ADDS. In the third write the section number (for example §16, or §25 for a brand-new one). In the last, one plain sentence on what changed and why. Keep entries newest-last so the register reads as a timeline, matching the way the code's own history reads.


## Why It Matters / Design Takeaways

A guide is only as valuable as it is trusted, and trust is fragile — it survives a hundred accurate chapters and dies on one confidently wrong one. The four rules here exist to protect that trust: stable numbers so references never rot, amendments so every change is visible and dated, a single source so the two output formats can never disagree, and source anchors so drift can be spotted before it misleads anyone. None of it is clever. All of it is durable.

Notice that these rules are the same instincts that run through the whole codebase, applied to prose: record change as a traceable addition rather than a silent rewrite (like migrations), keep one source of truth and generate the rest (like the shared UI seam), and make the truth easy to check rather than something to take on faith (like the test suite). A book maintained the way the code is built will age the way the code ages — gracefully, and legibly, long after its authors have moved on.

> **The maintainer's rule** — Never renumber a section and never edit the generated Word document. When the code changes, edit the chapter source, log a dated row in the Amendments Register (AMENDS the section, or ADDS a new one), and let the document regenerate. When a code change touches a file a chapter is anchored to, reread that chapter before you merge. A book kept honest one small amendment at a time stays worth reading for as long as the code lives.
