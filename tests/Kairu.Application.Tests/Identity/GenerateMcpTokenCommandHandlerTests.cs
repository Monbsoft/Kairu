using Kairu.Application.Identity.Commands.GenerateMcpToken;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Identity;

public sealed class GenerateMcpTokenCommandHandlerTests
{
    private readonly FakeMcpTokenRepository _repository = new();
    private readonly FakeMcpTokenGenerator _generator = new();
    private readonly GenerateMcpTokenCommandHandler _sut;

    public GenerateMcpTokenCommandHandlerTests()
        => _sut = new GenerateMcpTokenCommandHandler(
            _repository,
            _generator,
            NullLogger<GenerateMcpTokenCommandHandler>.Instance);

    // ── nominal ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnSuccess_When_NoExistingToken()
    {
        var userId = UserId.New();
        var command = new GenerateMcpTokenCommand(userId);

        var result = await _sut.Handle(command);

        Assert.True(result.IsSuccess);
        Assert.Single(_repository.Tokens);
    }

    [Fact]
    public async Task Should_ReturnRawToken_When_TokenGenerated()
    {
        var userId = UserId.New();

        var result = await _sut.Handle(new GenerateMcpTokenCommand(userId));

        Assert.True(result.IsSuccess);
        Assert.StartsWith("kairu_", result.Value.RawToken.Value);
    }

    [Fact]
    public async Task Should_SetExpiryOneYearFromNow_When_TokenGenerated()
    {
        var userId = UserId.New();
        var before = DateTime.UtcNow;

        var result = await _sut.Handle(new GenerateMcpTokenCommand(userId));

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.ExpiresAt >= before.AddYears(1));
        Assert.True(result.Value.ExpiresAt <= after.AddYears(1));
    }

    [Fact]
    public async Task Should_PersistTokenWithCorrectUserId_When_Generated()
    {
        var userId = UserId.New();

        await _sut.Handle(new GenerateMcpTokenCommand(userId));

        Assert.Single(_repository.Tokens);
        Assert.Equal(userId, _repository.Tokens[0].UserId);
    }

    // ── régénération ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReplaceExistingToken_When_UserAlreadyHasToken()
    {
        var userId = UserId.New();

        // First generation
        var first = await _sut.Handle(new GenerateMcpTokenCommand(userId));

        // Second generation (regenerate)
        var second = await _sut.Handle(new GenerateMcpTokenCommand(userId));

        Assert.True(second.IsSuccess);
        // Only one token persisted (old one deleted)
        Assert.Single(_repository.Tokens);
        // New raw token differs from first
        Assert.NotEqual(first.Value.RawToken.Value, second.Value.RawToken.Value);
    }

    [Fact]
    public async Task Should_StoreDifferentHash_When_RegeneratingToken()
    {
        var userId = UserId.New();

        await _sut.Handle(new GenerateMcpTokenCommand(userId));
        var firstHash = _repository.Tokens[0].TokenHash;

        await _sut.Handle(new GenerateMcpTokenCommand(userId));
        var secondHash = _repository.Tokens[0].TokenHash;

        Assert.NotEqual(firstHash, secondHash);
    }
}
