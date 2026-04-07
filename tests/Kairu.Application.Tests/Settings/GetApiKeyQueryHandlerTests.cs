using Kairu.Application.Settings.Queries.GetApiKey;
using Kairu.Application.Tests.Common;
using Kairu.Domain.Identity;
using Kairu.Domain.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kairu.Application.Tests.Settings;

public sealed class GetApiKeyQueryHandlerTests
{
    private readonly FakeApiKeyRepository _repository = new();
    private readonly GetApiKeyQueryHandler _sut;

    public GetApiKeyQueryHandlerTests() =>
        _sut = new GetApiKeyQueryHandler(
            _repository,
            new FakeCurrentUserService(),
            NullLogger<GetApiKeyQueryHandler>.Instance);

    [Fact]
    public async Task Should_ReturnExistsFalse_When_NoKeyForUser()
    {
        var result = await _sut.Handle(new GetApiKeyQuery());

        Assert.False(result.Exists);
        Assert.Null(result.CreatedAt);
    }

    [Fact]
    public async Task Should_ReturnExistsTrue_When_KeyExists()
    {
        var createdAt = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc);
        var apiKey = UserApiKey.Create(
            FakeCurrentUserService.TestUserId, "somehash", createdAt);
        await _repository.UpsertAsync(apiKey);

        var result = await _sut.Handle(new GetApiKeyQuery());

        Assert.True(result.Exists);
        Assert.Equal(createdAt, result.CreatedAt);
    }

    [Fact]
    public void Should_NotExposeHash_When_KeyExists()
    {
        // GetApiKeyResult must only expose Exists + CreatedAt — never hash or token
        var properties = typeof(GetApiKeyResult).GetProperties();
        var names = properties.Select(p => p.Name).OrderBy(n => n).ToArray();

        Assert.Equal(["CreatedAt", "Exists"], names);
    }
}
