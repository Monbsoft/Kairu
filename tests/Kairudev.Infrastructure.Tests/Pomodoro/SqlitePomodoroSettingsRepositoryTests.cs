using Kairudev.Domain.Identity;
using Kairudev.Domain.Pomodoro;
using Kairudev.Infrastructure.Persistence;

namespace Kairudev.Infrastructure.Tests.Pomodoro;

public sealed class SqlitePomodoroSettingsRepositoryTests : InfrastructureTestBase
{
    private static readonly UserId OwnerId = UserId.From("test-user");

    private readonly SqlitePomodoroSettingsRepository _repository;

    public SqlitePomodoroSettingsRepositoryTests()
    {
        _repository = new SqlitePomodoroSettingsRepository(Context);
    }

    [Fact]
    public async Task Should_ReturnDefaults_When_NoSettingsPersisted()
    {
        var settings = await _repository.GetByUserIdAsync(OwnerId);

        Assert.Equal(PomodoroSettings.Default.SprintDurationMinutes, settings.SprintDurationMinutes);
        Assert.Equal(PomodoroSettings.Default.ShortBreakDurationMinutes, settings.ShortBreakDurationMinutes);
        Assert.Equal(PomodoroSettings.Default.LongBreakDurationMinutes, settings.LongBreakDurationMinutes);
    }

    [Fact]
    public async Task Should_PersistSettings_When_Saved()
    {
        var settings = PomodoroSettings.Create(30, 10, 20).Value;

        await _repository.SaveAsync(settings, OwnerId);

        var stored = await _repository.GetByUserIdAsync(OwnerId);
        Assert.Equal(30, stored.SprintDurationMinutes);
        Assert.Equal(10, stored.ShortBreakDurationMinutes);
        Assert.Equal(20, stored.LongBreakDurationMinutes);
    }

    [Fact]
    public async Task Should_UpdateExistingSettings_When_SavedTwice()
    {
        var initial = PomodoroSettings.Create(25, 5, 15).Value;
        await _repository.SaveAsync(initial, OwnerId);

        var updated = PomodoroSettings.Create(50, 10, 20).Value;
        await _repository.SaveAsync(updated, OwnerId);

        var stored = await _repository.GetByUserIdAsync(OwnerId);
        Assert.Equal(50, stored.SprintDurationMinutes);
        Assert.Equal(10, stored.ShortBreakDurationMinutes);
        Assert.Equal(20, stored.LongBreakDurationMinutes);
    }
}
