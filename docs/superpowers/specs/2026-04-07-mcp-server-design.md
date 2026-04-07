# Kairu MCP Server — Design Spec
**Date :** 2026-04-07
**Itération :** #25
**Branche :** `feature/25-mcp-server`

---

## Contexte

Exposer Kairu comme un **MCP server** (Model Context Protocol) utilisable par des clients IA
(Claude Code, Codex, Claude Desktop, etc.) via HTTP + SSE.
L'objectif est qu'un agent IA puisse gérer les tâches Kairu d'un développeur sans quitter
son environnement de travail.

---

## Décisions clés

| Décision | Choix | Raison |
|---|---|---|
| Transport | HTTP + SSE (Streamable HTTP) | API hébergée sur Azure, accessible depuis n'importe quel client IA |
| Auth | API Key dédiée (`kairu_xxx`) | Token longue durée adapté aux clients IA, pas d'expiration courte JWT |
| Scope itération #25 | Tasks uniquement | Valeur immédiate, livraison rapide |
| Hébergement | Intégré dans `Kairu.Api` | Même process, même déploiement Azure, zéro infrastructure supplémentaire |
| SDK | `ModelContextProtocol.AspNetCore` 1.2.0 | SDK officiel stable (mars 2026), zéro code de protocole à écrire |

---

## Architecture

```
Client IA (Claude Code, Codex…)
        │  HTTP + SSE  →  Authorization: Bearer kairu_xxxxx
        ▼
Kairu.Api/Mcp/
├── KairuMcpTools.cs          ← tools MCP (create, list, complete, delete)
└── ApiKeyAuthHandler.cs      ← nouveau scheme d'auth ASP.NET Core

        │ injecte et appelle
        ▼
Kairu.Application/Tasks/      ← handlers CQRS existants (inchangés)

        │ lit la clé via
        ▼
Kairu.Application/Settings/
└── Commands/GenerateApiKey/  ← nouveau use case
└── Commands/RevokeApiKey/    ← nouveau use case
└── Queries/GetApiKey/        ← nouveau use case

        │ stocke dans
        ▼
Kairu.Infrastructure/Persistence/
└── ApiKeyRepository.cs       ← nouvelle table UserApiKeys
```

---

## Section 1 — API Key

### Table `UserApiKeys`

```
UserId      string   PK + FK → Users
KeyHash     string   SHA-256 du token brut (jamais le token lui-même)
CreatedAt   DateTime
```

Contrainte : un utilisateur = une clé active maximum.

### Flux de génération

1. `POST /api/settings/api-key` → `GenerateApiKeyCommandHandler`
2. Génère : `"kairu_" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))`
3. Stocke `SHA256(token)` dans `UserApiKeys` (upsert — régénère si existante)
4. Retourne le token brut **une seule fois** — jamais renvoyé ensuite

### Endpoints Settings

| Méthode | Route | Description |
|---|---|---|
| `POST` | `/api/settings/api-key` | Génère ou régénère la clé — retourne le token brut |
| `GET` | `/api/settings/api-key` | Retourne `{ exists: bool, createdAt: string }` — jamais le token |
| `DELETE` | `/api/settings/api-key` | Révoque la clé |

### Validation à chaque appel MCP

1. `ApiKeyAuthHandler` extrait le token du header `Authorization: Bearer kairu_xxx`
2. Calcule `SHA256(token)`
3. Cherche le hash en base → identifie `UserId`
4. Alimente `HttpContext.User` avec un `ClaimsPrincipal` (claim `sub` = UserId)
5. `ICurrentUserService` fonctionne de manière identique au flux JWT — zéro duplication

Auth échouée → `401 Unauthorized` HTTP standard.

---

## Section 2 — MCP Tools

**Endpoint SSE :** `https://kairudev-prod.azurewebsites.net/mcp`
**Auth requise :** `Authorization: Bearer kairu_xxxxx`

### Tools exposés

| Tool | Handler CQRS | Description |
|---|---|---|
| `create_task` | `AddTaskCommandHandler` | Crée une tâche (titre requis, description optionnelle) |
| `list_tasks` | `ListTasksQueryHandler` | Liste les tâches (filtre optionnel : `pending`/`completed`/`all`) |
| `complete_task` | `CompleteTaskCommandHandler` | Marque une tâche complétée par son ID |
| `delete_task` | `DeleteTaskCommandHandler` | Supprime une tâche par son ID |

### Classe `KairuMcpTools`

