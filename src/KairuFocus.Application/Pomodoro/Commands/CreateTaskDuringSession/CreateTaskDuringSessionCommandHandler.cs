using KairuFocus.Application.Common;
using KairuFocus.Application.Tasks.Common;
using KairuFocus.Domain.Pomodoro;
using KairuFocus.Domain.Tasks;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;
using PomodoroErrors = KairuFocus.Domain.Pomodoro.DomainErrors;

namespace KairuFocus.Application.Pomodoro.Commands.CreateTaskDuringSession;

public sealed class CreateTaskDuringSessionCommandHandler : ICommandHandler<CreateTaskDuringSessionCommand, CreateTaskDuringSessionResult>
{
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTaskDuringSessionCommandHandler> _logger;

    public CreateTaskDuringSessionCommandHandler(
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateTaskDuringSessionCommandHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CreateTaskDuringSessionResult> Handle(
        CreateTaskDuringSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        _logger.LogDebug("Creating task during session for user {UserId}", userId);

        var session = await _sessionRepository.GetActiveAsync(userId, cancellationToken);
        if (session is null)
        {
            _logger.LogWarning("No active session found for user {UserId}", userId);
            return CreateTaskDuringSessionResult.Failure(PomodoroErrors.Pomodoro.NoActiveSession);
        }

        if (session.SessionType != PomodoroSessionType.Sprint)
        {
            _logger.LogWarning("Cannot create task during non-sprint session for user {UserId}", userId);
            return CreateTaskDuringSessionResult.Failure(PomodoroErrors.Pomodoro.TaskLinkingNotAllowedForBreak);
        }

        var titleResult = TaskTitle.Create(command.Title);
        if (titleResult.IsFailure)
            return CreateTaskDuringSessionResult.Failure(titleResult.Error);

        var descriptionResult = TaskDescription.Create(command.Description);
        if (descriptionResult.IsFailure)
            return CreateTaskDuringSessionResult.Failure(descriptionResult.Error);

        var task = DeveloperTask.Create(titleResult.Value, descriptionResult.Value, DateTime.UtcNow, userId);
        await _taskRepository.AddAsync(task, cancellationToken);

        var linkResult = session.LinkTask(task.Id);
        if (linkResult.IsFailure)
            return CreateTaskDuringSessionResult.Failure(linkResult.Error);

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        _logger.LogInformation("Task {TaskId} created and linked to session {SessionId} for user {UserId}", task.Id.Value, session.Id.Value, userId);

        return CreateTaskDuringSessionResult.Success(TaskViewModel.From(task));
    }
}
