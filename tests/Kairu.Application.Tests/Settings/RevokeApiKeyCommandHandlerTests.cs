using Kairu.Application.Settings.Commands.GenerateApiKey;
using Kairu.Application.Settings.Commands.RevokeApiKey;
using Kairu.Application.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Settings;

public sealed class RevokeApiKeyCommandHandlerTests
{
    private readonly FakeApiKeyRepository _repository = new();
    private readonly RevokeApiKeyCommandHandler _sut;
    private readonly GenerateApiKeyCommandHandler _generateSut;

    public RevokeApiKeyCommandHandlerTests()
    {
        _sut = new RevokeApiKeyCommandHandler(
            _repository,
            new FakeCurrentUserService(),
            NullLogger<RevokeApiKeyCommandHandler>.Instance);

        _generateSut = new GenerateApiKeyCommandHandler(
            _repository,
            new FakeCurrentUserService(),
            NullLogger<GenerateApiKeyCommandHandler>.Instance);
    }

    [Fact]
    public async Task Should_DeleteKey_When_KeyExists()
    {
        await _generateSut.Handle(new GenerateApiKeyCommand());
        Assert.NotNull(_repository.Stored);

        await _sut.Handle(new RevokeApiKeyCommand());

        Assert.Null(_repository.Stored);
    }

    [Fact]
    public async Task Should_Succeed_When_NoKeyExists()
    {
        // Idempotent — pas d'erreur même si pas de clé
        var result = await _sut.Handle(new RevokeApiKeyCommand());

        Assert.True(result.IsSuccess);
        Assert.Null(_repository.Stored);
    }
}
