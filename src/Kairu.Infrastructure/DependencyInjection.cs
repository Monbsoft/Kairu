using Kairu.Application.Tickets;
using Kairu.Domain.Identity;
using Kairu.Domain.Journal;
using Kairu.Domain.Pomodoro;
using Kairu.Domain.Settings;
using Kairu.Domain.Tasks;
using Kairu.Infrastructure.Identity;
using Kairu.Infrastructure.Jira;
using Kairu.Infrastructure.Persistence;
using Kairu.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kairu.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<KairuDbContext>(options =>
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
