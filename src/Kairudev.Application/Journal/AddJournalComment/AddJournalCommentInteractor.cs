using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.AddJournalComment;

public sealed class AddJournalCommentInteractor : IAddJournalCommentUseCase
{
    private readonly IJournalEntryRepository _repository;
    private readonly IAddJournalCommentPresenter _presenter;

    public AddJournalCommentInteractor(IJournalEntryRepository repository, IAddJournalCommentPresenter presenter)
    {
        _repository = repository;
        _presenter = presenter;
    }

    public async Task Execute(AddJournalCommentRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(JournalEntryId.From(request.EntryId), cancellationToken);
        if (entry is null)
        {
            _presenter.PresentNotFound();
            return;
        }

        var result = entry.AddComment(request.Text);
        if (result.IsFailure)
        {
            _presenter.PresentValidationError(result.Error);
            return;
        }

        await _repository.UpdateAsync(entry, cancellationToken);
        _presenter.PresentSuccess(JournalEntryViewModel.From(entry));
    }
}
