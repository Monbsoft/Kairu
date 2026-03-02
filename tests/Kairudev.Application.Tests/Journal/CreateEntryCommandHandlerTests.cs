using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Tests.Journal;

public sealed class CreateEntryCommandHandlerTests
{
    private readonly FakeJournalEntryRepository _repository = new();
    private readonly CreateEntryCommandHandler _sut;

    public CreateEntryCommandHandlerTests() =>
        _sut = new CreateEntryCommandHandler(_repository);

    // ── SprintStarted ─────────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignSequence1_When_FirstSprintStartedToday()
    {
        var command = new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), DateTime.UtcNow);

        await _sut.HandleAsync(command);

        Assert.Equal(1, _repository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_AssignSequence2_When_SecondSprintStartedToday()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));

        Assert.Equal(1, _repository.Entries[0].Sequence);
        Assert.Equal(2, _repository.Entries[1].Sequence);
    }

    // ── SprintCompleted ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignCurrentSprintNumber_When_SprintCompleted()
    {
        var today = DateTime.UtcNow;

        // Sprint #1 démarre
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));
        // Sprint #1 se complète
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintCompleted, Guid.NewGuid(), today));

        // count(SprintStarted today) = 1 → Sequence = 1
        Assert.Equal(1, _repository.Entries[1].Sequence);
    }

    [Fact]
    public async Task Should_AssignSequence2_When_SecondSprintCompleted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintCompleted, Guid.NewGuid(), today));

        // count(SprintStarted today) = 2 → Sequence = 2
        Assert.Equal(2, _repository.Entries[2].Sequence);
    }

    // ── SprintInterrupted ─────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignCurrentSprintNumber_When_SprintInterrupted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintInterrupted, Guid.NewGuid(), today));

        // count(SprintStarted today) = 1 → Sequence = 1
        Assert.Equal(1, _repository.Entries[1].Sequence);
    }

    // ── BreakCompleted (déjà existant, non-régression) ────────────────────

    [Fact]
    public async Task Should_AssignBreakSequence_When_BreakCompleted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.BreakCompleted, Guid.NewGuid(), today));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.BreakCompleted, Guid.NewGuid(), today));

        Assert.Equal(1, _repository.Entries[0].Sequence);
        Assert.Equal(2, _repository.Entries[1].Sequence);
    }

    // ── Événements non-séquencés ──────────────────────────────────────────

    [Fact]
    public async Task Should_AssignNullSequence_When_EventIsTaskCompleted()
    {
        var command = new CreateEntryCommand(JournalEventType.TaskCompleted, Guid.NewGuid(), DateTime.UtcNow);

        await _sut.HandleAsync(command);

        Assert.Null(_repository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_CommandIsValid()
    {
        var command = new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), DateTime.UtcNow);

        var result = await _sut.HandleAsync(command);

        Assert.True(result.IsSuccess);
    }
}
