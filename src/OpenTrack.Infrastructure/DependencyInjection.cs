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
using Microsoft.Extensions.Logging;
using OpenTrack.Core.Entities;
using OpenTrack.Infrastructure.Data;

namespace OpenTrack.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the SMTP email sender (as both the Identity IEmailSender and the general
    /// IEmailService, sharing one instance) plus the notification dispatcher. Used by both hosts.</summary>
    public static IServiceCollection AddOpenTrackEmailAndNotifications(this IServiceCollection services)
    {
        services.AddSingleton<Email.SmtpEmailSender>();
        services.AddSingleton<IEmailSender<User>>(sp => sp.GetRequiredService<Email.SmtpEmailSender>());
        services.AddSingleton<Email.IEmailService>(sp => sp.GetRequiredService<Email.SmtpEmailSender>());
        // Typed HttpClient for outgoing project webhooks — short timeout so a dead endpoint can't hang.
        services.AddHttpClient<Webhooks.WebhookDispatch>(c => c.Timeout = TimeSpan.FromSeconds(5));
        services.AddScoped<Notifications.NotificationDispatch>();
        return services;
    }

    /// <summary>Registers the optional, tiered AI assistant. Reads "OpenTrack:Ai" from configuration; if
    /// disabled or unconfigured the assistant simply reports IsEnabled=false. The base provider (selected by
    /// OpenTrack:Ai:Provider — "anthropic" (Claude, default) or "openai" (any OpenAI-compatible Chat
    /// Completions endpoint, including local Ollama/LM Studio)) handles the menial tasks. An OPTIONAL second
    /// provider under "OpenTrack:Ai:Smart" handles the reasoning-heavy "Suggest a fix"; when absent the base
    /// provider handles everything. The classic tiering is a local model (base) + cloud Claude (Smart). API
    /// keys are only ever read server-side. Used by both hosts.</summary>
    public static IServiceCollection AddOpenTrackAi(this IServiceCollection services, IConfiguration config)
    {
        var options = new Ai.AiOptions();
        config.GetSection(Ai.AiOptions.Section).Bind(options);
        services.AddSingleton(options);

        // Named HttpClients per provider kind. A local OpenAI-compatible engine can be slow to first-token,
        // so that client gets a longer timeout than the cloud Anthropic client.
        services.AddHttpClient("ai-anthropic", c => c.Timeout = TimeSpan.FromSeconds(30));
        services.AddHttpClient("ai-openai", c => c.Timeout = TimeSpan.FromSeconds(60));

        services.AddScoped<Ai.IAiAssistant>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            // Build a concrete provider for one AiOptions (cloud Anthropic or OpenAI-compatible/local).
            Ai.IAiAssistant Build(Ai.AiOptions o) => o.IsOpenAi
                ? new Ai.OpenAiAssistant(httpFactory.CreateClient("ai-openai"), o, loggerFactory.CreateLogger<Ai.OpenAiAssistant>())
                : new Ai.AnthropicAiAssistant(httpFactory.CreateClient("ai-anthropic"), o, loggerFactory.CreateLogger<Ai.AnthropicAiAssistant>());

            var menial = Build(options);                            // base "OpenTrack:Ai" provider
            var smart = options.Smart is { } s ? Build(s) : null;   // optional "OpenTrack:Ai:Smart" provider
            return new Ai.AiAssistantRouter(menial, smart);
        });
        return services;
    }

    /// <summary>
    /// Registers OpenTrack's data layer (EF Core + SQLite).
    /// Registers an <see cref="IDbContextFactory{TContext}"/> so the Blazor Server data service can
    /// create a short-lived context per operation — a single scoped context lives for the entire
    /// SignalR circuit and DbContext is not thread-safe, so concurrent component operations on a
    /// shared instance throw. A scoped shim is also registered so ASP.NET Identity and the API
    /// endpoints can still inject <see cref="AppDbContext"/> directly (they run per-HTTP-request,
    /// where a request-scoped context is short-lived and safe).
    /// </summary>
    public static IServiceCollection AddOpenTrackInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
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

        // NOTE: We deliberately do NOT call .AddRoles<IdentityRole<int>>(). OpenTrack authorization
        // is driven by the custom UserRole enum (User.Role, global) and ProjectMembership.Role
        // (per-project) — never by ASP.NET Identity's role store. Registering AddRoles would expose
        // a second, parallel role system (UserManager.AddToRoleAsync) that has zero effect on
        // OpenTrack's checks and could silently diverge. The AspNetRoles/AspNetUserRoles tables that
        // remain from the initial migration are vestigial and unused.
        services.AddIdentityCore<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<Identity.ActiveUserSignInManager>()
            .AddDefaultTokenProviders();

        return services;
    }
}
