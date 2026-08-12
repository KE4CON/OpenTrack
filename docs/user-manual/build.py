"""
OpenTrack User Manual — builder.

Markdown is the living source of truth; a styled .docx (navy+gold, matching the Installation Guide and
Programming Guide) is generated from the same chapter JSON. Chapters are validated JSON under
./chapters/*.json, each {"order": N, "title", "subtitle", "in_this_chapter"[], "blocks"[]}. Blocks reuse
the shared renderer (h1/h2/p/steps/bullets/callout/screenshot/code/table).

Run:  python build.py     -> writes ../guides/OpenTrack_User_Manual.docx and ./OpenTrack_User_Manual.md
Env:  MANUAL_OUT overrides the .docx path (used when Word has the file locked).
"""
import os
import sys
import glob
import json
import re
import datetime

HERE = os.path.dirname(__file__)
sys.path.insert(0, os.path.abspath(os.path.join(HERE, "..", "..", "docs_generators")))
import style as S  # noqa: E402


def load_chapters():
    out = []
    for path in sorted(glob.glob(os.path.join(HERE, "chapters", "*.json"))):
        with open(path, encoding="utf-8") as f:
            raw = json.load(f)
        ch = raw.get("chapter", raw)
        order = int(raw.get("order", ch.get("order", 999)))
        out.append((order, ch))
    out.sort(key=lambda x: x[0])
    return out


# ---- Markdown emitter (source of truth) -----------------------------------
def _md_inline(text):
    return re.sub(r"__(.+?)__", r"*\1*", str(text))


def _md_blocks(blocks):
    lines = []
    for blk in blocks:
        if not isinstance(blk, dict):
            continue
        if "h1" in blk:
            lines.append(f"\n## {blk['h1']}\n")
        elif "h2" in blk:
            lines.append(f"\n### {blk['h2']}\n")
        elif "p" in blk:
            lines.append(_md_inline(blk["p"]) + "\n")
        elif "steps" in blk:
            lines += [f"{i}. {_md_inline(s)}" for i, s in enumerate(blk["steps"], 1)]
            lines.append("")
        elif "bullets" in blk:
            lines += [f"- {_md_inline(b)}" for b in blk["bullets"]]
            lines.append("")
        elif "callout" in blk:
            c = blk["callout"] if isinstance(blk["callout"], dict) else {}
            label = c.get("label", c.get("kind", "NOTE").upper())
            lines.append(f"> **{label}** — {_md_inline(c.get('text', ''))}\n")
        elif "screenshot" in blk:
            lines.append(f"> _[Figure: {blk['screenshot']}]_\n")
        elif "code" in blk:
            code = blk["code"]
            code = "\n".join(code) if isinstance(code, list) else str(code)
            lines.append("```\n" + code + "\n```\n")
        elif "table" in blk:
            t = blk["table"] if isinstance(blk["table"], dict) else {}
            headers = [str(h) for h in t.get("headers", [])]
            rows = t.get("rows", [])
            if headers:
                lines.append("| " + " | ".join(headers) + " |")
                lines.append("| " + " | ".join(["---"] * len(headers)) + " |")
                for r in rows:
                    lines.append("| " + " | ".join(_md_inline(c) for c in r) + " |")
                lines.append("")
    return "\n".join(lines)


def to_markdown(chapters):
    parts = [
        "# OpenTrack User Manual\n",
        "*Every feature, explained and step by step — in plain language.*\n",
        f"*Generated {datetime.date.today().strftime('%B %d, %Y')} · Markdown is the living source of truth.*\n",
        "\n---\n",
    ]
    for number, (_order, ch) in enumerate(chapters, 1):
        parts.append(f"\n# {number}. {ch.get('title', 'Untitled')}\n")
        if ch.get("subtitle"):
            parts.append(f"*{_md_inline(ch['subtitle'])}*\n")
        parts.append(_md_blocks(ch.get("blocks", [])))
    return "\n".join(parts)


# ---- styled .docx (generated artifact) ------------------------------------
def build_docx(chapters):
    doc = S.new_document(
        header_title="OpenTrack — User Manual",
        header_sub="Self-hosted issue & bug tracking",
        footer_left="OpenTrack  ·  Open-source (AGPL v3)  ·  KE4CON",
    )
    S.cover(
        doc,
        kicker="OPENTRACK",
        big_title="OpenTrack",
        subtitle="Self-Hosted Issue Tracker",
        doc_kind="USER MANUAL",
        version="v1.0",
        tagline="Every feature, explained and step by step — from your first issue to SLAs, automation, Git, AI, and the field.",
        author="James Rospopo  ·  KE4CON",
        date_str=datetime.date.today().strftime("%B %d, %Y"),
    )
    S.section_title(doc, "Contents")
    S.toc(doc)
    for number, (_order, ch) in enumerate(chapters, 1):
        S.render_chapter(doc, ch, number)
    out = os.environ.get("MANUAL_OUT") or os.path.join(
        HERE, "..", "guides", "OpenTrack_User_Manual.docx")
    out = os.path.abspath(out)
    doc.save(out)
    return out


def main():
    chapters = load_chapters()
    if not chapters:
        print("No chapters found in ./chapters/*.json — nothing to build yet.")
        return
    md = to_markdown(chapters)
    md_path = os.path.abspath(os.path.join(HERE, "OpenTrack_User_Manual.md"))
    with open(md_path, "w", encoding="utf-8") as f:
        f.write(md)
    docx_path = build_docx(chapters)
    print(f"OK — {len(chapters)} chapter(s)")
    print(f"  Markdown: {md_path}")
    print(f"  Word:     {docx_path}")


if __name__ == "__main__":
    main()
