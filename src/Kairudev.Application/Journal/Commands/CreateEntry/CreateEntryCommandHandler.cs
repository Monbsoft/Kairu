using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.Commands.CreateEntry;

public sealed class CreateEntryCommandHandler
{
    private readonly IJournalEntryRepository _repository;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public CreateEntryCommandHandler(IJournalEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateEntryResult> HandleAsync(
        CreateEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            int? sequence = null;
            var today = DateOnly.FromDateTime(command.OccurredAt);

            if (command.EventType == JournalEventType.BreakCompleted)
            {
                var count = await _repository.GetTodayCountByTypeAsync(JournalEventType.BreakCompleted, today, command.OwnerId, cancellationToken);
                sequence = count + 1;
            }
            else if (command.EventType == JournalEventType.SprintStarted)
            {
                var count = await _repository.GetTodayCountByTypeAsync(JournalEventType.SprintStarted, today, command.OwnerId, cancellationToken);
                sequence = count + 1;
            }
            else if (command.EventType is JournalEventType.SprintCompleted or JournalEventType.SprintInterrupted)
            {
                // Le sprint était déjà démarré : count(SprintStarted today) = numéro du sprint courant
                var count = await _repository.GetTodayCountByTypeAsync(JournalEventType.SprintStarted, today, command.OwnerId, cancellationToken);
                if (count > 0)
                    sequence = count;
            }

            var entry = JournalEntry.Create(command.EventType, command.ResourceId, command.OccurredAt, command.OwnerId, sequence);
            await _repository.AddAsync(entry, cancellationToken);
            return CreateEntryResult.Success();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
