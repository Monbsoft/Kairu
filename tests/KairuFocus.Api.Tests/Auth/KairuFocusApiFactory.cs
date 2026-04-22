using KairuFocus.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KairuFocus.Api.Tests.Auth;

/// <summary>
/// WebApplicationFactory configurée pour les tests d'intégration.
/// - Utilise l'environnement "Testing" pour désactiver la migration SQL et les validations prod.
/// - Remplace KairuFocusDbContext (SQL Server) par un provider InMemory.
/// </summary>
public sealed class KairuFocusApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environnement "Testing" : désactive la migration EF, la validation
        // des secrets GitHub et le fallback Data Protection prod.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Configuration minimale pour les tests — pas de secrets réels nécessaires.
            var testConfig = new Dictionary<string, string?>
            {
                ["WebBaseUrl"] = "http://localhost",
                ["AllowedCallbackUrls:0"] = "http://localhost/callback",
                // GitHub vides — pas testés ici, le check est conditionné sur !Testing.
                ["GitHub:ClientId"] = "test-client-id",
                ["GitHub:ClientSecret"] = "test-client-secret",
            };
            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Remplacer le DbContext SQL Server par InMemory pour les tests.
            services.RemoveAll<DbContextOptions<KairuFocusDbContext>>();
            services.RemoveAll<KairuFocusDbContext>();

            services.AddDbContext<KairuFocusDbContext>(options =>
                options.UseInMemoryDatabase("KairuFocusTests_" + Guid.NewGuid()));
        });
    }
}
