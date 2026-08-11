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

using System.Globalization;
using System.Xml.Linq;
using OpenTrack.Core.Enums;

namespace OpenTrack.Core.Import;

/// <summary>One note parsed from a MantisBT issue.</summary>
public sealed record MantisNote(string? Reporter, DateTime? Date, string Text, bool Private);

/// <summary>One issue parsed from a MantisBT XML export, with fields already mapped to OpenTrack's
/// enums (their numeric ids are identical to Mantis's, so mapping is exact).</summary>
public sealed record MantisIssue(
    int? MantisId,
    string ProjectName,
    string Summary,
    string? Description,
    string? StepsToReproduce,
    string? AdditionalInformation,
    IssueStatus Status,
    IssueSeverity Severity,
    IssuePriority Priority,
    IssueReproducibility Reproducibility,
    IssueResolution Resolution,
    string? Category,
    string? Reporter,
    string? Handler,
    DateTime? DateSubmitted,
    DateTime? LastUpdated,
    bool Private,
    bool Sticky,
    IReadOnlyList<string> Tags,
    IReadOnlyList<MantisNote> Notes);

/// <summary>
/// Parses a MantisBT XML issue export (the format produced by Mantis's Import/Export Issues feature)
/// into OpenTrack-shaped records. Pure and tolerant: each coded field maps from its numeric
/// <c>id</c> attribute (Mantis and OpenTrack share the same enum values), falling back to a sensible
/// default when a field is missing or unrecognized. No persistence — the importer service applies ACL
/// and writes the rows.
/// </summary>
public static class MantisImport
{
    /// <summary>Parse the export. Throws <see cref="FormatException"/> if the document isn't a
    /// recognizable Mantis export.</summary>
    public static IReadOnlyList<MantisIssue> Parse(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return [];

        XDocument doc;
        try { doc = XDocument.Parse(xml); }
        catch (System.Xml.XmlException ex) { throw new FormatException("The file isn't valid XML.", ex); }

        var root = doc.Root;
        if (root is null || !string.Equals(root.Name.LocalName, "mantis", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("This doesn't look like a MantisBT XML export (expected a <mantis> root element).");

        var issues = new List<MantisIssue>();
        foreach (var el in root.Elements().Where(e => string.Equals(e.Name.LocalName, "issue", StringComparison.OrdinalIgnoreCase)))
        {
            var summary = Text(el, "summary");
            if (string.IsNullOrWhiteSpace(summary)) summary = "(no summary)";

            issues.Add(new MantisIssue(
                MantisId: IntOrNull(Text(el, "id")),
                ProjectName: string.IsNullOrWhiteSpace(Text(el, "project")) ? "Imported" : Text(el, "project")!.Trim(),
                Summary: summary!.Trim(),
                Description: NullIfBlank(Text(el, "description")),
                StepsToReproduce: NullIfBlank(Text(el, "steps_to_reproduce")),
                AdditionalInformation: NullIfBlank(Text(el, "additional_information")),
                Status: EnumFromId(el, "status", IssueStatus.New),
                Severity: EnumFromId(el, "severity", IssueSeverity.Minor),
                Priority: EnumFromId(el, "priority", IssuePriority.Normal),
                Reproducibility: EnumFromId(el, "reproducibility", IssueReproducibility.HaveNotTried),
                Resolution: EnumFromId(el, "resolution", IssueResolution.Open),
                Category: NullIfBlank(Text(el, "category")),
                Reporter: NullIfBlank(Text(el, "reporter")),
                Handler: NullIfBlank(Text(el, "handler")),
                DateSubmitted: UnixOrNull(Text(el, "date_submitted")),
                LastUpdated: UnixOrNull(Text(el, "last_updated")),
                Private: IsPrivate(el.Element(Named(el, "view_state"))),
                Sticky: Text(el, "sticky") == "1",
                Tags: ParseTags(el),
                Notes: ParseNotes(el)));
        }
        return issues;
    }

    private static IReadOnlyList<string> ParseTags(XElement issue)
    {
        var tags = issue.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "tags", StringComparison.OrdinalIgnoreCase));
        if (tags is null) return [];
        return tags.Elements()
            .Where(t => string.Equals(t.Name.LocalName, "tag", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Value.Trim())
            .Where(t => t.Length > 0)
            .ToList();
    }

    private static IReadOnlyList<MantisNote> ParseNotes(XElement issue)
    {
        var notes = issue.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "notes", StringComparison.OrdinalIgnoreCase));
        if (notes is null) return [];
        var list = new List<MantisNote>();
        foreach (var n in notes.Elements().Where(e => string.Equals(e.Name.LocalName, "note", StringComparison.OrdinalIgnoreCase)))
        {
            var text = Text(n, "note") ?? n.Value;
            if (string.IsNullOrWhiteSpace(text)) continue;
            list.Add(new MantisNote(
                Reporter: NullIfBlank(Text(n, "reporter")),
                Date: UnixOrNull(Text(n, "date_submitted")),
                Text: text.Trim(),
                Private: IsPrivate(n.Element(Named(n, "view_state")))));
        }
        return list;
    }

    private static XName Named(XElement scope, string local) => scope.Name.Namespace + local;

    private static string? Text(XElement parent, string local) =>
        parent.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, local, StringComparison.OrdinalIgnoreCase))?.Value;

    private static XElement? Child(XElement parent, string local) =>
        parent.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, local, StringComparison.OrdinalIgnoreCase));

    private static TEnum EnumFromId<TEnum>(XElement issue, string local, TEnum fallback) where TEnum : struct, Enum
    {
        var el = Child(issue, local);
        var idAttr = el?.Attribute("id")?.Value;
        if (int.TryParse(idAttr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && Enum.IsDefined(typeof(TEnum), id))
            return (TEnum)Enum.ToObject(typeof(TEnum), id);
        return fallback;
    }

    private static bool IsPrivate(XElement? viewState)
    {
        // Mantis view_state: 10 = public, 50 = private.
        var id = viewState?.Attribute("id")?.Value;
        if (int.TryParse(id, out var n)) return n >= 50;
        return string.Equals(viewState?.Value.Trim(), "private", StringComparison.OrdinalIgnoreCase);
    }

    private static int? IntOrNull(string? s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

    private static DateTime? UnixOrNull(string? s) =>
        long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var secs) && secs > 0
            ? DateTimeOffset.FromUnixTimeSeconds(secs).UtcDateTime
            : null;

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
