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

using OpenTrack.Core.Enums;

namespace OpenTrack.API.Contracts;

// Field names match ChecklistItemView so the desktop client deserializes straight into it.
public record ChecklistItemDtoContract(
    int Id, int ProjectId, string Title, string? Details, string? Area,
    ChecklistItemStatus Status, string? Notes, int? LinkedIssueId, int DisplayOrder);

public record AddChecklistItemRequest(string Title, string? Details, string? Area);

public record ImportChecklistRequest(string Text);

public record UpdateChecklistItemRequest(string Title, string? Details, string? Area);

public record SetChecklistStatusRequest(ChecklistItemStatus Status, string? Notes);
