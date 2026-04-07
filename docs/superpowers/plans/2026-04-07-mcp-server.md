# MCP Server — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Exposer Kairu comme un MCP server HTTP+SSE sur `/mcp`, authentifié par API Key, avec 4 tools Tasks (create, list, complete, delete).

**Architecture:** SDK officiel `ModelContextProtocol.AspNetCore` 1.2.0 intégré dans `Kairu.Api`. Un nouveau scheme d'auth `ApiKey` coexiste avec JWT Bearer existant. Les tools MCP délèguent directement aux handlers CQRS existants via injection. Les API Keys sont stockées hashées (SHA-256) dans une nouvelle table `UserApiKeys`.

**Tech Stack:** .NET 10, `ModelContextProtocol.AspNetCore` 1.2.0, ASP.NET Core `AuthenticationHandler<T>`, EF Core 10, xUnit, BrilliantMediator.

---

## File Map

### Nouveaux fichiers — Domain
- `src/Kairu.Domain/Settings/UserApiKey.cs` — entité domaine (OwnerId, KeyHash, CreatedAt)

### Nouveaux fichiers — Application
- `src/Kairu.Application/Settings/Common/IApiKeyRepository.cs` — interface repository
- `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyCommand.cs`
- `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyCommandHandler.cs`
- `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyResult.cs`
- `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyQuery.cs`
- `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyQueryHandler.cs`
- `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyResult.cs`
- `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyCommand.cs`
- `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyCommandHandler.cs`
- `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyResult.cs`

### Nouveaux fichiers — Infrastructure
- `src/Kairu.Infrastructure/Persistence/UserApiKeyConfiguration.cs` — EF Core config
- `src/Kairu.Infrastructure/Persistence/Repositories/EfCoreApiKeyRepository.cs`

### Nouveaux fichiers — API
- `src/Kairu.Api/Mcp/ApiKeyAuthOptions.cs` — options du scheme auth
- `src/Kairu.Api/Mcp/ApiKeyAuthHandler.cs` — AuthenticationHandler ASP.NET Core
- `src/Kairu.Api/Mcp/KairuMcpTools.cs` — tools MCP [McpServerToolType]
- `src/Kairu.Api/Settings/ApiKeyController.cs` — endpoints REST API Key

### Nouveaux fichiers — Tests
- `tests/Kairu.Application.Tests/Settings/FakeApiKeyRepository.cs`
- `tests/Kairu.Application.Tests/Settings/GenerateApiKeyCommandHandlerTests.cs`
- `tests/Kairu.Application.Tests/Settings/GetApiKeyQueryHandlerTests.cs`
- `tests/Kairu.Application.Tests/Settings/RevokeApiKeyCommandHandlerTests.cs`

### Fichiers modifiés
- `src/Kairu.Domain/Settings/` — ajout de `UserApiKey.cs` (nouveau fichier dans dossier existant)
- `src/Kairu.Infrastructure/Persistence/KairuDbContext.cs` — ajout `DbSet<UserApiKey>`
- `src/Kairu.Infrastructure/DependencyInjection.cs` — enregistrement `IApiKeyRepository`
- `src/Kairu.Api/Kairu.Api.csproj` — PackageReference `ModelContextProtocol.AspNetCore`
- `src/Kairu.Api/Program.cs` — AddMcpServer, ApiKey auth scheme, policy, MapMcp

---

## Task 1 — Domain entity `UserApiKey`

**Files:**
- Create: `src/Kairu.Domain/Settings/UserApiKey.cs`

- [ ] **Créer `UserApiKey.cs`**

```csharp
using Kairu.Domain.Identity;

namespace Kairu.Domain.Settings;

public sealed class UserApiKey
{
    public UserId OwnerId { get; private set; }
    public string KeyHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserApiKey() { OwnerId = null!; KeyHash = null!; }

    public static UserApiKey Create(UserId ownerId, string keyHash, DateTime createdAt)
        => new() { OwnerId = ownerId, KeyHash = keyHash, CreatedAt = createdAt };

    /// <summary>Remplace la clé existante (upsert).</summary>
    public void Regenerate(string keyHash, DateTime createdAt)
    {
        KeyHash = keyHash;
        CreatedAt = createdAt;
    }
}
```

