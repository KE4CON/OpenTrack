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

using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Authorization;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.CustomFields;

/// <summary>A custom-field definition, as read for display/editing.</summary>
public readonly record struct CustomFieldDefItem(
    int Id, int ProjectId, string Name, CustomFieldType Type, string? EnumOptions, bool Required, int DisplayOrder);

/// <summary>A field definition paired with an issue's current value for it (null when unset).</summary>
public readonly record struct CustomFieldValueItem(
    int DefinitionId, string Name, CustomFieldType Type, string? EnumOptions, bool Required, int DisplayOrder, string? Value);

/// <summary>
/// ACL-aware custom-field operations, shared by the Web API and the web/EF data service so the
/// authorization logic lives once. Defining fields on a project is a project-management action
/// (Manager+, like categories/versions); reading a field's definitions requires view access to the
/// project; reading/writing an issue's field values follows that issue's own view/edit ACL (Updater+
/// to write, exactly like editing any core issue field or a tag).
/// </summary>
public static class CustomFieldOperations
{
    // ---- Definitions (project-scoped, Manager+) ----

    public static async Task<IReadOnlyList<CustomFieldDefItem>> ListDefinitionsAsync(
        AppDbContext db, AccessSnapshot access, int projectId, CancellationToken ct = default)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null || !access.For(projectId).CanViewProject(project.IsPublic)) return [];
        return await db.CustomFieldDefinitions.AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderBy(d => d.DisplayOrder).ThenBy(d => d.Name)
            .Select(d => new CustomFieldDefItem(d.Id, d.ProjectId, d.Name, d.Type, d.EnumOptions, d.Required, d.DisplayOrder))
            .ToListAsync(ct);
    }

    public static async Task<string?> CreateDefinitionAsync(
        AppDbContext db, AccessSnapshot access, int projectId, string name, CustomFieldType type,
        string? enumOptions, bool required, CancellationToken ct = default)
    {
        // Authorize before revealing whether the project exists (no existence oracle).
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing custom fields requires the Manager role on this project.");
        if (!await db.Projects.AnyAsync(p => p.Id == projectId, ct)) return "Project not found.";

        var error = ValidateDefinition(ref name, type, ref enumOptions);
        if (error is not null) return error;

        var lowered = name.ToLowerInvariant();
        if (await db.CustomFieldDefinitions.AnyAsync(d => d.ProjectId == projectId && d.Name.ToLower() == lowered, ct))
            return "A custom field with that name already exists on this project.";

        var maxOrder = await db.CustomFieldDefinitions.Where(d => d.ProjectId == projectId)
            .Select(d => (int?)d.DisplayOrder).MaxAsync(ct) ?? 0;
        db.CustomFieldDefinitions.Add(new CustomFieldDefinition
        {
            ProjectId = projectId,
            Name = name,
            Type = type,
            EnumOptions = enumOptions,
            Required = required,
            DisplayOrder = maxOrder + 1,
        });
        await db.SaveChangesAsync(ct);
        return null;
    }

    /// <summary>Edit a definition's name/options/required/order. The field <em>type</em> is immutable
    /// after creation — changing it would silently invalidate existing stored values. Scoped to
    /// <paramref name="projectId"/>: a field belonging to another project is treated as not found, and
    /// the manage check is on that project — so this never reveals whether a field in some other
    /// project exists.</summary>
    public static async Task<string?> UpdateDefinitionAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int definitionId, string name, string? enumOptions,
        bool required, int displayOrder, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing custom fields requires the Manager role on this project.");
        var def = await db.CustomFieldDefinitions.FirstOrDefaultAsync(d => d.Id == definitionId && d.ProjectId == projectId, ct);
        if (def is null) return "Custom field not found.";

        var error = ValidateDefinition(ref name, def.Type, ref enumOptions);
        if (error is not null) return error;

        var lowered = name.ToLowerInvariant();
        if (await db.CustomFieldDefinitions.AnyAsync(d => d.ProjectId == def.ProjectId && d.Id != def.Id && d.Name.ToLower() == lowered, ct))
            return "A custom field with that name already exists on this project.";

        def.Name = name;
        def.EnumOptions = enumOptions;
        def.Required = required;
        def.DisplayOrder = displayOrder;
        await db.SaveChangesAsync(ct);
        return null;
    }

    /// <summary>Delete a definition (and, by cascade, its values). Scoped to <paramref name="projectId"/>
    /// so it can neither touch nor reveal a field belonging to another project.</summary>
    public static async Task<bool> DeleteDefinitionAsync(
        AppDbContext db, AccessSnapshot access, int projectId, int definitionId, CancellationToken ct = default)
    {
        if (!access.For(projectId).CanManageProject())
            throw new UnauthorizedAccessException("Managing custom fields requires the Manager role on this project.");
        var def = await db.CustomFieldDefinitions.FirstOrDefaultAsync(d => d.Id == definitionId && d.ProjectId == projectId, ct);
        if (def is null) return false;
        db.CustomFieldDefinitions.Remove(def); // cascades to its values
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ---- Values (issue-scoped, follows the issue ACL) ----

    public static async Task<IReadOnlyList<CustomFieldValueItem>> ListValuesForIssueAsync(
        AppDbContext db, AccessSnapshot access, int issueId, CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return [];

        var defs = await db.CustomFieldDefinitions.AsNoTracking()
            .Where(d => d.ProjectId == issue.ProjectId)
            .OrderBy(d => d.DisplayOrder).ThenBy(d => d.Name)
            .ToListAsync(ct);
        var values = await db.CustomFieldValues.AsNoTracking()
            .Where(v => v.IssueId == issueId)
            .ToDictionaryAsync(v => v.CustomFieldDefinitionId, v => v.Value, ct);

        return defs.Select(d => new CustomFieldValueItem(
            d.Id, d.Name, d.Type, d.EnumOptions, d.Required, d.DisplayOrder,
            values.TryGetValue(d.Id, out var val) ? val : null)).ToList();
    }

    /// <summary>Set (or, with a blank value, clear) one custom-field value on an issue. Validates the
    /// value against the field type; a blank value clears the field unless it is required.</summary>
    public static async Task<string?> SetValueAsync(
        AppDbContext db, AccessSnapshot access, int issueId, int definitionId, string? rawValue,
        CancellationToken ct = default)
    {
        var issue = await db.Issues.AsNoTracking().Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == issueId, ct);
        if (issue is null || !CanView(access, issue)) return "Issue not found.";
        if (!access.For(issue.ProjectId).CanEditIssue())
            throw new UnauthorizedAccessException("Editing custom fields requires the Updater role on this project.");

        var def = await db.CustomFieldDefinitions.AsNoTracking().FirstOrDefaultAsync(d => d.Id == definitionId, ct);
        if (def is null || def.ProjectId != issue.ProjectId) return "Custom field not found on this issue's project.";

        var result = CustomFieldValidation.ValidateValue(def.Type, def.EnumOptions, rawValue);
        if (!result.Ok) return result.Error;
        if (result.Normalized is null && def.Required) return $"{def.Name} is required.";

        var existing = await db.CustomFieldValues.FirstOrDefaultAsync(v => v.IssueId == issueId && v.CustomFieldDefinitionId == definitionId, ct);
        if (result.Normalized is null)
        {
            if (existing is not null) { db.CustomFieldValues.Remove(existing); await db.SaveChangesAsync(ct); }
            return null;
        }
        if (existing is null)
            db.CustomFieldValues.Add(new CustomFieldValue { IssueId = issueId, CustomFieldDefinitionId = definitionId, Value = result.Normalized });
        else
            existing.Value = result.Normalized;
        await db.SaveChangesAsync(ct);
        return null;
    }

    private static string? ValidateDefinition(ref string name, CustomFieldType type, ref string? enumOptions)
    {
        name = name.Trim();
        if (name.Length == 0) return "Field name is required.";
        if (name.Length > FieldLimits.CustomFieldName) return $"Field name must be {FieldLimits.CustomFieldName} characters or fewer.";

        if (type == CustomFieldType.Enum)
        {
            var options = CustomFieldValidation.ParseEnumOptions(enumOptions);
            if (options.Count == 0) return "A list field needs at least one option (one per line).";
            enumOptions = string.Join('\n', options); // normalize (trimmed, blank lines dropped)
            if (enumOptions.Length > FieldLimits.CustomFieldEnumOptions)
                return $"The option list must be {FieldLimits.CustomFieldEnumOptions} characters or fewer.";
        }
        else
        {
            enumOptions = null; // only Enum fields carry options
        }
        return null;
    }

    private static bool CanView(AccessSnapshot access, Issue issue) =>
        access.For(issue.ProjectId).CanViewIssue(issue.Project.IsPublic, issue.IsPrivate, issue.ReporterId, issue.AssigneeId);
}
