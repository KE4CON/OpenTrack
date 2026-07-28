# OpenTrack — Decisions Log

**Read this alongside `OpenTrack_Reference.pdf` and `CLAUDE.md`.** This is a living document, updated across sessions, tracking open items from the Reference doc through to resolution. Re-upload this file (or point Claude at it) at the start of each new session so decisions aren't lost or re-litigated from scratch.

**Legend:** 🟢 Jim's idea &nbsp;|&nbsp; 🔵 Claude's suggestion/research — not a decision &nbsp;|&nbsp; 🟣 Decided together, treat as settled &nbsp;|&nbsp; 🟠 Still open, not yet decided

**Working agreement:** Decisions are worked through one at a time to a real conclusion, then recorded here and in `CLAUDE.md` (the project's living source of truth, currently updated through section 14). This log is expected to grow (new items added) and shrink (items resolved) as sessions continue.

---

## Status Overview

| # | Item | Status |
|---|---|---|
| 1 | Standalone vs. panel-in-existing-app | N/A — OpenTrack was conceived standalone from the start |
| 2 | Technology stack (.NET 10 / C# 14, Blazor, EF Core, SQLite, Identity) | 🟣 **SETTLED** |
| 3 | Solution structure — six projects incl. shared `OpenTrack.UI` library | 🟣 **SETTLED** |
| 4 | Issue sponsorship (Mantis parity item) | 🟣 **SETTLED — deferred** |
| 5 | External wiki (Mantis parity item) | 🟣 **SETTLED — built-in lightweight wiki** |
| 6 | Localization (Mantis parity item) | 🟣 **SETTLED — English only, i18n framework in place** |
| 7 | Legacy source control (Mantis parity item) | 🟣 **SETTLED — Git only** |
| 8 | Build desktop app alongside web app | 🟣 **SETTLED — yes, shared components + thin MAUI shell** |
| 9 | Desktop architecture: thin client vs. standalone | 🟠 **OPEN** |
| 10 | Hardware / deployment drive layout (Beelink EQi12) | 🟣 **SETTLED** |
| 11 | Documentation & user manual format | 🟣 **SETTLED** (incremental approach); tooling choice deferred to ~Phase 3 |
| 12 | Phase 1 — solution skeleton | 🟣 **SETTLED — built & verified** |
| 13 | Phase 1 — EF Core data layer | 🟣 **SETTLED — built & verified** |
| 14 | Phase 1 — ASP.NET Core Identity auth | 🟣 **SETTLED — built & verified (corrected; see Item #14 note)** |
| 15 | Security advisories (Microsoft.OpenApi, SQLitePCLRaw) | 🟣 **SETTLED — patched** |
| 16 | Git incident (nested clone / deleted `.git`) | 🟣 **SETTLED — resolved, no work lost** |
| 17 | First Blazor CRUD screens + `[Authorize]` | 🟣 **SETTLED — built & verified** |
| 18 | Role-based authorization policies (`UserRole` enum) | 🟣 **SETTLED — built & verified** |
| 19 | `OpenTrack.Desktop` MAUI shell (install MAUI workload) | 🟠 **OPEN — next up** |

---

## Item #2 — Technology Stack 🟣 SETTLED

Jim confirmed the stack directly: **.NET 10 LTS / C# 14**, **Blazor Web App** (Server render mode), **ASP.NET Core Web API**, **EF Core** with **SQLite**, and **ASP.NET Core Identity**. Chosen to match the discipline already proven in Jim's other C#/.NET projects.

---

## Item #3 — Solution Structure 🟣 SETTLED

**Claude's suggestion, adopted:** six projects, with all Blazor components consolidated into one shared Razor Class Library so the web and (future) desktop apps render identical UI without duplicating work.

- `OpenTrack.Core` — domain models, interfaces, business logic
- `OpenTrack.Infrastructure` — EF Core, repositories, email, file storage
- `OpenTrack.API` — ASP.NET Core REST API + SignalR
- `OpenTrack.UI` — shared Razor Class Library (all Blazor components live here)
- `OpenTrack.Web` — Blazor web host
- `OpenTrack.Desktop` — .NET MAUI Blazor Hybrid shell

---

## Item #4 — Issue Sponsorship 🟣 SETTLED (deferred)

**Background:** Mantis lets users pledge money toward getting an issue fixed (an `IssueSponsor` model — a ledger only, not payment processing). Flagged as a genuine keep/drop call rather than auto-matched, since it introduces payment/money semantics.

**Decision:** Jim deferred it — *"lets defer the issue sponsorship."* Not built in Phases 1–3; revisit for Phase 4 or post-1.0 only if a community forms around the project and actually wants it. Low cost to skip now; genuinely easy to add later since it's a self-contained addition bolted onto the issue entity.

---

## Item #5 — External Wiki 🟣 SETTLED

**Decision:** Built-in, lightweight wiki. No external DokuWiki or MediaWiki integration — avoids standing up and maintaining a second system.

---

## Item #6 — Localization 🟣 SETTLED

**Background:** Mantis ships 49+ languages; true parity there is an ongoing community effort, not a launch item.

**Decision:** Ship **English only**. Strings are externalized so the i18n *framework* exists (near-free to build in now) and community translations are *possible* later — but there's no multi-language commitment or workload on Jim's part.

---

## Item #7 — Legacy Source Control 🟣 SETTLED

**Decision:** **Git only.** Mantis links to SVN and Mercurial as well as Git; those are dropped entirely as not reflecting modern reality.

---

## Item #8 — Build Desktop Alongside Web 🟣 SETTLED

**Context:** Jim asked whether adding a desktop build would complicate things, noting he has a Windows laptop, a MacBook Pro, a Raspberry Pi, and a Linux Mint laptop available.

**Decision:** Yes — build for desktop from the start, but as **shared components + a thin MAUI shell** (Item #3), making it low-cost insurance rather than a second project.

**Key research surfaced along the way:**
- .NET MAUI officially targets Windows, macOS (Mac Catalyst), iOS, Android — **not Linux desktop** (Microsoft: "not planned"). Community backends (Avalonia's MAUI backend, OpenMaui) exist but are preview-stage; not depended on.
- Linux users get the fully cross-platform **web app** instead — no functionality is lost.

**Hardware-to-role mapping (Claude's research):**

| Machine | Role |
|---|---|
| Windows laptop | Primary dev; web app + native Windows desktop app |
| MacBook Pro | Required to build/test the macOS desktop app (Mac Catalyst); also web app |
| Raspberry Pi | Test Linux server on ARM64 + Docker; web client in browser |
| Linux Mint laptop | Test Linux server build + web client in Firefox/Chromium |

---

## Item #9 — Thin Client vs. Standalone Desktop 🟠 OPEN

Is the desktop app a **thin client** talking to the `OpenTrack.API` server (shared issues, collaborative), or a **standalone single-user** instance with its own local SQLite?

**Claude's recommendation, not yet confirmed by Jim:** default to thin client — a bug tracker is inherently collaborative — with standalone left as a possible later option.

**Next step:** confirm before the `OpenTrack.Desktop` MAUI shell is scaffolded (Item #19), since it affects how that project is wired to the API from day one.

---

## Item #10 — Hardware / Deployment Drive Layout 🟣 SETTLED

Target server: Jim's **Beelink EQi12** mini PC.
- **500 GB C: drive** — OS and application.
- **1 TB D: drive (WD Black SN7100)** — SQLite database and file attachments, deliberately separated from the system drive.

---

## Item #11 — Documentation & User Manual 🟣 SETTLED (approach); tooling deferred

**Decision:** A full user manual is a committed deliverable, written **incrementally as features land** — each phase adds its own manual section, so it's never stale and never a single end-of-project push. Table of contents mirrors the feature list. Lives in `docs/manual/`.

**Still open, non-blocking:** exact tooling — MkDocs Material vs. DocFX — to be finalized around Phase 3. Either produces a searchable HTML site and an exportable PDF from one source. Lightweight in-app contextual help is planned as a later, separate nicety.

---

## Item #12 — Phase 1: Solution Skeleton 🟣 SETTLED — built & verified

Six projects scaffolded per Item #3, clean build, tests passing, AGPL-3.0 header applied to every file. Delivered as an overlay zip, verified end-to-end in a container environment before handoff.

---

## Item #13 — Phase 1: EF Core Data Layer 🟣 SETTLED — built & verified

Nine domain entities — `User`, `Project`, `Issue`, `IssueNote`, `IssueHistory`, `IssueAttachment`, `Category`, `ProjectVersion`, `ProjectMembership` — plus supporting enums (Mantis-style explicit numeric values, e.g. `IssueStatus.Resolved = 80`). SQLite-backed `AppDbContext` with relationships, unique indexes, and delete rules configured. The `InitialCreate` migration applies automatically on app startup via `db.Database.Migrate()`. `opentrack.db` is git-ignored.

**Naming note:** the product-version entity is named `ProjectVersion` rather than `Version` to avoid clashing with `System.Version`; its `DbSet` remains `Versions`.

---

## Item #14 — Phase 1: ASP.NET Core Identity Auth 🟣 SETTLED — built & verified

`User` derives from `IdentityUser<int>`. Working register/login/logout with antiforgery protection. The first registered account is auto-promoted to Administrator.

**Correction, discovered while applying the Phase 2 CRUD overlay:** despite this item having been marked settled from an earlier session, the actual committed repo still had the *pre-Identity* `User` (plain entity with `Username`/`PasswordHash`) and a plain `AppDbContext : DbContext` — no `AspNetUsers`/`AspNetRoles` tables, no Login/Register pages existed on disk at all. The Identity work had been built and verified in a prior session but never actually applied to Jim's local repo or pushed to GitHub. This surfaced as a genuine schema mismatch (migration file didn't match the C# model) when applying the Phase 2 CRUD overlay, and was fixed properly in this session: `User` converted to `IdentityUser<int>`, `AppDbContext` converted to `IdentityDbContext<User, IdentityRole<int>, int>` (preserving all existing domain-model configuration), a new `AddOpenTrackIdentity()` extension added alongside the existing `AddOpenTrackInfrastructure()` pattern in `DependencyInjection.cs`, the full Login/Register/Manage account pages added, and the migration regenerated from scratch. Verified end-to-end (register → auto-admin → create project → create issue → edit → status change reflected) directly against Jim's real repo conventions, then committed and pushed.

**Lesson for future sessions:** a milestone being marked "settled" in this log or in `CLAUDE.md` reflects that it was *built and verified in a session* — it does not guarantee the deliverable was actually applied and pushed by Jim. Worth a quick sanity check (e.g. `git log`, or spot-checking a key file) before assuming a documented milestone is live in the actual repo, especially after any period where zip application or git issues were in play.

---

## Item #15 — Security Advisories 🟣 SETTLED — patched

Two transitive advisories surfaced and were resolved during the Phase 1 build, in both cases staying on the same major version the surrounding tooling expected rather than jumping majors:

| Package | Pinned Version | Where |
|---|---|---|
| `Microsoft.OpenApi` | 2.11.0 | `OpenTrack.API` |
| `SQLitePCLRaw.bundle_e_sqlite3` | 2.1.12 | `OpenTrack.Infrastructure` |

Final state: 0 warnings, 0 errors, all tests passing.

---

## Item #16 — Git Incident 🟣 SETTLED — resolved, no work lost

GitHub Desktop cloned the repo one level too deep (`C:\Dev\OpenTrack\OpenTrack`). While correcting the nesting, the hidden `.git` folder was deleted and build artifacts were briefly committed.

**Resolution (no files moved or altered):**
```
cd C:\Dev\OpenTrack
git init
git remote add origin https://github.com/KE4CON/OpenTrack.git
git fetch origin
git reset --mixed origin/main
git branch -M main
```
This relinked the existing on-disk files to GitHub's history directly, without touching any file content. The full Visual Studio `.gitignore` was subsequently restored from git history.

---

## Item #17 & #18 — Blazor CRUD Screens + Role-Based Authorization 🟣 SETTLED — built & verified

**What was built**, all living in the shared `OpenTrack.UI` Razor Class Library so both the web app and the future desktop shell get it for free (Item #3):

- **Projects:** list (`/projects`), create (`/projects/create`, Manager+ only), details with per-project issue list (`/projects/{id}`), edit (`/projects/{id}/edit`, Manager+ only). Creating a project auto-adds the creator as a Manager-level `ProjectMembership`.
- **Issues:** global list with a project filter (`/issues`), create scoped to a project (`/projects/{id}/issues/create`, any authenticated user — matches Mantis's open-reporting model), details with a notes thread (`/issues/{id}`), edit (`/issues/{id}/edit`, Updater+ only).
- **Authorization policies**, built on the `UserRole` enum in `Program.cs`: `RequireUpdater`, `RequireDeveloper`, `RequireManager`, `RequireAdministrator` — each accepts its own role and everything above it, since `UserRole` values are ordered ascending (Viewer=10 … Administrator=90). A claims factory (`RoleClaimsPrincipalFactory`) stamps the user's role onto their sign-in principal as an `OpenTrack.Role` claim so policy checks don't need a DB round trip.
- Issue edits that change **Status** or **Assignee** write an `IssueHistory` row automatically, per the audit-trail design in the original report.

**Two real bugs found and fixed during verification** (worth remembering for future forms work):

1. **Routing:** a Blazor Web App's `Router` component alone isn't enough to make a Razor Class Library's pages reachable — the static SSR request path is matched by ASP.NET Core's endpoint routing, which needs `app.MapRazorComponents<App>().AddAdditionalAssemblies(...)` in `Program.cs` too. Without it, requests to `OpenTrack.UI` pages 404'd even though the pages compiled fine and the client-side `Router.AdditionalAssemblies` was set correctly.
2. **Form binding:** `EditForm` models must be marked `[SupplyParameterFromForm]` (as a settable property, not a `readonly` field) or posted values never bind back into the model on the SSR round-trip — forms silently "lose" what the user typed. Edit forms additionally need a `Loaded` guard (with a hidden input to round-trip it) so the initial DB-loaded values aren't clobbered by a fresh, empty model on POST.

**Verified end-to-end** (register → auto-admin → create project → create issue → edit issue → status change reflected everywhere) with zero runtime exceptions, plus explicit role-gating checks: a plain `Reporter` account can file issues but is blocked from creating projects and from editing issues, exactly as designed. Full solution rebuild: 0 warnings, 0 errors.

**Delivered as:** `OpenTrack-Phase2-crud.zip` overlay. Apply into `C:\Dev\OpenTrack`, then either add `OpenTrack.UI` to `OpenTrack.slnx` if it isn't already listed, or run `dotnet sln add src/OpenTrack.UI/OpenTrack.UI.csproj` from the repo root — the schema is unchanged, so no new migration is needed.

---

## Open / Not Yet Discussed

- **Item #9:** Thin client vs. standalone desktop architecture — confirm before scaffolding `OpenTrack.Desktop`.
- **Item #19:** Add the `OpenTrack.Desktop` MAUI shell — requires installing the MAUI workload first.
- **Item #11 (tooling sub-decision):** MkDocs Material vs. DocFX for the manual — target ~Phase 3.

---

*Document generated from full project chat history. Update and re-save after each future session before ending.*
