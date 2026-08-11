#!/usr/bin/env python
"""Minimal, dependency-light Markdown -> PDF for OpenTrack docs (reportlab only).

Handles the subset used by our docs: #/##/### headings, --- rules, bullet and
ordered lists, fenced ``` code blocks, pipe tables, and inline **bold**, `code`,
and [text](url) links. Not a general Markdown engine -- just enough, done cleanly.
"""
import html
import re
import sys

from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import LETTER
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.platypus import (
    HRFlowable, ListFlowable, ListItem, Paragraph, Preformatted,
    SimpleDocTemplate, Spacer, Table, TableStyle,
)

ACCENT = colors.HexColor("#2563eb")
CODE_BG = colors.HexColor("#f3f4f6")
BORDER = colors.HexColor("#d1d5db")
HEAD_BG = colors.HexColor("#eef2ff")

styles = getSampleStyleSheet()
BODY = ParagraphStyle("Body", parent=styles["Normal"], fontSize=10.5, leading=15,
                      spaceAfter=6, alignment=TA_LEFT)
H1 = ParagraphStyle("H1", parent=styles["Heading1"], fontSize=20, leading=24,
                    textColor=ACCENT, spaceBefore=6, spaceAfter=10)
H2 = ParagraphStyle("H2", parent=styles["Heading2"], fontSize=15, leading=19,
                    textColor=colors.HexColor("#111827"), spaceBefore=14, spaceAfter=6)
H3 = ParagraphStyle("H3", parent=styles["Heading3"], fontSize=12.5, leading=16,
                    textColor=colors.HexColor("#111827"), spaceBefore=10, spaceAfter=4)
CELL = ParagraphStyle("Cell", parent=BODY, fontSize=9.5, leading=13, spaceAfter=0)
CELLH = ParagraphStyle("CellH", parent=CELL, textColor=colors.white, fontName="Helvetica-Bold")
CODE = ParagraphStyle("Code", parent=styles["Code"], fontSize=9, leading=12,
                      backColor=CODE_BG, borderPadding=6, leftIndent=4, spaceAfter=8)

LINK = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")
BOLD = re.compile(r"\*\*([^*]+)\*\*")
CODE_SPAN = re.compile(r"`([^`]+)`")


def inline(text: str) -> str:
    """Convert inline markdown to reportlab mini-HTML (escape first)."""
    # Protect code spans from escaping their contents oddly: escape whole string,
    # then re-apply markup on the escaped text (our markers survive escaping).
    t = html.escape(text)
    t = CODE_SPAN.sub(lambda m: f'<font face="Courier" size="9" backColor="#f3f4f6">{m.group(1)}</font>', t)
    t = BOLD.sub(lambda m: f"<b>{m.group(1)}</b>", t)
    t = LINK.sub(lambda m: f'<link href="{html.unescape(m.group(2))}" color="#2563eb"><u>{m.group(1)}</u></link>', t)
    return t


def parse(md: str):
    lines = md.splitlines()
    flow = []
    i = 0
    n = len(lines)
    while i < n:
        line = lines[i]

        # Fenced code block
        if line.strip().startswith("```"):
            i += 1
            buf = []
            while i < n and not lines[i].strip().startswith("```"):
                buf.append(lines[i])
                i += 1
            i += 1  # skip closing fence
            flow.append(Preformatted("\n".join(buf) if buf else " ", CODE))
            continue

        # Horizontal rule
        if line.strip() == "---":
            flow.append(Spacer(1, 4))
            flow.append(HRFlowable(width="100%", color=BORDER, spaceAfter=8))
            i += 1
            continue

        # Table (a header row followed by a |---| separator)
        if line.lstrip().startswith("|") and i + 1 < n and re.match(r"^\s*\|?[\s:|-]+\|?\s*$", lines[i + 1]) and "-" in lines[i + 1]:
            rows = []
            while i < n and lines[i].lstrip().startswith("|"):
                rows.append(lines[i])
                i += 1
            flow.append(build_table(rows))
            flow.append(Spacer(1, 6))
            continue

        # Headings
        if line.startswith("### "):
            flow.append(Paragraph(inline(line[4:]), H3)); i += 1; continue
        if line.startswith("## "):
            flow.append(Paragraph(inline(line[3:]), H2)); i += 1; continue
        if line.startswith("# "):
            flow.append(Paragraph(inline(line[2:]), H1)); i += 1; continue

        # Bullet list
        if re.match(r"^\s*[-*] ", line):
            items = []
            while i < n and re.match(r"^\s*[-*] ", lines[i]):
                items.append(Paragraph(inline(re.sub(r"^\s*[-*] ", "", lines[i])), BODY))
                i += 1
            flow.append(ListFlowable([ListItem(it, leftIndent=14) for it in items],
                                     bulletType="bullet", start="•", leftIndent=12))
            flow.append(Spacer(1, 4))
            continue

        # Ordered list
        if re.match(r"^\s*\d+\. ", line):
            items = []
            while i < n and re.match(r"^\s*\d+\. ", lines[i]):
                items.append(Paragraph(inline(re.sub(r"^\s*\d+\. ", "", lines[i])), BODY))
                i += 1
            flow.append(ListFlowable([ListItem(it, leftIndent=16) for it in items],
                                     bulletType="1", leftIndent=14))
            flow.append(Spacer(1, 4))
            continue

        # Blank line
        if not line.strip():
            i += 1
            continue

        # Paragraph (gather until blank / block start)
        buf = [line]
        i += 1
        while i < n and lines[i].strip() and not re.match(r"^(#{1,3} |```|\||\s*[-*] |\s*\d+\. |---$)", lines[i]):
            buf.append(lines[i])
            i += 1
        flow.append(Paragraph(inline(" ".join(s.strip() for s in buf)), BODY))
    return flow


def build_table(rows):
    def cells(r):
        return [c.strip() for c in r.strip().strip("|").split("|")]
    header = cells(rows[0])
    body = [cells(r) for r in rows[2:]]
    data = [[Paragraph(inline(c), CELLH) for c in header]]
    for r in body:
        # pad short rows
        r = r + [""] * (len(header) - len(r))
        data.append([Paragraph(inline(c), CELL) for c in r])
    t = Table(data, repeatRows=1, hAlign="LEFT")
    t.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), ACCENT),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, HEAD_BG]),
        ("GRID", (0, 0), (-1, -1), 0.5, BORDER),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    return t


def main():
    src, dst = sys.argv[1], sys.argv[2]
    with open(src, encoding="utf-8") as f:
        md = f.read()
    doc = SimpleDocTemplate(dst, pagesize=LETTER,
                            leftMargin=0.9 * inch, rightMargin=0.9 * inch,
                            topMargin=0.85 * inch, bottomMargin=0.85 * inch,
                            title="OpenTrack — AI assist (setup guide)",
                            author="OpenTrack")
    doc.build(parse(md))
    print("wrote", dst)


if __name__ == "__main__":
    main()
