using KairuFocus.Application.Common;
using KairuFocus.Application.Journal.Commands.CreateEntry;
using KairuFocus.Application.Pomodoro.Common;
using KairuFocus.Domain.Identity;
using KairuFocus.Domain.Journal;
using KairuFocus.Domain.Pomodoro;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.StartSession;

public sealed class StartSessionCommandHandler : ICommandHandler<StartSessionCommand, StartSessionResult>
{
    /// <summary>
    /// Time window during which a start request for the very same session type is treated as a
    /// re-submission of the same user action (double-click, retry after a slow response) instead
    /// of a conflict. Kept deliberately short: beyond it, an already active session is a genuine
    /// conflict, not a duplicate click.
    /// </summary>
    private static readonly TimeSpan IdempotencyWindow = TimeSpan.FromSeconds(5);

    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly IPomodoroSettingsRepository _settingsRepository;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<StartSessionCommandHandler> _logger;

    public StartSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        IPomodoroSettingsRepository settingsRepository,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<StartSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _settingsRepository = settingsRepository;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<StartSessionResult> Handle(
        StartSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Starting session of type {SessionType} for user {UserId}", command.SessionType, userId);

        // Parse session type — sprint libre forces Sprint type.
        // Resolved before the conflict check: idempotence compares the effective type.
        var sessionType = command.IsFreeSession
            ? PomodoroSessionType.Sprint
            : string.IsNullOrWhiteSpace(command.SessionType) ||
              !Enum.TryParse<PomodoroSessionType>(command.SessionType, true, out var parsedType)
                ? PomodoroSessionType.Sprint
                : parsedType;

        var existingSession = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (existingSession is not null)
        {
            // Compared against a timestamp sampled after the read, never against an older one:
            // the active row may have been inserted while this read was in flight, which would
            // make its age negative and defeat the lower bound below.
            if (IsSameStartResubmitted(existingSession, sessionType, command.IsFreeSession, DateTime.UtcNow))
            {
                // Not an error: the same start was submitted twice within the idempotency window
                // (double-click / retry). Returning the existing session keeps the caller in sync
                // instead of showing a spurious failure while a session is in fact running.
                _logger.LogInformation(
                    "Duplicate start ignored for user {UserId}: returning active session {SessionId} of type {SessionType}",
                    userId, existingSession.Id.Value, sessionType);
                return StartSessionResult.Success(PomodoroSessionViewModel.From(existingSession));
            }

            _logger.LogWarning("Session already active for user {UserId}", userId);
            return StartSessionResult.Failure(PomodoroErrors.Pomodoro.SessionAlreadyActive);
        }

        var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);

        // Sprint libre: duration = 0 (free end). Regular: read from settings.
        var durationMinutes = command.IsFreeSession
            ? 0
            : sessionType switch
            {
                PomodoroSessionType.Sprint => settings.SprintDurationMinutes,
                PomodoroSessionType.ShortBreak => settings.ShortBreakDurationMinutes,
                PomodoroSessionType.LongBreak => settings.LongBreakDurationMinutes,
                _ => settings.SprintDurationMinutes
            };

        var journalComment = command.IsFreeSession ? command.JournalComment : null;
        var session = PomodoroSession.Create(sessionType, durationMinutes, userId, journalComment);

        // Sampled here, as late as possible before the insert, and not on handler entry: the two
        // reads above go through EF's retry policy (up to 5 attempts, ~10s of backoff), so an
        // entry timestamp could be several seconds older than the row actually written. That gap
        // would eat into the idempotency window seen by the next request — which is now measured
        // from a fresh timestamp, so nothing compensates it — and the client, which derives
        // elapsed time from StartedAt, would show a timer already running on open.
        var startedAt = DateTime.UtcNow;
        var startResult = session.Start(startedAt);
        if (startResult.IsFailure)
            return StartSessionResult.Failure(startResult.Error);

