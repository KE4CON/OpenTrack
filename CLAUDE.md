# CLAUDE.md — OpenTrack

> Project context and working agreement for OpenTrack. Read this first at the start of
> every session so we stay consistent and don't re-decide settled questions.
> **When a decision changes, update this file** — it is the single source of truth and
> it overrides the research report wherever the two disagree.

---

## 1. What OpenTrack is

An open-source, cross-platform **bug / issue tracker** written in C#, aiming for
feature parity with **MantisBT** (the tracker used by Ham Radio Deluxe). Deployable as a
self-hosted web app, with an optional native desktop build later.

- **Primary goal:** **full feature parity with MantisBT** — match every feature Mantis has.
  The report's section 1 is the authoritative Mantis feature inventory; the roadmap delivers
  each item. A few Mantis features need an explicit keep-or-defer call — see **§11 Open questions**.
- **Local path:** `C:\Dev\OpenTrack`
- **Repo:** `github.com/KE4CON/OpenTrack` (public vs. private — TBD; fine to start
  private until Phase 1 runs and login works, then flip to public)
- **License:** **AGPL-3.0** (GNU Affero General Public License v3.0) — *leaning/decided;
  confirm in the GitHub license dropdown at repo creation.* Chosen over plain GPL v3 because
  OpenTrack is a hosted web app and AGPL's network clause (section 13) also requires anyone
  running a **modified** version as a service to share their source. Trade-off accepted:
  some organizations ban AGPL internally.
- **Full research report / roadmap:** `docs/reference/OpenTrack_BugTracker_Report.pdf`

---

## 2. Tech stack (authoritative)

This list **supersedes the research report** where they differ.

| Layer            | Choice                                                        |
|------------------|--------------------------------------------------------------|
| Language/runtime | **C# 14 / .NET 10 LTS**  *(report says .NET 9 — outdated; .NET 9 hits end-of-support Nov 10 2026)* |
| Web UI           | **Blazor Web App** (unified template), **Server** render mode as default |
| API              | ASP.NET Core 10 Web API (REST + OpenAPI/Swagger)             |
| ORM              | EF Core 10, code-first migrations                            |
| Database         | **SQLite** by default  *(report/early chat mentioned SQL Server Express — not using that)*; swappable to PostgreSQL / SQL Server / MySQL via EF Core |
| Auth             | ASP.NET Core Identity + JWT (LDAP/AD later, Phase 4)         |
| Email            | MailKit                                                      |
| Realtime         | SignalR (live issue updates)                                 |
| Background jobs  | Hangfire (email queue, scheduled reports)                    |
| Charts           | ApexCharts.Blazor or Chart.js interop                        |
| Containers       | Docker + docker-compose                                      |
| CI/CD            | GitHub Actions                                               |

---

## 3. Solution structure

```
OpenTrack/
├── src/
│   ├── OpenTrack.Core/            # Domain models, interfaces, business logic
│   ├── OpenTrack.Infrastructure/  # EF Core, repositories, email, file storage
│   ├── OpenTrack.API/             # ASP.NET Core REST API + SignalR
│   ├── OpenTrack.UI/              # Shared Razor Class Library — ALL Blazor components live here
│   ├── OpenTrack.Web/             # Blazor web host (references OpenTrack.UI)
│   └── OpenTrack.Desktop/         # .NET MAUI Blazor Hybrid shell — ADDED NEXT STEP (needs `dotnet workload install maui`)
├── tests/
│   ├── OpenTrack.Core.Tests/
│   ├── OpenTrack.API.Tests/
│   └── OpenTrack.Web.Tests/
├── docker/                        # Dockerfile, docker-compose.yml
├── docs/                          # Report + design notes + user manual
└── OpenTrack.slnx                 # modern .NET 10 solution format (not classic .sln)
```

