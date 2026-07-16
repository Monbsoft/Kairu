using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Pomodoro;

namespace KairuFocus.Domain.Tests.Gamification;

public sealed class SprintXpPolicyTests
{
    private static readonly DateTime Start = new(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Should_Return10_When_CompletedSprintOfAtLeast5Minutes()
    {
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Completed,
            Start,
            Start.AddMinutes(25));

        Assert.Equal(10, amount);
    }

    [Fact]
    public void Should_Return10_When_DurationIsExactly5Minutes()
    {
        // Boundary: exactly 5:00 is eligible (>=).
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Completed,
            Start,
            Start.AddMinutes(5));

        Assert.Equal(10, amount);
    }

    [Fact]
    public void Should_Return10_When_FreeSprintOfAtLeast5Minutes()
    {
        // Free sprints share the same facts (type Sprint, status Completed):
        // the rule is uniform for standard and free sprints.
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Completed,
            Start,
            Start.AddMinutes(7).AddSeconds(31));

        Assert.Equal(10, amount);
    }

    [Fact]
    public void Should_ReturnZero_When_DurationUnder5Minutes()
    {
        // Boundary: 4:59 is not eligible.
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Completed,
            Start,
            Start.AddMinutes(4).AddSeconds(59));

        Assert.Equal(0, amount);
    }

    [Theory]
    [InlineData(PomodoroSessionType.ShortBreak)]
    [InlineData(PomodoroSessionType.LongBreak)]
    public void Should_ReturnZero_When_SessionIsBreak(PomodoroSessionType breakType)
    {
        var amount = SprintXpPolicy.Evaluate(
            breakType,
            PomodoroSessionStatus.Completed,
            Start,
            Start.AddMinutes(25));

        Assert.Equal(0, amount);
    }

    [Fact]
    public void Should_ReturnZero_When_SessionInterrupted()
    {
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Interrupted,
            Start,
            Start.AddMinutes(25));

        Assert.Equal(0, amount);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void Should_ReturnZero_When_DatesMissing(bool hasStart, bool hasEnd)
    {
        var amount = SprintXpPolicy.Evaluate(
            PomodoroSessionType.Sprint,
            PomodoroSessionStatus.Completed,
            hasStart ? Start : null,
            hasEnd ? Start.AddMinutes(25) : null);

        Assert.Equal(0, amount);
    }
}
