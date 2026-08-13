# OpenTrack — Recovered Session Summary

> Companion to `opentrack-recovered-history.md`. Reconstructed from the lost session
> "Continue" (`local_a5f1130c-beec-4fb9-a17c-23180b7762f8`), OpenTrack work spanning
> approximately **2026-08-10 → 2026-08-13**. This summary states where OpenTrack stood at the
> end of that session, the decisions made, what shipped, open next steps, and — importantly —
> **what is NOT already in the repo's `CLAUDE.md`** so you can see what is genuinely recovered.

---

## Where OpenTrack stood at the end of the session

OpenTrack went from a **chat-delivered, never-verified Phase-1 app** to a **fully audited,
test-covered, MantisBT-surpassing issue tracker with three complete documentation books**, all
built and committed in Claude Code with build/test/commit loops.

- **Security audit: 100% closed.** Every Critical/High/Medium finding fixed, covered by real
  tests (started at **zero** real tests; ended well over 200 across the feature work).
- **MantisBT parity: achieved and exceeded.** Full parity feature set plus differentiators
  Mantis structurally can't easily match.
- **Runs as web + native desktop + REST API from one codebase**, plus tablet (PWA/offline) use,
  with per-project ACL enforced identically everywhere via one shared decision layer.
- **Documentation: standardized.** Installation Guide, User Manual, and Programming Guide all on
  the shared navy+gold per-chapter-JSON pipeline. Last merge was **PR #35** (`main` at
  `54469dd`), which matches the repo's current HEAD — so the code/docs on disk reflect the end
  state; what was lost is the **conversation and the decision record**, not committed work.

**The one standing verification caveat:** the **macOS desktop build (Mac Catalyst head) still
needs confirming on the user's Mac** — only the Windows head could be built in-session. Also, the
**AI-assist feature was never live-verified** (needs a real Anthropic API key).

---

## Key decisions made (not all captured in CLAUDE.md)

1. **Audit forward, not backward.** Because the base app was chat-built, it was given a full
   multi-lens audit once; new features are then built **lens-first** (ACL on every new query,
   reachability, a regression test per feature) with a **consolidated reviewer pass per feature
   batch**, rather than re-auditing hardened code.
2. **One shared authorization layer.** API and web/EF paths must call the *same* Core/Infra
   access logic (`AccessSnapshot` / `VisibilityQueries` / `WhereVisibleTo`) so they can never
   drift — this is the architectural fix for the systemic root cause.
3. **No known-vulnerable dependencies.** MailKit was rejected (its MimeKit dependency had an
   unpatched advisory); email uses the framework's `System.Net.Mail.SmtpClient` instead.
   Dependency-free wherever practical: hand-rolled **safe Markdown renderer** (encode-first) and
   hand-rolled **inline-SVG charts** rather than pulling libraries.
4. **Cross-host UI pattern.** Shared Blazor pages can't declare an interactive render mode
   (throws in the desktop BlazorWebView), so host-varying features (attachments, etc.) branch on
   an injected host-kind: web = static-SSR form posts; desktop = interactive `InputFile` /
   buttons. Static-SSR forms are also what make the tablet check-off flow work.
5. **Real-time = smart-poll, not SignalR.** A tiny "anything changed?" token, reload only on
   change — near-live, no wasted reloads, still cross-host, no SignalR complexity.
6. **HTTPS is a config switch** (`OpenTrack:RequireHttps`, default `false` for trusted-LAN plain
   HTTP; `true` enforces redirect + HSTS). Documented in `docs/DEPLOYMENT.md`.
7. **AI is strictly opt-in, bring-your-own-key, billed separately.** Server-side Anthropic
   Messages API calls, off by default, key never in DB/browser, every AI output is a
   human-confirmed suggestion. **API usage bills against a separate Anthropic Console account,
   NOT the $100/mo Claude Max subscription.**
8. **Public trouble-ticket intake** is a built-in per-project page (no separate website),
   Manager-enabled, off by default, with rate-limit + honeypot + length-cap guardrails — kept in
   so OpenTrack can flex toward a helpdesk role later without a retrofit.
9. **Documentation house standards locked** (now global): write for a lazy non-technical reader,
   spell out every click, no literal placeholders, automate installs, define every acronym on
   first use, two install methods (signed + build-from-source) per OS, per-chapter-JSON pipeline,
   APRS-Command manual as the depth benchmark. These were propagated into every repo's CLAUDE.md.
10. **Design-time EF migration drift (D1) fixed for good** — a single correct
    `AppDbContextFactory` that applies the app's Identity registration, so `dotnet ef` scaffolds
    cleanly and never tries to drop the live `AspNetUserPasskeys` table. No more hand-authoring.

---

## What was built / shipped (by area)

**Audit remediation** (branches `claude/audit-fixes-2026-08-10`, `claude/fix-passkey-migration-drift`,
`claude/issue-attachments`, `claude/deployment-items`):
- Shared per-project ACL enforcement (private issues/notes, IDOR closed, mass-assignment blocked,
  note/create authZ, per-project roles).
- `IsActive` sign-in enforcement; safe config-driven admin bootstrap; member-management UI;
  web-only user/role admin.
- Finish-wired the dangling layer: expected/actual behavior, due date, reproducibility, versions,
  categories, issue history, private notes, portable issues filter.
- D1 design-time factory fix + M2 optimistic concurrency (RowVersion).
- Cross-host secure attachments (GUID storage, traversal-safe, `Content-Disposition: attachment`
  + `nosniff`).
