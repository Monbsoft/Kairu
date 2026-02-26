namespace Kairudev.Application.Journal.RemoveJournalComment;

public interface IRemoveJournalCommentPresenter
{
    void PresentSuccess();
    void PresentNotFound();
    void PresentFailure(string reason);
}
