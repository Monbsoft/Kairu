using Kairu.Application.Identity.Queries.GetMcpTokenStatus;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Identity;

public sealed class GetMcpTokenStatusQueryHandlerTests
{
    private readonly FakeMcpTokenRepository _repository = new();
    private readonly GetMcpTokenStatusQueryHandler _sut;

    public GetMcpTokenStatusQueryHandlerTests()
        => _sut = new GetMcpTokenStatusQueryHandler(
            _repository,
            NullLogger<GetMcpTokenStatusQueryHandler>.Instance);

    [Fact]
    public async Task Should_ReturnHasTokenFalse_When_NoTokenExists()
    {
        var result = await _sut.Handle(new GetMcpTokenStatusQuery(UserId.New()));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasToken);
        Assert.Null(result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Should_ReturnHasTokenTrue_When_TokenExists()
    {
        var userId = UserId.New();
        var expiresAt = DateTime.UtcNow.AddYears(1);
        var hash = McpTokenHash.Create(new string('d', 64)).Value;
        _repository.Tokens.Add(McpToken.Create(userId, hash, expiresAt));

        var result = await _sut.Handle(new GetMcpTokenStatusQuery(userId));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasToken);
        Assert.NotNull(result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Should_ReturnCorrectExpiresAt_When_TokenExists()
    {
        var userId = UserId.New();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddYears(1);
        var hash = McpTokenHash.Create(new string('e', 64)).Value;
        _repository.Tokens.Add(McpToken.Create(userId, hash, expiresAt, now));

        var result = await _sut.Handle(new GetMcpTokenStatusQuery(userId));

        Assert.Equal(expiresAt, result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Should_NeverExposeHash_When_TokenExists()
    {
        // This test verifies the result type does not expose token hash or raw value.
        // Compile-time check: GetMcpTokenStatusResult must not have a Hash or RawToken property.
        var userId = UserId.New();
        var hash = McpTokenHash.Create(new string('f', 64)).Value;
        _repository.Tokens.Add(McpToken.Create(userId, hash, DateTime.UtcNow.AddYears(1)));

        var result = await _sut.Handle(new GetMcpTokenStatusQuery(userId));

        // GetMcpTokenStatusResult only exposes HasToken and ExpiresAt
        var resultType = result.Value.GetType();
        Assert.Null(resultType.GetProperty("Hash"));
        Assert.Null(resultType.GetProperty("RawToken"));
        Assert.Null(resultType.GetProperty("TokenHash"));
    }
}
