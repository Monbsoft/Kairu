using Kairudev.Application.Journal.Common;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.UpdateJournalComment;

public sealed class UpdateJournalCommentInteractor : IUpdateJournalCommentUseCase
{
    private readonly IJournalEntryRepository _repository;
    private readonly IUpdateJournalCommentPresenter _presenter;

    public UpdateJournalCommentInteractor(IJournalEntryRepository repository, IUpdateJournalCommentPresenter presenter)
    {
        _repository = repository;
        _presenter = presenter;
    }

    public async Task Execute(UpdateJournalCommentRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await _repository.GetByIdAsync(JournalEntryId.From(request.EntryId), cancellationToken);
        if (entry is null)
        {
            _presenter.PresentNotFound();
            return;
        }

        var result = entry.UpdateComment(JournalCommentId.From(request.CommentId), request.Text);
        if (result.IsFailure)
        {
            _presenter.PresentFailure(result.Error);
            return;
        }

        await _repository.UpdateAsync(entry, cancellationToken);
        _presenter.PresentSuccess(JournalEntryViewModel.From(entry));
    }
}
