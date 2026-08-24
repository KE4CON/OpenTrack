# Public trouble tickets (let people report problems without an account)

## In a nutshell

OpenTrack can take problem reports from people who do **not** have an account.
You turn the feature on for one project, and OpenTrack gives you a link (and you
can turn that link into a scannable poster). Anyone who has the link can fill in
a short form and send you their problem. Each report shows up as a normal issue
in your project, and the person who sent it gets a reference number so they can
check on it later.

This is handy if you run a helpdesk, a club, or you collect reports from people
out in the field. The feature is a page that is already built into OpenTrack —
there is no separate website or extra app to install — and it is **switched off
for every project until you decide to turn it on**.

## Turn it on for a project

You do this once, inside the project you want to collect reports for. You need
the **Manager** role in that project (if you created the project, you already
have it).

1. Open the project. Look for the **Settings** button and click it.
2. On the Settings screen, find the section titled **Public trouble-ticket
   intake**. Inside that section, click the button labeled **Turn on public
   intake**.
3. A public web link now appears on the screen. It will look something like
   `http://your-opentrack-server/report/3`.

   That address is just an example. The part that says `your-opentrack-server`
   stands in for the real web address of *your* OpenTrack server — replace it
   with your actual address. For example, if your server is reached at
   `helpdesk.example.com`, your real link would be
   `http://helpdesk.example.com/report/3`. The number on the end (here it is
   `3`) is the ID number of this particular project, and OpenTrack fills that in
   for you automatically — you do not type it yourself.

4. Copy that link and share it with whoever should be able to send you reports.
   You can email it, put it on a web page, or print it (see the QR poster
   section below).

Once the feature is on, anyone who has the link can open it, fill in a short
form, and send it. On the form, the person's name and email address are
**optional** — they can leave them blank. Each thing they submit becomes a
normal issue in that project. You and your team then sort it and work on it just
like any other bug report, and it sets off the usual notifications and any
webhooks (automatic messages sent to other software) that you have set up.

## What the person filling it out sees

Here is what happens on the other end, for the person sending you a report:

- They see a simple form titled **Report a problem**. There is **no login** —
  they do not need an account or a password.
- After they press submit, the screen shows them a **reference number**, for
  example `#42`. Tell them to write this number down or keep the page open —
  they will need it to check on their report later.
- If they typed in an email address, **and** you have set your OpenTrack server
  up to send email, they will also receive an acknowledgement message by email.
- There is a separate **Check your ticket** page, found at the web address
  `/report/status` on your server (for example
  `http://helpdesk.example.com/report/status`). On that page they type in their
  reference number **and** the email address they used when they first reported
  the problem. Only then does it show them the current status of their report.
  Requiring both pieces is on purpose: a person can only see **their own**
  ticket, and the reference number by itself is not enough to look one up.

## Make a scannable QR poster

Instead of asking people to type a long web link, you can print a poster with a
Quick Response (QR) code on it — the square, scannable pattern that phones can
read with their camera. Someone points their phone camera at the code, and their
phone offers to open the report link for them. This is useful when you want to
put a sign on a wall, a piece of equipment, or a noticeboard.

To make one, take the public link from step 3 above (the
`http://your-opentrack-server/report/3` address, with your real server address
filled in) and turn it into a QR code. You can use any QR-code generator you
trust — many free ones exist online — then print the result on your poster.
When someone scans it, their phone opens the same **Report a problem** form
described above.

## Letting people reach it from outside your own network

The report page lives on your OpenTrack server, the same as the rest of
OpenTrack. Here is who can reach it, and what to do:

- **People on your local network** (the same office or building network as your
  server) can open the link right away. Nothing extra is needed.
- **People out on the public internet** (anyone, anywhere) can only reach it if
  your OpenTrack server is set up to be reachable from the public internet. You
  do this by exposing your OpenTrack server through the hosting or reverse proxy
  (a piece of software that sits in front of your server and passes web traffic
  through to it) that you already use, and by turning on HTTPS — the secure,
  encrypted version of web connections, the one that shows a padlock in the
  browser. See the separate network-access guide for how to do that.

