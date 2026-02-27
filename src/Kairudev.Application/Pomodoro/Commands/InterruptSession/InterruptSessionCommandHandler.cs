using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Commands.InterruptSession;

public sealed class InterruptSessionCommandHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly CreateEntryCommandHandler _journalHandler;

    public InterruptSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        CreateEntryCommandHandler journalHandler)
    {
        _sessionRepository = sessionRepository;
        _journalHandler = journalHandler;
    }

    public async Task<InterruptSessionResult> HandleAsync(
        InterruptSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetActiveAsync(cancellationToken);
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
                DateTime.UtcNow),
            cancellationToken);

        return InterruptSessionResult.Success();
    }
}
