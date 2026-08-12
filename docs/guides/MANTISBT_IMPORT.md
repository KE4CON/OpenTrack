# Importing your data from MantisBT

## In a nutshell

MantisBT is a popular older bug-tracking program (a tool for keeping a list of software problems, or "issues," and following them until they're fixed). If you've been using it and want to move to OpenTrack, you can bring all your existing issues over in just a few minutes. First you save a copy of your issues out of MantisBT as a file, then you load that file into OpenTrack. That's the whole job — two short steps, explained below in full detail.

You don't need any special database (a program's behind-the-scenes storage) access or technical know-how. Everything is done through the normal web pages you already use.

## What comes across

When you import, OpenTrack copies over the following. You don't have to do anything to make this happen — it all comes automatically:

- **Projects** — A "project" is a container that groups related issues together. OpenTrack creates these automatically from the projects in your file. They start out **private** (only you and people you invite can see them). You can switch any of them to public later if you want.
- **Issues** — Each issue's summary (its short title), description, steps to reproduce (the actions that trigger the problem), and additional information all come across. So do its **status** (where it is in the process — for example, new, in progress, or resolved), **severity** (how serious it is), **priority** (how urgent it is), and **reproducibility** (how reliably the problem can be repeated). These four values map across exactly, because OpenTrack uses the same values MantisBT does.
- **Categories, tags, and notes** — Categories and tags are labels used to sort and group issues. Notes are the comments and updates people added to an issue over time. All of these come across, attached to each issue just like before.
- **Original dates** — Each issue keeps its original MantisBT submit date (when it was first created) and last-updated date (when it was last changed). These are not reset to today.
- **Public/private flags** — If an issue or a note was marked public or private in MantisBT, that setting is kept exactly as it was.

**About people (who reported and who is assigned to each issue):** OpenTrack tries to match each MantisBT username to an existing OpenTrack account. A username is simply the name a person signs in with. If a MantisBT username matches one of your OpenTrack accounts, that person is set as the reporter, assignee, or note author, just as you'd expect. If there is no matching account, the item is instead attributed to you — the person doing the import — and the original MantisBT name is kept inside the text so nothing is ever lost.

## Step 1 — Save your issues out of MantisBT (the "export")

"Export" just means saving a copy of your data to a file so you can move it somewhere else. Here's exactly how:

1. Sign in to MantisBT the way you normally do. Once you're in, click **View Issues** in the menu. This shows your list of issues.
2. Decide which issues you want to bring over. You can use the filter controls on that page to narrow the list down to certain issues, or you can leave it showing all of them — whatever you want to move.
3. Now save them to a file. Look for **Export Issues**. Depending on your version of MantisBT, this might appear as a button, as a small XML icon, or as a choice under a plugin (an add-on feature) named "Import/Export Issues." The important thing is that you're using the **XML** export. XML is just a plain text file format that programs use to hand data to each other — you don't need to open or understand it.
4. When you click it, your browser will download an `.xml` file (a file whose name ends in `.xml`). Note where it saves — usually your **Downloads** folder. You'll pick this same file in Step 2.

## Step 2 — Load the file into OpenTrack (the "import")

"Import" just means loading that saved file into OpenTrack. Here's exactly how:

1. Open OpenTrack in your web browser (the **web version**, not any separate app). Sign in, then open the menu and click **Backup & export**. Note: you must have the **Manager** role (a permission level) on your account to do this. If you don't see this option, ask whoever runs your OpenTrack to give you the Manager role.
2. On that page, find the section named **Import from MantisBT**. Click the button there to choose a file, and pick the `.xml` file you saved in Step 1 (the one from your Downloads folder, or wherever it landed). Then click **Import**.
3. Wait a moment while it works. When it's done, OpenTrack shows you a summary telling you how many projects, issues, notes, and tags were brought in. Read this to confirm everything arrived.

## Good to know

A few things worth knowing before and after you import:

- **Import each file only once.** OpenTrack does not yet check for duplicates. If you import the same file twice, every issue in it will be added a second time, leaving you with two copies of everything. So load each exported file a single time.
- **Big files are fine — up to 25 MB (megabytes) each.** Most exports are well under this. If yours happens to be larger than 25 MB, split it up: go back to Step 1 and export in smaller batches — for example, one project at a time — then import each of those smaller files separately (once each).
- **If OpenTrack rejects the file:** If you see a message saying the file "didn't look like a MantisBT XML export," it almost always means the wrong kind of file was saved. Go back to Step 1 and make sure you used MantisBT's **XML** export — not a CSV file (a spreadsheet-style format) and not a printed or printer-friendly page.
