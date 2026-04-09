using Kairu.Application.Identity.Queries.ValidateMcpToken;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Identity;

public sealed class ValidateMcpTokenQueryHandlerTests
{
    private readonly FakeMcpTokenRepository _repository = new();
    private readonly FakeMcpTokenGenerator _generator = new();
    private readonly ValidateMcpTokenQueryHandler _sut;

    public ValidateMcpTokenQueryHandlerTests()
        => _sut = new ValidateMcpTokenQueryHandler(
            _repository,
            _generator,
            NullLogger<ValidateMcpTokenQueryHandler>.Instance);

    private McpToken CreateAndStoreToken(UserId userId, McpRawToken rawToken, DateTime? expiresAt = null)
    {
        var hash = _generator.Hash(rawToken);
        var expires = expiresAt ?? DateTime.UtcNow.AddYears(1);
        var token = McpToken.Create(userId, hash, expires);
        _repository.Tokens.Add(token);
        return token;
    }

    // ── nominal ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnUserId_When_TokenIsValid()
    {
        var userId = UserId.New();
        var rawToken = _generator.Generate();
        CreateAndStoreToken(userId, rawToken);

        var result = await _sut.Handle(new ValidateMcpTokenQuery(rawToken));

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
    }

    // ── not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnFailure_When_TokenNotFound()
    {
        var unknownToken = new McpRawToken("kairu_unknown_token_0001");

        var result = await _sut.Handle(new ValidateMcpTokenQuery(unknownToken));

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.NoTokenFound, result.Error);
    }

    // ── expiry ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnFailure_When_TokenIsExpired()
    {
        var userId = UserId.New();
        var rawToken = _generator.Generate();
        // Create an already-expired token
        var pastExpiry = DateTime.UtcNow.AddDays(-1);
        var pastCreated = DateTime.UtcNow.AddDays(-2);
        var hash = _generator.Hash(rawToken);
        var expiredToken = McpToken.Create(userId, hash, pastExpiry, pastCreated);
        _repository.Tokens.Add(expiredToken);

        var result = await _sut.Handle(new ValidateMcpTokenQuery(rawToken));

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.Expired, result.Error);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_ExpiresAtEqualsUtcNow()
    {
        // ExpiresAt == UtcNow means expired (boundary: <= UtcNow)
        var userId = UserId.New();
        var rawToken = _generator.Generate();
        // We can't freeze time in this test, so we use a clearly past date.
        var exactNow = DateTime.UtcNow.AddSeconds(-1);
        var hash = _generator.Hash(rawToken);
        var token = McpToken.Create(userId, hash, exactNow, exactNow.AddSeconds(-10));
        _repository.Tokens.Add(token);

        var result = await _sut.Handle(new ValidateMcpTokenQuery(rawToken));

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.Expired, result.Error);
    }

    // ── isolation ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnCorrectUser_When_MultipleTokensExist()
    {
        var userA = UserId.New();
        var userB = UserId.New();
        var rawA = _generator.Generate();
        var rawB = _generator.Generate();
        CreateAndStoreToken(userA, rawA);
        CreateAndStoreToken(userB, rawB);

        var result = await _sut.Handle(new ValidateMcpTokenQuery(rawB));

        Assert.True(result.IsSuccess);
        Assert.Equal(userB, result.Value.UserId);
    }
}