- [ ] **Build pour vérifier**

```bash
cd src/Kairu.Domain && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Domain/Settings/UserApiKey.cs
git commit -m "feat(api-key): domain entity UserApiKey"
```

---

## Task 2 — Interface `IApiKeyRepository`

**Files:**
- Create: `src/Kairu.Application/Settings/Common/IApiKeyRepository.cs`

- [ ] **Créer `IApiKeyRepository.cs`**

```csharp
using Kairu.Domain.Identity;
using Kairu.Domain.Settings;

namespace Kairu.Application.Settings.Common;

public interface IApiKeyRepository
{
    /// <summary>Upsert la clé pour un utilisateur (une seule clé active par user).</summary>
    Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default);

    /// <summary>Retourne le UserId associé à ce hash, ou null si inconnu.</summary>
    Task<UserId?> GetUserIdByHashAsync(string keyHash, CancellationToken cancellationToken = default);

    /// <summary>Retourne la clé de l'utilisateur, ou null si absente.</summary>
    Task<UserApiKey?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);

    /// <summary>Supprime la clé de l'utilisateur. Idempotent.</summary>
    Task DeleteByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
```

- [ ] **Build**

```bash
cd src/Kairu.Application && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Application/Settings/Common/IApiKeyRepository.cs
git commit -m "feat(api-key): interface IApiKeyRepository"
```

---

## Task 3 — Command `GenerateApiKey`

**Files:**
- Create: `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyCommand.cs`
- Create: `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyResult.cs`
- Create: `src/Kairu.Application/Settings/Commands/GenerateApiKey/GenerateApiKeyCommandHandler.cs`

- [ ] **Créer `GenerateApiKeyCommand.cs`**

```csharp
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.GenerateApiKey;

/// <summary>Génère (ou régénère) une API Key pour l'utilisateur courant.</summary>
public sealed record GenerateApiKeyCommand : ICommand<GenerateApiKeyResult>;
```

- [ ] **Créer `GenerateApiKeyResult.cs`**

```csharp
namespace Kairu.Application.Settings.Commands.GenerateApiKey;

public sealed record GenerateApiKeyResult
{
    public bool IsSuccess { get; init; }
    /// <summary>Token brut — retourné une seule fois, jamais stocké en clair.</summary>
    public string? Token { get; init; }
    public string? Error { get; init; }

    private GenerateApiKeyResult() { }

    public static GenerateApiKeyResult Success(string token) =>
        new() { IsSuccess = true, Token = token };

    public static GenerateApiKeyResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
```

- [ ] **Créer `GenerateApiKeyCommandHandler.cs`**

```csharp
using System.Security.Cryptography;
using System.Text;
using Kairu.Application.Common;
using Kairu.Application.Settings.Common;
using Kairu.Domain.Settings;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.GenerateApiKey;

public sealed class GenerateApiKeyCommandHandler
    : ICommandHandler<GenerateApiKeyCommand, GenerateApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GenerateApiKeyCommandHandler> _logger;

    public GenerateApiKeyCommandHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<GenerateApiKeyCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GenerateApiKeyResult> Handle(
        GenerateApiKeyCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;

        // Génère un token URL-safe : "kairu_" + base64url(32 bytes aléatoires)
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = "kairu_" + Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // Hash SHA-256 — seul ce hash est persisté
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var keyHash = Convert.ToHexString(hashBytes).ToLower();

        var apiKey = UserApiKey.Create(userId, keyHash, DateTime.UtcNow);
        await _repository.UpsertAsync(apiKey, cancellationToken);

        _logger.LogInformation("API Key generated for user {UserId}", userId);
        return GenerateApiKeyResult.Success(token);
    }
}
```

- [ ] **Build**

