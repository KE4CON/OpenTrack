# Importing your data from MantisBT

Already using MantisBT and want to switch to OpenTrack? You can bring your
existing issues across in a few minutes. OpenTrack reads MantisBT's own XML
export, so you don't need database access — just the MantisBT web interface.

## What comes across

- **Projects** — created automatically from the projects in your export (they
  start **private**; you can make them public afterward if you want).
- **Issues** — summary, description, steps to reproduce, and additional
  information, plus **status, severity, priority, and reproducibility**, which
  map across exactly (OpenTrack uses the same values MantisBT does).
- **Categories**, **tags**, and **notes** attached to each issue.
- **Original dates** — an issue keeps its MantisBT submit and last-updated dates.
- **Public/private** flags on issues and notes are preserved.

**People:** if a MantisBT username matches one of your OpenTrack accounts, that
person is set as the reporter/assignee/note author. If there's no matching
account, the item is attributed to you (the person doing the import) and the
original MantisBT name is kept in the text, so nothing is lost.

## Step 1 — Export from MantisBT

1. Sign in to MantisBT and go to **View Issues**.
2. Filter to the issues you want (or show all).
3. Use **Export Issues** (the XML export — sometimes shown as an XML icon or
   under the "Import/Export Issues" plugin). Save the `.xml` file.

## Step 2 — Import into OpenTrack

1. In OpenTrack (the **web version**, in a browser), open **Backup & export**
   from the menu. You'll need the **Manager** role.
2. Under **Import from MantisBT**, choose your `.xml` file and click **Import**.
3. You'll see a summary — how many projects, issues, notes, and tags came in.

## Good to know

- **Run it once per export.** Importing the same file twice will add the issues
  a second time (there's no automatic duplicate detection yet), so import each
  export a single time.
- **Large exports** are fine, up to 25 MB per file. If yours is bigger, export in
  batches (e.g. one project at a time) and import each file.
- If OpenTrack says the file "didn't look like a MantisBT XML export," make sure
  you used MantisBT's **XML** export (not CSV or a printed page).
