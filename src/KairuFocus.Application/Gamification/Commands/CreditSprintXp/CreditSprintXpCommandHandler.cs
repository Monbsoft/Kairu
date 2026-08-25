using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using GamificationErrors = KairuFocus.Domain.Gamification.DomainErrors;

namespace KairuFocus.Application.Gamification.Commands.CreditSprintXp;

public sealed class CreditSprintXpCommandHandler : ICommandHandler<CreditSprintXpCommand, CreditSprintXpResult>
{
    private readonly IXpGainRepository _xpGainRepository;
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ILogger<CreditSprintXpCommandHandler> _logger;

    public CreditSprintXpCommandHandler(
        IXpGainRepository xpGainRepository,
        IPomodoroSessionRepository sessionRepository,
        ILogger<CreditSprintXpCommandHandler> logger)
    {
        _xpGainRepository = xpGainRepository;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<CreditSprintXpResult> Handle(
        CreditSprintXpCommand command,
        CancellationToken cancellationToken = default)
    {
        // Idempotence pre-check: re-dispatching for an already-credited session is a no-op.
        // The unique index on XpGains.SessionId is the last line of defense against a race.
        if (await _xpGainRepository.ExistsForSessionAsync(command.SessionId, cancellationToken))
        {
            _logger.LogDebug("XP already credited for session {SessionId}", command.SessionId);
            return CreditSprintXpResult.Success(0);
        }

        var session = await _sessionRepository.GetByIdAsync(PomodoroSessionId.From(command.SessionId), cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("Session {SessionId} not found for XP credit", command.SessionId);
            return CreditSprintXpResult.Failure(GamificationErrors.Gamification.SessionNotFound);
        }

        var amount = SprintXpPolicy.Evaluate(session.SessionType, session.Status, session.StartedAt, session.EndedAt);
        if (amount == 0)
            return CreditSprintXpResult.Success(0);

        // Eligibility implies EndedAt has a value (policy returns 0 when dates are missing).
        var gain = XpGain.Credit(session.OwnerId, command.SessionId, amount, session.EndedAt!.Value);
        await _xpGainRepository.AddAsync(gain, cancellationToken);

        _logger.LogInformation(
            "Credited {Amount} XP to user {UserId} for session {SessionId}",
            amount, session.OwnerId, command.SessionId);

        return CreditSprintXpResult.Success(amount);
    }
}
