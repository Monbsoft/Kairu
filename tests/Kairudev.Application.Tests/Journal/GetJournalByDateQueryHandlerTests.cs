using Kairudev.Application.Journal.Queries.GetJournalByDate;
using Kairudev.Application.Tests.Pomodoro;
using Kairudev.Application.Tests.Tasks;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Tests.Journal;

public sealed class GetJournalByDateQueryHandlerTests
{
    private readonly FakeJournalEntryRepository _journalRepo = new();
    private readonly FakePomodoroSessionRepository _sessionRepo = new();
    private readonly FakeTaskRepository _taskRepo = new();
    private readonly GetJournalByDateQueryHandler _sut;

    public GetJournalByDateQueryHandlerTests() =>
        _sut = new GetJournalByDateQueryHandler(_journalRepo, _sessionRepo, _taskRepo);

    [Fact]
    public async Task Should_ReturnEntries_When_DateHasEntries()
    {
        var date = new DateOnly(2026, 1, 15);
        var occurredAt = date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);
        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.SprintStarted, Guid.NewGuid(), occurredAt));

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(date));

        Assert.Single(result.Entries);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_DateHasNoEntries()
    {
        var date = new DateOnly(2026, 1, 15);

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(date));

        Assert.Empty(result.Entries);
    }

    [Fact]
    public async Task Should_NotReturnOtherDateEntries_When_FilteringByDate()
    {
        var targetDate = new DateOnly(2026, 1, 15);
        var otherDate  = new DateOnly(2026, 1, 14);

        _journalRepo.Entries.Add(JournalEntry.Create(
            JournalEventType.SprintStarted, Guid.NewGuid(),
            targetDate.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc)));

        _journalRepo.Entries.Add(JournalEntry.Create(
            JournalEventType.SprintStarted, Guid.NewGuid(),
            otherDate.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc)));

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(targetDate));

        Assert.Single(result.Entries);
    }

    [Fact]
    public async Task Should_ReturnMultipleEntries_When_DateHasMultipleEvents()
    {
        var date = new DateOnly(2026, 1, 15);
        var base_time = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.SprintStarted,     Guid.NewGuid(), base_time.AddHours(9)));
        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.SprintCompleted,   Guid.NewGuid(), base_time.AddHours(9).AddMinutes(25)));
        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.BreakCompleted,    Guid.NewGuid(), base_time.AddHours(9).AddMinutes(30)));

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(date));

        Assert.Equal(3, result.Entries.Count);
    }

    [Fact]
    public async Task Should_PreserveSequence_When_EntryHasSequence()
    {
        var date = new DateOnly(2026, 1, 15);
        var occurredAt = date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);
        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.SprintStarted, Guid.NewGuid(), occurredAt, sequence: 3));

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(date));

        Assert.Equal(3, result.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_ReturnCorrectEventType_When_EntryExists()
    {
        var date = new DateOnly(2026, 1, 15);
        var occurredAt = date.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);
        _journalRepo.Entries.Add(JournalEntry.Create(JournalEventType.SprintCompleted, Guid.NewGuid(), occurredAt));

        var result = await _sut.HandleAsync(new GetJournalByDateQuery(date));

        Assert.Equal(nameof(JournalEventType.SprintCompleted), result.Entries[0].EventType);
    }
}
