using Kairudev.Application.Common;
using Kairudev.Application.Pomodoro.Common;
using Kairudev.Domain.Pomodoro;

namespace Kairudev.Application.Pomodoro.Queries.GetSettings;

public sealed class GetSettingsQueryHandler
{
    private readonly IPomodoroSettingsRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetSettingsQueryHandler(IPomodoroSettingsRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetSettingsResult> HandleAsync(
        GetSettingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var settings = await _repository.GetByUserIdAsync(userId, cancellationToken);
        var viewModel = PomodoroSettingsViewModel.From(settings);
        return new GetSettingsResult(viewModel);
    }
}
