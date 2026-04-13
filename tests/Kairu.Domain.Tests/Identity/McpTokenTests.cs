using Kairu.Domain.Identity;

namespace Kairu.Domain.Tests.Identity;

public sealed class McpTokenTests
{
    private static McpTokenHash ValidHash()
        => McpTokenHash.Create(new string('a', 64)).Value;

    [Fact]
    public void Should_CreateToken_When_ValidParametersProvided()
    {
        var userId = UserId.New();
        var hash = ValidHash();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddYears(1);

        var token = McpToken.Create(userId, hash, expiresAt, now);

        Assert.Equal(userId, token.UserId);
        Assert.Equal(hash, token.TokenHash);
        Assert.Equal(now, token.CreatedAt);
        Assert.Equal(expiresAt, token.ExpiresAt);
    }

    [Fact]
    public void Should_GenerateUniqueIds_When_TwoTokensCreated()
    {
        var userId = UserId.New();
        var hash = ValidHash();
        var expiresAt = DateTime.UtcNow.AddYears(1);

        var t1 = McpToken.Create(userId, hash, expiresAt);
        var t2 = McpToken.Create(userId, hash, expiresAt);

        Assert.NotEqual(t1.Id, t2.Id);
    }

    [Fact]
    public void Should_NotBeExpired_When_ExpiresAtIsInFuture()
    {
        var token = McpToken.Create(UserId.New(), ValidHash(), DateTime.UtcNow.AddYears(1));

        Assert.False(token.IsExpired(DateTime.UtcNow));
    }

    [Fact]
    public void Should_BeExpired_When_ExpiresAtIsInPast()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        var token = McpToken.Create(UserId.New(), ValidHash(), past, DateTime.UtcNow.AddDays(-2));

        Assert.True(token.IsExpired(DateTime.UtcNow));
    }

    [Fact]
    public void Should_BeExpired_When_ExpiresAtEqualsUtcNow()
    {
        var now = DateTime.UtcNow;
        var token = McpToken.Create(UserId.New(), ValidHash(), now, now.AddSeconds(-1));

        Assert.True(token.IsExpired(now));
    }
}
