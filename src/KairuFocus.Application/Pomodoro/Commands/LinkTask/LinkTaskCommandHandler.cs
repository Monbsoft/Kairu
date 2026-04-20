using KairuFocus.Application.Common;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.LinkTask;

public sealed class LinkTaskCommandHandler : ICommandHandler<LinkTaskCommand, LinkTaskResult>
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LinkTaskCommandHandler> _logger;

    public LinkTaskCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUserService,
        ILogger<LinkTaskCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LinkTaskResult> Handle(
        LinkTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Linking task {TaskId} to active session for user {UserId}", command.TaskId, userId);

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("No active session found for user {UserId}", userId);
            return LinkTaskResult.Failure(PomodoroErrors.Pomodoro.NoActiveSession);
        }

        var task = await _taskRepository.GetByIdAsync(TaskId.From(command.TaskId), userId, cancellationToken);
        if (task is null)
        {
            _logger.LogWarning("Task {TaskId} not found for user {UserId}", command.TaskId, userId);
            return LinkTaskResult.NotFound();
        }

        var result = session.LinkTask(task.Id);
        if (result.IsFailure)
            return LinkTaskResult.Failure(result.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Task {TaskId} linked to session {SessionId} for user {UserId}", command.TaskId, session.Id.Value, userId);
        return LinkTaskResult.Success();
    }
}
