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

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Enums;
using OpenTrack.Infrastructure;
using OpenTrack.Infrastructure.Data;
using OpenTrack.Infrastructure.Identity;
using OpenTrack.UI.Services;
using OpenTrack.Web.Components;
using OpenTrack.Web.Components.Account;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Role-based authorization policies — shared with OpenTrack.API so both surfaces
// enforce identical access rules.
// Role-based authorization policies built on the UserRole enum. Registered inline (rather
// than via a shared helper) because the DI extension lives in ASP.NET Core packages; the
// desktop and API hosts register the identical four policies the same way.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Updater)))
    .AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Developer)))
    .AddPolicy("RequireManager", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Manager)))
    .AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => WebRoleCheck(ctx, UserRole.Administrator)));

static bool WebRoleCheck(Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext ctx, UserRole minimum)
{
    var roleClaim = ctx.User.FindFirst("OpenTrack.Role")?.Value;
    return roleClaim is not null
        && Enum.TryParse<UserRole>(roleClaim, out var role)
        && (int)role >= (int)minimum;
}

// OpenTrack data layer (EF Core + SQLite) and Identity (auth).
var connectionString = builder.Configuration.ResolveOpenTrackConnectionString();
builder.Services.AddOpenTrackInfrastructure(connectionString);
builder.Services.AddOpenTrackIdentity();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Adds the user's OpenTrack.Role as a claim on sign-in so [Authorize(Policy = "...")] and
// AuthorizeView can check it without an extra DB round trip on every request.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<OpenTrack.Core.Entities.User>, RoleClaimsPrincipalFactory>();

// The shared UI's data seam, backed by direct EF Core access in the web app.
builder.Services.AddScoped<OpenTrack.UI.Services.IOpenTrackDataService, OpenTrack.Web.Services.DbOpenTrackDataService>();

// Web-only administrative surface (global user/role management).
builder.Services.AddScoped<OpenTrack.Web.Services.AdminService>();

builder.Services.AddSingleton<IEmailSender<OpenTrack.Core.Entities.User>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Apply any pending EF Core migrations at startup so the SQLite database (and the
// Identity tables) are created/updated automatically when the app runs.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await OpenTrackSeeder.EnsureBootstrapAdminAsync(scope.ServiceProvider, app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(OpenTrack.UI.AssemblyMarker).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
