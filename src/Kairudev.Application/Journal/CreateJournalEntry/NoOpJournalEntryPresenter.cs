using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.CreateJournalEntry;

public sealed class NoOpJournalEntryPresenter : ICreateJournalEntryPresenter
{
    public void PresentSuccess(JournalEntryViewModel entry) { }
    public void PresentFailure(string reason) { }
}
