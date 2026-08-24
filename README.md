# OpenTrack

**A modern, open-source bug & issue tracker built in C# — feature-complete like MantisBT, cross-platform by design.**

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET 10](https://img.shields.io/badge/.NET-10_LTS-512BD4.svg)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-v1_complete_&_audited-brightgreen.svg)](#project-status)

OpenTrack is an open-source issue tracker inspired by [MantisBT](https://www.mantisbt.org/), rebuilt on a modern .NET stack. It reaches full feature parity with Mantis — projects, workflows, custom fields, notifications, roadmaps, a REST API, and more — delivered as a cross-platform web app (with an optional native desktop build), under a users-friendly but strongly copyleft license.

---

## Project status

**v1 is complete and security-audited.** The core tracker, the shared per-project authorization layer, and the full MantisBT-parity feature set are built and covered by an automated test suite (0 build errors; 263 tests passing). AI-assist and email intake are opt-in and off by default. Candidate features for the next round are tracked in [docs/planning/V2_ROADMAP.md](docs/planning/V2_ROADMAP.md).

---

## Why OpenTrack?

MantisBT is a battle-tested tracker, but it's built on legacy PHP. OpenTrack matches its capabilities on a modern foundation:

- **One language, top to bottom** — C# for the backend, the API, *and* the UI (via Blazor). No separate JavaScript framework to maintain.
- **Truly cross-platform** — the web app runs anywhere .NET does (Windows, macOS, Linux, ARM64/Raspberry Pi), with an optional native desktop app for Windows and macOS.
- **Modern architecture** — strongly typed, EF Core migrations, near-live updates via lightweight smart-polling, first-party Docker support, and a fully documented REST API.
- **Yours to run and change** — self-hosted, no vendor lock-in, AGPL-licensed so improvements stay open.

## Features

- **Issue management** — rich bug reports (steps, expected vs. actual), unique IDs, per-project categories, sticky issues, private/public issues and notes, full edit-history audit trail
- **Workflow** — configurable statuses, severity, priority, reproducibility, and resolution; per-project custom workflow rules and automation rules
- **Relationships** — parent/child, duplicate, related, blocks/blocked-by; smart duplicate detection
- **Search & filtering** — full-text search (including note text) plus advanced multi-field filters; saveable and shareable; a command palette (Ctrl/⌘+K)
- **Custom fields** — text, numeric, date, dropdown, checkbox, and more, per project
- **Users & access** — role-based permissions, per-project roles, public/private projects, self-registration, **passkeys**, and one shared authorization layer enforced identically across the web and API
- **Notifications** — email on create/update/close, per-issue monitoring, plus outgoing webhooks (Slack/Discord/generic)
- **Attachments** — files on issues and notes
- **Reporting** — cross-project dashboard, inline-SVG trend charts, roadmap and changelog views, CSV/JSON export, printable/PDF issue views
- **Versioning** — product versions with "affects" / "fixed in" tagging and roadmap grouping
- **Time tracking** — log and summarize time per issue and project
- **SLA** — per-project SLA targets with background breach escalation
- **Public trouble-ticket intake** — a public "Report a problem" page (opt-in per project), a QR "scan to report" poster, and status lookup
- **Friendly ticket numbers** — optional per-project key so tickets read as `WEB-42` (great when one instance tracks several apps)
- **Email-to-ticket** — inbound email becomes a ticket, routed to a project by the recipient address (opt-in)
- **AI-assist (opt-in, off by default)** — smart triage, plain-English search, thread summaries, and a **"Suggest a fix"** helper; tiered so a free local model can do the menial work while cloud Claude handles the harder suggestions. Every AI output is a human-confirmed suggestion.
- **Git integration** — link commits to issues and auto-close from commit messages; incoming & outgoing webhooks
- **REST API** — CRUD with token auth, OpenAPI/Swagger documented
- **Backups** — scheduled SQLite snapshots with retention, plus CSV/JSON export

> Scoped differently from MantisBT: OpenTrack ships **English-only** at launch (strings externalized so translations can be added later) and integrates with **Git only** (not SVN/Mercurial). **Planned but not yet built:** a built-in wiki, LDAP/AD authentication, a plugin system, and monetary issue sponsorship — see the [v2 roadmap](docs/planning/V2_ROADMAP.md).

## Tech stack

| Layer | Technology |
|-------|-----------|
| Language / runtime | C# 14 · .NET 10 (LTS) |
| Web & desktop UI | Blazor (shared components) · .NET MAUI Blazor Hybrid for desktop |
| API | ASP.NET Core Web API — REST + OpenAPI/Swagger (JWT) |
| Data | Entity Framework Core · **SQLite** by default (PostgreSQL / SQL Server / MySQL supported) |
| Auth | ASP.NET Core Identity + JWT · passkeys |
| Email | Framework `System.Net.Mail` SMTP |
| Realtime | Smart-poll (a tiny "anything changed?" token — not SignalR) |
| Background jobs | Hosted `BackgroundService`s (SLA scanner, backup scheduler) |
| Charts | Hand-rolled inline SVG |
| Deployment | Docker + docker-compose · GitHub Actions CI/CD |

## Getting started

### Docker (recommended)

```bash
git clone https://github.com/KE4CON/OpenTrack.git
cd OpenTrack
docker compose up -d
# OpenTrack will be available at http://localhost:8080
```

The first account you create becomes the administrator. Full setup, configuration, and administration steps are in the **[Installation Guide](docs/published/)** and **User Manual** (see the [documentation](docs/)).

### Run a single instance for a whole network

OpenTrack is a normal web app: run it on one machine and every computer on the network uses it from a browser at `http://<server-ip>:<port>`. For a turnkey Ubuntu-based deployment (OpenTrack + an optional local AI), see the companion **[AI-Server](https://github.com/KE4CON/AI-Server)** repository.

## OpenTrack vs. MantisBT

| | MantisBT | OpenTrack |
|---|---|---|
| Language | PHP | C# 14 / .NET 10 |
| Platforms | Web only | Web (all OSes) + desktop (Windows/macOS) |
| Real-time updates | No | Near-live (smart-poll) |
| ORM / migrations | ADOdb (legacy) | EF Core (code-first) |
| REST API | Limited | Full, OpenAPI-documented |
| AI-assist | No | Optional (triage, search, summaries, suggest-a-fix) |
| Docker | Third-party | First-party |
| License | GPL v2 | AGPL v3 |

## Roadmap

v1 is complete and security-audited. Candidate features for the next round (2FA, audit log, real pagination, SSO/OIDC, and more) are tracked in [docs/planning/V2_ROADMAP.md](docs/planning/V2_ROADMAP.md).

## Contributing

Issues, ideas, and pull requests are welcome. Contributions are accepted under the project's AGPL-3.0 license.

## License

OpenTrack is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See [LICENSE](LICENSE) for the full text.

In short: you're free to use, modify, and self-host OpenTrack. If you distribute a modified version **or run one as a network service**, you must make your source code available under the same license. This keeps OpenTrack and its improvements open for everyone, including in hosted deployments.

## Acknowledgments

Inspired by [MantisBT](https://www.mantisbt.org/), whose feature set defined the bar OpenTrack aims to meet. Built with [.NET](https://dotnet.microsoft.com/) and [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor).
