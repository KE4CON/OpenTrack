# OpenTrack Programming Guide — Locked Outline

**Audience:** developers, future maintainers, and the curious — a non-programmer should be able to
follow the story, while a programmer gets enough depth to work confidently. **Goal:** a complete
picture of *how and why* the code was written, so the project can live on after the author.

**Standard:** thorough, in everyday-layman's language — every section answers **What it does → Why it
was built this way → How it works**, defines jargon on first use (a "Jargon, in plain words" callout
where needed), and grounds every claim in the real source. Length is fine; clarity is the goal.

**Format:** Markdown is the living source of truth (in-repo, `chapters/*.json` → `PROGRAMMING_GUIDE.md`);
a styled Word `.docx` (navy + gold, matching the User Manual and Installation Guide) is generated from the
same chapter JSON. No screenshots — this is a code book. Stable section numbers; dated amendments
(`AMENDS §X`, `ADDS §Z`) with an Amendments Register — improvements are *added*, never renumbered.

**Status:** Outline drafted. Calibration chapter — **§8, The Access-Control Authority** — written first
to lock the voice for approval before the rest are generated (same process the APRS-Command guide used).

---

## Part I — Orientation
| § | Chapter | Anchor / source |
|---|---|---|
| 1 | What OpenTrack Is, and How to Read This Book | — (incl. maintenance/amendment model) |
| 2 | The Big Picture: Architecture at 10,000 Feet | Blazor Server web + minimal-API + MAUI desktop, one database, the shared data-service seam |
| 3 | The Solution Layout: Six Projects & Their Boundaries | `OpenTrack.Core`, `.Infrastructure`, `.UI`, `.Web`, `.API`, `.Desktop` + tests; the dependency arrows |

## Part II — The Core Domain (`OpenTrack.Core`)
| § | Chapter | Anchor / source |
|---|---|---|
| 4 | Issues as Data: the Entity Model | `Entities/*` (Issue, Note, Attachment, Tag, Relationship, CustomField, Project, User, ProjectMembership) |
| 5 | The Fixed Menus: Enums & the Issue Vocabulary | `Enums/*` (Status, Severity, Priority, Reproducibility, Resolution, UserRole) |
| 6 | Pure Rules in Core: Why some logic lives with no database | `Authorization/AccessContext`, SLA math, and the "keep it pure and testable" principle |

## Part III — Data & Security (`OpenTrack.Infrastructure`)
| § | Chapter | Anchor / source |
|---|---|---|
| 7 | Entity Framework Core & the One Shared Database | `Data/AppDbContext`, migrations, SQLite, the shared connection string |
| 8 | **The Access-Control Authority** (per-project ACL) | `AccessContext`, `AccessSnapshot`, `VisibilityQueries`, `ApiAuthorization` — *calibration chapter* |
| 9 | The Operations Pattern: one place per action, so Web and API never drift | `*/…Operations.cs` (Tags, Workflow, Sla, Automation, Relationships, Bulk, …) |
| 10 | Queries & Row-Level Security | `Queries/*`, `VisibilityQueries.WhereVisibleTo` — filtering in SQL, not memory |

## Part IV — The Shared Seam & the Three Hosts
| § | Chapter | Anchor / source |
|---|---|---|
| 11 | The `IOpenTrackDataService` Seam: one interface, two implementations | `UI/Services/IOpenTrackDataService`, `Web/…DbOpenTrackDataService`, `Desktop/…HttpOpenTrackDataService` |
| 12 | The Web Host: Blazor Server, Identity & endpoints | `OpenTrack.Web` (`Program.cs`, `Endpoints/*`, Identity, the Git webhook receiver) |
| 13 | The API Host: a Minimal API for the desktop app | `OpenTrack.API` (`Program.cs`, `Endpoints/*`, `ApiAuthorization`) |
| 14 | The Desktop App: MAUI Blazor Hybrid over HTTP | `OpenTrack.Desktop` (`HttpOpenTrackDataService`, settings, the shared UI) |

## Part V — The Feature Subsystems
| § | Chapter | Anchor / source |
|---|---|---|
| 15 | Issues End to End: create, edit, workflow, statuses | issue create/edit path + `Workflow/WorkflowOperations` |
| 16 | Service-Level Agreements & Escalation | `Sla/SlaPolicyOperations`, `Sla/SlaBoard`, the breach scanner |
| 17 | Automation Rules | `Automation/AutomationRuleOperations` (match → act on new issue) |
| 18 | Notifications, Webhooks & Two-Way Git | `Notifications/*`, `Webhooks/*`, `Git/GitIntegrationOperations`, the inbound webhook |
| 19 | Public Intake, QR & the Guardrails | public intake endpoints, honeypot / rate-limit / length guards, the QR poster |
| 20 | The AI Assistant: one provider seam, three helpers | the AI provider abstraction (Anthropic + OpenAI-compatible), triage / NL-search / summarize |
| 21 | The Blazor UI Layer: components, PWA & offline | `OpenTrack.UI` pages/components, the service worker, command palette, offline checklist |

## Part VI — Quality & Longevity
| § | Chapter | Anchor / source |
|---|---|---|
| 22 | Testing: the xUnit Suite and How to Add to It | the `*.Tests` projects; what's covered and why |
| 23 | How This Codebase Is Meant to Grow | adding a feature end to end; the rules that must not erode |
| 24 | How This Book Is Maintained + Amendments Register | numbering + amendment discipline |

---

## Build plan
Same source-driven pipeline as the other guides: chapters are validated JSON under `chapters/*.json`;
`guide_build.py` emits Markdown (source of truth) + a styled `.docx`. Blocks reuse the shared renderer
(`h1/h2/p/steps/bullets/callout/code/table`), so the book looks and reads like the User Manual.
Generate §8 first, lock the voice, then batch the rest.
