# Using OpenTrack from a tablet or another computer on your network

## In a nutshell

You can open OpenTrack on an iPad, an Android tablet, a phone, or another
computer without installing anything special. Start OpenTrack in a special
"network" mode on the computer that runs it, find that computer's network
number (its address), then type that number plus `:5035` into the tablet's web
browser. If you want, you can then add OpenTrack to the tablet's home screen so
it opens like a real app.

---

## What this lets you do

OpenTrack's web version is just a normal website. When it runs on one computer,
any other device on the **same home or office network** — an iPad, an Android
tablet, a laptop, a phone — can open it in a web browser and use it. There is
nothing to install on the tablet, and you do not need an internet connection.
Everything stays on your own local network (the Wi-Fi and router in your home or
office). The technical name for that local network is a **Local Area Network
(LAN)**.

This is handy for the **bug-hunt checklist**: prop the tablet next to you, walk
through the list, and tap **Pass / Fail / N/A** on each item. A **Fail** can
create a linked issue that you deal with later on your desktop.

---

## Step 1 — Start OpenTrack so the network can reach it

Go to the computer that will run OpenTrack (the "host" computer). Open a
terminal (a text command window) and go to the `src/OpenTrack.Web` folder. Then
type this command exactly and press Enter:

```bash
dotnet run --launch-profile network
```

This starts OpenTrack and makes it available to **every device on your network**,
using port number **5035**. (A port number is like an apartment number: the
network address gets you to the building, and the port gets you to the right
door inside it.) Behind the scenes, the `network` profile does this by listening
on the special address `0.0.0.0`, which means "accept connections from anywhere
on the network," instead of only `localhost`, which means "accept connections
only from this same computer."

Leave that terminal window open the whole time you are using OpenTrack. If you
close it, OpenTrack stops running.

> The normal `http` and `https` profiles only accept connections from the
> **same** computer. That is exactly why there is a separate `network` profile
> for reaching OpenTrack from other devices.

## Step 2 — Find the host computer's network address

Now you need the host computer's local network address. This is a number that
looks like `192.168.x.x` — for example `192.168.1.50`. Every device on your
network has one. The technical name for it is an **Internet Protocol address
(IP address)**.

Find it on the host computer using whichever of these matches its operating
system:

- **Windows:** Open a terminal and type `ipconfig`, then press Enter. Look for
  the line labeled **IPv4 Address** under your active Wi-Fi or Ethernet adapter.
  The number next to it is the address you want.
- **macOS (an Apple Mac):** Open a terminal and type `ipconfig getifaddr en0`
  (that gets the Wi-Fi address), then press Enter. Or click the Apple menu →
  **System Settings** → **Network** and read it there.
- **Linux:** Open a terminal and type `hostname -I`, then press Enter. Use the
  first address it shows.

Write that number down. In the next step, wherever you see the example
`192.168.1.50`, you will type **your own** number instead.

## Step 3 — Open it on the tablet

Pick up the tablet. Open its web browser (Safari on an iPad, Chrome on an
Android tablet, or whatever browser you normally use). In the address bar at the
top, type your host computer's address followed by `:5035`, like this:

```
http://192.168.1.50:5035
```

Replace `192.168.1.50` with the real number you wrote down in Step 2. So if your
host computer's IPv4 Address was `192.168.1.42`, you would type
`http://192.168.1.42:5035` instead. Do not forget the `:5035` at the end — that
is the port number, and OpenTrack will not answer without it.

Press Go (or Enter). OpenTrack should appear. Sign in with your normal OpenTrack
account and you are in. To make it easy to return, bookmark the page or add it
to the home screen (the next section explains how).

---

## Install it as an app (and use the checklist offline)

Once you can open OpenTrack in the tablet's browser, you can install it so it
behaves like a real app. This works because OpenTrack is a **Progressive Web App
(PWA)** — a website that a phone or tablet can save and run like an installed
program. Here is how to install it:

- **iPad (using the Safari browser):** Tap the **Share** button (the square with
  an arrow pointing up, usually at the top of the screen). In the menu that
  slides up, scroll down and tap **Add to Home Screen**. Then tap **Add** in the
  top corner to confirm.
- **Android tablet (using the Chrome browser):** Tap the **⋮** menu (three dots
  stacked vertically, usually in the top corner). Tap **Install app**. If you do
  not see "Install app," tap **Add to Home screen** instead — it does the same
  thing.

After that, OpenTrack launches full-screen straight from your home screen, with
no browser buttons or address bar around it — just like an app you downloaded
from a store.

Pages you have already opened stay viewable **offline** (without a network or
internet connection). That is handy for the **bug-hunt checklist**: open the
checklist while you are still on the network, then walk somewhere with no signal
and keep tapping **Pass / Fail / N/A**. Those taps are saved right on the device
and **sync automatically the next time you are back online** (a "pending sync"
note stays on screen until they finish saving to the host computer). One thing
that still needs a connection: creating an issue from a failure. It will be
ready to go as soon as you are back on the network.

## If the tablet can't connect

If OpenTrack will not open on the tablet, check these one at a time:

- **Are they on the same network?** The tablet and the host computer must be on
  the same Wi-Fi or router. Guest networks often block devices from talking to
  each other, so use your main network, not a guest one.
- **Did the firewall block it?** The first time you start the `network` profile,
  Windows may pop up a box asking whether to allow the app through the firewall.
  Choose to allow it on **private** networks. If you missed that box, open
  **Windows Defender Firewall** and allow `dotnet` (or port 5035) for private
  networks.
- **Did you type the port?** Make sure you typed `:5035` right after the address,
  with nothing missing (for example `http://192.168.1.50:5035`).

## A note on security

The `network` profile serves plain **Hypertext Transfer Protocol (HTTP)**, the
ordinary unencrypted way web pages travel. That is perfectly fine on a
**trusted home or office network**. On such a network, your password and login
are only as private as the network itself, which is normally fine at home.

But if you ever make OpenTrack reachable from **outside** a trusted network — for
example by port-forwarding it so people on the internet can reach it — turn on
encryption first. Do that by setting `OpenTrack:RequireHttps=true` in the
configuration and serving OpenTrack behind a real security certificate. That
switches it to **Hypertext Transfer Protocol Secure (HTTPS)**, the encrypted
version. Without it, your username and password would travel across the internet
in plain readable text, where others could capture them.

For everyday use on your own network with a tablet, plain HTTP is the normal,
expected setup, and you do not need to do anything extra.
