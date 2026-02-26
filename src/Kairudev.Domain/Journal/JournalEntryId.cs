namespace Kairudev.Domain.Journal;

public sealed record JournalEntryId(Guid Value)
{
    public static JournalEntryId New() => new(Guid.NewGuid());
    public static JournalEntryId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
