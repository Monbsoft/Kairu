using KairuFocus.Application.Common;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.UnlinkTask;

public sealed class UnlinkTaskCommandHandler : ICommandHandler<UnlinkTaskCommand, UnlinkTaskResult>
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UnlinkTaskCommandHandler> _logger;

    public UnlinkTaskCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        ICurrentUserService currentUserService,
        ILogger<UnlinkTaskCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UnlinkTaskResult> Handle(
        UnlinkTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Unlinking task {TaskId} from active session for user {UserId}", command.TaskId, userId);

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("No active session found for user {UserId}", userId);
            return UnlinkTaskResult.Failure(PomodoroErrors.Pomodoro.NoActiveSession);
        }

        var result = session.UnlinkTask(TaskId.From(command.TaskId));
        if (result.IsFailure)
        {
            if (result.Error == PomodoroErrors.Pomodoro.TaskNotLinked)
                return UnlinkTaskResult.NotFound();
            return UnlinkTaskResult.Failure(result.Error);
        }

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Task {TaskId} unlinked from session {SessionId} for user {UserId}", command.TaskId, session.Id.Value, userId);
        return UnlinkTaskResult.Success();
    }
}
