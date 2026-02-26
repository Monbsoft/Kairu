using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.RemoveJournalComment;

public sealed class RemoveJournalCommentInteractor : IRemoveJournalCommentUseCase
{
    private readonly IJournalEntryRepository _repository;
    private readonly IRemoveJournalCommentPresenter _presenter;

    public RemoveJournalCommentInteractor(IJournalEntryRepository repository, IRemoveJournalCommentPresenter presenter)
    {
        _repository = repository;
        _presenter = presenter;
    }

    public async Task Execute(RemoveJournalCommentRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(JournalEntryId.From(request.EntryId), cancellationToken);
        if (entry is null)
        {
            _presenter.PresentNotFound();
            return;
        }

        var result = entry.RemoveComment(JournalCommentId.From(request.CommentId));
        if (result.IsFailure)
        {
            _presenter.PresentFailure(result.Error);
            return;
        }

        await _repository.UpdateAsync(entry, cancellationToken);
        _presenter.PresentSuccess();
    }
}
