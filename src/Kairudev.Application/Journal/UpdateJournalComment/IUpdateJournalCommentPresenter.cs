using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.UpdateJournalComment;

public interface IUpdateJournalCommentPresenter
{
    void PresentSuccess(JournalEntryViewModel entry);
    void PresentNotFound();
    void PresentValidationError(string error);
    void PresentFailure(string reason);
}
