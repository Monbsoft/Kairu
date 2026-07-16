using KairuFocus.Application.Gamification.Queries.GetXpSummary;
using KairuFocus.Application.Tests.Common;
using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace KairuFocus.Application.Tests.Gamification;

public sealed class GetXpSummaryQueryHandlerTests
{
    // Wednesday 2026-07-15 → UTC calendar week [Mon 2026-07-13 00:00, Mon 2026-07-20 00:00)
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 15, 14, 30, 0, TimeSpan.Zero);

    private readonly FakeXpGainRepository _xpGainRepository = new();
    private readonly GetXpSummaryQueryHandler _sut;

    public GetXpSummaryQueryHandlerTests()
    {
        _sut = new GetXpSummaryQueryHandler(
            _xpGainRepository,
            new FakeCurrentUserService(),
            new FixedTimeProvider(UtcNow),
            NullLogger<GetXpSummaryQueryHandler>.Instance);
    }

    private void AddGain(DateTime earnedAtUtc, UserId? owner = null) =>
        _xpGainRepository.Gains.Add(
            XpGain.Credit(owner ?? FakeCurrentUserService.TestUserId, Guid.NewGuid(), 10, earnedAtUtc));

    [Fact]
    public async Task Should_ReturnZeroTotals_When_LedgerIsEmpty()
    {
        var result = await _sut.Handle(new GetXpSummaryQuery());

        Assert.Equal(0, result.TotalXp);
        Assert.Equal(0, result.WeekXp);
    }

    [Fact]
    public async Task Should_ReturnTotalAndWeekXp_When_GainsSpanSeveralWeeks()
    {
        AddGain(new DateTime(2026, 7, 6, 9, 0, 0, DateTimeKind.Utc));   // previous week
        AddGain(new DateTime(2026, 7, 14, 9, 0, 0, DateTimeKind.Utc));  // current week
        AddGain(new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc));  // current week

        var result = await _sut.Handle(new GetXpSummaryQuery());

        Assert.Equal(30, result.TotalXp);
        Assert.Equal(20, result.WeekXp);
    }

    [Fact]
    public async Task Should_UseUtcCalendarWeekBounds_When_ComputingWeekXp()
    {
        // Inclusive lower bound: Monday 2026-07-13 00:00:00 UTC counts.
        AddGain(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc));
        // Just before the window: Sunday 2026-07-12 23:59:59 UTC does not count.
        AddGain(new DateTime(2026, 7, 12, 23, 59, 59, DateTimeKind.Utc));

        var result = await _sut.Handle(new GetXpSummaryQuery());

        Assert.Equal(20, result.TotalXp);
        Assert.Equal(10, result.WeekXp);
    }

    [Fact]
    public async Task Should_IgnoreOtherUsersGains_When_ComputingTotals()
    {
        AddGain(new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc));
        AddGain(new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc), UserId.New());

        var result = await _sut.Handle(new GetXpSummaryQuery());

        Assert.Equal(10, result.TotalXp);
        Assert.Equal(10, result.WeekXp);
    }
}
