# AI assist (optional)

OpenTrack can use an AI model to help with issues — starting with **smart
triage** (suggesting a severity, priority, category, and tags for a new issue
from what you typed). It's **off by default** and only ever runs when you turn it
on and point it at a provider. Whatever it suggests is just a suggestion — a
person always accepts or changes it.

You are **not** locked into one AI company. OpenTrack talks to two kinds of
provider, so you can pick what fits your budget and privacy needs.

## Which AI can I use?

| You want… | Provider setting | Notes |
|---|---|---|
| **Anthropic Claude** (what the author uses) | `anthropic` | Cloud. Needs an Anthropic API key. |
| **OpenAI** (ChatGPT models, e.g. GPT-4o mini) | `openai` | Cloud. Needs an OpenAI API key. |
| **Azure OpenAI** | `openai` + `BaseUrl` | Cloud, your Azure resource. |
| **Groq / OpenRouter / other hosted** | `openai` + `BaseUrl` | Cloud. Any OpenAI-compatible endpoint. |
| **Ollama** (free, runs on your own PC) | `openai` + `BaseUrl` | **Local — no key, no data leaves your machine.** |
| **LM Studio** (free, runs on your own PC) | `openai` + `BaseUrl` | **Local — same privacy benefit.** |

The `openai` setting means "any service that speaks OpenAI's Chat Completions
format." A huge number of tools — including the free local ones — do, which is
why one setting covers so many options.

## Important: who pays?

The cloud providers bill an **API account** at that provider — this is
**separate** from any monthly chat subscription. In particular, an Anthropic API
key is billed to your **Anthropic API account**, which is **not** the same as a
Claude Pro/Max subscription; turning on AI here does **not** draw from a Claude.ai
plan. The **local** options (Ollama, LM Studio) are **free** and run on your own
computer — no account, no per-use charge.

Cloud costs for these features are small — a triage suggestion is typically a
fraction of a cent — but always check the provider's current pricing, and set a
spend limit where the provider offers one.

---

## Step-by-step: get an Anthropic (Claude) API key

1. Open a browser and go to **<https://console.anthropic.com>**. This is the
   **Anthropic Console** (the developer/API site) — *not* claude.ai, the chat
   site. They are separate accounts even if you use the same email.
2. **Sign in**, or click **Sign up** and create an account (email + password, or
   "Continue with Google"). Verify your email if asked.
3. Once you're in the Console, look at the **left sidebar** (or the gear/⚙
   **Settings** menu) and click **API Keys**. (Direct link:
   <https://console.anthropic.com/settings/keys>.)
4. Click **Create Key** (sometimes labeled "Create API Key").
5. Give it a name you'll recognize later, like `OpenTrack`, and click **Create**
   / **Add**.
6. The key is shown **once**. It looks like `sk-ant-api03-XXXXXXXX…`. Click
   **Copy** and paste it somewhere safe *right now* — you can't view it again
   later (only delete it and make a new one).
7. **Add credit so the key works.** A brand-new API account usually has a $0
   balance and calls will fail until you fund it. In the sidebar go to
   **Billing** → **Plans** / **Buy credits**, add a payment method, and buy a
   small amount (even $5 is plenty for triage). While you're there, set a
   **monthly spend limit** so it can never surprise you.
8. Keep that `sk-ant-…` key handy for the "Turn it on" step below.

## Step-by-step: get an OpenAI API key

1. Go to **<https://platform.openai.com>** — the **OpenAI developer platform**,
   *not* chatgpt.com. Different account from a ChatGPT Plus subscription.
2. **Sign in** or **Sign up** and verify your email (and phone, if asked).
3. Click your **profile icon (top-right)** → **View API keys**, or go straight to
   <https://platform.openai.com/api-keys>.
4. Click **Create new secret key**, name it `OpenTrack`, and click **Create**.
5. Copy the key (it starts with `sk-…`) **now** — it's shown only once.
6. Go to **Settings → Billing**, add a payment method, and add a little credit.
   Set a **usage limit** while you're there.
7. Pick a cheap, capable model name for the config below — `gpt-4o-mini` is a
   good default.

## Free & private option: run it locally with Ollama (no key at all)

If you'd rather **not** send issue text to any cloud, run a model on your own
machine. Nothing leaves your computer.

1. Download and install **Ollama** from **<https://ollama.com>** (Windows, Mac,
   Linux).
2. Open a terminal and pull a model, e.g.:
   ```bash
   ollama pull llama3.1
   ```
   Ollama then serves an OpenAI-compatible API at `http://localhost:11434/v1`.
3. Use the "Local (Ollama)" settings below — **no API key needed**.

*(LM Studio works the same way; it serves on `http://localhost:1234/v1`.)*

---

## Turn it on

Put the settings in configuration — `appsettings.json`, environment variables, or
.NET user-secrets. **Prefer environment variables or user-secrets for the key so
it isn't committed to source control.** As environment variables, replace each
`:` with `__` (double underscore), e.g. `OpenTrack__Ai__Enabled=true`.

**Claude (Anthropic):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = anthropic
OpenTrack:Ai:ApiKey   = sk-ant-...            (your Anthropic key)
OpenTrack:Ai:Model    = claude-haiku-4-5-20251001   (fast & inexpensive)
```

**OpenAI (cloud):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = openai
OpenTrack:Ai:ApiKey   = sk-...                (your OpenAI key)
OpenTrack:Ai:Model    = gpt-4o-mini
```

**Local, free & private (Ollama):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = openai
OpenTrack:Ai:BaseUrl  = http://localhost:11434/v1
OpenTrack:Ai:Model    = llama3.1
                                              (no ApiKey needed for local)
```

**Azure OpenAI / Groq / OpenRouter / other hosted:** use `Provider = openai`,
set `BaseUrl` to that service's endpoint, `ApiKey` to its key, and `Model` to a
model it offers.

Restart OpenTrack. On the **New issue** page you'll now see a **✨ Suggest with
AI** button that fills in severity/priority/category and proposes tags.

## Privacy & safety

- **Opt-in.** With AI off (the default) or unconfigured, OpenTrack behaves exactly
  as before and makes no AI calls at all.
- **Cloud providers see the text.** With a cloud provider on, a suggestion sends
  that issue's summary/description to that provider. Don't enable a cloud provider
  for projects whose contents can't leave your environment — use a **local**
  engine (Ollama/LM Studio) instead, which keeps everything on your machine.
- **Server-side only.** The key is read from server configuration and is never
  sent to the browser or stored in the database.
- **Best-effort.** Every AI result is a suggestion with a human in the loop; if a
  call fails, issue creation is completely unaffected.
