using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;

namespace KairuFocus.Domain.Tests.Gamification;

public sealed class XpGainTests
{
    private static readonly UserId Owner = UserId.From(new Guid("00000000-0000-0000-0000-000000000001"));
    private static readonly DateTime EarnedAtUtc = new(2026, 7, 15, 10, 25, 0, DateTimeKind.Utc);

    [Fact]
    public void Should_CreateGain_When_ArgumentsAreValid()
    {
        var sessionId = Guid.NewGuid();

        var gain = XpGain.Credit(Owner, sessionId, 10, EarnedAtUtc);

        Assert.NotEqual(Guid.Empty, gain.Id.Value);
        Assert.Equal(Owner, gain.OwnerId);
        Assert.Equal(sessionId, gain.SessionId);
        Assert.Equal(10, gain.Amount);
        Assert.Equal(EarnedAtUtc, gain.EarnedAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Throw_When_AmountIsNotPositive(int amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => XpGain.Credit(Owner, Guid.NewGuid(), amount, EarnedAtUtc));
    }

    [Fact]
    public void Should_Throw_When_EarnedAtUtcIsDefault()
    {
        Assert.Throws<ArgumentException>(
            () => XpGain.Credit(Owner, Guid.NewGuid(), 10, default));
    }
}
