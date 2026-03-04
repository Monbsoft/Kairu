using Kairudev.Domain.Identity;
using Kairudev.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kairudev.Infrastructure.Persistence.Repositories;

internal sealed class SqliteUserSettingsRepository : IUserSettingsRepository
{
    private readonly KairudevDbContext _context;

    public SqliteUserSettingsRepository(KairudevDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings> GetByUserIdAsync(UserId userId)
    {
        // PK is the UserId string value — use FindAsync with the raw string
        var settings = await _context.Set<UserSettings>()
            .FindAsync(userId);

        if (settings is null)
        {
            // Create default settings if none exist for this user
            settings = UserSettings.CreateDefault(userId);
            _context.Set<UserSettings>().Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task UpdateAsync(UserSettings settings)
    {
        _context.Set<UserSettings>().Update(settings);
        await _context.SaveChangesAsync();
    }
}
