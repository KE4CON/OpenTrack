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

public record ProjectDto(int Id, string Name, string? Description, bool IsPublic, int OwnerId, int OpenIssueCount);

public record ProjectDetailDto(int Id, string Name, string? Description, bool IsPublic, int OwnerId, DateTime CreatedAt, Guid RowVersion);

public record CreateProjectRequest(string Name, string? Description, bool IsPublic);

public record UpdateProjectRequest(string Name, string? Description, bool IsPublic, Guid RowVersion);

public record CategoryDto(int Id, string Name);

public record ProjectMemberDto(int Id, string UserName);

public record ProjectMemberDetailDto(int UserId, string UserName, UserRole Role, bool IsOwner);

public record AddProjectMemberRequest(string Email, UserRole Role);

public record SetMemberRoleRequest(UserRole Role);

public record CreateCategoryRequest(string Name);

public record ProjectVersionDto(int Id, string Name, string? Description, DateTime? ReleaseDate, bool IsReleased);

public record CreateVersionRequest(string Name, string? Description, DateTime? ReleaseDate, bool IsReleased);