There is no second app to install and no separate server to run — it is all part
of the OpenTrack server you already have.

## Keeping out spam

Because this form is open to the public, automated junk programs (bots) will try
to abuse it. OpenTrack has several built-in protections already working for you:

- **Off by default, one project at a time.** Nothing is public until you
  personally turn it on for a project.
- **Rate limiting.** Any single source can only submit a small handful of
  reports within a few minutes. This stops one bot from flooding you.
- **A hidden honeypot field.** This is an invisible box on the form that a real
  person never sees and never fills in, but automated bots do fill in. When
  OpenTrack sees that box filled, it quietly ignores that submission.
- **Length limits.** Every box on the form has a maximum length, so nobody can
  paste in an enormous amount of text.

If you expect a lot of public traffic, you can add extra protection. Put a
CAPTCHA (a "prove you're human" challenge, such as clicking pictures or checking
a box) or your reverse proxy's own bot protection in front of the `/report/...`
web addresses as well.

## Email-to-ticket (let people report a problem by email)

### In a nutshell

On top of the web form above, OpenTrack can turn **an incoming email** into a
trouble ticket. Someone fills in the contact form on your website, that form
sends an email to a special "tickets" address, a mail service reads that email
and hands it to OpenTrack, and OpenTrack files it as a ticket in the right
project. The person never sees OpenTrack at all — they just use the contact form
they already trust on your website.

This is **off until you switch it on**, and switching it on takes three things:
a shared password (called a *secret*), a project that has a **Key** and already
accepts public tickets, and a mail service that forwards received email to
OpenTrack. This section walks through all three in plain steps.

### How the whole thing flows

Here is the journey of one report, from the person on your website to a ticket
in OpenTrack:

1. A visitor fills in the **contact form on your website** and presses send.
2. Your website emails that message to an **intake address** you picked — for
   example `tickets+WEB@yourdomain.com`.
3. A **mail inbound-parse service** — the piece that receives email and can hand
   it onward as a web request — catches that email. Common ones are **Mailgun**,
   **SendGrid Inbound Parse**, and **ImprovMX** (paired with a small webhook). A
   webhook is just "when an email arrives, send its contents to this web
   address."
4. That service **POSTs** (sends) the parsed email to OpenTrack's email-intake
   web address, `POST /intake/email`, including your shared secret.
5. OpenTrack reads the address it was sent to, works out which project it belongs
   to from the project **Key**, and **creates the ticket** — exactly as if the
   person had used the public "Report a problem" form.

The sender's name and email become the ticket's contact details, the email
**subject** becomes the ticket's title, and the email **body** becomes the
ticket's description. On success OpenTrack hands back the new ticket's reference
number (the friendly form like `WEB-42` when the project has a Key). The team
gets the usual notifications, and any webhooks you set up fire, just like a
web-form report.

### Routing to the right project by its Key

You do not need a separate OpenTrack for each product. One OpenTrack can take
email tickets for many projects, and the **address** decides which project a
message lands in. OpenTrack looks at the project's **Key** — the short code like
`WEB` you can give a project (see the *Friendly ticket numbers* material and
the project Settings). There are two ways to point an address at a Key:

- **Sub-addressing (recommended):** `tickets+WEB@yourdomain.com` routes to the
  project whose Key is `WEB`. The part after the `+` is the Key. This lets you
  use **one real mailbox** (`tickets@yourdomain.com`) for every project — the
  `+WEB`, `+STORE`, `+SHOP` and so on are just labels on the same mailbox.
- **Whole local part:** `web@yourdomain.com` (the bit before the `@` is exactly
  the Key) also routes to the `WEB` project. Use this when you would rather have
  one clean address per product.

The match is not case-sensitive, so `tickets+web@` and `tickets+WEB@` both find
the `WEB` project.

