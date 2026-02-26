namespace Kairudev.Application.Journal.CreateJournalEntry;

public interface ICreateJournalEntryUseCase
{
    Task Execute(CreateJournalEntryRequest request, CancellationToken cancellationToken = default);
}
