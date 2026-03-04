using Kairudev.Application.Common;
using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Commands.CompleteSession;

public sealed class CompleteSessionCommandHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;
    private readonly CreateEntryCommandHandler _journalHandler;
    private readonly ICurrentUserService _currentUserService;

    public CompleteSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository,
        CreateEntryCommandHandler journalHandler,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
        _journalHandler = journalHandler;
        _currentUserService = currentUserService;
    }

    public async Task<CompleteSessionResult> HandleAsync(
        CompleteSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
            return CompleteSessionResult.Failure("No active session");

        var result = session.Complete(DateTime.UtcNow);
        if (result.IsFailure)
            return CompleteSessionResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        var eventType = session.SessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintCompleted
            : JournalEventType.BreakCompleted;

        // Generate journal entry
        await _journalHandler.HandleAsync(
            new CreateEntryCommand(
                eventType,
                session.Id.Value,
                DateTime.UtcNow,
                userId),
            cancellationToken);

        return CompleteSessionResult.Success();
    }
}