- H5 configurable HTTPS + `docs/DEPLOYMENT.md`; L8 real macOS target framework; L9 desktop
  in-app Settings screen.

**MantisBT parity + beyond:**
- Advanced search/filter/sort, saved/shareable filters, full-text search incl. note text,
  command palette (Ctrl/⌘+K), per-user preferences.
- Real SMTP email; issue relationships; tags; notifications/monitoring (view-filtered);
  bulk actions; custom fields.
- Cross-project dashboard, bug-hunt checklists (tablet + LAN, failure→linked issue), safe
  Markdown, quick-capture, backup/export (CSV/JSON), Kanban board, stale surfacing.
- MantisBT importer (lossless enum mapping, duplicate-safe) + `docs/OPENTRACK_VS_MANTISBT.md`.
- Smart duplicate detection, smart-poll live refresh, PWA + offline check-off, webhooks
  (Slack/Discord/generic).
- Public trouble-ticket intake (`/report/{project}` + `/report/status`).
- Roadmap & changelog views, reporting/trend charts (inline SVG), time logging, per-project
  workflow rules, print/PDF issue view.
- AI foundation + smart triage (`IAiAssistant`, `docs/AI_ASSIST.md`); GPS-on-issues + QR
  "scan to report" poster (QRCoder); scheduled backups + Docker packaging.

**Documentation:**
- `docs/PRODUCT_LISTING.md` (sales/positioning copy), plus the three core books —
  **Installation Guide, User Manual, Programming Guide** — all on the shared per-chapter-JSON
  build pipeline, merged through PR #35.

---

## Identified next steps / open questions (from the session)

- **Confirm the macOS desktop build** on the Mac (Mac Catalyst head) — only verified on Windows.
- **Live-verify the AI features** with a real Anthropic API key (the one feature never exercised).
- **Optional AI/leapfrog items not yet built:** automation rules engine, SLA & escalation,
  two-way Git integration (link commits/PRs, `fixes #42` auto-transition), import-from-anything
  (Jira/GitHub/CSV), PWA push notifications, natural-language search + semantic dup detection +
  thread summarization + auto-generated checklists, localization (i18n), immutable audit log,
  **email-to-ticket** intake model.
- **Produce the PDF** of step-by-step Anthropic API-key setup instructions (and local-AI hardware
  guidance) the user asked for.
- **Deferred, low-priority** items explicitly judged non-blocking: cyclic/contradictory
  relationship guard, reset-links-in-Debug-logs hardening, the latent project-delete FK note.
- Still parked per CLAUDE.md §11: **built-in wiki** and **issue sponsorship** (both future).

---

## What is genuinely NEW vs. the repo's current `CLAUDE.md`

The repo's `CLAUDE.md` is **badly out of date** relative to what this session accomplished. This
is the most important thing to fix so a future session doesn't re-decide settled questions.

- **§5 "Current status" is stale by an enormous margin.** It still reads
  *"Phase 1 — IN PROGRESS … Next actions: (1) ASP.NET Core Identity auth; (2) first project/issue
  CRUD UI; (3) add the OpenTrack.Desktop MAUI shell."* **All of that was already done before this
  session even started**, and the session then delivered: a full security audit + fixes, the
  entire MantisBT-parity feature set, a "superior to MantisBT" roadmap (Tracks A/B/C), public
  intake, AI-assist, GPS/QR, Docker, and three complete documentation books. Realistically the
  project is at/near **feature-complete beyond Phase 4**, not mid-Phase-1.
- **The chat→Claude-Code origin story is not recorded** — that the base app was delivered via
  claude.ai chat (zip handoffs), never build/test-verified, which is *why* the whole audit
  happened. Future sessions should know this history.
- **The audit and its shared-ACL architecture are not in CLAUDE.md** — the systemic root cause
  (ProjectMembership/IsPrivate/IsPublic stored but never enforced) and the fix (one shared
  Core/Infra decision layer both hosts call) are load-bearing design facts worth recording.
- **Several concrete decisions above are undocumented in CLAUDE.md**, notably: MailKit rejected
  for a vulnerable MimeKit (framework SMTP instead); real-time = smart-poll not SignalR; HTTPS as
  a config switch (`OpenTrack:RequireHttps`); the D1 design-time factory fix (migrations now
  scaffold cleanly — the older note to hand-author migrations is obsolete); passkeys are a **live
  feature**, never to be dropped; the cross-host static-SSR-vs-interactive UI pattern.
- **The AI-assist billing decision is new and worth pinning:** API usage is billed on a separate
  Anthropic Console account, not the Claude Max subscription.
- **The documentation house standards** (already added to the global/user CLAUDE.md per the
  system context) plus the fact that **all three OpenTrack books now exist and are on the
  per-chapter-JSON pipeline** — CLAUDE.md §10 still describes the manual as a future/incremental
  deliverable and mentions MkDocs/DocFX, which was superseded by the python-docx pipeline.
- **New docs on disk not referenced by CLAUDE.md:** `docs/DEPLOYMENT.md`, `docs/AI_ASSIST.md`,
  `docs/OPENTRACK_VS_MANTISBT.md`, `docs/PRODUCT_LISTING.md`, `docs/contributing/AUDIT_2026-08-10.md`,
  and the `docs/*/chapters/` guide pipelines.

**Recommended action:** update `CLAUDE.md` §5 (Current status), §6 (Decisions log), and §10
(Documentation) to reflect the end state above, and add a short "chat-origin + audit" note, so
the single source of truth stops pointing a future session back at Phase 1.
