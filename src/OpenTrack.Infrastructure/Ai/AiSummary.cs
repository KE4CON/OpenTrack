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

namespace OpenTrack.Infrastructure.Ai;

/// <summary>Provider-agnostic prompt for summarizing an issue thread. This one asks for a plain-text
/// answer (no tool-use), so the providers just return the model's text.</summary>
public static class AiSummary
{
    public static string BuildPrompt(string title, string? description, IReadOnlyList<string> notes)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(
            "Summarize this issue-tracker thread for a busy maintainer in a few plain-language sentences. " +
            "Cover: what the problem is, what has been tried or discussed, and the current state — what's " +
            "blocking it or what's needed next. Be concise and neutral, and don't invent details. If there " +
            "is very little information, say so briefly.\n\n");
        sb.Append("Title: ").Append(title).Append('\n');
        sb.Append("Description: ").Append(string.IsNullOrWhiteSpace(description) ? "(none)" : description).Append('\n');
        if (notes.Count == 0)
            sb.Append("Comments: (none)\n");
        else
        {
            sb.Append("Comments (oldest first):\n");
            foreach (var n in notes)
                sb.Append("- ").Append(n).Append('\n');
        }
        return sb.ToString();
    }
}
