using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Settings;

namespace KairuFocus.Application.Tests.Common;

internal sealed class FakeUserSettingsRepository : IUserSettingsRepository
{
    public UserSettings Settings { get; set; } = UserSettings.CreateDefault(FakeCurrentUserService.TestUserId);
    public int UpdateCallCount { get; private set; }

    public Task<UserSettings> GetByUserIdAsync(UserId userId) => Task.FromResult(Settings);

    public Task UpdateAsync(UserSettings settings)
    {
        Settings = settings;
        UpdateCallCount++;
        return Task.CompletedTask;
    }
}
