using KairuFocus.Application.Common;
using KairuFocus.Application.Journal.Commands.CreateEntry;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.CompleteSession;

public sealed class CompleteSessionCommandHandler : ICommandHandler<CompleteSessionCommand, CompleteSessionResult>
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompleteSessionCommandHandler> _logger;

    public CompleteSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<CompleteSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CompleteSessionResult> Handle(
        CompleteSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Completing active session for user {UserId}", userId);

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("No active session found for user {UserId}", userId);
            return CompleteSessionResult.Failure(PomodoroErrors.Pomodoro.NoActiveSession);
        }

        var result = session.Complete(DateTime.UtcNow);
        if (result.IsFailure)
            return CompleteSessionResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Session {SessionId} of type {SessionType} completed for user {UserId}", session.Id.Value, session.SessionType, userId);

        var eventType = session.SessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintCompleted
            : JournalEventType.BreakCompleted;

        // Generate journal entry
        await _mediator.DispatchAsync<CreateEntryCommand, CreateEntryResult>(
            new CreateEntryCommand(
                eventType,
                session.Id.Value,
                DateTime.UtcNow,
                userId,
                session.JournalComment),
            cancellationToken);

        return CompleteSessionResult.Success();
    }
}
