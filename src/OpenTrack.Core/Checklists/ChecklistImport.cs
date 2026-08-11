// OpenTrack — open-source issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version. This program is distributed WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License <https://www.gnu.org/licenses/> for
// more details.

using System.Text.RegularExpressions;

namespace OpenTrack.Core.Checklists;

/// <summary>One item parsed from an imported checklist: its title and the section heading above it.</summary>
public readonly record struct ParsedChecklistItem(string? Area, string Title);

/// <summary>
/// Parses a pasted bug-hunt checklist into items. Deliberately forgiving so a plain or Markdown list
/// both work: a line starting with '#' is a section heading (becomes the <see cref="ParsedChecklistItem.Area"/>
/// of the items under it); every other non-blank line is an item, with any leading bullet
/// (<c>-</c>/<c>*</c>/<c>+</c>), task-list checkbox (<c>- [ ]</c>/<c>- [x]</c>), or number
/// (<c>1.</c>/<c>2)</c>) stripped. Pure and unit-tested; the persistence layer applies length limits.
/// </summary>
public static partial class ChecklistImport
{
    [GeneratedRegex(@"^\s*[-*+]\s+\[[ xX]\]\s+")] private static partial Regex TaskBox();
    [GeneratedRegex(@"^\s*[-*+]\s+")] private static partial Regex Bullet();
    [GeneratedRegex(@"^\s*\d+[.)]\s+")] private static partial Regex Numbered();

    public static IReadOnlyList<ParsedChecklistItem> Parse(string? text)
    {
        var items = new List<ParsedChecklistItem>();
        if (string.IsNullOrWhiteSpace(text)) return items;

        string? area = null;
        foreach (var raw in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (line.StartsWith('#'))
            {
                area = line.TrimStart('#').Trim().TrimEnd(':').Trim();
                if (area.Length == 0) area = null;
                continue;
            }

            // Strip a leading task-checkbox, bullet, or number marker (whichever matches).
            var title = TaskBox().Replace(line, "");
            if (title == line) title = Bullet().Replace(line, "");
            if (title == line) title = Numbered().Replace(line, "");
            title = title.Trim();

            if (title.Length > 0)
                items.Add(new ParsedChecklistItem(string.IsNullOrEmpty(area) ? null : area, title));
        }
        return items;
    }
}