        // The read above is a check-then-act: two concurrent starts can both pass it.
        // The "one active session per user" unique index is the real guard; TryAddAsync
        // reports its rejection as false instead of letting a persistence error escape.
        var inserted = await _sessionRepository.TryAddAsync(session, cancellationToken);
        if (!inserted)
            return await HandleRejectedInsertAsync(userId, sessionType, command.IsFreeSession, cancellationToken);

        _logger.LogInformation("Session {SessionId} of type {SessionType} started for user {UserId}", session.Id.Value, sessionType, userId);

        var eventType = sessionType == PomodoroSessionType.Sprint
            ? JournalEventType.SprintStarted
            : JournalEventType.BreakStarted;

        // Generate journal entry — the session is already persisted, so a failing journal
        // must NEVER fail the start (ADR-023), hence log-only error handling.
        try
        {
            // CancellationToken.None: the session is persisted, so a client disconnect must not
            // abort the journal entry (it would be lost with no automatic replay).
            await _mediator.DispatchAsync<CreateEntryCommand, CreateEntryResult>(
                new CreateEntryCommand(
                    eventType,
                    session.Id.Value,
                    DateTime.UtcNow,
                    userId),
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Journal entry failed for session {SessionId}", session.Id.Value);
        }

        return StartSessionResult.Success(PomodoroSessionViewModel.From(session));
    }

    /// <summary>
    /// The insert was rejected by the "at most one Active session per owner" invariant:
    /// another request won the race. Re-read the active session to decide between an idempotent
    /// success (same start, submitted twice) and a genuine conflict.
    /// </summary>
    private async Task<StartSessionResult> HandleRejectedInsertAsync(
        UserId userId,
        PomodoroSessionType sessionType,
        bool isFreeSession,
        CancellationToken cancellationToken)
    {
        var concurrentSession = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (concurrentSession is null)
        {
            // Rejected but nothing readable: the winning row was already closed in between.
            // Fail cleanly rather than letting a persistence error surface as a 500.
            _logger.LogWarning(
                "Session insert rejected for user {UserId} but no active session could be read back",
                userId);
            return StartSessionResult.Failure(PomodoroErrors.Pomodoro.ConcurrentSessionStart);
        }

        // Same reason as above: the winning row was necessarily inserted after this handler
        // started, so its age is measured from a freshly sampled timestamp.
        if (IsSameStartResubmitted(concurrentSession, sessionType, isFreeSession, DateTime.UtcNow))
        {
            _logger.LogInformation(
                "Duplicate start ignored for user {UserId}: returning session {SessionId} created by the concurrent request",
                userId, concurrentSession.Id.Value);
            return StartSessionResult.Success(PomodoroSessionViewModel.From(concurrentSession));
        }

        _logger.LogWarning("Session already active for user {UserId} (detected on insert)", userId);
        return StartSessionResult.Failure(PomodoroErrors.Pomodoro.SessionAlreadyActive);
    }

    /// <summary>
    /// True when the active session can only be the one the caller is trying to start again:
    /// same type, same flavour (free sprint vs. fixed duration), started within the idempotency
    /// window. A free sprint has no planned duration, so returning it in place of a regular
    /// sprint (or the other way round) would hand the caller a session it cannot run.
    /// </summary>
    private static bool IsSameStartResubmitted(
        PomodoroSession active,
        PomodoroSessionType requestedType,
        bool isFreeSession,
        DateTime now)
    {
        if (active.SessionType != requestedType)
            return false;

        if ((active.PlannedDurationMinutes == 0) != isFreeSession)
            return false;

        if (!active.StartedAt.HasValue)
            return false;

        // Lower bound: with clock drift between instances, a StartedAt in the future would
        // otherwise always look like a re-submission, whatever its age.
        var elapsed = now - active.StartedAt.Value;
        return elapsed >= TimeSpan.Zero && elapsed <= IdempotencyWindow;
    }
}
