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
            options.UseSqlite(connectionString));

        services.AddScoped<IUserRepository, SqliteUserRepository>();
        services.AddScoped<ITaskRepository, SqliteTaskRepository>();
        services.AddScoped<IPomodoroSessionRepository, SqlitePomodoroSessionRepository>();
        services.AddScoped<IPomodoroSettingsRepository, SqlitePomodoroSettingsRepository>();
        services.AddScoped<IJournalEntryRepository, SqliteJournalEntryRepository>();
        services.AddScoped<IUserSettingsRepository, SqliteUserSettingsRepository>();

        services.AddHttpClient<IJiraTicketService, JiraApiClient>();

        return services;
    }
}
