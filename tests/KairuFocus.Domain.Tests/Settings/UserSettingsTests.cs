using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Settings;

namespace KairuFocus.Domain.Tests.Settings;

public sealed class UserSettingsTests
{
    private static UserSettings CreateSut() =>
        UserSettings.CreateDefault(UserId.From(Guid.NewGuid()));

    [Fact]
    public void Should_DefaultAlarmVolumeTo80_When_CreatedWithDefaults()
    {
        var settings = CreateSut();

        Assert.Equal(80, settings.AlarmVolume);
    }

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(150, 100)]
    [InlineData(70, 70)]
    [InlineData(0, 0)]
    [InlineData(100, 100)]
    public void Should_ClampAlarmVolume_When_Updating(int input, int expected)
    {
        var settings = CreateSut();

        settings.UpdateAlarmVolume(input);

        Assert.Equal(expected, settings.AlarmVolume);
    }
}
