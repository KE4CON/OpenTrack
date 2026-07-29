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
using OpenTrack.API;
using OpenTrack.API.Endpoints;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Serialize/deserialize enums (IssueStatus, IssueSeverity, etc.) as their string names
// rather than raw integers, so API consumers send/receive "Major" instead of "60".
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// OpenTrack data layer (shared SQLite database with OpenTrack.Web).
var connectionString = builder.Configuration.ResolveOpenTrackConnectionString();
builder.Services.AddOpenTrackInfrastructure(connectionString);

// Bearer-token Identity for the API (desktop/thin-client consumers) — separate auth
// scheme from OpenTrack.Web's cookie-based Identity, but backed by the same AppDbContext
// and User type, so an account works identically whether signing in via the web app or
// the desktop app.
builder.Services.AddIdentityApiEndpoints<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, RoleClaimsPrincipalFactory>();

// Role-based authorization policies — shared with OpenTrack.Web so both surfaces
// enforce identical access rules.
// Role-based authorization policies built on the UserRole enum, matching the web/desktop
// definitions. Kept inline here so the API doesn't need to reference the Blazor UI project.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Updater)))
    .AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Developer)))
    .AddPolicy("RequireManager", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Manager)))
    .AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => ApiRoleCheck.HasRoleAtLeast(ctx, UserRole.Administrator)));

var app = builder.Build();

// Apply any pending EF Core migrations at startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Bearer-token auth endpoints: /api/auth/register, /api/auth/login, /api/auth/refresh, etc.
app.MapGroup("/api/auth").MapIdentityApi<User>();

// "Who am I" — returns the signed-in user's id, name, and OpenTrack role. The desktop client
// calls this right after login (the Identity /login token is opaque, so the client can't read
// the role from the token itself). Requires a valid bearer token.
app.MapGet("/api/auth/me", (System.Security.Claims.ClaimsPrincipal user) =>
{
    var id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var name = user.Identity?.Name ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var role = user.FindFirst("OpenTrack.Role")?.Value;
    return Results.Ok(new { id, name, role });
}).RequireAuthorization();

app.MapProjectEndpoints();
app.MapIssueEndpoints();

app.Run();
