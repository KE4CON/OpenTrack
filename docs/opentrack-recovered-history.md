# OpenTrack — Recovered Session History

> **What this is.** The OpenTrack portion of a large, lost Claude Code session, reconstructed
> after a Windows reboot wiped the live chat. Recovered from session **"Continue"**
> (`local_a5f1130c-beec-4fb9-a17c-23180b7762f8`, working directory tag `continue-d02f77`).
>
> **Source caveats.** That single session (~10,525 messages, created 2026-08-08, last active
> 2026-08-13) was reused across **three** projects — OpenTrack, ActivationPlanner, and
> FieldCommand-IMS. Only the OpenTrack turns are captured here. The session had to be walked
> **backward** from the end in ~500-message chunks; the OpenTrack work occupies roughly the
> middle of the session (approx. messages ~3,150–9,810). Long tool-output dumps (file writes,
> builds, test runs) have been **condensed** to short notes like "(built file X)"; the
> conversation flow and the substance of each turn are preserved. Dates are inferred from
> branch names and context (audit branch `claude/audit-fixes-2026-08-10`), so treat them as
> approximate.
>
> **Not included (other projects, deliberately excluded):** APRS-Command / ActivationPlanner
> release + code-signing work, and everything after the pivot to FieldCommand-IMS (member ID
> cards, HTTPS/nginx/TLS, the value & cost brief, git-history purge).

---

## 0. Immediately before OpenTrack (context, not OpenTrack work)

Just prior to turning to OpenTrack, the user was finishing **APRS-Command** pre-beta work
(Azure artifact signing + Apple Developer setup, a cross-platform smoke-test guide, a
code-signing PDF). Two items from that stretch matter as context for what follows:

- The user asked Claude to create a reusable **audit-methods `.md` file** — "several
  different methods to thoroughly audit this program and my other projects" — to live in each
  repo. This file is the "audit methods file" referenced throughout the OpenTrack audit.
