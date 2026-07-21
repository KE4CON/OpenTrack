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

namespace OpenTrack.Core.Entities;

/// <summary>An OpenTrack account. (Auth/Identity integration comes in the auth step;
/// for now this is a plain entity carrying a password hash and a default role.)</summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Default/global role. Per-project roles live on <see cref="ProjectMembership"/>.</summary>
    public UserRole Role { get; set; } = UserRole.Reporter;

    public bool IsActive { get; set; } = true;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ProjectMembership> Memberships { get; set; } = [];
    public ICollection<Issue> ReportedIssues { get; set; } = [];
    public ICollection<Issue> AssignedIssues { get; set; } = [];
}