```bash
cd src/Kairu.Application && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Application/Settings/Commands/GenerateApiKey/
git commit -m "feat(api-key): GenerateApiKey command + handler"
```

---

## Task 4 — Tests `GenerateApiKeyCommandHandler`

**Files:**
- Create: `tests/Kairu.Application.Tests/Settings/FakeApiKeyRepository.cs`
- Create: `tests/Kairu.Application.Tests/Settings/GenerateApiKeyCommandHandlerTests.cs`

- [ ] **Créer `FakeApiKeyRepository.cs`**

```csharp
using Kairu.Application.Settings.Common;
using Kairu.Domain.Identity;
using Kairu.Domain.Settings;

namespace Kairu.Application.Tests.Settings;

internal sealed class FakeApiKeyRepository : IApiKeyRepository
{
    public UserApiKey? Stored { get; private set; }

    public Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default)
    {
        Stored = apiKey;
        return Task.CompletedTask;
    }

    public Task<UserId?> GetUserIdByHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        if (Stored is not null && Stored.KeyHash == keyHash)
            return Task.FromResult<UserId?>(Stored.OwnerId);
        return Task.FromResult<UserId?>(null);
    }

    public Task<UserApiKey?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        if (Stored is not null && Stored.OwnerId == userId)
            return Task.FromResult<UserApiKey?>(Stored);
        return Task.FromResult<UserApiKey?>(null);
    }

    public Task DeleteByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        if (Stored?.OwnerId == userId) Stored = null;
        return Task.CompletedTask;
    }
}
```

- [ ] **Créer `GenerateApiKeyCommandHandlerTests.cs`**

```csharp
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
        // Le hash est bien le SHA-256 du token
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(result.Token!))).ToLower();
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
        // Une seule clé en base (upsert)
        Assert.NotNull(_repository.Stored);
        var expectedHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(second.Token!))).ToLower();
        Assert.Equal(expectedHash, _repository.Stored.KeyHash);
    }
}
```

- [ ] **Lancer les tests pour vérifier qu'ils passent**

```bash
cd tests/Kairu.Application.Tests && dotnet test --filter "FullyQualifiedName~GenerateApiKeyCommandHandlerTests" -v normal
```
Expected: 4 tests passent.

- [ ] **Commit**

```bash
git add tests/Kairu.Application.Tests/Settings/
git commit -m "test(api-key): GenerateApiKeyCommandHandler tests"
```

---

## Task 5 — Query `GetApiKey`

**Files:**
- Create: `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyQuery.cs`
- Create: `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyResult.cs`
- Create: `src/Kairu.Application/Settings/Queries/GetApiKey/GetApiKeyQueryHandler.cs`

- [ ] **Créer `GetApiKeyQuery.cs`**

```csharp
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Settings.Queries.GetApiKey;

/// <summary>Retourne le statut de la clé API de l'utilisateur courant (jamais le hash).</summary>
public sealed record GetApiKeyQuery : IQuery<GetApiKeyResult>;
```

- [ ] **Créer `GetApiKeyResult.cs`**

```csharp
namespace Kairu.Application.Settings.Queries.GetApiKey;

public sealed record GetApiKeyResult(bool Exists, DateTime? CreatedAt);
```

- [ ] **Créer `GetApiKeyQueryHandler.cs`**

```csharp
using Kairu.Application.Common;
using Kairu.Application.Settings.Common;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Settings.Queries.GetApiKey;

public sealed class GetApiKeyQueryHandler : IQueryHandler<GetApiKeyQuery, GetApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetApiKeyQueryHandler> _logger;

    public GetApiKeyQueryHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<GetApiKeyQueryHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GetApiKeyResult> Handle(
        GetApiKeyQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        var apiKey = await _repository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogDebug("GetApiKey for user {UserId}: exists={Exists}", userId, apiKey is not null);
        return new GetApiKeyResult(apiKey is not null, apiKey?.CreatedAt);
    }
}
```

- [ ] **Build**

