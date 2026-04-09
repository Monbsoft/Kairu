using Kairu.Application.Identity.Commands.RevokeMcpToken;
using Kairu.Domain.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Identity;

public sealed class RevokeMcpTokenCommandHandlerTests
{
    private readonly FakeMcpTokenRepository _repository = new();
    private readonly RevokeMcpTokenCommandHandler _sut;

    public RevokeMcpTokenCommandHandlerTests()
        => _sut = new RevokeMcpTokenCommandHandler(
            _repository,
            NullLogger<RevokeMcpTokenCommandHandler>.Instance);

    private static McpToken CreateToken(UserId userId)
    {
        var hash = McpTokenHash.Create(new string('c', 64)).Value;
        return McpToken.Create(userId, hash, DateTime.UtcNow.AddYears(1));
    }

    // ── nominal ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnSuccess_When_TokenExists()
    {
        var userId = UserId.New();
        _repository.Tokens.Add(CreateToken(userId));

        var result = await _sut.Handle(new RevokeMcpTokenCommand(userId));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Should_RemoveToken_When_RevocationSucceeds()
    {
        var userId = UserId.New();
        _repository.Tokens.Add(CreateToken(userId));

        await _sut.Handle(new RevokeMcpTokenCommand(userId));

        Assert.Empty(_repository.Tokens);
    }

    // ── not found ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnFailure_When_NoTokenExists()
    {
        var result = await _sut.Handle(new RevokeMcpTokenCommand(UserId.New()));

        Assert.True(result.IsFailure);
        Assert.Equal(McpTokenErrors.NoTokenFound, result.Error);
    }

    [Fact]
    public async Task Should_NotAffectOtherUsersTokens_When_RevokingOneUser()
    {
        var userA = UserId.New();
        var userB = UserId.New();

        var hashA = McpTokenHash.Create(new string('a', 64)).Value;
        var hashB = McpTokenHash.Create(new string('b', 64)).Value;
        _repository.Tokens.Add(McpToken.Create(userA, hashA, DateTime.UtcNow.AddYears(1)));
        _repository.Tokens.Add(McpToken.Create(userB, hashB, DateTime.UtcNow.AddYears(1)));

        await _sut.Handle(new RevokeMcpTokenCommand(userA));

        Assert.Single(_repository.Tokens);
        Assert.Equal(userB, _repository.Tokens[0].UserId);
    }
}
