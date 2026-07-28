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
| 9 | Desktop architecture: thin client vs. standalone | 🟣 **SETTLED — thin client** |
| 10 | Hardware / deployment drive layout (Beelink EQi12) | 🟣 **SETTLED** |
| 11 | Documentation & user manual format | 🟣 **SETTLED** (incremental approach); tooling choice deferred to ~Phase 3 |
| 12 | Phase 1 — solution skeleton | 🟣 **SETTLED — built & verified** |
| 13 | Phase 1 — EF Core data layer | 🟣 **SETTLED — built & verified** |
| 14 | Phase 1 — ASP.NET Core Identity auth | 🟣 **SETTLED — built & verified (corrected; see Item #14 note)** |
| 15 | Security advisories (Microsoft.OpenApi, SQLitePCLRaw) | 🟣 **SETTLED — patched** |
| 16 | Git incident (nested clone / deleted `.git`) | 🟣 **SETTLED — resolved, no work lost** |
| 17 | First Blazor CRUD screens + `[Authorize]` | 🟣 **SETTLED — built & verified** |
| 18 | Role-based authorization policies (`UserRole` enum) | 🟣 **SETTLED — built & verified** |
| 19 | `OpenTrack.API` REST endpoints (for thin client) | 🟣 **SETTLED — built & verified** |
| 20 | Shared database path (Web + API use one DB) | 🟣 **SETTLED — built & verified** |
| 21 | `OpenTrack.Desktop` MAUI shell | 🟠 **OPEN — scaffolded; needs a Windows/Mac build session** |
| 22 | Data-abstraction layer so `OpenTrack.UI` works over HTTP | 🟠 **OPEN — prerequisite for desktop** |
| 23 | First-user-admin rule for API-registered accounts | 🟠 **OPEN — known gap (see Item #19 note)** |

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

## Item #9 — Thin Client vs. Standalone Desktop 🟣 SETTLED — thin client

Is the desktop app a **thin client** talking to the `OpenTrack.API` server (shared issues, collaborative), or a **standalone single-user** instance with its own local SQLite?

**Decision: thin client.** Jim confirmed he'll mostly be near the home server when using the desktop app, which tips the balance clearly — thin client gives one true, always-current set of issues viewable from web or desktop interchangeably, at the cost of needing the server reachable (a rare constraint given his actual usage pattern). Standalone would trade that away for offline capability that isn't really needed day-to-day, and would make real sync harder to add later if ever wanted (two independently-evolving databases to reconcile, vs. just adding offline queuing to a client that already talks to one source of truth).

**Real implication surfaced during this decision, important for scoping Item #19:** the Projects/Issues CRUD pages built in `OpenTrack.UI` currently call `AppDbContext` directly via EF Core — they do **not** go through `OpenTrack.API`. This was fine for the web app (same machine, same process), but a genuine thin-client desktop app needs real HTTP endpoints to talk to. So "add the desktop shell" is no longer just a MAUI scaffolding task — it now requires:
1. Building out REST endpoints in `OpenTrack.API` for the CRUD operations already built (projects, issues, notes, history).
2. Deciding whether `OpenTrack.UI`'s existing pages get rewired to call HTTP instead of `AppDbContext` directly (so web and desktop share the exact same code path), or whether the desktop app gets its own thinner UI layer that calls the API while the web app keeps its direct DB access. The former is more work up front but keeps the "write once" promise from the original shared-`OpenTrack.UI` architecture (Item #3); the latter is faster short-term but reintroduces some duplication.
3. Authentication over HTTP for the desktop client (the current cookie-based Identity setup is web-session-oriented; a desktop client typically needs a token-based flow instead).

None of this is decided yet — it's the honest scope of Item #19 now that thin client is confirmed, not a surprise to be discovered mid-build.

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

- **Item #21:** Add the `OpenTrack.Desktop` MAUI shell — scaffolded (MAUI workload installed, project trimmed to Windows + Mac targets, wired to `OpenTrack.UI`), but MAUI platform heads can only be compiled on their target OS (Windows head on Windows, Mac head on a Mac), so this needs a working session on Jim's Windows laptop rather than the Linux container everything else was verified in.
- **Item #22:** Data-abstraction layer (e.g. `IOpenTrackDataService`) so `OpenTrack.UI`'s CRUD pages can run over HTTP in the desktop client instead of injecting `AppDbContext` directly — prerequisite for the thin-client desktop app to actually function.
- **Item #23:** First-user-becomes-Administrator rule only exists on the web app's Register page; API-registered accounts default to Reporter with no promotion path yet. Practical rule for now: create the first/admin account via the web app. A proper role-management endpoint is future work.
- **Item #11 (tooling sub-decision):** MkDocs Material vs. DocFX for the manual — target ~Phase 3.
- **Deployment (future):** getting Web + API running as persistent services on the Beelink (not `dotnet run` in a terminal), reachable across the home network (Kestrel bound to the LAN interface, Windows Firewall port opened), with the database on the D: drive via a `ConnectionStrings:Default` entry.

---

## Item #19 — OpenTrack.API REST Endpoints 🟣 SETTLED — built & verified

Built the real REST API the thin-client desktop app will talk to. `OpenTrack.API` now references the domain/infrastructure layers and exposes:
- **Bearer-token auth** via ASP.NET Core Identity's built-in API endpoints (`/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`) — token-based rather than the web app's cookie-based flow, because a native desktop client can't use cookies the same way. Same `User` type and same `AppDbContext`, so an account works identically via web or API.
- **Projects**: list, get, create (Manager+), update (Manager+).
- **Issues**: global list (optional project filter), get detail (with notes), create, update (Updater+), add note.
- **Shared authorization**: the role-claims factory and the `RequireUpdater/Developer/Manager/Administrator` policies were moved into `OpenTrack.Infrastructure/Identity/` so the Web app and API enforce identical rules from one source instead of two copies. The web app's local copy of the factory was deleted.

**Two real bugs found and fixed during verification:**
1. The API's Identity setup was missing `options.Stores.SchemaVersion = IdentitySchemaVersions.Version3` (which the web app has), causing a startup crash from an EF `PendingModelChangesWarning` model mismatch.
2. Enum values (`"Major"`, `"High"`, etc.) failed to deserialize from JSON — the API expected raw integers. Fixed by registering a `JsonStringEnumConverter`, so the API accepts/returns friendly string names.

**Verified end-to-end** with real HTTP calls: register → login (token) → create project → create issue → get → update status → add note → confirm status+note reflected → global list. Plus role-gating: a plain Reporter (403 on create-project, 201 on file-issue) and unauthenticated requests (401). No schema changes were needed (confirmed by generating a migration that came back empty, then removing it).

**Known gap (Item #23):** the "first user → Administrator" rule lives only on the web app's Register page, so API-registered accounts land as Reporter. Create the admin account via the web app for now.

**Delivered as:** `OpenTrack-Phase3-api.zip`. On apply, delete the superseded `src/OpenTrack.Web/Components/Account/RoleClaimsPrincipalFactory.cs` (now shared in Infrastructure) or you'll get a duplicate-registration conflict.

---

## Item #20 — Shared Database Path 🟣 SETTLED — built & verified

Fixed the latent two-databases problem: previously both Web and API fell back to `"Data Source=opentrack.db"` resolved relative to their own launch directory, so running them from different folders silently created two separate databases. Added `ResolveOpenTrackConnectionString()` in `OpenTrack.Infrastructure`: if `ConnectionStrings:Default` is configured it's used as-is (this is where the Beelink points at the D: drive); otherwise both hosts fall back to the SAME absolute path — one `opentrack.db` under the per-machine LocalApplicationData folder. The explicit per-folder connection string was removed from the Web `appsettings.json` so it uses the shared resolver in dev too.

**Verified:** registered an account through the API, then logged into that same account through the web app — succeeds because they now share one database. Confirmed exactly one DB file is created (at the shared path) and none in the project folders.

**Note on first run after applying:** the database moves from the project folder to the shared LocalAppData location, so the app creates a fresh empty DB there on first run — any previously-registered dev accounts won't carry over (just re-register). The old `src/OpenTrack.Web/opentrack.db` is now unused and can be deleted.

**Delivered as:** `OpenTrack-shared-db-fix.zip` (touches `DependencyInjection.cs`, both `Program.cs` files, and the Web `appsettings.json`).

---
Item #22 — Data-Abstraction Layer (Shared UI over HTTP) 🟣 SETTLED — built & verified

The key enabler for the thin-client desktop app. Previously the seven CRUD pages in OpenTrack.UI injected AppDbContext directly, which a thin client can't do. Introduced IOpenTrackDataService (in OpenTrack.UI/Services/) as the single data seam, with matching view-model records. The web app implements it with direct EF Core access (DbOpenTrackDataService, resolving the current user from the Blazor circuit); the desktop app implements it over HTTP against OpenTrack.API (HttpOpenTrackDataService).

All seven pages (Projects Index/Create/Details/Edit, Issues Index/Create/Details/Edit) were refactored off AppDbContext onto the interface. The full web flow was re-run through the abstraction — register → create project → create issue → edit/status-change → notes → global list — with identical behavior and zero exceptions, confirming the refactor is behavior-preserving.

Architectural cleanup this unlocked: OpenTrack.UI no longer references OpenTrack.Infrastructure or EF Core at all — it now depends only on OpenTrack.Core plus Blazor packages. This is what lets the thin-client desktop consume the shared pages without dragging in the database layer. Authorization policies, which had briefly lived in Infrastructure, are now registered inline per-host (Web, API, Desktop each register the identical four Require* policies) with only the policy-name constants shared in OpenTrack.UI/Services/PolicyNames.cs.

Two new API endpoints were added to support the desktop create/edit forms: GET /api/projects/{id}/categories and GET /api/projects/{id}/members — both tested and working.

Delivery gotcha (resolved): the overlay relocated RoleClaimsPrincipalFactory to OpenTrack.Infrastructure/Identity/ (shared by Web + API). Because that move predated this session's zip it had to be delivered as a small follow-up (OpenTrack-factory-fix.zip) plus manually deleting the old src/OpenTrack.Web/Components/Account/RoleClaimsPrincipalFactory.cs. Applied and building clean on Jim's machine (0 errors, the 5 usual harmless BL0008 form-model warnings), verified running in the browser, committed and pushed.

Delivered as: OpenTrack-Phase4-shared-ui-verified.zip + OpenTrack-factory-fix.zip.

Item #21 — OpenTrack.Desktop MAUI Shell 🟠 OPEN — written, not yet compiled

The full desktop client is now written: HttpOpenTrackDataService, a bearer-token AuthTokenHandler, DesktopAuthState (session/login), a DesktopAuthenticationStateProvider bridging the JWT to Blazor's auth system so the shared pages' [Authorize]/<AuthorizeView> work unchanged, a login page, MauiProgram DI wiring, and the router pointed at the shared OpenTrack.UI pages. The project targets Windows + Mac only (mobile heads dropped).

Not yet compiled — MAUI platform heads only build on their target OS, so the first compile must happen on Jim's Windows laptop, where iteration on first-build errors is expected. This is the one part of the project not verified in the Linux build environment. Still needed after it compiles: confirming the JWT actually carries the OpenTrack.Role claim (bearer tokens don't include custom claims unless configured — may need an API-side adjustment), a nav shell/home redirect, and end-to-end testing against a running API.

Delivered as: OpenTrack-Desktop-UNVERIFIED.zip (clearly marked unverified).
---

*Document generated from full project chat history. Update and re-save after each future session before ending.*
