using KairuFocus.Application.Settings.Queries.GetUserSettings;
using KairuFocus.Application.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace KairuFocus.Application.Tests.Settings;

public sealed class GetUserSettingsQueryHandlerTests
{
    [Fact]
    public async Task Should_MapAlarmVolume_When_FetchingSettings()
    {
        var repository = new FakeUserSettingsRepository();
        repository.Settings.UpdateAlarmVolume(65);
        var sut = new GetUserSettingsQueryHandler(
            repository,
            new FakeCurrentUserService(),
            NullLogger<GetUserSettingsQueryHandler>.Instance);

        var result = await sut.Handle(new GetUserSettingsQuery());

        Assert.Equal(65, result.Settings.AlarmVolume);
    }
}
