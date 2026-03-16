using Kairudev.Application.Tickets;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Settings;
using Kairudev.Domain.Tasks;
using Kairudev.Infrastructure.Identity;
using Kairudev.Infrastructure.Jira;
using Kairudev.Infrastructure.Persistence;
using Kairudev.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kairudev.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<KairudevDbContext>(options =>
            options.UseSqlServer(connectionString));

        // All repositories use EF Core (works with both SQLite and SQL Server)
        services.AddScoped<IUserRepository, EfCoreUserRepository>();
        services.AddScoped<ITaskRepository, EfCoreTaskRepository>();
        services.AddScoped<IPomodoroSessionRepository, EfCorePomodoroSessionRepository>();
        services.AddScoped<IPomodoroSettingsRepository, EfCorePomodoroSettingsRepository>();
        services.AddScoped<IJournalEntryRepository, EfCoreJournalEntryRepository>();
        services.AddScoped<IUserSettingsRepository, EfCoreUserSettingsRepository>();

        services.AddHttpClient<IJiraTicketService, JiraApiClient>();

        return services;
    }
}
