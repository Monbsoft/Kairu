using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.GetTodayJournal;

public sealed class GetTodayJournalInteractor : IGetTodayJournalUseCase
{
    private readonly IJournalEntryRepository _repository;
    private readonly IGetTodayJournalPresenter _presenter;

    public GetTodayJournalInteractor(IJournalEntryRepository repository, IGetTodayJournalPresenter presenter)
    {
        _repository = repository;
        _presenter = presenter;
    }

    public async Task Execute(GetTodayJournalRequest request, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var entries = await _repository.GetTodayEntriesAsync(today, cancellationToken);
        if (entries.Count == 0)
        {
            _presenter.PresentEmpty();
            return;
        }
        _presenter.PresentSuccess(entries.Select(JournalEntryViewModel.From).ToList().AsReadOnly());
    }
}
