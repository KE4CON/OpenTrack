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
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTrack.Infrastructure.Data;

/// <summary>
/// Design-time factory so `dotnet ef` can build the context to create migrations without the web host.
///
/// It MUST build the context through the same registrations the running hosts use
/// (<see cref="DependencyInjection.AddOpenTrackInfrastructure"/> + AddOpenTrackIdentity, which
/// configures ASP.NET Identity at SchemaVersion v3) rather than a bare
/// <c>new AppDbContext(options)</c>. A bare construction has no application service provider, so
/// Identity's schema version never applies and the model silently OMITS the .NET 10
/// <c>AspNetUserPasskeys</c> table — which made every scaffolded migration try to DROP that table
/// (audit finding D1), and passkeys are a live feature here (login + Manage/Passkeys). A minimal
/// AddIdentityCore(SchemaVersion=v3) is NOT sufficient to reproduce the passkey model; the full
/// AddOpenTrackIdentity registration is required. Verified by
/// OpenTrack.Web.Tests.RuntimeModelTests and DesignTimeMigrationTests.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // The connection string is irrelevant for scaffolding (the model is built from
        // OnModelCreating without opening the connection) but must be a valid SQLite string.
        services.AddOpenTrackInfrastructure("Data Source=opentrack-designtime.db");
        services.AddOpenTrackIdentity();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
    }
}
