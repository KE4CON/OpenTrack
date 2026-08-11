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
using OpenTrack.Core.Validation;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Filters;

public readonly record struct SavedFilterItem(int Id, string Name, string Query);

/// <summary>
/// A user's saved issue filters, shared by the Web API and the web/EF data service. Every operation is
/// scoped to the owning user id, so one user can neither see nor change another's saved filters (there
/// is no cross-project ACL here — a filter is just a stored query string, and the issue list re-applies
/// row-level security whenever the filter is used).
/// </summary>
public static class SavedFilterOperations
{
    public static async Task<IReadOnlyList<SavedFilterItem>> ListForUserAsync(AppDbContext db, int userId, CancellationToken ct = default) =>
        await db.SavedFilters.AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Name)
            .Select(f => new SavedFilterItem(f.Id, f.Name, f.Query))
            .ToListAsync(ct);

    /// <summary>Create or (by name) overwrite a saved filter for the user. Returns null on success or a
    /// validation reason.</summary>
    public static async Task<string?> SaveAsync(AppDbContext db, int userId, string name, string? query, CancellationToken ct = default)
    {
        name = name.Trim();
        if (name.Length == 0) return "Give the filter a name.";
        if (name.Length > FieldLimits.SavedFilterName) return $"The name must be {FieldLimits.SavedFilterName} characters or fewer.";
        query = (query ?? "").TrimStart('?').Trim();
        if (query.Length > FieldLimits.SavedFilterQuery) return "That filter is too complex to save.";

        var lowered = name.ToLowerInvariant();
        var existing = await db.SavedFilters.FirstOrDefaultAsync(f => f.UserId == userId && f.Name.ToLower() == lowered, ct);
        if (existing is null)
            db.SavedFilters.Add(new SavedFilter { UserId = userId, Name = name, Query = query });
        else
            existing.Query = query; // update in place — re-saving a name replaces its query
        await db.SaveChangesAsync(ct);
        return null;
    }

    public static async Task<bool> DeleteAsync(AppDbContext db, int userId, int id, CancellationToken ct = default)
    {
        var filter = await db.SavedFilters.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, ct);
        if (filter is null) return false;
        db.SavedFilters.Remove(filter);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
