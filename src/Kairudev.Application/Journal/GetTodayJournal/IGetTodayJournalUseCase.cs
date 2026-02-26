namespace Kairudev.Application.Journal.GetTodayJournal;

public interface IGetTodayJournalUseCase
{
    Task Execute(GetTodayJournalRequest request, CancellationToken cancellationToken = default);
}
