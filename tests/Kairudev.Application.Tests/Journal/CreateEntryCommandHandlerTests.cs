using Kairudev.Application.Journal.Commands.CreateEntry;
using Kairudev.Domain.Identity;
using Kairudev.Domain.Journal;

namespace Kairudev.Application.Tests.Journal;

public sealed class CreateEntryCommandHandlerTests
{
    private static readonly UserId OwnerId = UserId.From("test-github-id-123");

    private readonly FakeJournalEntryRepository _repository = new();
    private readonly CreateEntryCommandHandler _sut;

    public CreateEntryCommandHandlerTests() =>
        _sut = new CreateEntryCommandHandler(_repository);

    // ── SprintStarted ─────────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignSequence1_When_FirstSprintStartedToday()
    {
        var command = new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), DateTime.UtcNow, OwnerId);

        await _sut.HandleAsync(command);

        Assert.Equal(1, _repository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_AssignSequence2_When_SecondSprintStartedToday()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));

        Assert.Equal(1, _repository.Entries[0].Sequence);
        Assert.Equal(2, _repository.Entries[1].Sequence);
    }

    // ── SprintCompleted ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignCurrentSprintNumber_When_SprintCompleted()
    {
        var today = DateTime.UtcNow;

        // Sprint #1 démarre
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));
        // Sprint #1 se complète
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintCompleted, Guid.NewGuid(), today, OwnerId));

        // count(SprintStarted today) = 1 → Sequence = 1
        Assert.Equal(1, _repository.Entries[1].Sequence);
    }

    [Fact]
    public async Task Should_AssignSequence2_When_SecondSprintCompleted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintCompleted, Guid.NewGuid(), today, OwnerId));

        // count(SprintStarted today) = 2 → Sequence = 2
        Assert.Equal(2, _repository.Entries[2].Sequence);
    }

    // ── SprintInterrupted ─────────────────────────────────────────────────

    [Fact]
    public async Task Should_AssignCurrentSprintNumber_When_SprintInterrupted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), today, OwnerId));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.SprintInterrupted, Guid.NewGuid(), today, OwnerId));

        // count(SprintStarted today) = 1 → Sequence = 1
        Assert.Equal(1, _repository.Entries[1].Sequence);
    }

    // ── BreakCompleted (déjà existant, non-régression) ────────────────────

    [Fact]
    public async Task Should_AssignBreakSequence_When_BreakCompleted()
    {
        var today = DateTime.UtcNow;

        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.BreakCompleted, Guid.NewGuid(), today, OwnerId));
        await _sut.HandleAsync(new CreateEntryCommand(JournalEventType.BreakCompleted, Guid.NewGuid(), today, OwnerId));

        Assert.Equal(1, _repository.Entries[0].Sequence);
        Assert.Equal(2, _repository.Entries[1].Sequence);
    }

    // ── Événements non-séquencés ──────────────────────────────────────────

    [Fact]
    public async Task Should_AssignNullSequence_When_EventIsTaskCompleted()
    {
        var command = new CreateEntryCommand(JournalEventType.TaskCompleted, Guid.NewGuid(), DateTime.UtcNow, OwnerId);

        await _sut.HandleAsync(command);

        Assert.Null(_repository.Entries[0].Sequence);
    }

    [Fact]
    public async Task Should_ReturnSuccess_When_CommandIsValid()
    {
        var command = new CreateEntryCommand(JournalEventType.SprintStarted, Guid.NewGuid(), DateTime.UtcNow, OwnerId);

        var result = await _sut.HandleAsync(command);

        Assert.True(result.IsSuccess);
    }
}