> **The project must already accept public tickets.** Email intake can only reach
> a project that has **public intake turned on** (the *Turn it on for a project*
> steps near the top of this guide). If a project's public intake is off, email
> to its address is refused. Turning email intake on cannot secretly open a
> project that wasn't already accepting public reports.

### A ready-to-copy website contact form

If your website doesn't already have a contact form, here is a bare one you can
drop into a page. It simply emails whatever the visitor types to your intake
address. Replace `tickets+WEB@yourdomain.com` with your real intake address, and
note that a plain `mailto:` form like this relies on the visitor's own email
program — most real sites instead let their hosting or form service send the
email. Use whichever your website already supports; the only thing OpenTrack
cares about is that an email eventually reaches your intake address.

```html
<!-- Simple "report a problem" form that emails your OpenTrack intake address. -->
<form action="mailto:tickets+WEB@yourdomain.com" method="post" enctype="text/plain">
  <label>Your name<br><input type="text" name="name"></label><br>
  <label>Your email<br><input type="email" name="email"></label><br>
  <label>Summary (what's wrong)<br><input type="text" name="subject"></label><br>
  <label>Details<br><textarea name="body"></textarea></label><br>
  <button type="submit">Send report</button>
</form>
```

Most website form builders (the "contact us" widget your site host provides) can
be told to send submissions to an email address instead of, or as well as,
storing them. Point that address at `tickets+WEB@yourdomain.com` and you are
done on the website side.

### Turn it on: the OpenTrack server settings

Email intake is controlled by **one setting on the server**: a shared secret. As
long as it is blank, the whole feature is off and the `/intake/email` address
politely pretends not to exist. Set it, and the feature turns on.

1. Pick a long, random secret — treat it like a password. A password manager can
   generate one; something like `9f3c1a7b5e2d4680bb11` is fine.
2. Set it as the environment variable **`OpenTrack__EmailIntake__Secret`** on the
   server (note the **double underscores**). How you set an environment variable
   depends on how you run OpenTrack:
   - **Docker / docker-compose:** add it under the service's `environment:` list,
     e.g. `- OpenTrack__EmailIntake__Secret=9f3c1a7b5e2d4680bb11`.
   - **Windows service / plain run:** set a system environment variable named
     `OpenTrack__EmailIntake__Secret` with your secret as its value.
   - **appsettings.json (alternative):** add a section
     `"OpenTrack": { "EmailIntake": { "Secret": "9f3c1a7b5e2d4680bb11" } }`.
     The environment variable is preferred so the secret isn't sitting in a file.
