using Kairudev.Application.Common;
using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;
using Kairudev.Domain.Pomodoro;
using Kairudev.Domain.Tasks;

namespace Kairudev.Application.Journal.Queries.GetTodayJournal;

public sealed class GetTodayJournalQueryHandler
{
    private readonly IJournalEntryRepository _repository;
    private readonly IPomodoroSessionRepository _sessionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTodayJournalQueryHandler(
        IJournalEntryRepository repository,
        IPomodoroSessionRepository sessionRepository,
        ITaskRepository taskRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetTodayJournalResult> HandleAsync(
        GetTodayJournalQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var entries = await _repository.GetEntriesByDateAsync(DateOnly.FromDateTime(DateTime.UtcNow), userId, cancellationToken);
        var viewModels = await JournalEntryMapper.MapToViewModelsAsync(
            entries, _sessionRepository, _taskRepository, userId, cancellationToken);

        return new GetTodayJournalResult(viewModels);
    }
}
