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

namespace OpenTrack.Core.Import;

/// <summary>
/// Parses a CSV export (a header row + data rows) into <see cref="ImportedIssue"/>s. Columns are matched by
/// header name against a set of common aliases, so a generic spreadsheet AND a Jira CSV export both work
/// without manual mapping. A field can span multiple columns (Jira repeats "Labels"): those are collected.
/// Rows without a title are skipped. Pure — no persistence.
/// </summary>
public static class CsvIssueImport
{
    private static readonly string[] TitleCols = ["title", "summary", "name", "subject"];
    private static readonly string[] DescCols = ["description", "body", "details"];
    private static readonly string[] StatusCols = ["status", "state"];
    private static readonly string[] SeverityCols = ["severity"];
    private static readonly string[] PriorityCols = ["priority"];
    private static readonly string[] CategoryCols = ["category", "component", "components", "component/s"];
    private static readonly string[] TagCols = ["tags", "labels", "label", "label/s"];
    private static readonly string[] AssigneeCols = ["assignee", "assigned to"];
    private static readonly string[] ReporterCols = ["reporter", "author", "creator", "created by", "reported by"];
    private static readonly string[] IdCols = ["id", "key", "issue key", "issue id", "issue"];
    private static readonly string[] CreatedCols = ["created", "created at", "created date", "date created", "date submitted"];

    public static IReadOnlyList<ImportedIssue> Parse(string? csv)
    {
        var records = Csv.Parse(csv);
        if (records.Count < 2) return []; // need a header + at least one row

        var header = records[0].Select(h => h.Trim().ToLowerInvariant()).ToArray();

        int[] Cols(string[] aliases) =>
            header.Select((h, idx) => (h, idx)).Where(x => aliases.Contains(x.h)).Select(x => x.idx).ToArray();
        int First(string[] aliases) { var c = Cols(aliases); return c.Length > 0 ? c[0] : -1; }

        int titleCol = First(TitleCols), descCol = First(DescCols), statusCol = First(StatusCols),
            sevCol = First(SeverityCols), priCol = First(PriorityCols), assigneeCol = First(AssigneeCols),
            reporterCol = First(ReporterCols), idCol = First(IdCols), createdCol = First(CreatedCols);
        int[] catCols = Cols(CategoryCols), tagCols = Cols(TagCols);
        if (titleCol < 0) return []; // can't import without a title column

        var issues = new List<ImportedIssue>();
        for (var r = 1; r < records.Count; r++)
        {
            var row = records[r];
            string? Get(int col) => col >= 0 && col < row.Length ? Blank(row[col]) : null;

            var title = Get(titleCol);
            if (title is null) continue; // skip rows with no title (e.g. trailing blank lines)

            var tags = tagCols
                .Where(c => c < row.Length)
                .SelectMany(c => (row[c] ?? "").Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var category = catCols.Select(c => Get(c)).FirstOrDefault(v => v is not null);

            issues.Add(new ImportedIssue(
                Title: title,
                Description: Get(descCol),
                Status: ImportMapping.Status(Get(statusCol)),
                Severity: ImportMapping.Severity(Get(sevCol)),
                Priority: ImportMapping.Priority(Get(priCol)),
                Category: category,
                Reporter: Get(reporterCol),
                Assignee: Get(assigneeCol),
                Tags: tags,
                CreatedAt: ParseDate(Get(createdCol)),
                ExternalId: Get(idCol)));
        }
        return issues;
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static DateTime? ParseDate(string? s) =>
        s is not null && DateTime.TryParse(s, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt)
            ? dt : null;
}