```bash
cd src/Kairu.Application && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Créer `GetApiKeyQueryHandlerTests.cs`**

```csharp
using Kairu.Application.Settings.Queries.GetApiKey;
using Kairu.Application.Tests.Common;
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
        var apiKey = UserApiKey.Create(
            FakeCurrentUserService.TestUserId, "somehash", new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc));
        await _repository.UpsertAsync(apiKey);

        var result = await _sut.Handle(new GetApiKeyQuery());

        Assert.True(result.Exists);
        Assert.Equal(new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc), result.CreatedAt);
    }

    [Fact]
    public async Task Should_NotExposeHash_When_KeyExists()
    {
        var apiKey = UserApiKey.Create(FakeCurrentUserService.TestUserId, "secrethash", DateTime.UtcNow);
        await _repository.UpsertAsync(apiKey);

        var result = await _sut.Handle(new GetApiKeyQuery());

        // GetApiKeyResult ne contient pas de hash — vérification structurelle
        Assert.DoesNotContain(
            typeof(GetApiKeyResult).GetProperties(),
            p => p.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase)
              || p.Name.Contains("Key", StringComparison.OrdinalIgnoreCase)
              || p.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));
    }
}
```

- [ ] **Lancer les tests**

```bash
cd tests/Kairu.Application.Tests && dotnet test --filter "FullyQualifiedName~GetApiKeyQueryHandlerTests" -v normal
```
Expected: 3 tests passent.

- [ ] **Commit**

```bash
git add src/Kairu.Application/Settings/Queries/GetApiKey/ tests/Kairu.Application.Tests/Settings/GetApiKeyQueryHandlerTests.cs
git commit -m "feat(api-key): GetApiKey query + handler + tests"
```

---

## Task 6 — Command `RevokeApiKey`

**Files:**
- Create: `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyCommand.cs`
- Create: `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyResult.cs`
- Create: `src/Kairu.Application/Settings/Commands/RevokeApiKey/RevokeApiKeyCommandHandler.cs`
- Create: `tests/Kairu.Application.Tests/Settings/RevokeApiKeyCommandHandlerTests.cs`

- [ ] **Créer `RevokeApiKeyCommand.cs`**

```csharp
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand : ICommand<RevokeApiKeyResult>;
```

- [ ] **Créer `RevokeApiKeyResult.cs`**

```csharp
namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed record RevokeApiKeyResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    private RevokeApiKeyResult() { }

    public static RevokeApiKeyResult Success() => new() { IsSuccess = true };
    public static RevokeApiKeyResult Failure(string error) => new() { IsSuccess = false, Error = error };
}
```

- [ ] **Créer `RevokeApiKeyCommandHandler.cs`**

```csharp
using Kairu.Application.Common;
using Kairu.Application.Settings.Common;
using Microsoft.Extensions.Logging;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed class RevokeApiKeyCommandHandler
    : ICommandHandler<RevokeApiKeyCommand, RevokeApiKeyResult>
{
    private readonly IApiKeyRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RevokeApiKeyCommandHandler> _logger;

    public RevokeApiKeyCommandHandler(
        IApiKeyRepository repository,
        ICurrentUserService currentUserService,
        ILogger<RevokeApiKeyCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<RevokeApiKeyResult> Handle(
        RevokeApiKeyCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUserId;
        await _repository.DeleteByUserIdAsync(userId, cancellationToken);
        _logger.LogInformation("API Key revoked for user {UserId}", userId);
        return RevokeApiKeyResult.Success();
    }
}
```

- [ ] **Créer `RevokeApiKeyCommandHandlerTests.cs`**

```csharp
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
```

- [ ] **Build + tests**

```bash
cd src/Kairu.Application && dotnet build
cd ../../tests/Kairu.Application.Tests && dotnet test --filter "FullyQualifiedName~RevokeApiKeyCommandHandlerTests" -v normal
```
Expected: Build succeeded, 2 tests passent.

- [ ] **Commit**

```bash
git add src/Kairu.Application/Settings/Commands/RevokeApiKey/ tests/Kairu.Application.Tests/Settings/RevokeApiKeyCommandHandlerTests.cs
git commit -m "feat(api-key): RevokeApiKey command + handler + tests"
```

---

## Task 7 — Infrastructure : EF Core + Repository

**Files:**
- Create: `src/Kairu.Infrastructure/Persistence/UserApiKeyConfiguration.cs`
- Create: `src/Kairu.Infrastructure/Persistence/Repositories/EfCoreApiKeyRepository.cs`
- Modify: `src/Kairu.Infrastructure/Persistence/KairuDbContext.cs`
- Modify: `src/Kairu.Infrastructure/DependencyInjection.cs`

- [ ] **Créer `UserApiKeyConfiguration.cs`**

```csharp
using Kairu.Domain.Identity;
using Kairu.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kairu.Infrastructure.Persistence;

internal sealed class UserApiKeyConfiguration : IEntityTypeConfiguration<UserApiKey>
{
    public void Configure(EntityTypeBuilder<UserApiKey> builder)
    {
        builder.ToTable("UserApiKeys");

        builder.HasKey(k => k.OwnerId);

        builder.Property(k => k.OwnerId)
            .HasConversion(v => v.Value, v => UserId.From(v))
            .ValueGeneratedNever();

        builder.Property(k => k.KeyHash)
            .HasColumnType("nvarchar(64)")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(k => k.CreatedAt)
            .IsRequired();
    }
}
```

- [ ] **Modifier `KairuDbContext.cs`** — ajouter le DbSet après `UserSettings` :

```csharp
// Ligne à ajouter après: public DbSet<UserSettings> UserSettings => Set<UserSettings>();
public DbSet<UserApiKey> UserApiKeys => Set<UserApiKey>();
```

Et dans `OnModelCreating`, après `modelBuilder.ApplyConfiguration(new UserSettingsConfiguration());` :
```csharp
modelBuilder.ApplyConfiguration(new UserApiKeyConfiguration());
```

- [ ] **Créer `EfCoreApiKeyRepository.cs`**

```csharp
using Kairu.Application.Settings.Common;
using Kairu.Domain.Identity;
using Kairu.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kairu.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreApiKeyRepository : IApiKeyRepository
{
    private readonly KairuDbContext _db;

    public EfCoreApiKeyRepository(KairuDbContext db) => _db = db;

    public async Task UpsertAsync(UserApiKey apiKey, CancellationToken cancellationToken = default)
    {
        var existing = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.OwnerId == apiKey.OwnerId, cancellationToken);

        if (existing is null)
            _db.UserApiKeys.Add(apiKey);
        else
            existing.Regenerate(apiKey.KeyHash, apiKey.CreatedAt);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserId?> GetUserIdByHashAsync(
        string keyHash, CancellationToken cancellationToken = default)
    {
        var apiKey = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, cancellationToken);
        return apiKey?.OwnerId;
    }

    public async Task<UserApiKey?> GetByUserIdAsync(
        UserId userId, CancellationToken cancellationToken = default)
        => await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.OwnerId == userId, cancellationToken);

    public async Task DeleteByUserIdAsync(
        UserId userId, CancellationToken cancellationToken = default)
    {
        var apiKey = await _db.UserApiKeys
            .FirstOrDefaultAsync(k => k.OwnerId == userId, cancellationToken);
        if (apiKey is not null)
        {
            _db.UserApiKeys.Remove(apiKey);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
```

- [ ] **Modifier `DependencyInjection.cs`** — ajouter après l'enregistrement de `IUserSettingsRepository` :

```csharp
services.AddScoped<IApiKeyRepository, EfCoreApiKeyRepository>();
```

Et ajouter le using manquant en haut du fichier :
```csharp
using Kairu.Application.Settings.Common;
```

- [ ] **Build Infrastructure**

```bash
cd src/Kairu.Infrastructure && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Infrastructure/Persistence/UserApiKeyConfiguration.cs \
        src/Kairu.Infrastructure/Persistence/Repositories/EfCoreApiKeyRepository.cs \
        src/Kairu.Infrastructure/Persistence/KairuDbContext.cs \
        src/Kairu.Infrastructure/DependencyInjection.cs
git commit -m "feat(api-key): infrastructure EfCoreApiKeyRepository + EF config"
```

---

## Task 8 — Migration EF Core

**Files:**
- Create: `src/Kairu.Infrastructure/Migrations/XXXXXX_AddUserApiKeys.cs` (généré automatiquement)

- [ ] **Générer la migration depuis `src/Kairu.Infrastructure`**

```bash
cd src/Kairu.Infrastructure
dotnet ef migrations add AddUserApiKeys --startup-project ../Kairu.Api
```
Expected: `Done. To undo this action, use 'ef migrations remove'`
Un nouveau fichier `Migrations/XXXXXX_AddUserApiKeys.cs` est créé.

- [ ] **Vérifier le contenu de la migration**

Ouvrir le fichier généré et vérifier :
- `Up()` crée la table `UserApiKeys` avec colonnes `OwnerId` (nvarchar PK), `KeyHash` (nvarchar(64)), `CreatedAt`
- `Down()` supprime la table `UserApiKeys`

- [ ] **Build final avec migration**

```bash
cd src/Kairu.Api && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Infrastructure/Migrations/
git commit -m "feat(api-key): migration EF Core AddUserApiKeys"
```

---

## Task 9 — Auth Handler `ApiKeyAuthHandler`

**Files:**
- Create: `src/Kairu.Api/Mcp/ApiKeyAuthOptions.cs`
- Create: `src/Kairu.Api/Mcp/ApiKeyAuthHandler.cs`

- [ ] **Créer `ApiKeyAuthOptions.cs`**

```csharp
using Microsoft.AspNetCore.Authentication;

namespace Kairu.Api.Mcp;

public sealed class ApiKeyAuthOptions : AuthenticationSchemeOptions { }
```

- [ ] **Créer `ApiKeyAuthHandler.cs`**

```csharp
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Kairu.Application.Settings.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kairu.Api.Mcp;

public sealed class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthHandler(
        IOptionsMonitor<ApiKeyAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyRepository apiKeyRepository)
        : base(options, logger, encoder)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extraire le token depuis le header Authorization: Bearer kairu_xxx
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = headerValue["Bearer ".Length..].Trim();

        if (string.IsNullOrEmpty(token) || !token.StartsWith("kairu_"))
            return AuthenticateResult.Fail("Invalid API key format.");

        // Calculer le hash SHA-256 du token
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var keyHash = Convert.ToHexString(hashBytes).ToLower();

        // Chercher en base
        var userId = await _apiKeyRepository.GetUserIdByHashAsync(keyHash, Context.RequestAborted);
        if (userId is null)
            return AuthenticateResult.Fail("Invalid or revoked API key.");

        // Créer un ClaimsPrincipal identique au flux JWT (claim "sub" = UserId GUID)
        var claims = new[] { new Claim("sub", userId.Value.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
```

- [ ] **Build**

```bash
cd src/Kairu.Api && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Api/Mcp/ApiKeyAuthOptions.cs src/Kairu.Api/Mcp/ApiKeyAuthHandler.cs
git commit -m "feat(api-key): ApiKeyAuthHandler ASP.NET Core"
```

---

## Task 10 — Controller `ApiKeyController`

**Files:**
- Create: `src/Kairu.Api/Settings/ApiKeyController.cs`

- [ ] **Créer `ApiKeyController.cs`**

```csharp
using Kairu.Application.Settings.Commands.GenerateApiKey;
using Kairu.Application.Settings.Commands.RevokeApiKey;
using Kairu.Application.Settings.Queries.GetApiKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monbsoft.BrilliantMediator.Abstractions;

namespace Kairu.Api.Settings;

[ApiController]
[Route("api/settings/api-key")]
[Authorize]
public sealed class ApiKeyController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApiKeyController(IMediator mediator) => _mediator = mediator;

    /// <summary>Retourne le statut de la clé API (jamais le token brut).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _mediator.SendAsync<GetApiKeyQuery, GetApiKeyResult>(
            new GetApiKeyQuery(), ct);
        return Ok(new { result.Exists, result.CreatedAt });
    }

    /// <summary>Génère (ou régénère) une API Key. Le token est retourné une seule fois.</summary>
    [HttpPost]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<GenerateApiKeyCommand, GenerateApiKeyResult>(
            new GenerateApiKeyCommand(), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { token = result.Token });
    }

    /// <summary>Révoque la clé API de l'utilisateur courant.</summary>
    [HttpDelete]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var result = await _mediator.DispatchAsync<RevokeApiKeyCommand, RevokeApiKeyResult>(
            new RevokeApiKeyCommand(), ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }
}
```

- [ ] **Build**

```bash
cd src/Kairu.Api && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Api/Settings/ApiKeyController.cs
git commit -m "feat(api-key): ApiKeyController REST endpoints"
```

---

## Task 11 — Package NuGet MCP + `KairuMcpTools`

**Files:**
- Modify: `src/Kairu.Api/Kairu.Api.csproj`
- Create: `src/Kairu.Api/Mcp/KairuMcpTools.cs`

- [ ] **Ajouter le package NuGet dans `Kairu.Api.csproj`**

Ajouter dans l'`<ItemGroup>` des PackageReferences :
```xml
<PackageReference Include="ModelContextProtocol.AspNetCore" Version="1.2.0" />
```

- [ ] **Restaurer les packages**

```bash
cd src/Kairu.Api && dotnet restore
```
Expected: Restore succeeded.

- [ ] **Créer `KairuMcpTools.cs`**

```csharp
using System.ComponentModel;
using System.Text.Json;
using Kairu.Application.Tasks.Commands.AddTask;
using Kairu.Application.Tasks.Commands.CompleteTask;
using Kairu.Application.Tasks.Commands.DeleteTask;
using Kairu.Application.Tasks.Queries.ListTasks;
using ModelContextProtocol.Server;

