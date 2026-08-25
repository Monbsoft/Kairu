using KairuFocus.Application.Gamification.Common;

namespace KairuFocus.Application.Tests.Gamification;

public sealed class UtcWeekWindowTests
{
    [Fact]
    public void Should_StartOnPreviousMonday_When_NowIsMidWeek()
    {
        // Wednesday 2026-07-15 14:30 UTC → week [Mon 2026-07-13, Mon 2026-07-20)
        var window = UtcWeekWindow.From(new DateTime(2026, 7, 15, 14, 30, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc), window.StartUtc);
        Assert.Equal(new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), window.EndUtc);
    }

    [Fact]
    public void Should_StartOnSameDay_When_NowIsMondayMidnight()
    {
        // Monday 2026-07-13 00:00 UTC is the first instant of its own week.
        var window = UtcWeekWindow.From(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc), window.StartUtc);
        Assert.Equal(new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), window.EndUtc);
    }

    [Fact]
    public void Should_BelongToCurrentWeek_When_NowIsSundayLastMinute()
    {
        // Sunday 2026-07-19 23:59 UTC still belongs to the week started Monday 2026-07-13.
        var window = UtcWeekWindow.From(new DateTime(2026, 7, 19, 23, 59, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc), window.StartUtc);
        Assert.Equal(new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), window.EndUtc);
    }

    [Fact]
    public void Should_SpanYearBoundary_When_WeekCrossesNewYear()
    {
        // Thursday 2026-01-01 10:00 UTC → week [Mon 2025-12-29, Mon 2026-01-05)
        var window = UtcWeekWindow.From(new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2025, 12, 29, 0, 0, 0, DateTimeKind.Utc), window.StartUtc);
        Assert.Equal(new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc), window.EndUtc);
    }
}
