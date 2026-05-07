using KairuFocus.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KairuFocus.Api.Tests.Auth;

/// <summary>
/// WebApplicationFactory configured for integration tests.
/// - Uses "Testing" environment to disable SQL migration and prod validations.
/// - Replaces KairuFocusDbContext (SQL Server) with an InMemory provider.
/// - Accepts an optional <see cref="ConfigureTestServices"/> action to allow
///   per-test service overrides (e.g., replacing authentication handlers or the mediator).
/// </summary>
public sealed class KairuFocusApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Optional additional service configuration applied after the base setup.
    /// Used by tests that need to override specific services (e.g., Cookie auth handler, IMediator).
    /// </summary>
    public Action<IServiceCollection>? ConfigureTestServices { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" environment: disables EF migration, GitHub secret validation,
        // and prod Data Protection fallback.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Minimal configuration for tests — no real secrets needed.
            var testConfig = new Dictionary<string, string?>
            {
                ["WebBaseUrl"] = "http://localhost",
                ["AllowedCallbackUrls:0"] = "http://localhost/callback",
                // GitHub placeholders — not validated in Testing env.
                ["GitHub:ClientId"] = "test-client-id",
                ["GitHub:ClientSecret"] = "test-client-secret",
                // Note: Jwt:SecretKey is captured by Program.cs before this config applies,
                // so the testing fallback in Program.cs is the single source of truth.
            };
            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server DbContext with InMemory for tests.
            services.RemoveAll<DbContextOptions<KairuFocusDbContext>>();
            services.RemoveAll<KairuFocusDbContext>();

            services.AddDbContext<KairuFocusDbContext>(options =>
                options.UseInMemoryDatabase("KairuFocusTests_" + Guid.NewGuid()));

            // Apply per-test overrides (e.g., auth handler replacement, mediator stub).
            ConfigureTestServices?.Invoke(services);
        });
    }
}