namespace Kairu.Api.Mcp;

[McpServerToolType]
public sealed class KairuMcpTools(
    AddTaskCommandHandler addTask,
    ListTasksQueryHandler listTasks,
    CompleteTaskCommandHandler completeTask,
    DeleteTaskCommandHandler deleteTask)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool, Description("Create a new task in Kairu. Returns the created task as JSON.")]
    public async Task<string> create_task(
        [Description("Task title (required, max 200 characters)")] string title,
        [Description("Optional task description")] string? description = null)
    {
        var result = await addTask.Handle(new AddTaskCommand(title, description));
        return result.IsSuccess
            ? JsonSerializer.Serialize(result.Task, JsonOptions)
            : $"Error: {result.Error}";
    }

    [McpServerTool, Description("List tasks in Kairu. Returns a JSON array of tasks.")]
    public async Task<string> list_tasks(
        [Description("Status filter: 'all', 'pending', 'inprogress', 'done', 'openonly' (default: 'openonly')")] string status = "openonly")
    {
        var filter = status.ToLower() switch
        {
            "all" => TaskStatusFilter.All,
            "pending" => TaskStatusFilter.Pending,
            "inprogress" => TaskStatusFilter.InProgress,
            "done" => TaskStatusFilter.Done,
            _ => TaskStatusFilter.OpenOnly
        };

        var result = await listTasks.Handle(new ListTasksQuery(StatusFilter: filter));
        return JsonSerializer.Serialize(result.Tasks, JsonOptions);
    }

    [McpServerTool, Description("Mark a task as completed in Kairu.")]
    public async Task<string> complete_task(
        [Description("Task ID (GUID format, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6')")] string taskId)
    {
        if (!Guid.TryParse(taskId, out var guid))
            return "Error: taskId must be a valid GUID.";

        var result = await completeTask.Handle(new CompleteTaskCommand(guid));
        if (result.IsSuccess) return $"Task {taskId} marked as completed.";
        if (result.IsNotFound) return $"Error: Task {taskId} not found.";
        return $"Error: {result.Error}";
    }

    [McpServerTool, Description("Delete a task from Kairu.")]
    public async Task<string> delete_task(
        [Description("Task ID (GUID format, e.g. '3fa85f64-5717-4562-b3fc-2c963f66afa6')")] string taskId)
    {
        if (!Guid.TryParse(taskId, out var guid))
            return "Error: taskId must be a valid GUID.";

        var result = await deleteTask.Handle(new DeleteTaskCommand(guid));
        if (result.IsSuccess) return $"Task {taskId} deleted.";
        if (result.IsNotFound) return $"Error: Task {taskId} not found.";
        return $"Error: {result.Error}";
    }
}
```

- [ ] **Build**

```bash
cd src/Kairu.Api && dotnet build
```
Expected: Build succeeded, 0 errors.

- [ ] **Commit**

```bash
git add src/Kairu.Api/Kairu.Api.csproj src/Kairu.Api/Mcp/KairuMcpTools.cs
git commit -m "feat(mcp): KairuMcpTools avec 4 tools Tasks"
```

---

## Task 12 — Wiring `Program.cs`

**Files:**
- Modify: `src/Kairu.Api/Program.cs`

- [ ] **Ajouter le scheme ApiKey dans le bloc `AddAuthentication()`**

Après `.AddOAuth("GitHub", ...)` (autour de la ligne 106), ajouter :
```csharp
.AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>("ApiKey", _ => { });
```

- [ ] **Ajouter la policy MCP dans `AddAuthorization()`**

Remplacer `builder.Services.AddAuthorization();` par :
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("McpApiKey", policy =>
        policy.AddAuthenticationSchemes("ApiKey")
              .RequireAuthenticatedUser());
});
```

