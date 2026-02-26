using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.CreateJournalEntry;

public interface ICreateJournalEntryPresenter
{
    void PresentSuccess(JournalEntryViewModel entry);
    void PresentFailure(string reason);
}
