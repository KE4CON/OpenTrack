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
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure.Issues;

/// <summary>Guards issue writes against cross-project id smuggling: a client can send any category/version
/// id, and the FK only checks the row exists — not that it belongs to the issue's project. Left unchecked,
/// a foreign id is stored and its NAME is echoed back on the detail view, leaking category/version names
/// from projects the caller can't see. This scopes the ids: any that don't belong to the target project
/// are dropped to null.</summary>
public static class IssueScope
{
    public static async Task<(int? CategoryId, int? AffectsVersionId, int? FixVersionId)> SanitizeAsync(
        AppDbContext db, int projectId, int? categoryId, int? affectsVersionId, int? fixVersionId, CancellationToken ct = default)
    {
        int? cat = categoryId is { } c && await db.Categories.AnyAsync(x => x.Id == c && x.ProjectId == projectId, ct) ? c : null;
        int? av = affectsVersionId is { } a && await db.Versions.AnyAsync(x => x.Id == a && x.ProjectId == projectId, ct) ? a : null;
        int? fv = fixVersionId is { } f && await db.Versions.AnyAsync(x => x.Id == f && x.ProjectId == projectId, ct) ? f : null;
        return (cat, av, fv);
    }
}
