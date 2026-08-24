# OpenTrack AI-Assist Plan — Tiered Local + Cloud, with Fix Suggestions

> **Status:** ✅ **Implemented** (was: Design / plan of record). Created 2026-08-24. The tiered
> routing and the "Suggest a fix" feature (Levels 1 & 2) described here are now shipped in code;
> this doc is kept as the design record.
> **One-line version:** Run a small **local** model (Ollama on the LAN) for the *menial* AI work and
> keep **cloud Claude** for the *thinking* work — including a new **"Suggest a fix"** feature — with
> every AI output remaining a human-confirmed suggestion.

This note captures (a) the tiered AI architecture, (b) the new "suggest a fix" capability at
**Levels 1 & 2**, (c) exactly what has to change in OpenTrack to support it, and (d) the
infrastructure/OS decision for the shared LAN AI box. It is grounded in the AI code that already
exists in the repo — see the inventory in §2.

---

## 1. Goal

OpenTrack is a **problem tracker**, so the most valuable AI feature is helping a human *resolve* a
problem, not just file it. We want:

- **Menial AI** (triage, tags, dedup, summaries) handled **locally** — free, private, offline.
- **Smart AI** (root-cause analysis, **fix suggestions**) handled by **cloud Claude** — quality
  matters, and the user already has a strong Claude subscription for the heavy lifting.
- A **shared local AI server on the LAN** so other tools on the network can use the same model,
  not just OpenTrack.

The user's heavy AI work lives in cloud Claude + ChatGPT; the local model only needs to be
"good enough for the menial stuff." Fix suggestions are explicitly **not** menial and route to the
cloud.

---

## 2. Where the AI code stands today (grounded inventory)

The AI layer is already provider-pluggable — this plan is mostly *additive*, not a rewrite.

| Piece | File | Notes |
|-------|------|-------|
| Interface | `src/OpenTrack.Infrastructure/Ai/IAiAssistant.cs` | `IsEnabled`, `SuggestTriageAsync`, `InterpretSearchAsync`, `SummarizeIssueAsync` |
| Config | `src/OpenTrack.Infrastructure/Ai/AiOptions.cs` | Section `OpenTrack:Ai`; `Provider` = `"anthropic"` \| `"openai"`; `Model`, `ApiKey`, `BaseUrl` |
| Cloud provider | `src/OpenTrack.Infrastructure/Ai/AnthropicAiAssistant.cs` | Claude Messages API |
| OpenAI-compatible provider | `src/OpenTrack.Infrastructure/Ai/OpenAiAssistant.cs` | **Already works with Ollama / LM Studio** via `BaseUrl` |
| Prompts / use cases | `Ai/AiTriage.cs`, `Ai/AiSearch.cs`, `Ai/AiSummary.cs`, `Ai/AiText.cs` | one file per task |
| DI | `AddOpenTrackAi(builder.Configuration)` (both `OpenTrack.Web/Program.cs` and `OpenTrack.API/Program.cs`) | |
| API surface | `src/OpenTrack.API/Endpoints/AiEndpoints.cs` | |
| Web surface | `DbOpenTrackDataService`: `IsAiEnabledAsync`, `SuggestTriageAsync`, `InterpretIssueSearchAsync`, `SummarizeIssueAsync` | |
| Similar-issue engine | `src/OpenTrack.Core/Text/IssueSimilarity.cs`; `DbOpenTrackDataService.FindSimilarIssuesAsync(...)` | the grounding source for fix suggestions |
| Docs | `docs/guides/AI_ASSIST.md` | user-facing setup |

**Two gaps this plan fills:**

1. **No "suggest a fix" capability.** The current use cases are triage, search, and summarize only.
2. **Only one provider is active at a time.** `AiOptions` binds a single `OpenTrack:Ai` provider, so
   you cannot currently run *local-menial* and *cloud-smart* **at the same time**. Tiered routing is
   the one real structural change.

Good news: because `OpenAiAssistant` + Ollama already work, the **local provider is essentially
config**, not new code. Point a provider at `http://<ai-box>:11434/v1` with a model name like
`llama3.1` and it runs.

---

## 3. Target architecture — two tiers

```
                         ┌─────────────────────────────────────────┐
   OpenTrack (issue) ──▶ │  AI router (per-task tier selection)     │
                         └───────────────┬───────────────┬─────────┘
                                         │               │
                    menial tasks         │               │   smart tasks
              (triage, tags, dedup,      │               │  (fix suggestion,
               summaries)                ▼               ▼   root-cause)
                              ┌───────────────────┐  ┌───────────────────┐
                              │ LOCAL  (Ollama)   │  │ CLOUD  (Claude)   │
                              │ OpenAI-compatible │  │ Anthropic API     │
                              │ http://box:11434  │  │ separate Console  │
                              │ 7–8B model, free  │  │ account, billed   │
                              └───────────────────┘  └───────────────────┘
```

