using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.GetTodayJournal;

public interface IGetTodayJournalPresenter
{
    void PresentSuccess(IReadOnlyList<JournalEntryViewModel> entries);
    void PresentEmpty();
}
