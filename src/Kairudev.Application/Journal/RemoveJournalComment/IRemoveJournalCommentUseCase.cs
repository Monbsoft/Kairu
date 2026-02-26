namespace Kairudev.Application.Journal.RemoveJournalComment;

public interface IRemoveJournalCommentUseCase
{
    Task Execute(RemoveJournalCommentRequest request, CancellationToken cancellationToken = default);
}