> **Why the shared `OpenTrack.UI` library:** the desktop app is a native MAUI shell hosting a
> WebView that renders the *same* Blazor components as the web app. Putting every component in
> a shared Razor Class Library means each feature is written **once** and both the web and
> desktop apps show it — the desktop version comes along almost for free. Avoid web-only or
> desktop-only assumptions inside these shared components.

---

## 4. Conventions

- **Namespaces:** `OpenTrack.*` (matches the project name — it is **OpenTrack**, not "OpenTracker").
- **C# style:** nullable reference types **enabled**, implicit usings on, file-scoped namespaces.
- **License header:** every `.cs` file starts with the short AGPL header comment (added by the
  Phase 1 scaffold so files are compliant from the start).
- Centralize shared build settings (TargetFramework, LangVersion, Nullable) in a
  `Directory.Build.props` so all projects stay in lockstep.

---

## 5. Roadmap & current status

Four phases, ~16 weeks (full detail in the report). Build **phase by phase — get Phase 1
running and log-in-able before expanding.**

- **Phase 1 — Foundation:** auth, projects, issue CRUD, notes, attachments, role-based access, SQLite. **Also: shared `OpenTrack.UI` library + a thin `OpenTrack.Desktop` MAUI shell scaffolded now** to validate the Blazor Hybrid architecture early (packaging/polish comes later).
- **Phase 2 — Power features:** advanced search/filters, relationships, custom fields, notifications, time tracking, bulk actions
- **Phase 3 — Reporting & integrations:** dashboards, roadmap/changelog, CSV/Excel export, REST API polish, Git webhooks
- **Phase 4 — Enterprise:** LDAP/AD, configurable workflow, plugins, localization framework (English only; community translations later), Docker publish, **desktop app packaging/signing/installers** (Windows + macOS)

**➡️ Current status: Phase 1 — IN PROGRESS.**
Done so far: (a) solution skeleton (8 projects, builds clean, tests pass, web app runs);
(b) **EF Core data layer** — all domain entities (`User`, `Project`, `ProjectMembership`,
`Category`, `ProjectVersion`, `Issue`, `IssueNote`, `IssueHistory`, `IssueAttachment`) + the
enums, an `AppDbContext` wired to **SQLite**, and the `InitialCreate` migration. The web app
auto-applies migrations on startup, so `opentrack.db` is created automatically when it runs
(verified: 9 tables created, app serves HTTP 200).
**Next actions:** (1) ASP.NET Core Identity auth — integrate the `User` entity; (2) first
project/issue CRUD UI in Blazor; (3) add the `OpenTrack.Desktop` MAUI shell (after
`dotnet workload install maui`).

---

## 6. Decisions log

| Decision            | Value                                              |
|---------------------|----------------------------------------------------|
| Project name        | **OpenTrack**                                       |
| License             | AGPL-3.0 (confirm at repo creation)                 |
| Target framework    | .NET 10 LTS / C# 14                                 |
| Default database    | SQLite                                              |
| Blazor render mode  | Server (default)                                    |
| Desktop app         | Built alongside web via shared `OpenTrack.UI` RCL; thin MAUI shell scaffolded in Phase 1 |
| Local path          | `C:\Dev\OpenTrack`                                  |
| Git workflow        | GitHub Desktop for commits/branches; VS Code for editing |
| Wiki                | Built-in lightweight wiki (no external DokuWiki/MediaWiki) |
| Localization        | English only; strings externalized so translations are possible later |
| Source control      | Git only (no SVN / Mercurial)                      |
| Issue sponsorship   | Deferred — not in Phases 1–3; revisit later only if wanted |

---

## 7. Environment & tooling

- **Server hardware:** Beelink EQi12, 1.5 TB across two NVMe drives.
  - `C:` (500 GB) — Windows, app, .NET runtime
  - `D:` (1 TB WD Black SN7100) — SQLite database file, issue attachments, backups
  - *Keeping DB + attachments on `D:` means a Windows reinstall on `C:` never touches project data.*
