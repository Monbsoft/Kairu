using Kairudev.Application.Common;
using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Commands.InterruptSession;

public sealed class InterruptSessionCommandHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly CreateEntryCommandHandler _journalHandler;
    private readonly ICurrentUserService _currentUserService;

    public InterruptSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        CreateEntryCommandHandler journalHandler,
        ICurrentUserService currentUserService)
    {
        _sessionRepository = sessionRepository;
        _journalHandler = journalHandler;
        _currentUserService = currentUserService;
    }

    public async Task<InterruptSessionResult> HandleAsync(
        InterruptSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
            return InterruptSessionResult.Failure("No active session");

        var result = session.Interrupt(DateTime.UtcNow);
        if (result.IsFailure)
            return InterruptSessionResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        var eventType = session.SessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintInterrupted
            : JournalEventType.BreakInterrupted;

        // Generate journal entry
        await _journalHandler.HandleAsync(
            new CreateEntryCommand(
                eventType,
                session.Id.Value,
                DateTime.UtcNow,
                userId),
            cancellationToken);

        return InterruptSessionResult.Success();
    }
}
