# Using OpenTrack from a tablet or another computer on your network

OpenTrack's web version is a normal website. If you run it on one computer, any
other device on the **same home/office network** — an iPad, an Android tablet, a
laptop, a phone — can open it in a browser and use it. There's nothing to
install on the tablet, and no internet connection is required: everything stays
on your local network.

This is handy for the **bug-hunt checklist**: prop the tablet next to you, walk
through the list, and tap **Pass / Fail / N/A** on each item. A **Fail** can
create a linked issue you triage later on your desktop.

---

## Step 1 — Start OpenTrack so the network can reach it

On the computer that will host it, from the `src/OpenTrack.Web` folder run:

```bash
dotnet run --launch-profile network
```

That serves OpenTrack on **all network connections** at port **5035** (the
`network` profile does this by listening on `0.0.0.0` instead of only
`localhost`). Leave that window open while you're using it.

> The normal `http`/`https` profiles only accept connections from the *same*
> computer — that's why there's a separate `network` profile for this.

## Step 2 — Find the host computer's network address

You need the host computer's local IP address (it looks like `192.168.x.x`).

- **Windows:** open a terminal and run `ipconfig`, then read the **IPv4 Address**
  under your active Wi-Fi/Ethernet adapter.
- **macOS:** run `ipconfig getifaddr en0` (Wi-Fi) or check System Settings →
  Network.
- **Linux:** run `hostname -I` and use the first address.

## Step 3 — Open it on the tablet

On the tablet's browser (Safari, Chrome, etc.), go to:

```
http://YOUR-COMPUTERS-IP:5035
```

For example: `http://192.168.1.42:5035`. Sign in with your normal OpenTrack
account and you're in. Bookmark it / add it to the home screen for one-tap access.

---

## If the tablet can't connect

- **Same network?** The tablet and the host computer must be on the same Wi-Fi /
  router. Guest networks often block device-to-device traffic — use the main one.
- **Firewall prompt.** The first time you start the `network` profile, Windows may
  ask whether to allow the app through the firewall — allow it on **private**
  networks. If you missed the prompt, allow `dotnet` (or port 5035) for private
  networks in Windows Defender Firewall.
- **Right port?** Make sure you typed `:5035` after the IP address.

## A note on security

The `network` profile serves plain **HTTP**, which is fine on a **trusted home or
office network**. Your password and login are only as private as your local
network. If you ever expose OpenTrack **beyond** a trusted network (e.g. port-
forwarding it to the internet), turn on HTTPS first by setting
`OpenTrack:RequireHttps=true` in configuration and serving it behind a real
certificate — otherwise credentials would travel in the clear. For everyday
same-network tablet use, plain HTTP is the normal, expected setup.