- **Editor:** VS Code + **C# Dev Kit** extension + **.NET 10 SDK** installed.
- **Git:** GitHub account + GitHub Desktop.
- **Copilot:** optional; free tier is fine to trial (autocomplete complements, doesn't replace,
  the architecture/scaffolding work done here).

---

## 8. Corrections applied to the original research report

Keep these in mind — the PDF in `docs/` predates them:

1. **.NET 9 → .NET 10** (report's stack table is out of date).
2. **SQL Server Express → SQLite** as the default database.
3. Name normalized to **OpenTrack** everywhere (no "OpenTracker").

---

## 9. How to work together in this project

- Treat this file as the source of truth; if a request conflicts with it, flag the conflict.
- Deliver **complete, drop-in files**, scaffolded straight into the structure above.
- Move one phase at a time; prove it runs before adding the next layer.
- Update the **Decisions log** and **Current status** whenever either changes.

---

## 10. Documentation & user manual

A full **user manual** is a committed deliverable (its table of contents will mirror the
feature list).

- **Write it incrementally** — each phase adds its own manual section as features land, so
  the manual is never stale and never a single end-of-project mountain.
- **Format (to finalize ~Phase 3):** a Markdown-based docs site — **MkDocs Material** or
  **DocFX** — producing a searchable HTML site *and* an exportable PDF from one source, so the
  online and downloadable manuals stay in sync.
- Lives in `docs/manual/` in the repo.
- Also plan for lightweight **in-app help** (contextual tips) as a separate, later nicety.

---

## 11. Open questions (feature-parity decisions)

"Full parity with Mantis" is the goal, but these specific Mantis features need an explicit
**keep / defer / drop** decision so the scope is intentional:

| Feature | Question | Status |
|---------|----------|--------|
| **Issue sponsorship** (monetary pledges toward a fix; `IssueSponsor` model — a *ledger only*, not payment processing) | Keep, or drop? | 🟡 **explained; DEFERRED** — not in Phases 1–3; revisit for Phase 4 / post-1.0 only if a funding community wants it. Easy to add later. |
| **External wiki** (DokuWiki / MediaWiki integration) | Match exactly, built-in, or skip? | ✅ **decided: built-in lightweight wiki** (no external dependency) |
| **Localization** (Mantis ships 49+ languages) | How far to go? | ✅ **decided: English only.** Keep strings externalized so the framework exists and community translations are *possible* later — but no multi-language commitment. |
| **Legacy source control** (SVN, Mercurial links) | Keep SVN/Hg, or Git-only? | ✅ **decided: Git only** |

Everything else in the Mantis inventory is unambiguous and will be matched.

---

## 12. Platforms & cross-platform testing

**Key constraint:** .NET MAUI officially targets **Windows, macOS (Mac Catalyst), iOS, Android
— NOT Linux desktop** (Microsoft: "not planned"). Community backends exist in 2026 (Avalonia's
MAUI backend, OpenMaui) but are preview/community — do **not** depend on them. Linux users get
the **web app**, which is fully cross-platform; they don't need a native desktop app.

| Product | Windows | macOS | Linux |
|---------|:-------:|:-----:|:-----:|
| Web app (primary) | ✅ | ✅ | ✅ (browser + server host) |
| Server / Docker   | ✅ | ✅ | ✅ (incl. ARM64) |
| MAUI desktop app  | ✅ | ✅ (build **requires** a Mac) | ❌ (not supported) |

**Test hardware → what each machine is for:**

| Machine | Role |
|---------|------|
| Windows laptop | Primary dev; web app + native **Windows** desktop app |
| MacBook Pro | **Required** to build/test the **macOS** desktop app (Mac Catalyst); also web app |
| Raspberry Pi | Test **Linux server on ARM64 + Docker**; web client in browser (no native desktop) |
| Linux Mint laptop | Test **Linux server** + web client in Firefox/Chromium (no native desktop) |

**Open architecture question:** Is the desktop app a **thin client** to the `OpenTrack.API`
server (shared issues, collaborative — recommended default), or a **standalone single-user**
instance with local SQLite? → *Leaning thin-client; standalone possible as a later option.*
❓ confirm.

---

## 13. Implementation notes (Phase 1)

- **Data layer location:** entities + enums live in `OpenTrack.Core` (`Entities/`, `Enums/`);
  `AppDbContext`, the design-time factory, and `Migrations/` live in `OpenTrack.Infrastructure`
  (`Data/`). `OpenTrack.Web` registers it via `AddOpenTrackInfrastructure(connectionString)`.
- **DB auto-creates:** `Program.cs` calls `db.Database.Migrate()` at startup, so running the web
  app creates/updates `opentrack.db`. Connection string is in `appsettings.json`
  (`ConnectionStrings:Default`). In dev the file sits next to the running app; the production
  `D:` drive path is just a connection-string change at deploy time.
- **`ProjectVersion`, not `Version`:** the product-version entity is named `ProjectVersion` to
  avoid clashing with `System.Version`. Its `DbSet` is still `Versions`.
- **User & auth:** `User` is a plain entity for now (has `PasswordHash` + `Role`). When we add
  **ASP.NET Core Identity** in the auth step, we'll integrate this entity with Identity rather
  than keep a second user type. Kept int-keyed and email/username-based to make that smooth.
- **Enum values:** enums use explicit Mantis-style numeric values (e.g. `IssueStatus.Resolved = 80`)
  and are stored as integers in the DB.
- **Security patches applied:** pinned `Microsoft.OpenApi` 2.11.0 (API project) and
  `SQLitePCLRaw.bundle_e_sqlite3` 2.1.12 (Infrastructure) to clear transitive advisories that the
  default templates/packages pulled in. Build is 0 warnings / 0 errors.
- **`opentrack.db` is git-ignored** (added `*.db` to `.gitignore`) — never commit the runtime database.

---

## Documentation & Installer Standards

These apply to **every** user-facing document (installation guide, user manual, programming guide, README) and **every** installer / setup script in this repo. Locked house standards.

### Audience & voice
- Assume a **lazy, non-technical reader**: does the least effort, won't read ahead, copy-pastes literally.
- **Plain, simple, layman's language**; short sentences. Detailed but easy to follow.
- **Spell out every step and click**; name the exact button/menu/field label. Say **what the reader will see** after each action.
- **Front-load a short "In a nutshell" / "Quick version" summary** at the top of each document and each chapter, so a skimmer still succeeds.
- **Define every acronym in full on first use** — e.g. "Application Programming Interface (API)" — then use the short form.
- **Never use a placeholder a reader might type literally** (e.g. `SERVER-IP`). Tell them to substitute their real value and give a concrete example.
- **Anticipate common errors** and say exactly what to do about each.
- **Length is not a concern — thoroughness is.** Never trade completeness for brevity.
- **American English spelling** (color, center, meters).

### User manuals — depth standard (the APRS-Command User Manual is the benchmark)
- Roughly **1,500–1,800 words and ~33 content blocks per chapter**.
- **Name every UI element exactly**; **tabulate every field / column / option** with a plain-language "what it means".
- Use sub-sections for variations/edge cases; **end every feature chapter with a Troubleshooting section** (symptom → fix).
- Include dedicated **reference chapters**: Troubleshooting & FAQ, Glossary, Keyboard Shortcuts, Menu/Navigation Reference (and Licenses & Credits where relevant).
- **Ground every step in the real application UI / source** — read the actual screens; never guess a label.

### Installers / setup
- **Automate as much as possible**: a **one-command setup script** with **interactive prompts** whose defaults are accepted by pressing **Enter**. The fewer chances to fail installing, the better.

### House doc pipeline
- Navy + gold python-docx house style (`style.py`). **Markdown is the living source of truth**; the styled `.docx` / PDF is generated from **per-chapter JSON** (`chapters/*.json` + a `build.py`) — the same pipeline as the APRS-Command manual and the OpenTrack guides.
- Each project ships three core documents: **Installation Guide, User Manual, Programming Guide.**
