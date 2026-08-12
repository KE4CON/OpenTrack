# OpenTrack

**A modern, open-source bug & issue tracker built in C# — feature-complete like MantisBT, cross-platform by design.**

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET 10](https://img.shields.io/badge/.NET-10_LTS-512BD4.svg)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-early_development-orange.svg)](#project-status)

OpenTrack is an open-source issue tracker inspired by [MantisBT](https://www.mantisbt.org/), rebuilt on a modern .NET stack. The goal is full feature parity with Mantis — projects, workflows, custom fields, notifications, roadmaps, a REST API, and more — delivered as a cross-platform web app (with an optional native desktop build), under a permissive-to-users but strongly copyleft license.

---

## 🚧 Project status

**Early development — not yet usable.** OpenTrack is in active initial construction (Phase 1: foundation). The instructions below describe the intended experience; they will become real as the first release lands. Watch or star the repo to follow along.

---

## Why OpenTrack?

MantisBT is a battle-tested tracker, but it's built on legacy PHP. OpenTrack aims to match its capabilities on a modern foundation:

- **One language, top to bottom** — C# for the backend, the API, *and* the UI (via Blazor). No separate JavaScript framework to maintain.
- **Truly cross-platform** — the web app runs anywhere .NET does (Windows, macOS, Linux, ARM64/Raspberry Pi), with an optional native desktop app for Windows and macOS.
- **Modern architecture** — strongly typed, EF Core migrations, real-time updates over SignalR, first-party Docker support, and a fully documented REST API.
- **Yours to run and change** — self-hosted, no vendor lock-in, AGPL-licensed so improvements stay open.

## Features

Targeting full parity with MantisBT:

- **Issue management** — rich bug reports (steps, expected vs. actual), unique IDs, per-project categories & sub-projects, sticky issues, private/public issues and notes, full edit-history audit trail
- **Workflow** — configurable statuses, severity, priority, reproducibility, and resolution; per-project custom workflows
- **Relationships** — parent/child, duplicate, related, blocks/blocked-by
- **Search & filtering** — full-text search plus advanced multi-field filters; saveable and shareable
- **Custom fields** — text, numeric, date, dropdown, checkbox, and more, per project
- **Users & access** — role-based permissions (Viewer → Administrator), per-project roles, public/private projects, self-registration, local + LDAP/AD auth
- **Notifications** — email on create/update/close, per-issue monitoring, configurable rules
- **Attachments** — files on issues and notes, with image previews
- **Reporting** — dashboards and charts, roadmap and changelog views, CSV/Excel export, RSS/Atom feeds
- **Versioning** — product versions with "affects" / "fixed in" tagging and roadmap grouping
- **Time tracking** — log and summarize time per issue and project
- **Built-in wiki** — lightweight, per-project documentation
- **Git integration** — link commits to issues and auto-close from commit messages; incoming & outgoing webhooks
- **REST API** — full CRUD with token auth, OpenAPI/Swagger documented
- **Plugin hooks** — extend functionality without forking

> Some MantisBT features are intentionally scoped differently: OpenTrack ships **English-only** at launch (with an i18n framework so translations can be added later), integrates with **Git only** (not SVN/Mercurial), and does **not** include monetary issue sponsorship in early releases.

## Tech stack

| Layer | Technology |
|-------|-----------|
| Language / runtime | C# 14 · .NET 10 (LTS) |
| Web & desktop UI | Blazor (shared components) · .NET MAUI Blazor Hybrid for desktop |
| API | ASP.NET Core Web API — REST + SignalR + OpenAPI |
| Data | Entity Framework Core · **SQLite** by default (PostgreSQL / SQL Server / MySQL supported) |
| Auth | ASP.NET Core Identity + JWT · LDAP/AD |
| Email · Jobs | MailKit · Hangfire |
| Deployment | Docker + docker-compose · GitHub Actions CI/CD |

## Getting started *(target experience)*

### Docker (recommended)

```bash
git clone https://github.com/KE4CON/OpenTrack.git
cd OpenTrack
docker compose up -d
# OpenTrack will be available at http://localhost:8080
```

### Standalone

Download the single-file release for your platform, run it, and open your browser — an embedded web server and SQLite database mean there's nothing else to install.

```bash
./OpenTrack
```

Full setup, configuration, and administration instructions will live in the [user manual](docs/manual/) as features are completed.

## Roadmap

| Phase | Focus |
|-------|-------|
| **1 — Foundation** | Auth, projects, issue CRUD, notes, attachments, roles, SQLite |
| **2 — Power features** | Advanced search/filters, relationships, custom fields, notifications, time tracking, bulk actions |
| **3 — Reporting & integrations** | Dashboards, roadmap/changelog, CSV/Excel export, REST API, Git webhooks |
| **4 — Enterprise** | LDAP/AD, configurable workflows, plugins, Docker publish, desktop app packaging |

## OpenTrack vs. MantisBT

| | MantisBT | OpenTrack |
|---|---|---|
| Language | PHP | C# 14 / .NET 10 |
| Platforms | Web only | Web (all OSes) + desktop (Windows/macOS) |
| Real-time updates | No | Yes (SignalR) |
| ORM / migrations | ADOdb (legacy) | EF Core (code-first) |
| REST API | Limited | Full, OpenAPI-documented |
| Docker | Third-party | First-party |
| License | GPL v2 | AGPL v3 |

## Contributing

OpenTrack is in its earliest stage — issues, ideas, and pull requests are welcome as the foundation takes shape. A contributing guide will be added soon. Please note that contributions are accepted under the project's AGPL-3.0 license.

## License

OpenTrack is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See [LICENSE](LICENSE) for the full text.

In short: you're free to use, modify, and self-host OpenTrack. If you distribute a modified version **or run one as a network service**, you must make your source code available under the same license. This keeps OpenTrack and its improvements open for everyone, including in hosted deployments.

## Roadmap

v1 is complete and security-audited. Candidate features for the next round (2FA,
audit log, pagination, SSO, email-to-ticket, and more) are tracked in
[docs/planning/V2_ROADMAP.md](docs/planning/V2_ROADMAP.md).

## Acknowledgments

Inspired by [MantisBT](https://www.mantisbt.org/), whose feature set defined the bar OpenTrack aims to meet. Built with [.NET](https://dotnet.microsoft.com/) and [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor).
