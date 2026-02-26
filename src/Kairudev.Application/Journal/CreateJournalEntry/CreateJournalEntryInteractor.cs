using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.CreateJournalEntry;

public sealed class CreateJournalEntryInteractor : ICreateJournalEntryUseCase
{
    private readonly IJournalEntryRepository _repository;
    private readonly ICreateJournalEntryPresenter _presenter;

    public CreateJournalEntryInteractor(IJournalEntryRepository repository, ICreateJournalEntryPresenter presenter)
    {
        _repository = repository;
        _presenter = presenter;
    }

    public async Task Execute(CreateJournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = JournalEntry.Create(request.EventType, request.ResourceId, request.OccurredAt);
        await _repository.AddAsync(entry, cancellationToken);
        _presenter.PresentSuccess(JournalEntryViewModel.From(entry));
    }
}
