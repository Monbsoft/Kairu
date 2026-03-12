namespace Kairudev.Domain.Identity;

public sealed record UserId
{
    public string Value { get; }

    private UserId(string value) => Value = value;

    public static UserId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("UserId value cannot be null, empty, or whitespace.", nameof(value));

        return new(value.Trim());
    }

    public override string ToString() => Value;
}
