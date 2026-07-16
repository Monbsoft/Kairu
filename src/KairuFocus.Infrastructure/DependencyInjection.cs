using KairuFocus.Application.Identity;
using KairuFocus.Application.Tickets;
using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Settings;
using KairuFocus.Domain.Tasks;
using KairuFocus.Infrastructure.Identity;
using KairuFocus.Infrastructure.Jira;
using KairuFocus.Infrastructure.Persistence;
using KairuFocus.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KairuFocus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<KairuFocusDbContext>(options =>
            options.UseSqlServer(connectionString));

        // All repositories use EF Core (works with both SQLite and SQL Server)
        services.AddScoped<IUserRepository, EfCoreUserRepository>();
        services.AddScoped<ITaskRepository, EfCoreTaskRepository>();
        services.AddScoped<IPomodoroSessionRepository, EfCorePomodoroSessionRepository>();
        services.AddScoped<IPomodoroSettingsRepository, EfCorePomodoroSettingsRepository>();
        services.AddScoped<IJournalEntryRepository, EfCoreJournalEntryRepository>();
        services.AddScoped<IUserSettingsRepository, EfCoreUserSettingsRepository>();
        services.AddScoped<IMcpTokenRepository, EfCoreMcpTokenRepository>();
        services.AddScoped<IXpGainRepository, EfCoreXpGainRepository>();

        services.AddSingleton<IMcpTokenGenerator, McpTokenGenerator>();
        services.AddHttpClient<IJiraTicketService, JiraApiClient>();

        return services;
    }
}
