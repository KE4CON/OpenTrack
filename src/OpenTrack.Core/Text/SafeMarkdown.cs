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

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenTrack.Core.Text;

/// <summary>
/// Renders a safe subset of Markdown to HTML for issue descriptions and notes. Safety is structural:
/// the entire input is HTML-encoded FIRST, so no user text can ever become live markup; only then are
/// our own fixed tags applied (headings are never emitted, raw HTML never passes through), and links
/// are limited to http/https/mailto schemes. Supports fenced code blocks (```), inline code (`),
/// **bold**, *italic*/_italic_, bullet and numbered lists, links, and line breaks — enough to paste a
/// stack trace or a snippet and have it read well. No external dependency (deliberately — this project
/// avoids Markdown libraries and their supply-chain surface).
/// </summary>
public static partial class SafeMarkdown
{
    [GeneratedRegex(@"^\s*[-*+]\s+")] private static partial Regex Bullet();
    [GeneratedRegex(@"^\s*\d+[.)]\s+")] private static partial Regex Ordered();
    [GeneratedRegex(@"`([^`]+)`")] private static partial Regex InlineCode();
    [GeneratedRegex(@"\[([^\]]+)\]\(([^)\s]+)\)")] private static partial Regex Link();
    [GeneratedRegex(@"\*\*([^*]+)\*\*")] private static partial Regex Bold();
    [GeneratedRegex(@"\*([^*]+)\*")] private static partial Regex ItalicStar();
    [GeneratedRegex(@"(?<!\w)_([^_]+)_(?!\w)")] private static partial Regex ItalicUnderscore();

    // A NUL char brackets protected inline-code spans while emphasis rules run; it can't occur in
    // normal pasted text, so it makes an unambiguous, collision-free placeholder.
    private static readonly Regex PlaceholderRe = new("\u0000(\\d+)\u0000", RegexOptions.Compiled);

    public static string ToHtml(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var sb = new StringBuilder();
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];

            if (line.TrimStart().StartsWith("```"))
            {
                i++;
                var code = new List<string>();
                while (i < lines.Length && !lines[i].TrimStart().StartsWith("```")) { code.Add(lines[i]); i++; }
                if (i < lines.Length) i++; // consume the closing fence
                sb.Append("<pre><code>").Append(Encode(string.Join("\n", code))).Append("</code></pre>");
                continue;
            }

            if (line.Trim().Length == 0) { i++; continue; }

            if (Bullet().IsMatch(line))
            {
                sb.Append("<ul>");
                while (i < lines.Length && Bullet().IsMatch(lines[i]))
                {
                    sb.Append("<li>").Append(Inline(Bullet().Replace(lines[i], ""))).Append("</li>");
                    i++;
                }
                sb.Append("</ul>");
                continue;
            }

            if (Ordered().IsMatch(line))
            {
                sb.Append("<ol>");
                while (i < lines.Length && Ordered().IsMatch(lines[i]))
                {
                    sb.Append("<li>").Append(Inline(Ordered().Replace(lines[i], ""))).Append("</li>");
                    i++;
                }
                sb.Append("</ol>");
                continue;
            }

            // A paragraph: consecutive plain lines, joined with <br>.
            var para = new List<string>();
            while (i < lines.Length)
            {
                var l = lines[i];
                if (l.Trim().Length == 0 || l.TrimStart().StartsWith("```") || Bullet().IsMatch(l) || Ordered().IsMatch(l)) break;
                para.Add(l);
                i++;
            }
            sb.Append("<p>").Append(string.Join("<br>", para.Select(Inline))).Append("</p>");
        }

        return sb.ToString();
    }

    private static string Encode(string s) => WebUtility.HtmlEncode(s);

    private static string Inline(string raw)
    {
        var s = Encode(raw);

        // Protect inline-code spans (their contents are already encoded) so emphasis rules skip them.
        var spans = new List<string>();
        s = InlineCode().Replace(s, m =>
        {
            spans.Add("<code>" + m.Groups[1].Value + "</code>");
            return "\u0000" + (spans.Count - 1) + "\u0000";
        });

        s = Link().Replace(s, m =>
        {
            var url = m.Groups[2].Value;
            return IsSafeUrl(url)
                ? $"<a href=\"{url}\" rel=\"noopener noreferrer\" target=\"_blank\">{m.Groups[1].Value}</a>"
                : m.Value; // not a safe scheme — leave the literal text
        });

        s = Bold().Replace(s, "<strong>$1</strong>");
        s = ItalicStar().Replace(s, "<em>$1</em>");
        s = ItalicUnderscore().Replace(s, "<em>$1</em>");

        s = PlaceholderRe.Replace(s, m => spans[int.Parse(m.Groups[1].Value)]);
        return s;
    }

    /// <summary>Only allow links whose scheme is http/https/mailto. The url is already HTML-encoded, so
    /// quotes can't break out of the href attribute; this additionally blocks javascript:/data: URIs.</summary>
    private static bool IsSafeUrl(string encodedUrl) =>
        encodedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        || encodedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        || encodedUrl.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);
}
