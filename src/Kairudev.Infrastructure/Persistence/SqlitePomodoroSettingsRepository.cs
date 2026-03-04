using Kairudev.Domain.Identity;
using Kairudev.Domain.Pomodoro;
using Kairudev.Infrastructure.Persistence.Internal;
using Microsoft.EntityFrameworkCore;

namespace Kairudev.Infrastructure.Persistence;

internal sealed class SqlitePomodoroSettingsRepository : IPomodoroSettingsRepository
{
    private readonly KairudevDbContext _context;

    public SqlitePomodoroSettingsRepository(KairudevDbContext context)
    {
        _context = context;
    }

    public async Task<PomodoroSettings> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var row = await _context.PomodoroSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (row is null)
            return PomodoroSettings.Default;

        return PomodoroSettings.Create(
            row.SprintDurationMinutes,
            row.ShortBreakDurationMinutes,
            row.LongBreakDurationMinutes).Value;
    }

    public async Task SaveAsync(PomodoroSettings settings, UserId userId, CancellationToken cancellationToken = default)
    {
        var row = await _context.PomodoroSettings
            .FirstOrDefaultAsync(s => s.UserId == userId.Value, cancellationToken);

        if (row is null)
        {
            row = new PomodoroSettingsRow
            {
                UserId = userId.Value,
                SprintDurationMinutes = settings.SprintDurationMinutes,
                ShortBreakDurationMinutes = settings.ShortBreakDurationMinutes,
                LongBreakDurationMinutes = settings.LongBreakDurationMinutes
            };
            await _context.PomodoroSettings.AddAsync(row, cancellationToken);
        }
        else
        {
            row.SprintDurationMinutes = settings.SprintDurationMinutes;
            row.ShortBreakDurationMinutes = settings.ShortBreakDurationMinutes;
            row.LongBreakDurationMinutes = settings.LongBreakDurationMinutes;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
