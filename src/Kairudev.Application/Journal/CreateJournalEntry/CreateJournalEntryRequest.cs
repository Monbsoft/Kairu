using Kairudev.Domain.Journal;

namespace Kairudev.Application.Journal.CreateJournalEntry;

public sealed record CreateJournalEntryRequest(
    JournalEventType EventType,
    Guid ResourceId,
    DateTime OccurredAt);
