namespace KairuFocus.Domain.Gamification;

public sealed record XpGainId(Guid Value)
{
    public static XpGainId New() => new(Guid.NewGuid());
    public static XpGainId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
