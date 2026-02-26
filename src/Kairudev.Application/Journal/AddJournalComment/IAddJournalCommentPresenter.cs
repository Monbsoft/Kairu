using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.AddJournalComment;

public interface IAddJournalCommentPresenter
{
    void PresentSuccess(JournalEntryViewModel entry);
    void PresentNotFound();
    void PresentValidationError(string error);
    void PresentFailure(string reason);
}