- [ ] **Ajouter `AddMcpServer()` après `builder.Services.AddControllers()`**

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<KairuMcpTools>();
```

- [ ] **Ajouter `MapMcp` après `app.MapControllers()`**

```csharp
app.MapMcp("/mcp").RequireAuthorization("McpApiKey");
```

- [ ] **Build complet de la solution**

```bash
cd C:/Users/oliver254/dev/Kairudev
dotnet build Kairudev.slnx
```
Expected: Build succeeded, 0 errors.

- [ ] **Lancer tous les tests**

```bash
dotnet test Kairudev.slnx --filter "FullyQualifiedName~Kairu.Application.Tests"
```
Expected: Tous les tests existants passent + les nouveaux (7 nouveaux tests).

- [ ] **Commit**

```bash
git add src/Kairu.Api/Program.cs
git commit -m "feat(mcp): wiring Program.cs — ApiKey auth + MCP server + MapMcp"
```

---

## Task 13 — Vérification finale et push

- [ ] **Build + tests complets**

```bash
cd C:/Users/oliver254/dev/Kairudev
dotnet build Kairudev.slnx && dotnet test Kairudev.slnx
```
Expected: Build succeeded, tous les tests passent (192 existants + 9 nouveaux).

- [ ] **Test manuel de l'endpoint MCP (optionnel en local)**

```bash
# Démarrer l'API
cd src/Kairu.Api && dotnet run

# Dans un autre terminal — générer une clé (nécessite un JWT valide)
curl -X POST https://localhost:5001/api/settings/api-key \
     -H "Authorization: Bearer <jwt_token>"
# → { "token": "kairu_xxx" }

# Tester l'endpoint MCP
curl https://localhost:5001/mcp \
     -H "Authorization: Bearer kairu_xxx"
# → réponse SSE de handshake MCP
```

- [ ] **Push de la branche**

```bash
git push -u origin feature/25-mcp-server
```

---

## Configuration client Claude Code

Une fois déployé, configurer dans `~/.claude.json` ou `claude_desktop_config.json` :

```json
{
  "mcpServers": {
    "kairu": {
      "url": "https://kairudev-prod.azurewebsites.net/mcp",
      "headers": {
        "Authorization": "Bearer kairu_xxxxx"
      }
    }
  }
}
```
