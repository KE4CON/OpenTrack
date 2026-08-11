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

        // Where the OpenTrack.API server lives. The startup default comes from the bundled
        // wwwroot/appsettings.json (localhost in dev, the Beelink's LAN address in deployment);
        // the user can then change it in the in-app Settings screen, which is remembered per machine
        // via Preferences and overrides the bundled default. The chosen value lives in ApiEndpoint,
        // which every HttpClient reads at creation time so a change takes effect without a restart.
        string defaultApiBaseUrl = "http://localhost:5003";
        using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("OpenTrack.Desktop.wwwroot.appsettings.json"))
        {
            if (stream is not null)
            {
                var cfg = new ConfigurationBuilder().AddJsonStream(stream).Build();
                var configured = cfg["ApiBaseUrl"];
                if (!string.IsNullOrWhiteSpace(configured))
                    defaultApiBaseUrl = configured;
            }
        }
        var savedApiBaseUrl = Preferences.Default.Get(ApiEndpoint.PreferenceKey, defaultApiBaseUrl);
        builder.Services.AddSingleton(new ApiEndpoint(savedApiBaseUrl));

        builder.Services.AddTransient<AuthTokenHandler>();

        // The authenticated client (bearer token attached by AuthTokenHandler) used for all data
        // calls once signed in. BaseAddress is read from ApiEndpoint on each client creation.
        builder.Services.AddHttpClient("OpenTrackApi", (sp, client) =>
            {
                client.BaseAddress = new Uri(sp.GetRequiredService<ApiEndpoint>().BaseUrl);
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

        // A plain (unauthenticated) client that DesktopAuthState uses to perform login itself.
        builder.Services.AddHttpClient("OpenTrackApiAnon", (sp, client) =>
            {
                client.BaseAddress = new Uri(sp.GetRequiredService<ApiEndpoint>().BaseUrl);
            });

        // Session state (holds the bearer/refresh tokens), driven by the login page. It creates its
        // anon client per login attempt so it always uses the currently configured server address.
        builder.Services.AddSingleton(sp =>
            new DesktopAuthState(sp.GetRequiredService<IHttpClientFactory>()));

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

        // Host-specific attachment transfer (desktop streams over the authenticated API client).
        builder.Services.AddScoped<IAttachmentTransfer>(sp =>
            new DesktopAttachmentTransfer(sp.GetRequiredService<IHttpClientFactory>().CreateClient("OpenTrackApi")));

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
