namespace Kairudev.Application.Journal.AddJournalComment;

public interface IAddJournalCommentUseCase
{
    Task Execute(AddJournalCommentRequest request, CancellationToken cancellationToken = default);
}
