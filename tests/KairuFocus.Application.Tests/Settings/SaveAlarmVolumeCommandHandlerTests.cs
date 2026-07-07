using KairuFocus.Application.Settings.Commands.SaveAlarmVolume;
using KairuFocus.Application.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace KairuFocus.Application.Tests.Settings;

public sealed class SaveAlarmVolumeCommandHandlerTests
{
    private readonly FakeUserSettingsRepository _repository = new();
    private readonly SaveAlarmVolumeCommandHandler _sut;

    public SaveAlarmVolumeCommandHandlerTests()
    {
        _sut = new SaveAlarmVolumeCommandHandler(
            _repository,
            new FakeCurrentUserService(),
            NullLogger<SaveAlarmVolumeCommandHandler>.Instance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Should_PersistVolume_When_WithinBounds(int volume)
    {
        var result = await _sut.Handle(new SaveAlarmVolumeCommand(volume));

        Assert.True(result.IsSuccess);
        Assert.Equal(volume, _repository.Settings.AlarmVolume);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Should_ReturnFailure_When_OutOfBounds(int volume)
    {
        var result = await _sut.Handle(new SaveAlarmVolumeCommand(volume));

        Assert.False(result.IsSuccess);
        Assert.Equal(0, _repository.UpdateCallCount);
    }
}
