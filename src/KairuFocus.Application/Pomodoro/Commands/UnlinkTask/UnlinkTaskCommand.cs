using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace KairuFocus.Application.Pomodoro.Commands.UnlinkTask;

public sealed record UnlinkTaskCommand(Guid TaskId) : ICommand<UnlinkTaskResult>;
