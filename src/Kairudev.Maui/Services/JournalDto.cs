namespace Kairudev.Maui.Services;

public sealed record JournalEntryDto(
    Guid Id,
    DateTime OccurredAt,
    string EventType,
    Guid ResourceId,
    List<JournalCommentDto> Comments);

public sealed record JournalCommentDto(
    Guid Id,
    string Text);

public sealed record AddCommentRequest(string Text);
public sealed record UpdateCommentRequest(string Text);
