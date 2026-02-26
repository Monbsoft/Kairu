namespace Kairudev.Application.Journal.UpdateJournalComment;

public interface IUpdateJournalCommentUseCase
{
    Task Execute(UpdateJournalCommentRequest request, CancellationToken cancellationToken = default);
}
