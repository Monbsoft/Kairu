using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Application.Pomodoro.Common;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Commands.StartSession;

public sealed class StartSessionCommandHandler
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;
    private readonly CreateEntryCommandHandler _journalHandler;

    public StartSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository,
        CreateEntryCommandHandler journalHandler)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
        _journalHandler = journalHandler;
    }

    public async Task<StartSessionResult> HandleAsync(
        StartSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingSession = await _sessionRepository.GetActiveAsync(cancellationToken);
        if (existingSession is not null)
            return StartSessionResult.Failure("A session is already active");

        var settings = await _settingsRepository.GetAsync(cancellationToken);

        // Parse session type
        var sessionType = string.IsNullOrWhiteSpace(command.SessionType) ||
                          !Enum.TryParse<PomodoroSessionType>(command.SessionType, true, out var parsedType)
            ? PomodoroSessionType.Sprint
            : parsedType;

        // Determine duration based on type (in minutes)
        var durationMinutes = sessionType switch
        {
            PomodoroSessionType.Sprint => settings.SprintDurationMinutes,
            PomodoroSessionType.ShortBreak => settings.ShortBreakDurationMinutes,
            PomodoroSessionType.LongBreak => settings.LongBreakDurationMinutes,
            _ => settings.SprintDurationMinutes
        };

        var session = PomodoroSession.Create(sessionType, durationMinutes);
        var startResult = session.Start(DateTime.UtcNow);
        if (startResult.IsFailure)
            return StartSessionResult.Failure(startResult.Error);

        await _sessionRepository.AddAsync(session, cancellationToken);

        var eventType = sessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintStarted
            : JournalEventType.BreakStarted;

        // Generate journal entry
        await _journalHandler.HandleAsync(
            new CreateEntryCommand(
                eventType,
                session.Id.Value,
                DateTime.UtcNow),
            cancellationToken);

        return StartSessionResult.Success(PomodoroSessionViewModel.From(session));
    }
}
