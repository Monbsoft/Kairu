namespace Kairu.Domain.Identity;

public sealed record McpTokenId(Guid Value)
{
    public static McpTokenId New() => new(Guid.NewGuid());
    public static McpTokenId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
