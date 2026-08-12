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
