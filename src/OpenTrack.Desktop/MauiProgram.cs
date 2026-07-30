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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Enums;
using OpenTrack.Desktop.Services;
using OpenTrack.UI.Services;

namespace OpenTrack.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Where the OpenTrack.API server lives. Defaults to the local dev address; on a
        // real deployment this points at the Beelink's LAN address (e.g. http://192.168.x.x:5xxx).
        // TODO: make this user-configurable in a settings screen rather than hardcoded.
        // Read the API address from the bundled wwwroot/appsettings.json so each machine
        // can point at its own server (localhost in dev, the Beelink's LAN address in
        // deployment) by editing that file — no recompile needed. Falls back to localhost
        // if the file or key is missing.
        string apiBaseUrl = "http://localhost:5003";
        using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("OpenTrack.Desktop.wwwroot.appsettings.json"))
        {
            if (stream is not null)
            {
                var cfg = new ConfigurationBuilder().AddJsonStream(stream).Build();
                var configured = cfg["ApiBaseUrl"];
                if (!string.IsNullOrWhiteSpace(configured))
                    apiBaseUrl = configured;
            }
        }

        builder.Services.AddTransient<AuthTokenHandler>();

        // The authenticated client (bearer token attached by AuthTokenHandler) used for
        // all data calls once signed in.
        builder.Services.AddHttpClient("OpenTrackApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

        // A plain (unauthenticated) client that DesktopAuthState uses to perform login itself.
        builder.Services.AddHttpClient("OpenTrackApiAnon", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

        // Session state (holds the bearer/refresh tokens), driven by the login page.
        builder.Services.AddSingleton(sp =>
            new DesktopAuthState(sp.GetRequiredService<IHttpClientFactory>().CreateClient("OpenTrackApiAnon")));

        // Bridge the bearer-token session into Blazor's authorization system so the shared
        // OpenTrack.UI pages' [Authorize] and <AuthorizeView> work unchanged.
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddSingleton<DesktopAuthenticationStateProvider>();
        builder.Services.AddSingleton<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<DesktopAuthenticationStateProvider>());
        // Same four role policies as web/API, registered inline (desktop is a thin client
        // and doesn't reference Infrastructure). Uses AddAuthorizationCore's options overload
        // since the fuller AddAuthorizationBuilder isn't available in a MAUI app.
        builder.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("RequireUpdater", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Updater)));
            options.AddPolicy("RequireDeveloper", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Developer)));
            options.AddPolicy("RequireManager", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Manager)));
            options.AddPolicy("RequireAdministrator", p => p.RequireAssertion(ctx => DesktopRoleCheck(ctx, UserRole.Administrator)));
        });
        // The shared UI's data seam, backed by HTTP calls to OpenTrack.API in the desktop app.
        builder.Services.AddScoped<IOpenTrackDataService>(sp =>
            new HttpOpenTrackDataService(sp.GetRequiredService<IHttpClientFactory>().CreateClient("OpenTrackApi")));

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Log any unhandled exception to a file on the desktop so we can diagnose crashes
        // the WebView dev tools won't show.
        var logPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "opentrack-error.txt");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            System.IO.File.AppendAllText(logPath, $"[Unhandled] {DateTime.Now}\n{e.ExceptionObject}\n\n");
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, e) =>
            System.IO.File.AppendAllText(logPath, $"[UnobservedTask] {DateTime.Now}\n{e.Exception}\n\n");



        // Connect the session's Changed event to the auth-state provider so the UI re-evaluates
        // authorization the moment the user signs in or out.
        var authState = app.Services.GetRequiredService<DesktopAuthState>();
        var authProvider = app.Services.GetRequiredService<DesktopAuthenticationStateProvider>();
        authState.Changed += authProvider.NotifyChanged;

        return app;
    }

    private static bool DesktopRoleCheck(AuthorizationHandlerContext ctx, UserRole minimum)
    {
        var roleClaim = ctx.User.FindFirst("OpenTrack.Role")?.Value;
        return roleClaim is not null
            && Enum.TryParse<UserRole>(roleClaim, out var role)
            && (int)role >= (int)minimum;
    }
}
