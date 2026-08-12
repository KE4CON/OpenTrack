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
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Export;

/// <summary>
/// Builds CSV and JSON exports of issues, shared by the Web host and the API so both produce the same
/// files. Every query is filtered through <see cref="VisibilityQueries"/> first, so an export never
/// contains an issue — or a private note — the requesting user couldn't see; a project the user can't
/// view yields nothing.
/// </summary>
public static class ExportBuilder
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    /// <summary>A flat CSV of the visible issues (all, or just one project). Good for spreadsheets.</summary>
    public static async Task<string> BuildIssuesCsvAsync(
        AppDbContext db, AccessSnapshot access, int? projectId, CancellationToken ct = default)
    {
        var query = db.Issues.AsNoTracking().WhereVisibleTo(access);
        if (projectId is { } pid) query = query.Where(i => i.ProjectId == pid);

        var rows = await query
            .OrderBy(i => i.ProjectId).ThenBy(i => i.Id)
            .Select(i => new
            {
                i.Id,
                Project = i.Project.Name,
                i.Title,
                Status = i.Status.ToString(),
                Severity = i.Severity.ToString(),
                Priority = i.Priority.ToString(),
                Category = i.Category != null ? i.Category.Name : "",
                Reporter = i.Reporter.UserName ?? "",
                Assignee = i.Assignee != null ? i.Assignee.UserName : "",
                i.CreatedAt,
                i.UpdatedAt,
                i.DueDate,
                Tags = string.Join(" ", i.IssueTags.Select(t => t.Tag.Name)),
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Id,Project,Title,Status,Severity,Priority,Category,Reporter,Assignee,CreatedUtc,UpdatedUtc,DueDate,Tags");
        foreach (var r in rows)
        {
            sb.Append(r.Id).Append(',')
              .Append(Csv(r.Project)).Append(',')
              .Append(Csv(r.Title)).Append(',')
              .Append(Csv(r.Status)).Append(',')
              .Append(Csv(r.Severity)).Append(',')
              .Append(Csv(r.Priority)).Append(',')
              .Append(Csv(r.Category)).Append(',')
              .Append(Csv(r.Reporter)).Append(',')
              .Append(Csv(r.Assignee)).Append(',')
              .Append(Iso(r.CreatedAt)).Append(',')
              .Append(Iso(r.UpdatedAt)).Append(',')
              .Append(r.DueDate is { } d ? Iso(d) : "").Append(',')
              .Append(Csv(r.Tags)).Append('\n');
        }
        return sb.ToString();
    }

    /// <summary>A full JSON backup of one project: its issues (visible only) with tags, custom-field
    /// values, and viewable notes, plus the project's bug-hunt checklist. Returns null if the project
    /// isn't viewable.</summary>
    public static async Task<string?> BuildProjectJsonAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return null;
        var ctx = access.For(projectId);

        var issues = await db.Issues.AsNoTracking().WhereVisibleTo(access)
            .Where(i => i.ProjectId == projectId)
            .Include(i => i.Category)
            .Include(i => i.Reporter)
            .Include(i => i.Assignee)
            .Include(i => i.Notes).ThenInclude(n => n.Author)
            .Include(i => i.IssueTags).ThenInclude(t => t.Tag)
            .Include(i => i.CustomFieldValues).ThenInclude(v => v.Definition)
            .OrderBy(i => i.Id)
            .ToListAsync(ct);

        var checklist = await db.ChecklistItems.AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id)
            .Select(c => new { c.Title, c.Area, Status = c.Status.ToString(), c.Notes, c.LinkedIssueId })
            .ToListAsync(ct);

        var export = new
        {
            exportedAtUtc = DateTime.UtcNow,
            project = new { project.Id, project.Name, project.Description, project.IsPublic },
            issues = issues.Select(i => new
            {
                i.Id,
                i.Title,
                i.Description,
                Status = i.Status.ToString(),
                Severity = i.Severity.ToString(),
                Priority = i.Priority.ToString(),
                Category = i.Category?.Name,
                Reporter = i.Reporter.UserName,
                Assignee = i.Assignee?.UserName,
                i.CreatedAt,
                i.UpdatedAt,
                i.DueDate,
                Tags = i.IssueTags.Select(t => t.Tag.Name).ToArray(),
                CustomFields = i.CustomFieldValues.Select(v => new { Field = v.Definition.Name, v.Value }).ToArray(),
                // Never leak a private note the exporter couldn't see on the issue page.
                Notes = i.Notes
                    .Where(n => ctx.CanViewNote(n.IsPrivate, n.AuthorId))
                    .OrderBy(n => n.CreatedAt)
                    .Select(n => new { Author = n.Author.UserName, n.Text, n.IsPrivate, n.CreatedAt })
                    .ToArray(),
            }).ToArray(),
            checklist,
        };
        return JsonSerializer.Serialize(export, Json);
    }

    private static string Iso(DateTime dt) => dt.ToString("o", CultureInfo.InvariantCulture);

    /// <summary>RFC-4180 CSV field: quote when it contains a comma, quote, CR, or LF; double inner quotes.
    /// Also neutralizes spreadsheet formula injection — a value beginning with =, +, -, @, or a tab is
    /// prefixed with a single quote so Excel/Sheets treat it as text, not a live formula.</summary>
    public static string Csv(string? value)
    {
        var s = value ?? "";
        // Formula-injection guard: values a spreadsheet would evaluate get a leading apostrophe.
        if (s.Length > 0 && (s[0] is '=' or '+' or '-' or '@' or '\t' or '\r'))
            s = "'" + s;
        if (s.IndexOfAny([',', '"', '\r', '\n']) < 0) return s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
