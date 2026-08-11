# AI assist (optional)

OpenTrack can use **Anthropic's Claude** to help with issues — starting with
**smart triage** (suggesting a severity, priority, category, and tags for a new
issue from what you typed). It's **off by default** and only ever runs when you
turn it on and provide a key.

## Important: billing

The AI features call the **Anthropic API**, which is billed to an **Anthropic API
account** — this is **separate** from any Claude subscription (Pro/Max). Turning
on AI here does **not** draw from a Claude.ai subscription. You'll need an API key
from <https://console.anthropic.com>, funded with credits or a card. The Console
also lets you set a hard monthly spend limit.

Costs for these features are small — a triage suggestion is typically a fraction
of a cent — but check current per-token pricing in the Console.

## Turn it on

Set these in configuration (`appsettings.json`, environment variables, or .NET
user-secrets — **prefer not committing the key**):

```
OpenTrack:Ai:Enabled = true
OpenTrack:Ai:ApiKey  = sk-ant-...            (your Anthropic API key)
OpenTrack:Ai:Model   = claude-haiku-4-5-20251001   (default; fast & inexpensive)
```

As environment variables that's `OpenTrack__Ai__Enabled=true`,
`OpenTrack__Ai__ApiKey=sk-ant-...`, etc.

Restart OpenTrack. On the **New issue** page you'll now see a **✨ Suggest with
AI** button that fills in severity/priority/category and proposes tags — always
as suggestions you can accept or change.

## Privacy & safety

- **Opt-in.** With AI off (the default) or no key set, OpenTrack behaves exactly
  as before and makes no external calls.
- **Data leaves your server when it's on.** A suggestion sends that issue's
  summary/description to Anthropic. Don't enable it for projects whose contents
  can't leave your environment.
- **Server-side only.** The key is read from server configuration and never sent
  to the browser or stored in the database.
- **Best-effort.** Every AI result is a suggestion with a human in the loop; if a
  call fails, issue creation is unaffected.
