using Kairu.Application.Identity;
using Kairu.Domain.Identity;

namespace Kairu.Application.Tests.Identity;

/// <summary>
/// Deterministic token generator for unit tests.
/// Each call to Generate() returns a token with a counter suffix.
/// Hash() returns a stable 64-char hex derived from the raw value.
/// </summary>
internal sealed class FakeMcpTokenGenerator : IMcpTokenGenerator
{
    private int _counter;

    public McpRawToken Generate()
    {
        _counter++;
        return new McpRawToken($"kairu_fake_token_{_counter:D4}");
    }

    public McpTokenHash Hash(McpRawToken rawToken)
    {
        // Produce a deterministic 64-char lowercase hex from the raw value.
        // We pad the raw string hash code into a repeating 64-char hex string.
        var seed = Math.Abs(rawToken.Value.GetHashCode());
        var hex = seed.ToString("x8");
        // Repeat to fill exactly 64 chars
        var repeated = string.Concat(Enumerable.Repeat(hex, 8))[..64];
        return McpTokenHash.Create(repeated).Value;
    }
}
