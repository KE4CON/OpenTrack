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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTrack.Infrastructure;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Tests;

/// <summary>
/// Verifies the model the RUNNING web app builds — i.e. AppDbContext created through the
/// IDbContextFactory + the real Identity registration (AddOpenTrackIdentity, SchemaVersion v3) —
/// matches what the code expects. The audit found that `dotnet ef` (design-time) builds the model
/// WITHOUT the .NET 10 passkey table (finding D1); this test confirms whether the RUNTIME model is
/// affected too, since passkeys are a live feature (login + Manage pages use them).
/// </summary>
public sealed class RuntimeModelTests
{
    private static ServiceProvider BuildAppServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        // Same registrations the web host uses.
        services.AddOpenTrackInfrastructure("Data Source=file:passkey-model-check?mode=memory&cache=shared");
        services.AddOpenTrackIdentity();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void RuntimeModel_IncludesPasskeyEntity()
    {
        using var provider = BuildAppServices();
        var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        var passkey = db.Model.FindEntityType("Microsoft.AspNetCore.Identity.IdentityUserPasskey<int>");
        Assert.NotNull(passkey); // if this fails, the running app cannot store passkeys — a real regression
    }

    [Fact]
    public void RuntimeModel_IncludesOpenTrackEntities()
    {
        using var provider = BuildAppServices();
        var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        Assert.NotNull(db.Model.FindEntityType(typeof(OpenTrack.Core.Entities.Issue)));
        Assert.NotNull(db.Model.FindEntityType(typeof(OpenTrack.Core.Entities.ProjectMembership)));
    }
}