**Task → tier routing (default):**

| Task | Tier | Why |
|------|------|-----|
| Triage (severity/priority/category/tags) | **Local** | menial, tolerant of small-model quality |
| Tag suggestions | **Local** | menial |
| Duplicate / similar detection | **Local** (embeddings) | cheap, retrieval-style |
| Thread summary | **Local** | menial |
| Natural-language search | **Local** | menial |
| **Fix suggestion / root-cause** | **Cloud Claude** | quality matters; this is the crux feature |

Tiers are configurable — any task can be pointed at either provider, and if the local box is down,
tasks can fall back to cloud (or degrade to "AI off," which the interface already handles by
returning null).

---

## 4. New capability: "Suggest a fix" (Levels 1 & 2)

**In scope now:** Levels 1 and 2. **Deferred:** Level 3 (actual code patches).

| Level | Uses | Produces | Status |
|-------|------|----------|--------|
| **1 — Likely causes + checklist** | issue title + description | probable causes, ordered "things to check" | ✅ in scope — works for non-code problems too |
| **2 — Root cause + suggested fix** | Level 1 **+ attached logs / stack traces / notes + similar *resolved* issues** | ranked root-cause hypothesis and a concrete fix to try, with citations | ✅ in scope — the sweet spot |
| **3 — Code patch** | Level 2 **+ repository source** | a proposed diff | ❌ **out of scope for OpenTrack — handled externally in Claude Code.** OpenTrack suggests the fix; the developer writes/commits the actual code change with Claude Code. |

### 4.1 Context assembly (what makes Level 2 good)

The suggestion is only as good as the grounding we feed it. For a fix suggestion we assemble:

1. Issue **title + description**.
2. **ACL-filtered notes** on the issue (reuse the same filtering `SummarizeIssueAsync` already uses).
3. **Text extracted from attachments** that look like logs / stack traces (`IssueAttachment`),
   size-capped (e.g. first N KB) so we don't blow the context window.
4. **Top-N similar *resolved* issues** from `FindSimilarIssuesAsync(...)`, including *how each was
   resolved* (resolution text + any linked commit from the Git integration). This is the highest-value
   signal — "you solved this before" — and it makes the feature smarter the longer OpenTrack is used.

### 4.2 Output shape

A new structured result, e.g.:

```
ResolutionSuggestion(
    string  Summary,               // one-paragraph root-cause hypothesis
    IReadOnlyList<string> Causes,  // ranked likely causes
    IReadOnlyList<string> Steps,   // concrete things to try, in order
    string  Confidence,            // low | medium | high (be honest)
    IReadOnlyList<SourceRef> Sources) // "issue #123", "attached log app.log", etc.
```

Presented as a **draft AI note** the user accepts (posts a clearly-labeled AI note) or discards.
Never auto-applied.

---

## 5. Changes required in OpenTrack (answering "will we need to change OpenTrack?")

**Yes — additive changes, no rewrite.** Checklist:

1. **New interface method** on `IAiAssistant`:
   `Task<ResolutionSuggestion?> SuggestResolutionAsync(ResolutionContext ctx, CancellationToken ct)`.
   Implement in **both** `AnthropicAiAssistant` and `OpenAiAssistant` (so either tier can serve it).
2. **New prompt/use-case file** `src/OpenTrack.Infrastructure/Ai/AiResolution.cs` (mirrors
   `AiTriage.cs` / `AiSummary.cs`).
3. **Tiered routing (the one structural change).** Extend `AiOptions` to a two-provider shape — e.g.
   `OpenTrack:Ai:Local` + `OpenTrack:Ai:Cloud` + a per-task tier map — and register a small router
   that resolves the right `IAiAssistant` per task (a keyed service). Keep single-provider config
   working as the default so nothing breaks.
4. **Context assembly** in `DbOpenTrackDataService` (and the API equivalent): a
   `SuggestResolutionAsync(int issueId, …)` that gathers title/description + ACL-filtered notes +
   attachment text + similar-resolved-issue resolutions, then calls the router.
5. **Attachment text extraction** — a helper to pull text out of `IssueAttachment`s that are logs /
   `.txt` / stack traces, size-capped. (Binary/image attachments are skipped.)
6. **UI** — a **"Suggest a fix"** button on the issue **Details** page
   (`src/OpenTrack.UI/Pages/Issues/Details.razor`), rendering the suggestion as a draft AI note with
   **accept / discard**, its **confidence**, and its **sources**. Reuse the existing triage/summary UI
   pattern.
7. **API endpoint** in `AiEndpoints.cs` for the same, so the desktop/API hosts get it too.
8. **Guardrail plumbing** — AI-note authorship label, per-project opt-in, size/rate caps; optionally
   log AI calls for audit.
9. **Tests** — Core prompt-shaping tests + API tests (mirroring `AiTriageTests`), including the
   AI-off/degrade-to-null path.
