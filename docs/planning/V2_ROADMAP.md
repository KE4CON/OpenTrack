# OpenTrack v2 roadmap

This is the backlog for the **next** major round of work. Everything here is
**not yet built** — v1 (see the README and `docs/`) is complete and audited. These
are candidate features, roughly prioritized, with a short note on *why* each one
matters. Nothing here is committed to a date; it's a menu to pull from.

Each item, when built, should follow the same rhythm as v1: its own tested,
mergeable branch; ACL applied to every new query; a regression test per feature;
and plain-language inline help.

---

## Tier 1 — highest leverage (recommended first)

These harden OpenTrack for real teams and let it scale past a hobby instance.

- **Two-factor authentication (TOTP).** Authenticator-app 2FA at login. The single
  biggest trust win for a self-hosted tool that holds real accounts, especially if
  it's ever reachable beyond a trusted LAN.
- **Audit log.** A record of who changed what — role changes, deletions, project
  and integration config, sign-ins. Expected in any "serious" tracker; also the
  deferred slice from the v1 Ops & reach work.
- **Real pagination on the issue list.** v1 added a *defensive* row cap during the
  security audit; a large instance needs true paging (or infinite scroll) with
  page-size controls, so big projects stay fast and don't materialize huge lists.

## Tier 2 — security & access

- **Login brute-force protection.** Throttle and temporarily lock repeated failed
  logins. (Today only the public intake endpoints are rate-limited.)
- **API personal access tokens.** Long-lived, revocable tokens for scripts/CI
  instead of signing in with a password — a natural companion to the REST API and
  the Git integration.
- **OIDC / SSO login.** Sign in with Google, Microsoft, Authentik, etc. for teams
  that don't want separate OpenTrack passwords.

## Tier 3 — everyday usability & data quality

- **Issue templates per project.** Structured "new bug vs feature vs task" forms so
  reports arrive complete (steps, expected/actual, environment). Big quality win.
- **Saved dashboards / scheduled report emails.** "Email me the weekly open and
  breached-SLA summary" — builds on the existing reports and SLA board.
- **Backup *restore* from the UI.** v1 can take scheduled snapshots; restore is
  still a manual file copy. A guided in-app restore closes the loop.

## Tier 4 — reach & intake

- **Email-to-ticket.** Create issues by emailing an address — closes the intake
  loop alongside the v1 public web form and QR poster.
- **Friendly per-project ticket / tracking numbers.** The public trouble-ticket
  intake already assigns a tracking number, but it is the raw issue ID (e.g. "#42").
  Add an optional per-project human-friendly format (e.g. `PROJ-42` from a project
  key) shown to submitters and on the status-lookup page — nicer to quote over the
  phone and clearer across multiple projects. The raw ID stays the internal key.
- **Web push notifications.** Real phone/desktop push on the installed PWA (needs
  VAPID keys + a subscription store + a sender). Deferred from v1 field/mobile.
- **Internationalization (i18n).** Localization groundwork plus a first
  non-English locale. Deferred from v1 Ops & reach.

## Tier 5 — polish

- **Accessibility pass (WCAG).** Keyboard navigation, screen-reader labeling,
  contrast — and a proper dark mode.
- **Onboarding / first-run experience.** A short guided setup for a brand-new
  instance (first admin, first project, optional AI/Git/SLA config).

---

## Explicitly out of scope for now

- Anything that would turn OpenTrack into a full project-management suite (Gantt,
  resourcing, time-tracking beyond the existing per-issue work log). OpenTrack is
  an **issue tracker** first; keep the core focused.

## Notes

- The v1 security audit (multi-lens, adversarially verified) found **no
  high-severity issues** and a clean per-project ACL core; a couple of low-severity
  items were consciously accepted and are documented in the code and commit
  history (non-unique import-dedup indexes; the 25 MB import upload cap).
- When picking up a v2 item, re-run a focused audit on the touched subsystem as
  part of the work, in the spirit of the v1 pass.
