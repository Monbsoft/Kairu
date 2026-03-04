using Kairudev.Application.Common;
using Kairudev.Application.Pomodoro.Common;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Queries.GetCurrentSession;

public sealed class GetCurrentSessionQueryHandler
{
    private readonly IPomodoroSessionRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentSessionQueryHandler(IPomodoroSessionRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetCurrentSessionResult> HandleAsync(
        GetCurrentSessionQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var session = await _repository.GetActiveAsync(userId, cancellationToken);

        return session is not null
            ? GetCurrentSessionResult.WithSession(PomodoroSessionViewModel.From(session))
            : GetCurrentSessionResult.NoSession();
    }
}
