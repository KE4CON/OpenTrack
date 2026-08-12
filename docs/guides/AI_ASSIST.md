# AI assist (optional)

## In a nutshell

OpenTrack has an optional helper powered by Artificial Intelligence (AI) — computer software that can read text and make smart guesses. It is **turned off until you turn it on**, and everything it does is just a suggestion you can accept or change. You can use a paid cloud service (like Anthropic's Claude or OpenAI) or a **free** program that runs on your own computer and keeps all your text private. This guide walks you through what it does, how to pick one, how to get set up click by click, and what it costs.

---

## What the AI can do

There are two helpers, and both only ever suggest things — a person always makes the final call.

- **Smart triage** — When you are filling out the **New issue** page, there is a button labeled **✨ Suggest with AI**. Click it, and the AI reads the summary and details you typed, then fills in a severity, a priority, a category, and some suggested tags for you. You can accept those, or change any of them, before you save.
- **Plain-English search** — On the **Issues** list page, there is a box labeled **✨ Ask in plain English**. You can type an everyday request like *"high-priority crashes nobody has touched in a month"*, and the AI turns it into the normal search filters (status, severity, priority, keywords, stale, project). It only ever builds a filter you could have set by hand yourself, and it can only look at projects you are already allowed to see — so it never gives you access to anything new.

You are **not** locked into one AI company. OpenTrack can talk to two kinds of provider, so you can pick whatever fits your budget and your privacy needs.

## Which AI can I use?

A "provider" is just the service that runs the AI. Here are your choices. The **Provider setting** column shows the exact value you will type into OpenTrack's settings later.

| You want… | Provider setting | Notes |
|---|---|---|
| **Anthropic Claude** (what the author uses) | `anthropic` | Cloud. Needs an Anthropic API key. |
| **OpenAI** (ChatGPT models, e.g. GPT-4o mini) | `openai` | Cloud. Needs an OpenAI API key. |
| **Azure OpenAI** | `openai` + `BaseUrl` | Cloud, your Azure resource. |
| **Groq / OpenRouter / other hosted** | `openai` + `BaseUrl` | Cloud. Any OpenAI-compatible endpoint. |
| **Ollama** (free, runs on your own PC) | `openai` + `BaseUrl` | **Local — no key, no data leaves your machine.** |
| **LM Studio** (free, runs on your own PC) | `openai` + `BaseUrl` | **Local — same privacy benefit.** |

A few of the terms above, in plain words:

- **Cloud** means the AI runs on someone else's computers over the internet. You send them your text and they send back an answer.
- **Local** means the AI runs on a computer you own, in your home or office. Your text never leaves your machine.
- An **Application Programming Interface (API) key** is a long secret password that proves the request is from your paid account. Cloud providers need one; local ones do not.
- **BaseUrl** is simply the web address where OpenTrack should send its requests. You only fill this in for some providers (the table shows which).

The `openai` provider setting really means "any service that speaks OpenAI's Chat Completions format" — a common language for these tools. A huge number of services — including the free local ones — speak that language, which is why this one setting covers so many options.

## Important: who pays?

The cloud providers bill an **API account** at that provider, and this is a **separate** account from any monthly chat subscription you might already pay for.

- An Anthropic API key is billed to your **Anthropic API account**. That is **not** the same thing as a Claude Pro or Claude Max subscription on claude.ai. Turning on AI here does **not** use up your Claude.ai plan — it draws from the API account instead.
- An OpenAI API key works the same way: it is billed to your OpenAI developer account, which is separate from a ChatGPT Plus subscription.
- The **local** options (Ollama, LM Studio) are **completely free** and run on your own computer. There is no account and no per-use charge.

Cloud costs for these features are small — one triage suggestion is typically a fraction of a cent — but always check the provider's current pricing, and set a spending limit if the provider lets you (both of the big ones do).

---

## Step-by-step: get an Anthropic (Claude) API key

1. Open a web browser and go to **<https://console.anthropic.com>**. This is the **Anthropic Console** — the site for developers and API accounts. It is *not* claude.ai, the chat site. These are separate accounts even if you sign in with the same email address.
2. Click **Sign in** if you already have an account. If not, click **Sign up** and create one (email and password, or the "Continue with Google" button). If the site emails you a verification link, click it.
3. Once you are signed in, look at the **menu on the left side** of the screen (or the gear-shaped ⚙ **Settings** menu) and click **API Keys**. If you cannot find it, go straight to this address: <https://console.anthropic.com/settings/keys>.
4. Click the **Create Key** button (it is sometimes labeled "Create API Key").
5. Type a name you will recognize later, such as `OpenTrack`, and click **Create** (or **Add**).
6. The key is shown **only once**. It is a long string that looks like `sk-ant-api03-XXXXXXXX…`. Click **Copy**, then paste it somewhere safe *right now* — for example a note in a password manager. You will **not** be able to see it again later; if you lose it you have to delete it and make a new one.
7. **Add money so the key works.** A brand-new API account usually has a $0 balance, and requests will fail until you add funds. In the left menu go to **Billing**, then **Plans** (or **Buy credits**), add a payment method, and buy a small amount — even $5 is plenty for triage. While you are there, set a **monthly spending limit** so the bill can never surprise you.
8. Keep that `sk-ant-…` key nearby — you will paste your real key (not the letters shown here) into the "Turn it on" step below.

## Step-by-step: get an OpenAI API key

1. Go to **<https://platform.openai.com>** — the **OpenAI developer platform**. This is *not* chatgpt.com. It is a different account from a ChatGPT Plus subscription.
2. Click **Sign in**, or **Sign up** to make a new account, and verify your email (and your phone number, if it asks).
3. Click your **profile icon in the top-right corner**, then click **View API keys** — or just go straight to <https://platform.openai.com/api-keys>.
4. Click **Create new secret key**, type the name `OpenTrack`, and click **Create**.
5. Copy the key **now** — it starts with `sk-…` and is shown only once. Paste it somewhere safe, such as a password manager.
6. Click **Settings**, then **Billing**, add a payment method, and add a little credit. Set a **usage limit** while you are there so the bill stays predictable.
7. Pick a cheap but capable model name to use in the settings below. `gpt-4o-mini` is a good default — type it exactly like that.

## Free & private option: run it locally with Ollama (no key at all)

If you would rather **not** send your issue text to any cloud company, you can run the AI on your own computer. Nothing you type ever leaves your machine. **Ollama** is a free program that makes this easy.

1. Download and install **Ollama** from **<https://ollama.com>**. It works on Windows, Mac, and Linux.
2. Open a terminal (on Windows this is the "Command Prompt" or "PowerShell" app; on Mac it is the "Terminal" app) and download a model by typing this command and pressing Enter:
   ```bash
   ollama pull llama3.1
   ```
   A "model" is the actual AI brain file. Once it finishes downloading, Ollama automatically serves an OpenAI-compatible connection at the address `http://localhost:11434/v1`. (The word `localhost` simply means "this same computer.")
3. Use the "Local (Ollama)" settings shown below in the "Turn it on" section — **you do not need an API key** for this.

*(LM Studio is another free program that works the same way. It serves at the address `http://localhost:1234/v1` instead.)*

### Run the local model on another machine (e.g. a Raspberry Pi) on your network

The local AI does **not** have to run on the same computer as OpenTrack. If you have Ollama (or LM Studio) running on a different machine on your home or office network — a Raspberry Pi (a small, cheap hobby computer), a Network-Attached Storage (NAS) box, or a spare desktop — you can point OpenTrack at **that machine's network address** instead of `localhost`.

Every device on your network has a Local Area Network (LAN) address that looks like `192.168.1.50`. To find the address of the machine running Ollama, you can usually check your router's device list, or run a command like `ipconfig` (Windows) or `ip addr` (Linux/Mac) on that machine. Then set OpenTrack's `BaseUrl` to it. For example, if the Raspberry Pi's address is `192.168.1.50`, you would use:

```
OpenTrack:Ai:BaseUrl = http://192.168.1.50:11434/v1     (the Pi's LAN IP)
```

Replace `192.168.1.50` with your own machine's real address — do not type `192.168.1.50` literally unless that truly happens to be the correct one.

There are two things to set up on the Pi so other machines can reach it:

1. **Let Ollama listen on the network, not just on itself.** By default, Ollama only answers requests from its own computer (`localhost`). Start it with the setting (called an environment variable) `OLLAMA_HOST=0.0.0.0:11434` so it accepts connections from across the network. On Raspberry Pi OS / Linux, you can add the line `Environment=OLLAMA_HOST=0.0.0.0:11434` to the Ollama service file, then run `sudo systemctl restart ollama` to restart it.
2. **Allow port 11434 through the Pi's firewall**, if the Pi has a firewall turned on. (A "port" is like a numbered door that network traffic comes in through; Ollama uses door number 11434.)

After that, OpenTrack (running on your Beelink mini PC, your laptop, wherever) will call the Pi across your own network, and your issue text still never leaves your network.

**A reality check on speed:** a Raspberry Pi has no graphics chip (Graphics Processing Unit, or GPU) to accelerate the AI, so it can only run small models and will be **noticeably slower** than a cloud service — fine for the occasional triage or summary, less good for rapid, repeated use. A Pi 5 with 8 gigabytes (GB) of memory can run a small model (roughly 1 to 3 billion parameters — "parameters" is a rough measure of a model's size and smarts, for example `llama3.2:3b`). Stick to small models and keep your expectations modest. This slowness is exactly why the OpenAI provider is given a longer, 60-second time limit than the cloud path.

## Good hardware for local AI (and what won't help)

You do not need a powerful machine for OpenTrack's use — triage and summaries are short, occasional requests, not constant heavy work. Two things decide whether a computer is "good enough": **enough memory to hold a decent model**, and (nice to have) **some graphics acceleration** for speed. Of the two, memory matters most.

Here "memory" means Random Access Memory (RAM) — the computer's short-term working memory, measured in gigabytes (GB).

**Memory is the main number — 16 GB is the sweet spot, and 24 to 32 GB gives extra breathing room:**

| RAM | What it can run | Verdict |
|---|---|---|
| 8 GB | ~3B models only | Works, but weaker suggestions |
| 16 GB | 7–8B models (~5–6 GB) | The practical sweet spot for triage/summaries |
| 24–32 GB | 13–14B models, or the AI **and** OpenTrack on one box | Comfortable headroom |
| 64 GB+ | 30B+ models | Overkill for this |

(In the table, "B" means billion parameters — the model's size. Bigger usually means smarter but heavier.)

**What actually helps speed:**

- **Apple Silicon (a Mac mini with an M-series chip)** — Apple's own chips (named M1, M2, M4, and so on) share their memory between the main processor and the graphics, and Ollama uses that graphics power automatically. So even a small Mac mini feels quick where a plain processor-only computer merely works. It is the easiest strong option, and it is a quiet, low-power machine you can leave on all the time. **Which one to get:**
    - **New: a current M4 Mac mini** — the fastest and most efficient, and its basic model now includes 16 GB of memory. Around $599 (16 GB) or around $799 (24 GB, the more comfortable pick). The pricier M4 **Pro** has much faster memory (quicker responses) but is overkill for occasional triage and summaries.
    - **Budget: a used or refurbished M1 (2020) or M2 (2023) mini with 16 GB** — both run a 7–8B model well.
    - **Avoid any Intel Mac mini** (2018 and earlier): those do not have Apple Silicon, so they get none of the shared-memory speed advantage — they are just ordinary processor-only boxes.
    - Buy enough memory up front: on Apple Silicon the memory is soldered in and **cannot** be upgraded later, and that memory doubles as the model's working space.
- **An NVIDIA GPU** (even a modest one) — NVIDIA graphics cards support an acceleration technology (called CUDA) that Ollama loves. This is the fastest option, but it is pricier and needs more setup.
- **A processor-only mini PC** (for example an Intel or AMD box like a Beelink) — runs local models on the main processor at a few words per second. Not instant, but perfectly fine for occasional triage and summaries, and it is often a machine you **already own**. If it is the same box already running OpenTrack, just install Ollama alongside it and point `BaseUrl` at `localhost` — no second machine and no network setup needed. A modern mini PC with **16 to 24 GB** of memory is a sensible home for local AI here.

**What will not help (for text AI):**

- **Vision NPUs and "AI HAT" add-on boards** (for example a Raspberry Pi AI HAT+ with a Hailo accelerator). "NPU" stands for Neural Processing Unit — a chip built to speed up AI. These particular ones are designed for **computer-vision** (image and camera) tasks; Ollama and the underlying engine (called llama.cpp) cannot hand language work to them, so they just sit idle for OpenTrack's needs. They are great for camera projects — not for this. Put that money toward more **RAM** or toward **Apple Silicon** instead.
- **Weak built-in graphics** (for example Intel UHD graphics): do not count on that kind of graphics for acceleration; the model will just run on the main processor. That is okay for occasional use.

**Bottom line:** for personal, occasional use, a mini PC with **16 to 24 GB of RAM** is plenty — ideally one you already own. Only reach for Apple Silicon or an NVIDIA GPU if you want it to feel snappy rather than merely work.

---

## Turn it on

To switch the AI on, you put a few settings into OpenTrack's configuration. There are three places you can put them: the `appsettings.json` file, "environment variables" (settings stored in your operating system), or .NET user-secrets (a safe local store for developers). **For the API key, prefer environment variables or user-secrets, so the secret key does not get saved into source control by accident.**

The settings below are written in the `OpenTrack:Ai:...` style used by the `appsettings.json` file. If you set them as **environment variables** instead, replace each `:` (colon) with `__` (two underscores in a row). For example, `OpenTrack:Ai:Enabled` becomes `OpenTrack__Ai__Enabled=true`.

In every block below, wherever you see a value like `sk-ant-...` or `sk-...`, replace it with **your own real key** that you copied earlier — do not type the dots literally.

**Claude (Anthropic):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = anthropic
OpenTrack:Ai:ApiKey   = sk-ant-...            (your Anthropic key)
OpenTrack:Ai:Model    = claude-haiku-4-5-20251001   (fast & inexpensive)
```
(Paste your real Anthropic key, the one that starts with `sk-ant-api03-`, in place of `sk-ant-...`.)

**OpenAI (cloud):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = openai
OpenTrack:Ai:ApiKey   = sk-...                (your OpenAI key)
OpenTrack:Ai:Model    = gpt-4o-mini
```
(Paste your real OpenAI key in place of `sk-...`.)

**Local, free & private (Ollama):**
```
OpenTrack:Ai:Enabled  = true
OpenTrack:Ai:Provider = openai
OpenTrack:Ai:BaseUrl  = http://localhost:11434/v1     (or a LAN IP like
                                                       http://192.168.1.50:11434/v1
                                                       for a Pi/NAS/other machine)
OpenTrack:Ai:Model    = llama3.1
                                              (no ApiKey needed for local)
```
(If Ollama runs on the same computer as OpenTrack, keep `http://localhost:11434/v1`. If it runs on another machine, replace `192.168.1.50` with that machine's real network address.)

**Azure OpenAI / Groq / OpenRouter / other hosted:** use `Provider = openai`, set `BaseUrl` to that service's web address, set `ApiKey` to the key that service gave you, and set `Model` to a model name that service offers.

After you save the settings, **restart OpenTrack**. On the **New issue** page you will now see the **✨ Suggest with AI** button, which fills in severity, priority, and category, and proposes tags.

## Privacy & safety

- **Opt-in.** With the AI off (which is the default) or not yet configured, OpenTrack behaves exactly as it did before and makes no AI calls at all.
- **Cloud providers see the text.** When a cloud provider is turned on, asking for a suggestion sends that one issue's summary and description to that provider. Do **not** turn on a cloud provider for projects whose contents are not allowed to leave your environment — use a **local** engine (Ollama or LM Studio) instead, which keeps everything on your own machine.
- **Server-side only.** The API key is read from the server's configuration. It is never sent to your web browser and never stored in the database.
- **Best-effort.** Every AI result is only a suggestion, with a person in the loop. If an AI call ever fails, creating the issue still works completely normally — it is not affected at all.