```csharp
[McpServerToolType]
public class KairuMcpTools(
    AddTaskCommandHandler addTask,
    ListTasksQueryHandler getTasks,
    CompleteTaskCommandHandler completeTask,
    DeleteTaskCommandHandler deleteTask,
    ICurrentUserService currentUser)
{
    [McpServerTool, Description("Create a new task in Kairu")]
    public async Task<string> create_task(
        [Description("Task title (required)")] string title,
        [Description("Optional description")] string? description = null) { ... }

    [McpServerTool, Description("List today's tasks")]
    public async Task<string> list_tasks(
        [Description("Filter: 'pending', 'completed', or 'all' (default)")] string status = "all") { ... }

    [McpServerTool, Description("Mark a task as completed")]
    public async Task<string> complete_task(
        [Description("Task ID (GUID)")] string taskId) { ... }

    [McpServerTool, Description("Delete a task")]
    public async Task<string> delete_task(
        [Description("Task ID (GUID)")] string taskId) { ... }
}
```

### Config `Program.cs`

```csharp
// Services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<KairuMcpTools>();

builder.Services.AddAuthentication()
    .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>("ApiKey", _ => { });

// Routes (après les routes existantes)
app.MapMcp("/mcp").RequireAuthorization();
```

### Gestion d'erreurs

Les tools ne lèvent pas d'exceptions — ils retournent toujours une réponse textuelle exploitable par l'IA :

```
Succès  → JSON sérialisé (tâche ou liste de tâches)
Échec   → "Error: {result.Error}" — message lisible par l'IA
```

---

## Section 3 — Tests

| Fichier | Scénarios |
|---|---|
| `GenerateApiKeyCommandHandlerTests` | Génération, hash stocké (jamais le brut), régénération (upsert) |
| `GetApiKeyQueryHandlerTests` | Clé existante → `exists: true`, absente → `exists: false`, hash jamais exposé |
| `RevokeApiKeyCommandHandlerTests` | Révocation, idempotence (pas d'erreur si déjà révoquée) |

`ApiKeyAuthHandler` est un handler ASP.NET Core — il n'est pas testable en unit test pur.
Il sera couvert par des tests d'intégration (`WebApplicationFactory`) lors d'une itération dédiée.

Les tools MCP (`KairuMcpTools`) ne sont pas testés unitairement :
ce sont de simples délégations aux handlers, déjà couverts par les 192 tests existants.

---

## Fichiers créés / modifiés

### Nouveaux fichiers
```
src/Kairu.Application/Settings/Commands/GenerateApiKey/
    GenerateApiKeyCommand.cs
    GenerateApiKeyCommandHandler.cs
    GenerateApiKeyResult.cs
src/Kairu.Application/Settings/Commands/RevokeApiKey/
    RevokeApiKeyCommand.cs
    RevokeApiKeyCommandHandler.cs
    RevokeApiKeyResult.cs
src/Kairu.Application/Settings/Queries/GetApiKey/
    GetApiKeyQuery.cs
    GetApiKeyQueryHandler.cs
    GetApiKeyResult.cs
src/Kairu.Application/Settings/Common/
    IApiKeyRepository.cs

src/Kairu.Infrastructure/Persistence/
    ApiKeyRepository.cs
    (migration EF Core)

src/Kairu.Api/Mcp/
    KairuMcpTools.cs
    ApiKeyAuthHandler.cs
    ApiKeyAuthOptions.cs
src/Kairu.Api/Controllers/
    ApiKeyController.cs

tests/Kairu.Application.Tests/Settings/
    GenerateApiKeyCommandHandlerTests.cs
    GetApiKeyQueryHandlerTests.cs
    RevokeApiKeyCommandHandlerTests.cs
```

### Fichiers modifiés
```
src/Kairu.Api/Program.cs              ← AddMcpServer + ApiKeyAuthHandler + MapMcp
src/Kairu.Infrastructure/DependencyInjection.cs  ← enregistrement ApiKeyRepository
src/Kairu.Api/Kairu.Api.csproj        ← PackageReference ModelContextProtocol.AspNetCore 1.2.0
```

---

## Configuration client IA (Claude Code)

Après génération d'une clé dans Settings, l'utilisateur configure son client MCP :

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

---

## Ce qui est hors scope (#25)

- UI Blazor pour afficher/copier la clé API (itération suivante)
- Tools Pomodoro et Journal
- Resources MCP (`kairu://tasks`)
- Révocation automatique (expiration)
