using System.Security.Cryptography;
using System.Text;
using Kairu.Application.Settings.Commands.GenerateApiKey;
using Kairu.Application.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Settings;

public sealed class GenerateApiKeyCommandHandlerTests
{
    private readonly FakeApiKeyRepository _repository = new();
    private readonly GenerateApiKeyCommandHandler _sut;

    public GenerateApiKeyCommandHandlerTests() =>
        _sut = new GenerateApiKeyCommandHandler(
            _repository,
            new FakeCurrentUserService(),
            NullLogger<GenerateApiKeyCommandHandler>.Instance);

    [Fact]
    public async Task Should_ReturnToken_When_CommandIsValid()
    {
        var result = await _sut.Handle(new GenerateApiKeyCommand());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Token);
        Assert.StartsWith("kairu_", result.Token);
    }

    [Fact]
    public async Task Should_PersistHashedKey_NotRawToken_When_CommandIsValid()
    {
        var result = await _sut.Handle(new GenerateApiKeyCommand());

        Assert.NotNull(_repository.Stored);
        // Le hash est différent du token brut
        Assert.NotEqual(result.Token, _repository.Stored.KeyHash);
        // Le hash est bien le SHA-256 du token (même normalisation ToLowerInvariant)
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(result.Token!))).ToLowerInvariant();
        Assert.Equal(expectedHash, _repository.Stored.KeyHash);
    }

    [Fact]
    public async Task Should_StoreCorrectUserId_When_CommandIsValid()
    {
        await _sut.Handle(new GenerateApiKeyCommand());

        Assert.Equal(FakeCurrentUserService.TestUserId, _repository.Stored!.OwnerId);
    }

    [Fact]
    public async Task Should_OverwritePreviousKey_When_CalledTwice()
    {
        var first = await _sut.Handle(new GenerateApiKeyCommand());
        var second = await _sut.Handle(new GenerateApiKeyCommand());

        // Les deux tokens sont différents
        Assert.NotEqual(first.Token, second.Token);
        // Une seule clé en base (upsert) avec le hash du dernier token
        Assert.NotNull(_repository.Stored);
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(second.Token!))).ToLowerInvariant();
        Assert.Equal(expectedHash, _repository.Stored.KeyHash);
    }
}
