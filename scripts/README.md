# scripts

Small helper scripts for OpenTrack maintenance.

## md2pdf.py — Markdown → PDF

Renders one of our Markdown docs to a PDF (used e.g. for `docs/guides/AI_ASSIST.pdf`).
Handles the subset of Markdown our docs use: `#`/`##`/`###` headings, `---`
rules, bullet/numbered lists, fenced ```` ``` ```` code blocks, pipe tables, and
inline `**bold**`, `` `code` ``, and `[links](url)`.

**Requires:** Python 3 with `reportlab` (`pip install reportlab`). No other tools
(no LaTeX, no pandoc, no system libraries) — so it runs anywhere Python does.

```bash
python scripts/md2pdf.py docs/guides/AI_ASSIST.md docs/guides/AI_ASSIST.pdf
```

Re-run it after editing the Markdown to refresh the PDF.
