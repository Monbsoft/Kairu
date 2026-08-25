using KairuFocus.Application.Common;
using KairuFocus.Application.Gamification.Common;
using KairuFocus.Domain.Gamification;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace KairuFocus.Application.Gamification.Queries.GetXpSummary;

public sealed class GetXpSummaryQueryHandler : IQueryHandler<GetXpSummaryQuery, GetXpSummaryResult>
{
    private readonly IXpGainRepository _xpGainRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<GetXpSummaryQueryHandler> _logger;

    public GetXpSummaryQueryHandler(
        IXpGainRepository xpGainRepository,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider,
        ILogger<GetXpSummaryQueryHandler> logger)
    {
        _xpGainRepository = xpGainRepository;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<GetXpSummaryResult> Handle(
        GetXpSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var window = UtcWeekWindow.From(_timeProvider.GetUtcNow().UtcDateTime);

        var totalXp = await _xpGainRepository.GetTotalXpAsync(userId, cancellationToken);
        var weekXp = await _xpGainRepository.GetXpBetweenAsync(userId, window.StartUtc, window.EndUtc, cancellationToken);

        _logger.LogDebug(
            "XP summary for user {UserId}: total={TotalXp}, week={WeekXp} ([{StartUtc:O}, {EndUtc:O}))",
            userId, totalXp, weekXp, window.StartUtc, window.EndUtc);

        return new GetXpSummaryResult(totalXp, weekXp);
    }
}
