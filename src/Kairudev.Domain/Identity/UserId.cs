namespace Kairudev.Domain.Identity;

public sealed record UserId
{
    public string Value { get; }

    private UserId(string value) => Value = value;

    public static UserId From(string value) => new(value);

    public override string ToString() => Value;
}