- The user asked whether Claude could **reach IcomRigControl and OpenTrack** (both in
  `C:\dev\`) so they could be worked on directly in Claude Code.

---

## 1. Turning to OpenTrack + the fine-tooth-comb audit (≈ 2026-08-10)

**User:** "ok thanks. now lets turn to the opentracker project. I want to work on that since we
will start testing the aprs-command app."

**Claude:** Switched over, read OpenTrack's CLAUDE.md and the real repo state. Reported that
OpenTrack was **well ahead of what its CLAUDE.md "status" said**: build clean; Phase 1
essentially done and running in a browser — EF Core data layer, **ASP.NET Core Identity auth**,
full **Projects + Issues CRUD** (7 pages) on a shared `IOpenTrackDataService` seam serving both
web (direct EF) and a thin-client desktop (over HTTP); the `OpenTrack.Desktop` MAUI shell
written and being tested on Mac. Offered a choice of directions; user picked Phase 2 features.

**User:** "I don't remember [if] you code[/Claude Code] were working on this or chat. can you
tell[?]"

**Claude:** Determined from the Decisions Log (every entry ended "Delivered as:
`OpenTrack-*.zip`") that **prior OpenTrack work was done in claude.ai chat, not Claude Code** —
zip-and-apply handoffs, never built/tested in place. This session (Claude Code) edits files
directly and can build/test/commit.

**User (interrupting the start of feature work):** "stop. given that chat worked on it you
better go over it with a fine toothed comb. i don't want a repeat of the fieldcommand ims."

**Claude:** Agreed — chat-built code never got the in-place build/test loop, and the
FieldCommand lesson (a defect-only audit that missed orphaned/unreachable features) meant the
audit had to be the **full multi-lens standard**: security/authZ, data/EF correctness +
web↔desktop parity, and reachability/orphaned-features. Before fanning out, Claude confirmed
two "chat-built, never verified" smells in place:

- **`Issue.IsPrivate` is never enforced** on any read path — any authenticated user could read
  every "private" issue (IDOR-class hole).
- **Zero real tests** — all three test projects held only the default `UnitTest1.cs` stub.

Claude then ran **three grounded reviewer sub-agents in parallel** (Security/AuthZ,
Data/EF-correctness + parity, Reachability). Baseline build was clean (0/0).

### The audit findings (consolidated)

The reviewers converged on a single **systemic root cause:** `ProjectMembership` (with its
per-project `Role`), `Issue.IsPrivate`, and `Project.IsPublic` were all stored and populated but
**never read for any authorization decision** — only the caller's single global `OpenTrack.Role`
claim was ever checked. Per-project scoping and privacy were purely decorative. Ranked findings:

- **CRITICAL 1** — `Issue.IsPrivate` never enforced on any read (API + web) → any user reads
  every private issue.
- **CRITICAL 2** — No per-project access control → full IDOR on projects & issues (read and
  write); any global Manager could rename/re-scope or make-public *any* project.
- **HIGH 3** — Mass assignment / over-posting on issue update (`IsPrivate`, `Status`,
  `AssigneeId`, `IsSticky` bound straight from the body).
- **HIGH 4** — IDOR writes via note-add and issue-create (only authentication required).
- **HIGH 5** — Bearer tokens and passwords in cleartext (TLS/`UseHttpsRedirection` commented
  out; desktop points at `http://localhost:5003`).
- **MEDIUM 6** — Open registration, no confirmation required.
- **MEDIUM 7** — First-registrant becomes Administrator (TOCTOU + race; web-only bootstrap).
- Plus lower/informational items and the data/EF + reachability lenses.

---

## 2. Audit remediation — Phases 1–4 (branch `claude/audit-fixes-2026-08-10`)

**Phase 1 — cleanup.** Sticky-sort parity, removed the dead role-system registration, deleted
orphan/dead files.

**Phase 2 — the security criticals.** Built **one shared decision layer** (Core/Infrastructure
`AccessSnapshot` / `VisibilityQueries` / `WhereVisibleTo`) that **both** the API and the web/EF
path call, so they can't drift: private issues/notes filtered everywhere; project/issue IDOR
closed; mass-assignment escalation blocked (H3); note/create authorization (H4); per-project role
enforced; plus H1 (DbContext-per-operation), H2 (form/column length match), L3 (atomic
issue+history). Wrote **30 tests** (27-case permission matrix in Core + 3 real-SQLite
row-visibility tests) — from a zero-real-tests starting point.

**The D1 discovery (recurring theme).** While generating a migration, `dotnet ef` kept trying to
**drop the `AspNetUserPasskeys` table** — a .NET 10 passkey feature that is **live and wired**
(login page has "Log in with a passkey," there's a Manage → Passkeys screen). Claude ran a long
decisive experiment (reverting DI, factory, and Identity wiring one at a time) and proved it was
a **pre-existing design-time tooling artifact** in the chat-delivered code, not caused by the
audit changes: runtime model is correct, only the design-time scaffold omits passkeys. Deferred
`RowVersion`/M2 (needs a migration), documented D1, and kept Phase 2 migration-free.

**Phase 3 — admin/user safety + management UIs.** Enforced `IsActive` at the sign-in gate
(custom `SignInManager`, covers cookie + bearer); safe **config-driven admin bootstrap** seeder
(replacing the racy first-registrant-wins); de-raced `Register.razor`. Then built
**project-member management** (shared seam, both hosts) and **web-only user/role administration**
(matching how Mantis does admin), avoiding desktop dead-links.

**Phase 4 — finish-wiring the "dangling" model layer** (the FieldCommand orphaned-feature fear).
Wired end-to-end: Expected/Actual behavior + DueDate + Reproducibility on issues; **versions**
(create/select/display/delete); **categories** (create/delete); **issue history** view;
**private notes**; converted the dead static-SSR issues filter to a portable form-GET (H6);
removed orphan template pages/nav (L10). Attachments deliberately **deferred** as a focused
follow-up (security-sensitive, cross-host).

**Claude's wrap-up:** 11 commits, clean build, **34 tests where there were zero**. Root cause
fixed with one shared, tested decision layer (proved by real-DB tests). User merged via GitHub
Desktop ("Branch published, and pulled and branch deleted").

---

## 3. Fixing D1 + M2 optimistic concurrency (branch `claude/fix-passkey-migration-drift`)

**User:** "when do you want to fix the design-time EF drift[?]"

**Claude:** Recommended doing it as the prerequisite before any migration (it blocks M2).
Confirmed passkeys are a **live feature** (so the fix is "make the tooling see passkeys," never
"drop them"). Worked straight down a list:

1. Wrote a runtime-model verification test — proved the running app's model **does** include
   passkeys (the audit's H1 `DbContextFactory` change did not break them; the bug was
   design-time only).
2. Found the true culprit — a **pre-existing hidden `AppDbContextFactory`** (chat-created) that
   built the context with a bare `new AppDbContext(options)`, no service provider, so Identity's
   schema version never applied and passkeys fell out of the design-time model. Fixed by
   building the design-time context through the same registrations the app uses. A probe scaffold
   then produced an **empty** migration (proof the model matches the DB).
3. Added a regression test guarding it, then generated the **RowVersion (M2)** migration cleanly
   (only the two columns) and fully wired **optimistic concurrency**: the token round-trips
   through the edit forms; a stale save is rejected ("reload and re-apply") instead of silently
   clobbering. Tests prove first-writer-wins.

**Net:** D1 fixed (no more hand-authoring migrations, no passkey-drop risk), M2 fixed, 40 tests.

---

## 4. Cross-host secure attachments (branch `claude/issue-attachments`)

**User:** "Lets do that cross host file upload."

**Claude:** Built it as the last deferred audit item. **Secure storage core** — files stored
outside any web root, named by a server-generated GUID key with no extension (client filename
can never influence the path → no traversal or execution), size-limited, read-path-validated;
8 security tests. **Endpoints on both hosts, all ACL-checked** — API (bearer) and web (cookie +
antiforgery) list/upload/download/delete; downloads always served
`Content-Disposition: attachment` + `nosniff`. **The cross-host UI** — a shared Blazor page can't
declare an interactive render mode (throws in the desktop BlazorWebView), so the issue page
branches on an injected `IAttachmentTransfer.Host`: web gets a plain multipart `<form>` +
`<a href>`; desktop gets `<InputFile>` → API upload + download-and-open. All three hosts compile
(incl. the MAUI desktop project); 48 tests.

> **Mid-session hiccup:** the user's GitHub Desktop pull briefly wiped two uncommitted files out
> from under Claude; Claude detected it, re-created them, and confirmed everything present. (This
> "write-loss during a git operation" recurred a few times in the session.)

---

## 5. The three deployment-decision items — H5 / L8 / L9 (branch `claude/deployment-items`)

**User:** asked what H5/L8/L9 meant. **Claude** explained: H5 = plain HTTP vs HTTPS; L8 = the
desktop `.csproj` *claims* macOS support it can't actually build; L9 = desktop server address is
only changeable by editing a file. User chose "all three."

**Claude** shipped: **H5** — HTTPS as a config switch `OpenTrack:RequireHttps` (default `false`
for trusted-LAN plain HTTP; `true` enforces redirect + HSTS on both hosts), documented in
`docs/DEPLOYMENT.md` (also fixed a latent bug — the web host called `UseHttpsRedirection()`
unconditionally). **L8** — OS-conditioned `<TargetFrameworks>` so Windows builds the Windows
head and macOS builds the `net10.0-maccatalyst` head (had to clear an inherited bare
`TargetFramework` from `Directory.Build.props`). **L9** — a real in-app **Settings** screen,
reachable *before* sign-in, to set the server address live (refactored the login path to pick it
up without restart). That **closed the OpenTrack audit 100%**. Caveat flagged: only the Windows
desktop head could be built here — the **macOS build needs confirming on the user's Mac**.

---

## 6. Deciding to go for MantisBT parity — audit forward, not backward

**User:** "what is there left to do code wise[?]"

**Claude:** Audit is 100% done. Loose ends: email doesn't actually send (no-op sender), vestigial
`AspNetRoles` tables, 2 cosmetic `BL0008` warnings, Mac build needs confirming. The big remaining
chunk is **MantisBT-parity features** (advanced search/saved filters, relationships,
notifications/monitoring, custom fields, tags, bulk actions, time tracking, roadmap/changelog,
dashboards/charts, wiki, sponsorship).

**User:** "i want to do all the above. but given this started in chat do you think we need to run
all the audit methods on it from the audit methods file?"

**Claude:** Recommended **not** re-auditing backward (the chat-origin risk was already retired for
existing code, now covered by tests). Instead **audit forward**: apply the lenses *while*
building (ACL on every new query, reachability, a regression test per feature) and run a
**consolidated reviewer pass per feature batch**. The one lens not yet run — a systematic
oracle-diff of behavior against actual MantisBT — fits naturally *as* each feature is built.

**User:** "ok lets do both of the above. start building."

---

## 7. Batch 1 — search/filter, email, relationships (branch `claude/mantis-batch1`)

- **Advanced search / filter / sort** on the issue list — shared `IssueFilter` (Core) +
  one `ApplyFilter` called by both API and web/EF paths (no drift); filtering runs *after* the
  ACL so it can only narrow; case-insensitive text search (a test caught SQLite's case-sensitive
  `Contains`). Loose ends the user reminded Claude not to forget were tracked as tasks (#26 email,
  #27 role tables, #28 warnings).
- **Real SMTP email sender** — used the framework's `System.Net.Mail.SmtpClient` **instead of
  MailKit**, because every current MailKit release pulled in a **MimeKit with an unpatched
  security advisory** and Claude refused to add a known-vulnerable dependency to a
  security-audited project. Logs the confirm link when unconfigured (LAN-friendly).
- Dropped the **vestigial role tables** cleanly (passkeys untouched); fixed the **BL0008**
  warnings → 0 warnings.
- **Issue relationships** — related / duplicate / parent-child / blocks with reciprocal labels;
  ACL logic shared between API and web; related-issue list filtered so it can't leak a private
  issue's existence (tested). Ran the **heavier audit-lens reviewer** on the new-entity diff.

**Reviewer result:** no Critical/High; ACL invariant holds; no injection; no existence leaks;
migrations correct. Four lower-severity items; Claude fixed the real one — a **cross-host bug**
where a denied relationship op returned HTTP 500 and threw an unhandled exception on the desktop
host (now both map to 403 → the same handled exception) — plus canonical dedup, not-found-vs-denied,
and moved reset-link logging to Debug.

**User:** "what did you not fix[?]"  **Claude** listed the three left: cyclic/contradictory
relationship guard (data-hygiene only), reset-links-in-Debug-logs (mitigated not eliminated),
and the project-delete FK note (can't trigger today). **User:** "are they really that
important...?"  **Claude:** No — none adversely affect the app; recommended merge as-is.

**User:** "ok merged pulled and deleted. it late going to bed. we'll start again in the morning."

---

## 8. Batch 2 — tags, notifications, bulk actions, custom fields

**User (next morning):** "start at the top of the list and work down it. we'll build everything
on the list."

- **Tags/labels** — global `Tag` + `IssueTag` join, shared ACL ops, tag filter that can't reveal
  a private issue (tested). 67 tests.
- **Notifications / monitoring** — monitor/unmonitor an issue; in-app notifications on note-add +
  status/assignee changes (to reporter, assignee, monitors), **view-filtered so a monitor who
  lost access gets nothing** (no title leak); notifications page + unread nav badge; best-effort
  email via a generalized `IEmailService`; dispatch made best-effort so a notification-write
  failure never 500s an already-committed update. Ran the audit reviewer on the tags+notifications
  diff.
- **Bulk actions** — mass status/assign/close with per-issue ACL.
- **Custom fields** (completing the batch).

---

## 9. The "6 features" enhancement round + MantisBT importer

**User:** "do you have any other suggestions for this app. I mainly want it to track any bug or
other problems with my different projects." Then, prompted by the idea of reusing bug-hunt
checklists across projects and running them from an iPad: "could you do 5 and 6 from your
suggestions. Lets build some more before we merge."

Built (each its own stacked branch, static-SSR forms so they work on tablets):
- **Cross-project dashboard** (open/overdue/stale counts, per-project table, recent activity).
- **Bug-hunt checklists** — import a markdown list, tap Pass/Fail/N-A on a tablet, a **Fail
  spawns a linked OpenTrack issue**; tablet-first big touch targets; works on **iPad and
  Android** over the LAN (plain HTTP LAN access already enabled). The user noted the irony:
  "opentrack will be using itself to track problems with itself."
- **Safe Markdown & code blocks** — a dependency-free renderer that HTML-encodes first (no risky
  Markdown library), with XSS tests.
- **Quick-capture** "new bug" (title + project only).
- **Backup & export** (ACL-aware CSV + JSON).
- **Board (Kanban)** view with ◀▶ status moves.
- **Stale-issue surfacing** (filter + dashboard count).

Mid-round the user added two ideas: **"can the data from MantisBT be imported into this app"**
and (for later) a **full OpenTrack vs MantisBT comparison**. After merging the seven-feature
stack (one PR, PR #8):

- **MantisBT importer** — OpenTrack's enums turned out to be an **exact numeric match** to
  MantisBT's IDs (a payoff of building for parity), so mapping was lossless; admin-gated,
  duplicate-safe on re-import via `ImportedMantisId`. Plain-language import guide. 126 tests.
- **OpenTrack vs MantisBT comparison** doc (`docs/OPENTRACK_VS_MANTISBT.md`) — what's the same,
  where OpenTrack leads (desktop+web+API from one codebase, near-zero setup, tablet checklist,
  dashboard/board/quick-add/stale/export), and MantisBT's remaining edges (saved filters,
  roadmap/changelog, reporting, time tracking, workflow customization, print/PDF, plugins,
  localization, LDAP, note search).

---

## 10. "Superior to MantisBT" — Tracks A, C, B + public intake

**User:** "I want opentrack to be superior to MantisBT. so what [can] we do to make that happen[?]"

**Claude** proposed three tracks; user chose **everything, top-down (A → C → B)**. Along the way
the user also asked for **product-listing / sales copy** (`docs/PRODUCT_LISTING.md`, tagline
"Modern issue tracking you actually own"), and mused about non-code uses ("it's almost like a
project tracker program... didn't Microsoft have one" → Claude mapped it to the Azure DevOps
work-item / Planner space, not MS Project).

- **Track A** — saved/shareable filters, full-text search including **note text**, command
  palette (Ctrl/⌘+K, live-verified in a browser), per-user preferences. (Track A merged as PR #9.)
- **Track C** — smart **duplicate detection** + duplicate-safe import; live auto-refresh
  (user asked pros/cons of light auto-refresh vs true SignalR real-time; Claude recommended and
  built the **smart-poll** middle option — a tiny "anything changed?" token, reload only on
  change, no SignalR, still cross-host); **PWA + offline checklist** (installable, service worker
  live-verified, offline check-off queue with optimistic UI + replay-when-online, built to work
  on iPad without Background Sync); Slack/Discord/generic **webhooks**.
- **Public trouble-ticket intake** — the "support ticket" feature the user recalled. A
  Manager-enabled per-project `/report/{project}` page anyone can use **with no account**;
  submissions become normal issues; submitters get a reference and can check status at
  `/report/status`. Guarded by off-by-default, per-IP rate limiting, a honeypot, length caps, and
  status-lookup needing reference **+** matching email. Live-verified anonymous access returns 200
  while `/issues` still 302s to login. (The user clarified it's a built-in page, not a separate
  website; email-to-ticket was noted as an optional future model.)
- **Track B** — roadmap & changelog views; **reporting / trend charts** (dependency-free
  hand-rolled inline SVG — the one Track B design call); **time / work logging** per issue;
  per-project **workflow customization** (allowed status transitions, default fully open);
  **print / PDF** issue view (print-optimized page). (Track B merged as PR #12.)

**Claude's recap:** the whole "superior to MantisBT" roadmap done, **156 tests green**, working
across web + native desktop + REST API + tablet, ACL enforced identically everywhere.

---

## 11. The AI-assist leapfrog + field/mobile + ops

**User:** "ok, is there any suggestions to make this app super superior to MantisBT[?]"

**Claude** proposed a "leapfrog" menu: **AI-assisted tracking** (smart triage, semantic dup
detection + natural-language search, thread summarization, auto-generated bug-hunt checklists),
an **automation rules engine**, **SLA & escalation**, **two-way Git integration**, **import from
anything (Jira/GitHub/CSV)**, **field/mobile power** (PWA push, QR-to-ticket, GPS reports), and
**ops & reach** (i18n, Docker, scheduled backups, audit log).

The user asked to understand the AI API more ("i'll probably want to use that with you claude").
**Claude** explained OpenTrack would call the **Anthropic Messages API** server-side with the
user's **own API key** (not "me" wired in), off by default, structured JSON via tool-use,
suggestions a human confirms; costs a fraction of a cent per call (Haiku-class), key never in the
DB/browser. Confirmed for the user that **API usage bills separately** from the $100/month Claude
Max subscription (separate Anthropic Console account, usage-based, hard spend limit available).

**User:** "ok start building all the items on your suggestion list from the top down."

- **#1 AI foundation + smart triage (branch `claude/ai-foundation`)** — opt-in `IAiAssistant`
  (Anthropic Messages API, tool-use structured output), off by default, key server-side only,
  best-effort (a failure never blocks anything); a **✨ Suggest with AI** button on the New-issue
  page fills severity/priority/category and proposes tags. `docs/AI_ASSIST.md` spells out the
  separate-billing point. 160 tests. Claude flagged this is the **one feature it can't
  live-verify** without a real API key.
- Discussion of **other AI providers** (the user will use Claude but others may not), **detailed
  step-by-step API-key instructions as a PDF**, and **local AI** (Ollama / LM Studio) on a
  Raspberry Pi / mini-PC / Mac mini — Claude gave hardware recommendations to put in the guide.
- **#6 field/mobile** — **GPS location on issues** (opt-in "📍 Attach my location," shown with an
  OpenStreetMap link) + a printable **QR "scan to report" poster** per project (added QRCoder —
  MIT, pure-managed — flagged as a new dependency). 205 tests.
- **#7 ops & reach** — scheduled automatic backups + **Docker packaging** (delivered as the
  cleanly-testable slice).

---

## 12. Documentation standardization + house pipeline (PRs up to #35, ≈ 2026-08-12/13)

**User:** "now i want you to organize the docs folder like we have done for the other repos,"
then "now we move to document creation. I want all the documents to use the same style and
formatting as what was done in the FieldCommand IMS," and "for each of my projects/repos I will
probably do the installation guide, user manual and programming guide."

**Claude** adopted the shared **navy + gold python-docx house style** (`style.py`) with the
**per-chapter-JSON pipeline** (Markdown as the living source of truth; `chapters/*.json` +
`build.py`), the same pipeline as APRS-Command's manual and FieldCommand's guides.

Running themes across this documentation stretch (all became **locked house standards**, and the
user asked they be written into every repo's CLAUDE.md and applied to future repos):
- **Write for a lazy, non-technical reader** who copy-pastes literally; spell out every click;
  say what the reader will see; front-load an "In a nutshell" summary.
- **Never use a placeholder a reader might type literally** (`SERVER-IP`) — the user flagged the
  exact line "open `http://SERVER-IP:5035`" as a trap; use the reader's real value with a concrete
  example, and fix real traps (the GitHub ZIP extracts to `OpenTrack-main`, not the path told to
  `cd` into).
- **Automate installs** — one-command setup script with interactive prompts (Enter = default).
- **Define every acronym in full on first use** (the user: "when using abbreviations they must be
  declared first. such as pwa what is pwa a person will ask") — API, LAN, AI, SDK, MAUI, PWA,
  DHCP, IP, SSD, SLA, QR, etc.
- **Two install methods** per OS: Method A (signed installer — the user confirmed they have Apple
  Developer + Windows Azure accounts) and Method B (build from source — the always-works fallback,
  covering SmartScreen / Gatekeeper prompts).
- The user asked whether it hurts to run the Beelink server **headless** (monitor/keyboard/mouse
  disconnected) — Claude answered it's fine.
- Benchmark for depth: the **APRS-Command User Manual** — the user asked to **match its level of
  detail/completeness/step-by-step** for the OpenTrack user manual and make it the standard for
  all projects.
- The user asked how to make sure these standards persist across new chat windows and future
  repos → Claude wrote the standards into each repo's **CLAUDE.md** and set up a survey to apply
  them to repos that didn't have them yet, extended to Installation & Programming guides too.

By the end of this stretch (last OpenTrack activity before the session pivoted away), **all three
OpenTrack documents — Installation Guide, User Manual, Programming Guide — were on the same
per-chapter-JSON pipeline under `docs/*/chapters/` with a `build.py`, meeting their document-type
standard.** Claude's final OpenTrack line: "OpenTrack's documentation set is fully standardized."
Merged through **PR #35** (`main` at `54469dd`), matching the repo's current HEAD.

---

## 13. End of OpenTrack in this session

**User:** "ok lets turn to the fieldcommand ims. probably do this in word so i can insert images
in the user manual..."

From this point the session is **FieldCommand-IMS** (member ID cards, HTTPS/nginx/TLS, value &
cost brief, git-history purge) — not OpenTrack, and not captured here.
