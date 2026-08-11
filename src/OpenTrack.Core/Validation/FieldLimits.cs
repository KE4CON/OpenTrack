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

namespace OpenTrack.Core.Validation;

/// <summary>
/// Single source of truth for text-field length limits, referenced by BOTH the EF column
/// configuration (AppDbContext) and the Blazor form <c>[StringLength]</c> annotations so a
/// value that passes client validation can never exceed the database column — which silently
/// truncates on SQLite but throws a DbUpdateException on SQL Server/PostgreSQL/MySQL.
/// </summary>
public static class FieldLimits
{
    public const int IssueTitle = 200;
    public const int ProjectName = 100;
    public const int Description = 2000;
    public const int CategoryName = 100;
    public const int VersionName = 50;
    public const int TagName = 50;
    public const int CustomFieldName = 100;
    public const int CustomFieldValue = 2000;
    public const int CustomFieldEnumOptions = 2000;
    public const int ChecklistTitle = 300;
    public const int ChecklistArea = 100;
    public const int ChecklistText = 2000;
    public const int SavedFilterName = 100;
    public const int SavedFilterQuery = 1000;
    public const int WebhookUrl = 500;
    public const int IntakeName = 100;
    public const int IntakeEmail = 200;
}
