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

using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Web.Tests;

/// <summary>
/// Guards audit finding D1: the design-time factory that `dotnet ef` uses to scaffold migrations
/// must build the SAME model the app runs — in particular it must include the .NET 10
/// AspNetUserPasskeys table. If the factory regresses to a bare `new AppDbContext(options)` (no
/// service provider → Identity schema version not applied), the passkey entity drops out of the
/// design-time model and every scaffolded migration tries to DROP the passkey table.
/// </summary>
public sealed class DesignTimeFactoryTests
{
    [Fact]
    public void DesignTimeModel_IncludesPasskeyEntity()
    {
        using var db = new AppDbContextFactory().CreateDbContext([]);
        var passkey = db.Model.FindEntityType("Microsoft.AspNetCore.Identity.IdentityUserPasskey<int>");
        Assert.NotNull(passkey); // absent => scaffolds would drop AspNetUserPasskeys
    }

    [Fact]
    public void DesignTimeModel_MatchesRuntimeModel_ForPasskeys()
    {
        // The design-time model and the runtime model must agree on the passkey entity, otherwise
        // migrations (design-time) diverge from what the app (runtime) actually persists.
        using var designTime = new AppDbContextFactory().CreateDbContext([]);
        var designHasPasskey = designTime.Model.FindEntityType("Microsoft.AspNetCore.Identity.IdentityUserPasskey<int>") is not null;
        Assert.True(designHasPasskey);
    }
}
