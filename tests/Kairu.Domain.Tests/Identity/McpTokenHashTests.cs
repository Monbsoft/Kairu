using Kairu.Domain.Identity;

namespace Kairu.Domain.Tests.Identity;

public sealed class McpTokenHashTests
{
    // ── nominal ──────────────────────────────────────────────────────────────

    [Fact]
    public void Should_CreateHash_When_Valid64CharHexProvided()
    {
        var sha256 = new string('a', 64);

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsSuccess);
        Assert.Equal(sha256, result.Value.Value);
    }

    [Fact]
    public void Should_NormalizeLowercase_When_UppercaseHexProvided()
    {
        var sha256 = new string('A', 64);

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsSuccess);
        Assert.Equal(new string('a', 64), result.Value.Value);
    }

    [Fact]
    public void Should_Accept_MixedCaseValidHex()
    {
        // 64 valid hex chars mixing digits and letters
        var sha256 = "0123456789abcdef0123456789ABCDEF0123456789abcdef0123456789abcdef";

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsSuccess);
    }

    // ── failure cases ─────────────────────────────────────────────────────────

    [Fact]
    public void Should_ReturnFailure_When_HashIsEmpty()
    {
        var result = McpTokenHash.Create(string.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.InvalidHash, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_HashIsWhitespace()
    {
        var result = McpTokenHash.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.InvalidHash, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_HashIsTooShort()
    {
        var sha256 = new string('a', 63);

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.InvalidHash, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_HashIsTooLong()
    {
        var sha256 = new string('a', 65);

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.InvalidHash, result.Error);
    }

    [Fact]
    public void Should_ReturnFailure_When_HashContainsInvalidChars()
    {
        // 'g' is not a valid hex character
        var sha256 = new string('g', 64);

        var result = McpTokenHash.Create(sha256);

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.InvalidHash, result.Error);
    }

    // ── equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void Should_BeEqual_When_SameHashValue()
    {
        var sha256 = new string('b', 64);

        var h1 = McpTokenHash.Create(sha256).Value;
        var h2 = McpTokenHash.Create(sha256).Value;

        Assert.Equal(h1, h2);
    }

    [Fact]
    public void Should_NotBeEqual_When_DifferentHashValues()
    {
        var h1 = McpTokenHash.Create(new string('a', 64)).Value;
        var h2 = McpTokenHash.Create(new string('b', 64)).Value;

        Assert.NotEqual(h1, h2);
    }
}
