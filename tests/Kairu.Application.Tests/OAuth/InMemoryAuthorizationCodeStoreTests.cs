using Kairu.Application.OAuth;
using Kairu.Domain.Identity;
using Kairu.Infrastructure.OAuth;

namespace Kairu.Application.Tests.OAuth;

public sealed class InMemoryAuthorizationCodeStoreTests
{
    private readonly InMemoryAuthorizationCodeStore _store = new();

    private static AuthorizationCodeEntry CreateEntry(int ttlMinutes = 5) => new(
        UserId.From(Guid.NewGuid()),
        "Test User",
        "testuser",
        "test-code-challenge",
        "http://localhost/callback",
        DateTime.UtcNow.AddMinutes(ttlMinutes));

    [Fact]
    public async Task Should_ReturnEntry_When_CodeIsValid()
    {
        var entry = CreateEntry();
        await _store.StoreAsync("code1", entry);

        var result = await _store.ConsumeAsync("code1");

        Assert.NotNull(result);
        Assert.Equal(entry.UserId, result.UserId);
        Assert.Equal(entry.CodeChallenge, result.CodeChallenge);
    }

    [Fact]
    public async Task Should_ReturnNull_When_CodeIsUnknown()
    {
        var result = await _store.ConsumeAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_ReturnNull_When_CodeConsumedTwice()
    {
        await _store.StoreAsync("code2", CreateEntry());

        var first = await _store.ConsumeAsync("code2");
        var second = await _store.ConsumeAsync("code2");

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public async Task Should_ReturnNull_When_CodeIsExpired()
    {
        var expiredEntry = CreateEntry(ttlMinutes: -1);
        await _store.StoreAsync("expired", expiredEntry);

        var result = await _store.ConsumeAsync("expired");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_OverwriteExistingCode_When_SameCodeStored()
    {
        var entry1 = CreateEntry();
        var entry2 = CreateEntry();
        await _store.StoreAsync("dup", entry1);
        await _store.StoreAsync("dup", entry2);

        var result = await _store.ConsumeAsync("dup");

        Assert.NotNull(result);
        Assert.Equal(entry2.UserId, result.UserId);
    }
}
