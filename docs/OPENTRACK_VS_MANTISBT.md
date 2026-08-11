# OpenTrack vs. MantisBT — how they compare

OpenTrack was deliberately built along the same lines as **MantisBT**, so if you
know Mantis you'll feel at home. This document lays out, in plain language, what
the two have in common, where they differ, and where OpenTrack could go next.

*Short version:* OpenTrack matches MantisBT's core bug-tracking model closely and
adds a modern, cross-device experience (web + native desktop + tablet, a
dashboard, a Kanban board, and a bug-hunt checklist). MantisBT, being ~20 years
old, still leads on breadth — plugins, reporting charts, time tracking, saved
filters, subprojects, and localization. The list at the end turns those gaps
into a concrete improvement roadmap.

---

## At a glance

| Area | MantisBT | OpenTrack |
| --- | --- | --- |
| Age / maturity | ~20 years, very mature | New, focused, actively built |
| Tech stack | PHP + MySQL | .NET 10, EF Core, SQLite (swappable to SQL Server/Postgres) |
| How you run it | Web server + MySQL to set up | Web app **or** native Windows/Mac desktop app; SQLite means near-zero setup |
| Programmatic access | REST + legacy SOAP | REST API (bearer/JWT) |
| Automated tests | Community test suite | 126 tests, security-lens reviewed |
| Issue model | Full | **Same field set and the same status/severity/priority values** |
| Extensibility | Large plugin ecosystem | None yet (built-in features instead) |

---

## What's the same (or very close)

These work essentially the same way in both tools:

- **The issue itself** — summary, description, steps to reproduce, plus
  **status, severity, priority, reproducibility, and resolution**. OpenTrack uses
  the *same underlying values* as Mantis, which is why importing maps them
  exactly.
- **Projects with per-project roles.** Both use a tiered role model
  (Viewer → Reporter → Updater/Developer → Manager → Administrator) where your
  authority can differ per project, and your effective role is the higher of your
  global and per-project role.
- **Private issues and private notes** — visible only to the reporter, assignee,
  and higher-level roles.
- **Categories and versions** (affects-version / fixed-in-version).
- **Notes/comments**, **issue history**, and **file attachments**.
- **Relationships** between issues (related, duplicate, parent/child, blocks).
- **Tags/labels** and **custom fields** (per-project, multiple types).
- **Email notifications** and **following/monitoring** an issue.
- **Search and filtering**, sortable issue lists.
- A **REST API**.

If you're migrating, OpenTrack's **Import from MantisBT** brings your projects,
issues, categories, tags, and notes across (see `MANTISBT_IMPORT.md`).

---

## Where OpenTrack is different — and often ahead

- **Runs as a real desktop app *and* a website, from one codebase.** Mantis is
  web-only. OpenTrack gives you a native Windows/Mac app, a browser version, and
  a REST API — the same features across all three.
- **Near-zero setup.** SQLite is a single file; there's no separate database
  server to install and maintain. (You can still point it at SQL Server or
  PostgreSQL later.)
- **Use it from a tablet on your network.** The **bug-hunt checklist** is
  designed to run down on an iPad or Android tablet — tap Pass/Fail/N-A, and a
  failure spins up a linked issue. Mantis has no equivalent.
- **Cross-project dashboard.** A single "where should I look" overview across all
  your projects (open, overdue, stale, by severity, recent activity). Mantis's
  summary is per-project and report-oriented.
- **Kanban board view** with move-by-arrow that works even on a tablet.
- **Quick-add capture** — log a problem in seconds with just a title and project.
- **Stale-issue surfacing** — automatically flags open issues gone quiet 30+ days.
- **Backup & export** to CSV/JSON in a click, plus clear database-backup guidance.
- **Safe Markdown** in descriptions and notes (fenced code blocks for stack
  traces), rendered with a security-first approach.
- **Security posture.** Access control lives in one place and is enforced
  identically on the API and the web UI, so the two can't drift — and the whole
  app was built and reviewed with explicit security "lenses" and regression tests.

---

## Where MantisBT is still ahead (OpenTrack's gaps)

Being two decades mature, Mantis has breadth OpenTrack hasn't built yet:

- **Plugin ecosystem.** Mantis has a large library of community plugins; OpenTrack
  has none (it favors built-in features).
- **Saved filters & shareable filter links.** Mantis lets you save named filters
  and share a permalink. OpenTrack's filters are URL-based but not saved/named.
- **Roadmap & changelog views.** Mantis rolls issues up by version into a roadmap
  and a changelog. OpenTrack tracks versions but doesn't yet present these views.
- **Reporting & charts.** Mantis has summary graphs (by status, by developer,
  resolution time, etc.). OpenTrack's dashboard shows counts but not trend charts.
- **Time tracking / work log** per issue — Mantis has it; OpenTrack doesn't.
- **Workflow customization.** Mantis lets an admin restrict which status
  transitions are allowed and set per-status thresholds. OpenTrack's workflow is
  currently open (any status → any status for editors).
- **Subprojects / hierarchy.** Mantis supports nested projects; OpenTrack's
  projects are flat.
- **Localization.** Mantis ships in many languages; OpenTrack is English-only.
- **Authentication options.** Mantis supports LDAP, OAuth, signup, and anonymous
  read access. OpenTrack uses its own accounts (with passkeys) but not yet
  LDAP/OAuth or configurable public read-only access.
- **Full-text search across notes.** Mantis searches note text; OpenTrack's text
  search covers title and description.
- **Source-control integration**, **RSS feeds**, **print/PDF issue view**, and
  **column-configurable Excel export** — all present in Mantis, not yet in
  OpenTrack.

---

## Recommended improvements for OpenTrack

A concrete roadmap, roughly high-to-lower value for a small team or solo user:

1. **Saved filters + shareable filter links** — name a filter ("My open
   crashes") and pin it; share a URL. High everyday value, modest effort.
2. **Full-text search including notes** — extend the existing search to note text.
3. **Roadmap & changelog views** — you already have versions; add the two views
   that roll issues up by affects/fixed version.
4. **Reporting & trend charts** — resolution time, open-vs-closed over time,
   by-severity trend. Extends the dashboard you already have.
5. **Workflow customization** — let a Manager define allowed status transitions
   per project (closes a real parity gap with Mantis).
6. **Time / work logging** — optional hours per issue, rolled up per project.
7. **PDF / print issue view** — you already bundle a PDF library elsewhere; a
   clean printable issue would be quick.
8. **Duplicate-safe import** — record the MantisBT issue id on import so
   re-importing updates instead of duplicating.
9. **Per-user preferences** — default project, default columns, default sort.
10. **Auth options** — OAuth/LDAP sign-in and optional public read-only projects.
11. **Integrations / webhooks** — notify Slack/Discord or link commits from
    GitHub/GitLab.
12. **Subprojects or project groups** — for anyone juggling many related projects.
13. **Localization framework** — even if only English ships first, structuring
    strings now makes translation possible later.

None of these are blockers for day-to-day bug tracking — OpenTrack already covers
that well. They're the path from "a focused, modern tracker" to "a full MantisBT
replacement and then some."