10. **Docs** — extend `docs/guides/AI_ASSIST.md` with the tiered setup and the fix-suggestion feature.

What **doesn't** change: putting the AI box on the LAN is invisible to OpenTrack beyond one setting —
the local provider's `BaseUrl` points at `http://<ai-box>:11434/v1` instead of `localhost`. Other
tools on the network using the same Ollama server is entirely outside OpenTrack.

---

## 6. Infrastructure: the shared LAN AI server

### 6.1 Host
- **Machine:** the **BOSGAME P2 Plus** (Intel i7-12700H, 32GB, expandable to 64GB, 2× NVMe). Adequate
  for menial local inference; no new hardware purchase needed since fix suggestions run in the cloud.
- **Beelink EQi12:** stays as spare / backup target.
- **RAM:** 32GB is enough for one 7–8B model + OpenTrack. The 64GB upgrade is **optional**, worth it
  only for a larger local model or running several at once.

### 6.2 Operating system — recommendation: **Ubuntu Server 26.04 LTS** ("Resolute Raccoon", released 2026-04-23; current LTS, ~5-yr support)
The box is becoming a **shared, always-on LAN AI server *and* app server**, which tilts the OS choice:

- **Ubuntu Server (recommended):** leaner and headless, Docker-native, more free RAM for models,
  easy remote admin over SSH, first-class Ollama/llama.cpp. Runs OpenTrack fine (Docker or native —
  Linux server is a fully supported OpenTrack target). Best fit for a 24/7 shared server.
- **Stay on Windows 11 Pro (also fine):** Ollama-for-Windows can serve the LAN too
  (`OLLAMA_HOST=0.0.0.0:11434` + firewall rule). Choose this only if the box must double as a Windows
  desktop or build the MAUI **Windows** desktop app (a dev task — normally done on the Windows laptop,
  not the server).
- **Not chosen:** dual-boot (bad for a 24/7 server).

> **Decision to confirm:** Ubuntu Server vs. keep Windows. Plan of record assumes **Ubuntu Server**;
> flip this if the box needs to stay a Windows desktop.

### 6.3 Local models
- **Menial chat model:** a 4-bit **7–8B** model — Llama 3.1 8B / Qwen2.5 7B / Gemma 2 9B (~5–6 GB
  resident). Usable speed on the i7 (a few-to-~10 tok/s); fine for confirm-before-apply suggestions.
- **Embeddings (dedup / similar):** a small embedding model (e.g. `nomic-embed-text`, ~0.5 GB).
- **Fix suggestions:** **cloud Claude** — Haiku for cheap first passes, a larger Claude for hard cases.

### 6.4 LAN exposure
Run Ollama with `OLLAMA_HOST=0.0.0.0:11434`, open the port on the LAN only (not the internet), and
point OpenTrack's local provider + any other network tools at `http://<ai-box>:11434`.

---

## 7. Guardrails (OpenTrack is a system of record)

- **Suggestion-only, human-confirmed** — AI never edits an issue or changes status automatically
  (already the AI-assist principle; keep it).
- **Labeled authorship** — accepted suggestions post as a clearly-marked *AI* note, never as a person.
- **Show sources + honest confidence** — cite the similar issues / attachments used so a human can judge.
- **Opt-in** — off by default; per-project enable; every call is an explicit action.
- **Caps** — size-limit context (attachment text, note count) and rate-limit calls.
- **Privacy** — menial data stays on the LAN (local model); only smart/fix-suggestion calls leave the
  network, to Claude, on the separate Console account.

---

## 8. Phased plan

- **Phase A — Tiered routing.** Extend `AiOptions` to two providers + task→tier map + router. Point
  menial tasks at local Ollama, keep everything else on cloud. (No new user-facing feature yet;
  proves the plumbing.)
- **Phase B — Fix suggestions, Level 1.** `SuggestResolutionAsync` using description only → cloud
  Claude → "Suggest a fix" button + draft AI note. Ship.
- **Phase C — Fix suggestions, Level 2.** Add context assembly (notes + attachment text + similar
  *resolved* issues with their resolutions). This is where it gets genuinely useful.
- **Phase D — Level 3 is NOT an OpenTrack phase.** Generating/committing actual code fixes is done
  externally in **Claude Code** by the developer. OpenTrack's responsibility ends at a well-grounded
  fix *suggestion* (Levels 1 & 2).

---

## 9. Open decisions

1. **OS:** Ubuntu Server (recommended) vs. keep Windows 11 Pro. *Confirm.*
2. **64GB upgrade:** defer unless a larger local model is wanted. *Confirm.*
3. **Local model choice:** Llama 3.1 8B vs. Qwen2.5 7B vs. Gemma 2 9B — pick after a quick bench.
4. **Config shape for tiers:** two named providers vs. a `Fast`/`Smart` keyed pair — settle in Phase A.
