using KairuFocus.Application.Common;
using KairuFocus.Application.Journal.Commands.CreateEntry;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.InterruptSession;

public sealed class InterruptSessionCommandHandler : ICommandHandler<InterruptSessionCommand, InterruptSessionResult>
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InterruptSessionCommandHandler> _logger;

    public InterruptSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<InterruptSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<InterruptSessionResult> Handle(
        InterruptSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Interrupting active session for user {UserId}", userId);

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("No active session found for user {UserId}", userId);
            return InterruptSessionResult.Failure(PomodoroErrors.Pomodoro.NoActiveSession);
        }

        var result = session.Interrupt(DateTime.UtcNow);
        if (result.IsFailure)
            return InterruptSessionResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Session {SessionId} of type {SessionType} interrupted for user {UserId}", session.Id.Value, session.SessionType, userId);

        var eventType = session.SessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintInterrupted
            : JournalEventType.BreakInterrupted;

        // Generate journal entry
        await _mediator.DispatchAsync<CreateEntryCommand, CreateEntryResult>(
            new CreateEntryCommand(
                eventType,
                session.Id.Value,
                DateTime.UtcNow,
                userId,
                session.JournalComment),
            cancellationToken);

        return InterruptSessionResult.Success();
    }
}