3. Restart OpenTrack so it picks up the new value.
4. Make sure the project you want tickets to land in has a **Key** (set it under
   the project's **Settings**) and has **public intake turned on**.

That's the whole server side. There is no new page to open and nothing to click
inside OpenTrack — the presence of the secret is the on-switch.

### Tell your mail service how to POST to OpenTrack

Your inbound-parse mail service (Mailgun, SendGrid Inbound Parse, ImprovMX +
webhook, or your own small forwarder) needs to be told to send received email to
OpenTrack. In that service's "inbound route" or "webhook" settings, point it at:

```
POST http://192.168.1.50:5035/intake/email
```

Replace `http://192.168.1.50:5035` with your real OpenTrack web address. If your
mail service lives out on the internet (Mailgun and SendGrid do), OpenTrack must
be reachable from the internet too — see the *Letting people reach it from
outside your own network* section above and the network-access guide.

The POST is an ordinary web form submission (`multipart/form-data` or
`application/x-www-form-urlencoded`). OpenTrack accepts the field names the
common services already use, so you usually don't have to rename anything:

| What OpenTrack needs | Field names it accepts (any one of them) |
|----------------------|------------------------------------------|
| Who it was sent to (picks the project) | `recipient`, `to`, `To` |
| Who sent it | `from`, `sender`, `From` |
| The subject (becomes the ticket title) | `subject`, `Subject` |
| The body (becomes the ticket description) | `body-plain`, `stripped-text`, `text`, `body`, `message` |

**The secret must ride along with every POST**, in one of two ways:

- an HTTP header **`X-OpenTrack-Secret: 9f3c1a7b5e2d4680bb11`**, or
- a form field named **`secret`** with the secret as its value.

If the secret is missing or wrong, OpenTrack rejects the POST with
"Unauthorized" and no ticket is created.

Here is what a hand-made test POST looks like from the command line, so you can
prove the plumbing works before wiring up the mail service. Substitute your real
server address and secret:

```bash
curl -X POST http://192.168.1.50:5035/intake/email \
  -H "X-OpenTrack-Secret: 9f3c1a7b5e2d4680bb11" \
  -F "recipient=tickets+WEB@yourdomain.com" \
  -F "from=Jane Field <jane@example.com>" \
  -F "subject=Radio won't key up on 2 meters" \
  -F "body-plain=The transmit light comes on but nothing goes out."
```

A success looks like `{"reference": 42}` and a new ticket appears in the `WEB`
project.

### The simpler alternative: POST straight to OpenTrack (no email at all)

If your website can reach the OpenTrack server directly over the network, you can
skip email entirely. Have your website's contact form POST straight to the
public report endpoint that the "Report a problem" page itself uses:

```
POST http://192.168.1.50:5035/report/3/submit
```

The `3` is the project's ID number (the same number you see in the public link,
`/report/3`). Send ordinary form fields **`name`**, **`email`**, **`title`**, and
**`description`**. The ticket is created instantly — there's no mail service in
the middle, no secret to manage, and no waiting for email to be delivered. This
path is the same one the built-in form uses, so it's rate-limited and honeypot-
protected already. Choose this when your site and OpenTrack can talk to each
other directly; choose email-to-ticket when they can't, or when you'd rather keep
everything flowing through email.

### Security notes

- **Off by default.** Until you set `OpenTrack__EmailIntake__Secret`, the
  `/intake/email` address returns "not found" and creates nothing.
- **Shared secret, checked safely.** Every POST must present the secret (header
  or `secret` field). OpenTrack compares it using a constant-time check, so an
  attacker can't learn the secret by measuring how fast rejections come back.
  Keep the secret private; anyone who has it can file tickets. If it leaks,
  change the environment variable and restart.
- **Public-intake gate still applies.** Email can only create tickets in a
  project that already has public intake on. It cannot reach private projects.
- **Rate-limited.** Email intake uses the same per-address rate limit as the web
  form, so a flood of email can't hammer OpenTrack.
- **Same length caps.** The subject and body are trimmed to the same maximum
  lengths as the web form, so an enormous email can't store an enormous ticket.

### Troubleshooting

- **Every POST comes back "not found" (404).** The feature is off. Set
  `OpenTrack__EmailIntake__Secret` on the server and restart. Remember the
  **double underscores** in the name.
- **Every POST comes back "Unauthorized" (401).** The secret is missing or
  doesn't match. Check that your mail service sends the `X-OpenTrack-Secret`
  header (or a `secret` field) and that its value exactly matches the server's
  `OpenTrack__EmailIntake__Secret` — no extra spaces, correct upper/lower case.
- **"Could not tell which project this email is for."** OpenTrack couldn't find a
  project Key in the address it was sent to. Use `tickets+WEB@yourdomain.com`
  (Key after the `+`) or `web@yourdomain.com` (Key is the whole name before the
  `@`), and make sure a project actually has that Key.
- **"No project is accepting email tickets for that address."** A project with
  that Key exists but its **public intake is off**, or no project has that Key.
  Turn public intake on for the project (see the top of this guide) and confirm
  its Key.
- **Ticket created but the title or body is empty.** Your mail service is using a
  field name OpenTrack doesn't recognize. Compare its outgoing fields against the
  table above and, if needed, configure the service to send `subject` and
  `body-plain` (or one of the other accepted names).
- **Nothing arrives and there's no error.** Check the mail service's own logs to
  confirm the email reached it and that its webhook actually fired to your
  `/intake/email` address. If OpenTrack is behind a reverse proxy or firewall,
  confirm the mail service can reach it from the internet.
