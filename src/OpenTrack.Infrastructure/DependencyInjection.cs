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

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTrack.Core.Entities;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers OpenTrack's data layer (EF Core + SQLite).</summary>
    public static IServiceCollection AddOpenTrackInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }

    /// <summary>
    /// Resolves the SQLite connection string all OpenTrack hosts should share. If the
    /// configuration provides ConnectionStrings:Default it's used as-is (this is where the
    /// Beelink deployment points at the D: drive). Otherwise every host falls back to the
    /// SAME absolute path — a single opentrack.db in a shared per-machine data folder —
    /// rather than each resolving "opentrack.db" relative to its own launch directory,
    /// which would silently create separate databases for the web app and the API.
    /// </summary>
    public static string ResolveOpenTrackConnectionString(this IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        var dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenTrack");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "opentrack.db");
        return $"Data Source={dbPath};Cache=Shared";
    }

    /// <summary>Registers ASP.NET Core Identity (cookie auth + EF Core stores), scoped to
    /// OpenTrack's <see cref="User"/> and int-keyed roles, backed by <see cref="AppDbContext"/>.</summary>
    public static IServiceCollection AddOpenTrackIdentity(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }
}
