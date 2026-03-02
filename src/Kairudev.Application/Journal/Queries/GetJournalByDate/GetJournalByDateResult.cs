using Kairudev.Application.Journal.Common;

namespace Kairudev.Application.Journal.Queries.GetJournalByDate;

public sealed record GetJournalByDateResult(IReadOnlyList<JournalEntryViewModel> Entries);
