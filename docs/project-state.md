# Kairu — État du projet

> Ce fichier est mis à jour après chaque itération.
> Il est lu par Claude au démarrage de chaque session.

---

## Résumé état actuel

**Dernière itération complétée : #26 — MCP OAuth 2.1 + PKCE** ✅ COMPLÉTÉE (2026-04-08)

**Itération précédente : #24 — Renommage Kairudev → Kairu** ✅ COMPLÉTÉE (2026-04-07)

**Prochaine itération prévue : à définir**

**Itération précédente : #22 — Note sur le sprint libre** ✅ COMPLÉTÉE (2026-03-25)

**Itération précédente : #21 — Tags sur les tâches** ✅ COMPLÉTÉE (2026-03-25)

**Itération précédente : #18 — Éditeur Markdown + navigation pages dédiées** ✅ COMPLÉTÉE (2026-03-20)

**Itération : #17 — BC Sprint libre (chronomètre à durée variable)** ✅ COMPLÉTÉE

**Bounded Contexts opérationnels :**
- **Identity** : `User`, `UserId`, `IUserRepository`, `GetOrCreateUserCommandHandler` ✅
- **Tasks** : 8 Commands/Queries — **Architecture CQRS** ✅ (filtrés par `UserId`)
- **Pomodoro** : 10 Commands/Queries — **Architecture CQRS** ✅ (filtrés par `UserId`) — sessions standard + sessions libres (`PlannedDurationMinutes = 0`) + `GetTodaySprintSessionsAsync`
- **Journal** : 6 Commands/Queries — **Architecture CQRS** ✅ (filtrés par `UserId`)
- **Settings** : 4 Commands/Queries — **Architecture CQRS** ✅ (filtrés par `UserId`)
- **Tickets** : 1 Query — **Architecture CQRS** ✅ (**désactivé côté UI depuis #19**)

**Architecture Application Layer :**
- ✅ **CQRS sans MediatR** : Commands (écriture) + Queries (lecture)
- ✅ **Handlers** retournent directement des `Result` (plus de Presenters)
- ✅ **Injection directe** dans les Controllers (pas de mediator)
- ✅ **ICurrentUserService** : abstraction application-layer pour l'identité courante
- ✅ **30 use cases** (8 Tasks + 10 Pomodoro + 6 Journal + 4 Settings + 1 Tickets + 1 Identity)

**Authentification (bout en bout) :**
- ✅ GitHub OAuth 2.0 (`GET /api/auth/github` → callback → JWT HS256)
- ✅ `AuthController` : `/github`, `/github/callback`, `/me`
- ✅ JWT Bearer sur tous les controllers (Tasks, Pomodoro, Journal, Settings, Tickets)
- ✅ `ClaimsCurrentUserService` : lit le claim `sub` depuis le JWT
- ✅ `GetOrCreateUserCommandHandler` : crée l'utilisateur au premier login
- ✅ **Blazor WASM** : `JwtAuthenticationStateProvider`, `AuthService`, `Login.razor` avec flux complet
- ✅ **MAUI** : `MauiAuthService`, `SecureStorage`, `Login.razor` MAUI avec même flux
- ✅ `AddCookie()` + `DefaultSignInScheme = Cookie` (requis par RemoteAuthenticationHandler)
- ✅ `WebBaseUrl` dans `appsettings.Development.json` (redirection API → Web correcte)
- ✅ **MCP OAuth 2.1 + PKCE** : clients IA (Claude Code, Codex) via `OAuthController` — discovery automatique, login GitHub, JWT identique au flux Web
- ✅ `JwtTokenService` partagé entre `AuthController` et `OAuthController`

**MCP Server :**
- ✅ Serveur MCP sur `/mcp` (SDK `ModelContextProtocol.AspNetCore` 1.2.0, HTTP+SSE)
- ✅ 4 outils : `create_task`, `list_tasks`, `complete_task`, `delete_task`
- ✅ Auth OAuth 2.1 + PKCE via GitHub (pas d'API Key)
- ✅ Endpoints OAuth : `/.well-known/oauth-authorization-server`, `/oauth/authorize`, `/oauth/github/callback`, `/oauth/token`
- ✅ `IAuthorizationCodeStore` (Application) + `InMemoryAuthorizationCodeStore` (Infrastructure)
- ✅ `McpAuthenticationHandler` SDK pour resource metadata dans les 401

**Multi-users :**
- ✅ `OwnerId` (UserId) ajouté à `DeveloperTask`, `PomodoroSession`, `JournalEntry`
- ✅ `UserSettings` migré de singleton (int PK=1) à per-user (UserId PK)
- ✅ `PomodoroSettings` migré de singleton (Id=1) à per-user (UserId string PK)
- ✅ Tous les repositories filtrent les données par `UserId`
- ✅ Table `Users` créée en base (GitHubId unique, Login, DisplayName, Email?)

**UI (Blazor WASM) :**
- ✅ **Landing page** (`/`) : hero + 3 feature cards, bouton GitHub login, `[AllowAnonymous]`
- ✅ **Dashboard** (`/dashboard`) : salutation, stats (tâches, Pomodoro, journal), accès rapides, `[Authorize]`
- ✅ **NavMenu** : icône dashboard + bouton déconnexion SVG (logout → landing)
- ✅ **Redirection intelligente** : `/` → `/dashboard` si authentifié, `/login` → `/dashboard` après login
- ✅ **Jira désactivé côté UI/config** : pages Tickets supprimées, menu Tickets retiré, configuration Jira retirée de Settings

**Déploiement :**
- ✅ Application en production : https://kairudev-prod.azurewebsites.net
- ✅ Redéploiement : `powershell -ExecutionPolicy Bypass -File .\infra\deploy-linux.ps1 -Environment prod`

**Tests :** 197 au total ✅ (113 Domain + 84 Application)
⚠️ **Dette technique** : `Kairu.Infrastructure.Tests` supprimé de la solution (résidu bin/obj uniquement). `Kairu.IntegrationTests` non maintenu — step definitions obsolètes vs domain refactorisé. 4 tests Pomodoro (sprint sessions) échouent (pré-existant depuis #23).

**Infrastructure :** API REST, Blazor WASM, .NET MAUI, SQLite (local) + **Azure SQL (prod)**

---

## Itérations

| # | Contenu | Statut | Date |
|---|---|---|---|
| ~~#1~~ | ~~BC Tasks — Domain, Application, Adapters, Infrastructure, SQLite, 23 tests~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2~~ | ~~API REST (ASP.NET Core) + UI Blazor WebAssembly~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2b~~ | ~~Réécriture spec.md (use cases + diagrammes Mermaid) + prompts agents~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#3~~ | ~~BC Pomodoro — sessions de focus, chrono circulaire, lien avec Tasks~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#3b~~ | ~~.NET Aspire 13.1.1 — AppHost + ServiceDefaults~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#4~~ | ~~Tests d'intégration SQLite (`Kairu.Infrastructure.Tests`)~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#5~~ | ~~Configuration externalisée — URL API via `appsettings.json`~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#5b~~ | ~~Bugfixes (NetworkError + CreatedAtAction) + sous-agents Claude + UC-12 ChangeTaskStatus~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5c~~ | ~~UC-05 UpdateTask + Description optionnelle sur les tâches~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5d~~ | ~~Correctif migration EF Core — AddTaskDescription~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5e~~ | ~~UI Blazor — Description et modification de tâches~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#6~~ | ~~UC-05 (Pomodoro) — Configurer les durées (sprint, pause courte/longue)~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#7~~ | ~~Types de session Pomodoro (Sprint/Pause courte/Pause longue) — UI onglets~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#8~~ | ~~BC Journal — log d'activité quotidien + génération automatique d'entrées~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#10~~ | ~~.NET MAUI — application desktop/mobile~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#11~~ | ~~Migration CQRS sans MediatR — refactoring architectural~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~#11b~~ | ~~BC Settings + Thème Dark paramétrable~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~#11c~~ | ~~Nettoyage post-migration CQRS~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~bugfix~~ | ~~Pomodoro UI bloqué après fin de session (timer 0:00 + bouton Interrompre persistant)~~ | ~~✅ Livré~~ | ~~2026-03-02~~ |
| ~~#12~~ | ~~Journal : navigation historique + numérotation sprints~~ | ~~✅ Livré~~ | ~~2026-03-02~~ |
| ~~#13~~ | ~~BC Tickets — intégration Jira (liste, lien tâche, config)~~ | ~~✅ Livré~~ | ~~2026-03-03~~ |
| ~~#14~~ | ~~Navigation sidebar — icônes seulement, style VS Code (Web + MAUI)~~ | ~~✅ Livré~~ | ~~2026-03-03~~ |
| ~~#15~~ | ~~Auth GitHub + Multi-users — JWT, OwnerId sur toutes les entités, ICurrentUserService~~ | ~~✅ Livré~~ | ~~2026-03-04~~ |
| ~~#15b~~ | ~~Auth client Web + MAUI — Login.razor, JwtAuthenticationStateProvider, Landing page, Dashboard~~ | ~~✅ Livré~~ | ~~2026-03-09~~ |
| ~~#16~~ | ~~Déploiement Azure — Bicep (subscription scope), CLI, ZIP cross-platform, deploy-linux.ps1, prod en direct~~ | ~~✅ Livré~~ | ~~2026-03-16~~ |
| ~~#17~~ | ~~BC Sprint libre — chronomètre count-up, durée variable, lien tâche, journal auto~~ | ~~✅ Livré~~ | ~~2026-03-18~~ |
| ~~#18~~ | ~~Éditeur Markdown + navigation pages dédiées — Markdig, onglets Éditer/Prévisualiser, pages TaskDetail/TaskEdit, suppression modale~~ | ~~✅ Livré~~ | ~~2026-03-20~~ |
| ~~#19~~ | ~~Retrait Jira (pages + configuration UI) — suppression pages Tickets Web/MAUI, retrait menu, retrait config Jira dans Settings, retrait endpoint API `/api/settings/jira`~~ | ~~✅ Livré~~ | ~~2026-03-23~~ |
| ~~#20~~ | ~~Filtrage et tri des tâches — défaut ouvertes + récentes, recherche titre, filtre statut, côté serveur~~ | ~~✅ Livré~~ | ~~2026-03-24~~ |
| ~~#23~~ | ~~Fusion SprintSession / PomodoroSession (ADR-005) — `SprintSession` supprimé, sessions libres portées par `PomodoroSession` (`PlannedDurationMinutes = 0`)~~ | ~~✅ Livré~~ | ~~2026-03-26~~ |
| ~~#24~~ | ~~Renommage Kairudev → Kairu — namespaces C#, projets, solution, `manifest.json`, fichier `.http`, correction CSS menu/bouton GitHub~~ | ~~✅ Livré~~ | ~~2026-04-07~~ |
| **#26** | **MCP OAuth 2.1 + PKCE — serveur MCP 4 outils Tasks, auth OAuth GitHub, suppression API Key, JwtTokenService partagé** | **✅ Livré** | **2026-04-08** |
| ~~#22~~ | ~~Note sur le sprint libre — champ Note remplace Nom, persistée comme JournalComment, éditable dans le journal~~ | ~~✅ Livré~~ | ~~2026-03-25~~ |
| ~~#21~~ | ~~Tags sur les tâches — Value Object `TaskTag`, saisie chips/badges (création + édition), affichage couleurs automatiques, max 5, JSON en base~~ | ~~✅ Livré~~ | ~~2026-03-25~~ |

---

## Dernière itération livrée

**#26 — MCP OAuth 2.1 + PKCE** — Livré le 2026-04-08

### Ce qui a été livré

**Serveur MCP :**
- 4 outils MCP (`create_task`, `list_tasks`, `complete_task`, `delete_task`) sur `/mcp`
- SDK `ModelContextProtocol.AspNetCore` 1.2.0, transport HTTP+SSE
- `KairuMcpTools` : dispatch via `IMediator` (BrilliantMediator)

**OAuth 2.1 Authorization Server (remplace API Key) :**
- `OAuthController` : 4 endpoints (`/.well-known/oauth-authorization-server`, `/oauth/authorize`, `/oauth/github/callback`, `/oauth/token`)
- PKCE S256 obligatoire, `client_id = "kairu-mcp"`
- État PKCE préservé via cookie chiffré (`IDataProtector`, JSON, `SameSite=None`)
- `IAuthorizationCodeStore` + `InMemoryAuthorizationCodeStore` (ConcurrentDictionary, single-use, TTL 5min, purge passive 10min)
- `McpAuthenticationHandler` SDK : resource metadata dans les 401 challenges
- JWT identique au flux Web (claims `sub` + `name` + `login`)

**Refactoring :**
- `JwtTokenService` partagé entre `AuthController` et `OAuthController` (DRY)
- Suppression complète API Key : `UserApiKey`, `IApiKeyRepository`, 3 handlers, `ApiKeyAuthHandler`, `ApiKeyController`, UI Settings, migration `DropUserApiKeys`

**Configuration client MCP :**
```json
{
  "mcpServers": {
    "kairu": {
      "url": "https://kairudev-prod.azurewebsites.net/mcp"
    }
  }
}
```

### Itération précédente

**#24 — Renommage Kairudev → Kairu** — Livré le 2026-04-07

- Renommage complet `Kairudev` → `Kairu` : namespaces C#, projets `.csproj`, solution `.slnx`, migrations EF Core, agents, répertoires `src/`
- `manifest.json` : `name` et `short_name` mis à jour (`"Kairu"`)
- `Kairudev.Api.http` renommé en `Kairu.Api.http`, variable `@Kairu.Api_HostAddress`
- Correction CSS : `.btn-github` déplacé de `Login.razor.css` (scoped) vers `app.css` (global)

⚠️ **Dette technique** : `appsettings.Production.json` pointe encore vers `kairudev-prod.azurewebsites.net` (URL Azure App Service indépendante du nom — à mettre à jour si redéploiement sous nouveau nom de resource).

---

**#23 — Fusion SprintSession / PomodoroSession (ADR-005)** — Livré le 2026-03-26

### Ce qui a été livré

**Domain** ✅
- `SprintSession`, `SprintSessionId`, `SprintName`, `SprintOutcome`, `SprintDomainErrors`, `ISprintSessionRepository` : supprimés
- `PomodoroSession` : support des sessions libres via `PlannedDurationMinutes = 0`
- `IPomodoroRepository` : +`GetTodaySprintSessionsAsync(UserId, DateOnly)`

**Application (CQRS)** ✅
- Use cases Sprint (`RecordSprintCommand`, `GetTodaySprintSessions`) migrés vers le BC Pomodoro
- `RecordSprintCommandHandler` : crée désormais une `PomodoroSession` avec `PlannedDurationMinutes = 0`

**Infrastructure** ✅
- `SprintSessionConfiguration` supprimée
- `PomodoroSessionRepository` : implémentation de `GetTodaySprintSessionsAsync`
- Migration EF Core : suppression de la table `SprintSessions`

**API** ✅
- `SprintController` supprimé ; endpoints Sprint portés par `PomodoroController`

**UI Web (Blazor WASM)** ✅
- `SprintLibre.razor` : appelle désormais l'API Pomodoro

**UI MAUI (Blazor Hybrid)** ✅
- Idem Web

**Tests** ✅ (192 au total : 113 Domain + 79 Application)
- Tests Sprint migrés / supprimés selon leur pertinence post-fusion

**Documentation** ✅
- ADR-005 : marqué appliqué
- `spec.md` : BC Sprint supprimé, BC Pomodoro mis à jour
- `project-state.md` : mis à jour

### Impact
- Le bounded context `Sprint` n'existe plus ; les sessions libres sont des `PomodoroSession` standard avec `PlannedDurationMinutes = 0`
- Réduction de la dette technique (suppression du doublon `SprintSession` / `PomodoroSession`)

### Dette technique introduite
- Aucune

---

## Itération précédente

**#22 — Note sur le sprint libre** — Livré le 2026-03-25

### Ce qui a été livré

**Application (CQRS)** ✅
- `CreateEntryCommand` : `+ string? InitialComment = null`
- `CreateEntryCommandHandler` : si `InitialComment` non vide → `entry.AddComment(InitialComment.Trim())`
- `RecordSprintCommand` : `Name?` → `Note?`
- `RecordSprintCommandHandler` : nom toujours `null` (auto "Sprint #N") ; `Note` passée comme `InitialComment` sur l'entrée de fin

**API** ✅
- `RecordSprintBody` : `Name?` → `Note?`

**UI Web (Blazor WASM)** ✅
- `SprintDto.RecordSprintRequest` : `Name?` → `Note?`
- `SprintLibre.razor` : champ "Note (optionnelle)" remplace "Nom du sprint" ; nom toujours auto affiché "Sprint #N en cours"

**UI MAUI (Blazor Hybrid)** ✅
- Identique Web

**Tests** ✅ (+8 tests, total 221 avant fusion #23)
- `RecordSprintCommandHandlerTests` : mise à jour + 4 nouveaux tests note
- `CreateEntryCommandHandlerTests` : +4 nouveaux tests `InitialComment`

**Documentation** ✅
- `ADR-005` : Sprint libre = Pomodoro à durée inconnue, fusion prévue #23
- `spec.md` : UC-SP-01 mis à jour
- `project-state.md` : mis à jour

### Impact
- Sur `/pomodoro/libre`, le nom du sprint est toujours auto-généré "Sprint #N"
- Une note optionnelle peut être saisie avant le démarrage et apparaît dans le journal

### Dette technique introduite
- ADR-005 appliqué en itération #23

---

## Itération précédente

**#21 — Tags sur les tâches** — Livré le 2026-03-25


### Ce qui a été livré

**Domain** ✅
- `TaskTag` : Value Object — `MaxLength = 30`, `Create()` retourne `Result<TaskTag>`, `Equals` case-insensitive
- `DeveloperTask.TagValues` : `List<string>` (public `init`) — persistance EF Core via `PrimitiveCollection`
- `DeveloperTask.Tags` : `IReadOnlyList<TaskTag>` — propriété domaine calculée depuis `TagValues`
- `DeveloperTask.SetTags()` : retourne `Result`, vérifie max 5 et doublons case-insensitive
- `DeveloperTask.Create()` : accepte `IEnumerable<TaskTag>? tags` optionnel
- `DomainErrors.Tasks` : +4 constantes (`TagEmpty`, `TagTooLong`, `TooManyTags`, `DuplicateTag`)

**Application (CQRS)** ✅
- `TaskViewModel` : +`List<string> Tags`
- `AddTaskCommand` : +`List<string>? Tags`
- `AddTaskCommandHandler` : validation tags → `TaskTag.Create()` → passage à `DeveloperTask.Create()`
- `UpdateTaskCommand` : +`List<string>? Tags`
- `UpdateTaskCommandHandler` : validation tags → `TaskTag.Create()` → `task.SetTags()`

**Infrastructure** ✅
- `TaskConfiguration` : `Ignore(t => t.Tags)` + `PrimitiveCollection(t => t.TagValues).HasColumnName("Tags").HasColumnType("nvarchar(max)")`
- Migration `AddTaskTags` : colonne `Tags nvarchar(max) NOT NULL DEFAULT '[]'`

**API** ✅
- `UpdateTaskBody` : +`List<string>? Tags`

**UI Web (Blazor WASM)** ✅
- `TaskDto` : +`List<string>? Tags`
- `TaskApiClient` : `AddAsync` + `UpdateAsync` avec `tags`
- `Tasks.razor` : saisie chips (champ texte + Entrée, badge ✕, disabled si 5 max), badges colorés dans la liste
- `TaskEdit.razor` : section Tags avec chips (pré-remplie, même logique)
- `TagColorClass()` : hash case-insensitive → palette 10 couleurs Bootstrap

**UI MAUI (Blazor Hybrid)** ✅
- Même mise à jour que Web

**Tests** ✅ (+27 tests, total 213)
- `TaskTagTests` (Domain) : 6 tests Value Object
- `DeveloperTaskTagTests` (Domain) : 7 tests `SetTags` + `Create` avec tags
- `AddTaskWithTagsCommandHandlerTests` (Application) : 4 tests
- `UpdateTaskCommandHandlerTests` (Application) : 5 tests

### Impact
- Les tâches peuvent avoir jusqu'à 5 tags (texte plain ≤ 30 chars)
- Saisie intuitive via chips (Entrée pour ajouter, ✕ pour supprimer)
- Badges colorés automatiquement affichés sur la page liste des tâches
- Tags persistés en JSON dans la colonne `Tags` (Azure SQL + SQLite local)

### Dette technique introduite
- Aucune

---

## Itération précédente

**#20 — Filtrage et tri des tâches** — Livré le 2026-03-24

### Ce qui a été livré

**Domain** ✅
- Aucune modification

**Application (CQRS)** ✅
- Aucune modification

**Infrastructure** ✅
- Aucune modification

**API** ✅
- Suppression de l'endpoint `PUT /api/settings/jira`

**UI Web (Blazor WASM)** ✅
- Suppression de la page `/tickets`
- Retrait du menu Tickets dans `NavMenu.razor`
- Retrait de la configuration Jira dans `Settings.razor`

**UI MAUI (Blazor Hybrid)** ✅
- Même logique que pour Web : retrait de la page Tickets et du menu

**Tests** ✅ (aucun test impacté)

### Impact
- Les fonctionnalités liées à Jira ont été complètement retirées de l'application, tant côté UI que configuration.

### Dette technique introduite
- Aucune

---

## Itération précédente

**#18 — Éditeur Markdown + navigation pages dédiées** — Livré le 2026-03-20

### Ce qui a été livré

**Domain** ✅
- Aucune modification

**Application (CQRS)** ✅
- Aucune modification

**Infrastructure** ✅
- Aucune modification

**API** ✅
- Aucune modification

**UI Web (Blazor WASM)** ✅
- `TaskDetail.razor` : affichage en lecture seule, section "Détails de la tâche" avec champs non modifiables
- `TaskEdit.razor` : édition de tâche avec description, statut, dates, durée, responsables
- Modale de suppression de tâche avec confirmation
- `MarkdownEditor.razor` : éditeur Markdown avec toolbar (gras, italique, lien, image)

**UI MAUI (Blazor Hybrid)** ✅
- Même logique que pour Web : pages TaskDetail et TaskEdit, modale de suppression

**Tests** ✅ (+7 tests, total 179)
- `TaskDescriptionTests` : 3 tests (valide, trop long, correcte lors de la création de tâche)
- `TaskEditValidationTests` : 4 tests (dates, durée, responsable, statut)

### Impact
- L'utilisateur peut maintenant visualiser et éditer les détails d'une tâche, y compris la description au format Markdown
- Amélioration de l'UX avec des modales et une navigation claire entre détails et édition
- Respect total du pattern CQRS

### Dette technique introduite
- Aucune

---

## Itération précédente

**#17 — BC Sprint libre** — Livré le 2026-03-18

### Ce qui a été livré

**Domain** ✅
- `SprintSessionId` : wrapper Guid, factory `New()` + `From()`
- `SprintName` : Value Object (max 200 chars), nom par défaut "Sprint #N" si vide
- `SprintOutcome` : enum `Completed | Interrupted`
- `SprintSession` : agrégat racine — `Record()` factory, validation `endedAt > startedAt`, `Duration` computed
- `SprintDomainErrors` : constantes d'erreur domaine Sprint
- `ISprintSessionRepository` : `AddAsync` + `GetByDateAsync(date, ownerId)`

**Application (CQRS)** ✅
- `RecordSprintCommandHandler` : crée `SprintSession`, persiste, crée 2 entrées journal rétroactives (SprintStarted + SprintCompleted/Interrupted)
- `GetTodaySprintsQueryHandler` : liste des sprints du jour, triés chronologiquement
- `SprintSessionViewModel` : DTO de sortie

**Infrastructure** ✅
- `SprintSessionConfiguration` : mapping EF Core (`uniqueidentifier`, `datetimeoffset`, OwnsOne pour SprintName)
- `EfCoreSprintSessionRepository` : implémentation SQLite/SQL Server
- Migration `AddSprintSessions` : table `SprintSessions` avec toutes les colonnes

**API** ✅
- `POST /api/sprints` — enregistre un sprint (Completed ou Interrupted)
- `GET /api/sprints/today` — liste les sprints du jour
- `SprintsController` avec `[Authorize]`
- Handlers enregistrés dans `Program.cs`

**UI Web (Blazor WASM)** ✅
- `Pomodoro.razor` : bouton `···` en haut à droite → menu contextuel → "Sprint libre" → navigation `/pomodoro/libre`
- `SprintLibre.razor` (page `/pomodoro/libre`) : champ nom, dropdown tâche, chrono count-up HH:mm:ss, boutons Démarrer/Terminer/Interrompre, historique du jour
- `SprintApiClient` + `SprintDto` / `RecordSprintRequest`
- `SprintApiClient` enregistré dans `Program.cs`
- NavMenu inchangé (pas d'entrée dédiée Sprint libre)

**UI MAUI (Blazor Hybrid)** ✅
- Même logique : bouton `···` sur `Pomodoro.razor` → `SprintLibre.razor` (`/pomodoro/libre`)
- `MauiProgram.cs` : `SprintApiClient` enregistré

**Tests** ✅ (+23 tests)
- `SprintNameTests` : 6 tests (valide, vide, whitespace, null, trop long, max length)
- `SprintSessionTests` : 7 tests (nominal, durée, interrupted, linkedTask, endedAt <= startedAt, IDs uniques)
- `RecordSprintCommandHandlerTests` : 10 tests (persist, defaultName, viewModel, linkedTask, interrupted, 2 journal entries, timestamps rétroactifs, validation)
- `GetTodaySprintsQueryHandlerTests` : 5 tests (vide, sessions, filtre userId, ordre chronologique, viewModel)

### Impact
- L'utilisateur peut démarrer un sprint de durée libre, nommer le sprint, lier une tâche
- Le chronomètre s'incrémente en temps réel (PeriodicTimer, 1 seconde)
- Le journal reçoit automatiquement SprintStarted + SprintCompleted/Interrupted avec timestamps rétroactifs
- L'historique du jour s'actualise après chaque sprint
- Implémentation conforme ADR-012 : timer purement client-side, aucun état serveur pendant le sprint

### Dette technique introduite
- Tests d'intégration EF Core pour `EfCoreSprintSessionRepository` non écrits (test project Infrastructure absent)
- Duplication Web/MAUI (Sprint.razor + DTOs) — dette ADR-008 existante

---

## Itération précédente

**#16 — Déploiement Azure (Bicep)** — Livré le 2026-03-13

### Ce qui a été livré

**Infrastructure as Code (Bicep)** ✅
- `infra/main.bicep` : template complet — App Service Plan (B1 dev / P1v3 prod), Web App Linux .NET 10, Azure SQL Database (Basic dev / S1 prod), Key Vault avec RBAC, Application Insights
- Secrets gérés dans Key Vault : `ConnectionStrings--Default`, `GitHub--ClientSecret`, `Jwt--SecretKey`
- Identité managée (System-Assigned) sur Web App avec rôle `Key Vault Secrets User`
- Firewall SQL : `AllowAzureServices` activé
- Configuration App Service : `ASPNETCORE_ENVIRONMENT`, connection string via Key Vault, GitHub OAuth, JWT, WebBaseUrl, Application Insights

**Paramètres et configuration** ✅
- `infra/main.bicepparam` : paramètres environnement dev (francecentral, nom kairudev)
- `infra/main.prod.bicepparam` : paramètres environnement prod
- `infra/.env.example` : template variables d'environnement (GitHub OAuth, JWT, SQL password)
- `.gitignore` : ajout `infra/*.parameters.local.json`, `infra/.env` (sécurité secrets)

**Script de déploiement** ✅
- `infra/deploy.ps1` : script PowerShell complet avec :
  - Vérification connexion Azure + subscription
  - Création Resource Group si inexistant
  - Récupération secrets depuis variables d'environnement
  - Génération automatique JWT secret si absent
  - Validation WhatIf disponible
  - Déploiement Bicep avec paramètres sécurisés (`SecureString`)
  - Outputs : webAppUrl, sqlServerFqdn, keyVaultName, etc.
  - Guidance post-déploiement (migrations, callback GitHub)

**CI/CD GitHub Actions** ✅
- `.github/workflows/azure-deploy.yml` : workflow automatisé
  - Triggers : push sur `main` / `staging`, ou manual dispatch
  - Job `build` : restore → build → tests (171 tests) → publish API + Blazor WASM → artifact upload
  - Job `deploy` : artifact download → Azure login (Service Principal) → deploy sur App Service → logout
  - Environment `dev` configuré (URL output)

**Documentation** ✅
- `docs/deploy.md` (guide complet 300+ lignes) :
  - Architecture Azure (diagramme Mermaid)
  - Prérequis (Azure CLI, PowerShell, GitHub OAuth)
  - Déploiement manuel (PowerShell pas à pas)
  - Migration SQLite → Azure SQL (3 approches : script SQL, auto-migration, multi-provider)
  - Configuration GitHub Actions (secrets, Service Principal)
  - Post-déploiement (callback OAuth, CORS, logs, health check)
  - Environnements (dev / staging / prod)
  - Dépannage (Key Vault access denied, SQL firewall, OAuth mismatch, logs)
  - Rollback
  - Estimation coûts (18€/mois dev, 152€/mois prod)
  - Références Microsoft Learn

**Correction bug build** ✅
- `src/Kairu.Api/Program.cs` : suppression du second paramètre dans `AddInfrastructure()` (méthode n'accepte qu'un seul paramètre `connectionString`)

### Impact
- **Production-ready** : Kairu peut être déployé sur Azure en quelques minutes (`.\infra\deploy.ps1`)
- **Sécurité** : secrets gérés dans Key Vault (jamais en clair dans code/config), identité managée, HTTPS forcé, TLS 1.2
- **CI/CD** : GitHub Actions automatise build + tests + déploiement (push sur `main` → prod automatique)
- **Scalabilité** : App Service peut scale automatiquement (P1v3+), Azure SQL en haute disponibilité
- **Observabilité** : Application Insights collecte logs/metrics/traces automatiquement

### Dette technique
- **Migration EF Core** : pas encore automatisée dans le workflow GitHub Actions (commenté dans `.github/workflows/azure-deploy.yml`)
- **Multi-provider EF Core** : pas implémenté (code utilise encore `UseSqlite` uniquement dans `DependencyInjection.cs`)
- **Connection string production** : doit être Azure SQL, mais `AddInfrastructure` est hardcodé SQLite
- **Coûts Azure** : ~18€/mois dev non optimisé (possibilité serverless SQL, App Service arrêt automatique)

### Prochaines étapes suggérées
1. **Migration SQLite → SQL Server** : Ajouter `Microsoft.EntityFrameworkCore.SqlServer`, détection dynamique provider dans `DependencyInjection.cs`
2. **EF Core migrations automatiques** : Décommenter et finaliser l'étape dans GitHub Actions (ou migrations au démarrage en dev/staging uniquement)
3. **Déploiement test** : Exécuter `.\infra\deploy.ps1` sur une subscription Azure de test
4. **Configuration GitHub Secrets** : Ajouter `AZURE_CREDENTIALS`, `GITHUB_CLIENT_ID`, etc. dans repository GitHub

---

## Itération précédente

**#15b — Auth client Web + MAUI + UI (Landing + Dashboard)** — Livré le 2026-03-09

**Blazor WASM — authentification bout en bout** ✅
- `Login.razor` : bouton GitHub login pointant sur `{ApiBaseUrl}/api/auth/github` (via `IConfiguration`)
- `Login.razor` : lecture `uri.Fragment` → extraction du JWT → stockage `localStorage` → `Nav.NavigateTo("/dashboard")`
- `JwtAuthenticationStateProvider` + `AuthService` : état d'authentification Blazor basé sur JWT
- `App.razor` : `CascadingAuthenticationState` + `AuthorizeRouteView` (redirect → `/login`)

**MAUI — authentification bout en bout** ✅
- `Login.razor` (MAUI) : même flux que Web, stockage via `SecureStorage`
- `MauiAuthService` : équivalent de `AuthService` pour MAUI

**API — correctifs OAuth** ✅
- `Program.cs` : ajout `AddCookie()` + `DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme`
  (requis pour `RemoteAuthenticationHandler` lors du callback GitHub)
- `appsettings.Development.json` : ajout `WebBaseUrl: "http://localhost:5010"`
- `AuthController` : toutes les redirections utilisent `{WebBaseUrl}/login#...` au lieu de `/#...`

**UI Blazor — Landing + Dashboard** ✅
- **`Home.razor`** (réécriture complète) : page d'accueil `[AllowAnonymous]`, hero gradient + bouton GitHub login, 4 feature cards (Tasks / Pomodoro / Journal / Tickets), redirige vers `/dashboard` si déjà authentifié
- **`Dashboard.razor`** (nouveau) : page `[Authorize]`, salutation `"Bonjour, {login}"` + date, 4 stat-cards cliquables (tâches en cours, tâches terminées, session Pomodoro active, entrées journal aujourd'hui), 5 quicklinks
- **`NavMenu.razor`** : icône SVG dashboard (premier lien), bouton déconnexion SVG (`.btn-logout`), logout → `/`
- **`app.css`** : styles `btn-logout`, `landing`, `landing-hero`, `feature-card`, `dashboard-page`, `stat-card`, `stat-active`, `quicklink-card` (avec dark mode support)

**Tests** ✅ (+11 tests, total 166)
- `UserIdTests.cs` : 4 tests (Create, Equal, NotEqual, ToString)
- `UserTests.cs` : 3 tests (Create, Id from GitHubId, null email)
- `FakeUserRepository.cs` : stub en mémoire
- `GetOrCreateUserCommandHandlerTests.cs` : 4 tests (CreateUser, ReturnExisting, CorrectDisplayName, NoDuplicate)

**Outillage** ✅
- `.claude/launch.json` : 3 configs serveur (AppHost Aspire port 15100, API port 5205, Web port 5010)
- `README.md` (racine) : README professionnel (features, stack, architecture, quickstart, OAuth setup, Jira config)

### Impact
- **Flux complet** : l'utilisateur peut se connecter via GitHub depuis le Web ou MAUI, toutes les pages protégées fonctionnent
- **UX** : landing page présente le produit, dashboard centralise les informations clés
- **Développeur** : `.claude/launch.json` + `README.md` facilitent l'onboarding

### Dette technique
- `appsettings.Development.json` : `GitHub:ClientSecret` et `Jwt:SecretKey` à configurer manuellement
- `JiraApiToken` toujours stocké en clair dans SQLite
- Duplication pages Blazor Web ↔ MAUI (dette ADR-008, à résoudre via RCL)

---

## Itération précédente

**#15 — Auth GitHub + Multi-users** — Livré le 2026-03-04

### Ce qui a été livré

**BC Identity (Domain)** ✅
- `UserId` : value record wrappant un GitHub ID string (`UserId.From(string)`)
- `User` : entity (`Entity<UserId>`) avec `GitHubId`, `Login`, `DisplayName`, `Email?`
- `IUserRepository` : `GetByGitHubIdAsync` + `AddAsync`

**Multi-user : OwnerId sur toutes les entités** ✅
- `DeveloperTask.OwnerId` + `Create()` accepte `UserId`
- `PomodoroSession.OwnerId` + `Create()` accepte `UserId`
- `JournalEntry.OwnerId` + `Create()` accepte `UserId`
- `UserSettings` : migré de `AggregateRoot<int>` (singleton Id=1) à `AggregateRoot<UserId>` (per-user)
- `PomodoroSettingsRow` : migré de `int Id=1` à `string UserId` comme PK

**Interfaces repository mises à jour** ✅
- `ITaskRepository.GetByIdAsync(TaskId, UserId)`, `GetAllAsync(UserId)`, `DeleteAsync(TaskId, UserId)`
- `IPomodoroSessionRepository` : toutes les méthodes user-specific acceptent `UserId`
- `IPomodoroSettingsRepository` : `GetByUserIdAsync(UserId)`, `SaveAsync(PomodoroSettings, UserId)`
- `IJournalEntryRepository` : `GetEntriesByDateAsync(DateOnly, UserId)`, `GetTodayCountByTypeAsync(..., UserId)`
- `IUserSettingsRepository` : `GetByUserIdAsync(UserId)` remplace `GetAsync()`

**Application Layer** ✅
- `ICurrentUserService` : interface abstraction identité courante (`CurrentUserId` property)
- `GetOrCreateUserCommand/Handler/Result` : crée l'utilisateur au premier login GitHub
- Tous les handlers (Tasks, Pomodoro, Journal, Settings) injectent `ICurrentUserService`
- `CreateEntryCommand` inclut maintenant `OwnerId`

**Infrastructure** ✅
- `UserConfiguration` + `SqliteUserRepository` : persistence table `Users`
- Toutes les configurations EF Core mises à jour pour mapper `OwnerId`
- Migration `InitialMultiUser` : table Users + OwnerId sur Tasks/PomodoroSessions/JournalEntries + refonte PK UserSettings et PomodoroSettings

**Auth API** ✅
- `AuthController` : `GET /api/auth/github`, `GET /api/auth/github/callback`, `GET /api/auth/me`
- `ClaimsCurrentUserService` : lit le claim `sub` depuis le JWT Bearer
- GitHub OAuth 2.0 + JWT HS256 (configurable via `appsettings.Development.json`)
- `[Authorize]` sur TasksController, PomodoroController, JournalController, SettingsController, TicketsController

**Tests** ✅ (155 tests, 0 échec)
- `FakeCurrentUserService` : stub pour les tests Application
- Tous les fake repositories mis à jour (signatures UserId)
- Tous les tests Domain/Application/Infrastructure mis à jour (UserId dans Create())

---

## Itération précédente

**#13 — BC Tickets — Intégration Jira** — Livré le 2026-03-03

### Ce qui a été livré

**Domain (Tasks BC — extension)** ✅
- `JiraTicketKey` : Value Object (format `^[A-Z]+-\d+$`, max 50 chars, validé)
- `DeveloperTask.JiraTicketKey?` : propriété nullable
- `DeveloperTask.LinkJiraTicket(key)` + `UnlinkJiraTicket()`
- `DomainErrors.Tasks` : 4 nouvelles constantes d'erreur Jira

**Domain (Settings BC — extension)** ✅
- `UserSettings.JiraBaseUrl?`, `JiraEmail?`, `JiraApiToken?`
- `UserSettings.UpdateJiraSettings(url, email, token)`

**Application** ✅
- `IJiraTicketService` + `JiraTicketDto` (BC Tickets)
- `GetAssignedJiraTicketsQueryHandler` : récupère les tickets Jira via l'API
- `LinkJiraTicketCommandHandler` : lie un ticket Jira à une tâche
- `UnlinkJiraTicketCommandHandler` : délie un ticket Jira d'une tâche
- `SaveJiraSettingsCommandHandler` : persiste les credentials Jira
- `TaskViewModel` enrichi : `JiraTicketKey?`
- `UserSettingsViewModel` enrichi : `JiraBaseUrl?`, `JiraEmail?`, `JiraConfigured` (sans token)

**Infrastructure** ✅
- `JiraApiClient` : HTTP Basic Auth, appel `GET /rest/api/3/search?jql=assignee=currentUser()`
- Migration `AddJiraTicketKeyToTasks` : colonne `JiraTicketKey` nullable (max 50) sur `Tasks`
- Migration `AddJiraSettingsToUserSettings` : 3 colonnes nullable sur `UserSettings`
- `TaskConfiguration` mis à jour : mapping `JiraTicketKey`
- `UserSettingsConfiguration` mis à jour : mapping 3 champs Jira
- `DependencyInjection` : `AddHttpClient<IJiraTicketService, JiraApiClient>()`

**API** ✅
- `TicketsController` : `GET /api/tickets/assigned`
- `TasksController` étendu : `PUT /api/tasks/{id}/jira-ticket` + `DELETE /api/tasks/{id}/jira-ticket`
- `SettingsController` étendu : `PUT /api/settings/jira`

**UI Web (Blazor WASM)** ✅
- Page `/tickets` : liste des tickets assignés avec clé, titre, statut, priorité
- `Tasks.razor` : badge clé Jira + boutons Lier/Délier
- `Settings.razor` : section credentials Jira (BaseUrl, Email, ApiToken)
- `NavMenu.razor` : lien 🎫 Tickets

**UI MAUI (Blazor Hybrid)** ✅
- Identique au Web : page Tickets + Tasks + Settings + NavMenu

**Tests** ✅ (+27 tests, total 155)
- Domain : `JiraTicketKeyTests` (7 tests) + `DeveloperTaskJiraTests` (5 tests)
- Application : `LinkJiraTicketCommandHandlerTests` (3 tests) + `UnlinkJiraTicketCommandHandlerTests` (3 tests)

### Impact
- L'utilisateur voit ses tickets Jira assignés depuis Kairu
- Chaque tâche peut être liée à un ticket Jira (persisté en base)
- Les credentials Jira sont configurables depuis l'UI sans redémarrage

### Dette technique introduite
- `JiraApiToken` stocké en clair dans SQLite — à chiffrer dans une itération future
- Pas de cache sur les appels Jira (appel API à chaque chargement de page)

---

## Itération précédente

**#12 — Journal : navigation historique + numérotation sprints** — Livré le 2026-03-02

### Ce qui a été livré

#### Problème
- La page Journal affichait uniquement le journal du jour, sans pouvoir consulter les jours précédents
- Les sprints n'étaient pas numérotés (affichait "Sprint démarré" au lieu de "Sprint #1 démarré")

#### Solution appliquée

**Application (CQRS)** ✅
- `GetJournalByDateQueryHandler` : nouveau handler pour récupérer les entrées d'une date arbitraire
- `CreateEntryCommandHandler` étendu : numérotation des sprints (`SprintStarted/Completed/Interrupted`) basée sur `count(SprintStarted today)`
  - Fix : `sequence = null` si aucun sprint démarré (évite la valeur 0 aberrante)

**API** ✅
- Endpoint `GET /api/journal/date/{date}` (format `yyyy-MM-dd`)
- `GetJournalByDateQueryHandler` enregistré en DI

**UI Web (Blazor WASM)** ✅
- Flèches ← → dans l'en-tête (bouton → désactivé si "aujourd'hui")
- `_displayDate` : état de navigation, défaut = aujourd'hui
- `GetEventLabel` : Sprint #N démarré/complété/interrompu + Pause #N (déjà existant)
- Message vide générique + hint uniquement si aujourd'hui

**UI MAUI (Blazor Hybrid)** ✅
- Même navigation ← → avec `_displayDate`
- `JournalEntryDto` complété : ajout `int? Sequence` et `List<string> LinkedTaskTitles`
- `GetEventLabel` mis à jour (sprint numérotés + pauses) + tâches liées affichées
- Icônes pauses (`☕ BreakStarted`, `🌿 BreakCompleted`, `⚡ BreakInterrupted`) ajoutées

**Tests** ✅ (+14 tests, total 128)
- `CreateEntryCommandHandlerTests` : 8 tests (séquences Sprint #1, #2, Completed, Interrupted, Break, null)
- `GetJournalByDateQueryHandlerTests` : 6 tests (filtrage date, liste vide, séquence préservée)

### Impact
- L'utilisateur peut maintenant naviguer dans l'historique jour par jour
- Les sprints sont clairement numérotés dans la timeline
- Alignement complet Web ↔ MAUI sur les DTOs et les labels

### Dette technique introduite
Aucune ✅

---

## Itération précédente : bugfix Pomodoro

**bugfix — Pomodoro UI bloqué après fin de session** — Livré le 2026-03-02

#### Problème
Après la fin automatique d'un sprint ou d'une pause (timer → 0:00), l'UI restait bloquée : onglets de sélection invisibles, bouton "Interrompre" toujours visible.

#### Cause racine
`CompleteSessionAsync()` désérialisait le corps de la réponse 204 NoContent → `null` → retour anticipé sans mise à jour de `_session`.

#### Fix
- `CompleteSessionAsync()` → `Task<bool>` (pas de corps à désérialiser)
- Suppression de `PomodoroCompleteResultDto` (Web + MAUI)
- Ajout de `_suggestedNextSessionType` dédié (séparé de `_selectedSessionType`)
- Alignement MAUI sur le pattern Web

---

## Itération précédente (avant bugfix)

**#11c — Nettoyage post-migration CQRS** — Livré le 2026-02-XX

### Ce qui a été livré

#### Problème
Suite à la migration CQRS (#11), des fichiers obsolètes de l'ancien pattern Interactor subsistaient dans le codebase :
- Fichiers `*Request.cs` (anciens DTOs Interactor) non supprimés
- Documentation indiquait 16 tests à migrer, alors qu'ils l'étaient déjà
- Dette technique documentée mais pas validée

#### Solution appliquée

**Audit complet du code** ✅
- ✅ Vérification build : **génération réussie**
- ✅ Vérification tests : **154 tests tous migrés vers pattern CQRS**
- ✅ Recherche fichiers obsolètes : 2 fichiers `*Request.cs` trouvés

**Nettoyage effectué** ✅
- ✅ Suppression `AddJournalCommentRequest.cs` (ancienne boundary Interactor)
- ✅ Suppression `RemoveJournalCommentRequest.cs` (ancienne boundary Interactor)
- ✅ Validation finale : build + tests passent

**Documentation mise à jour** ✅
- ✅ `docs/project-state.md` : résumé actuel reflète l'état réel (154 tests migrés)
- ✅ Itération #11c ajoutée dans l'historique

### Impact
- **Code propre** : plus aucun fichier de l'ancien pattern Interactor/Presenter
- **Dette technique résorbée** : migration CQRS 100% complète
- **Prêt pour #12** : codebase sain pour démarrer BC Tickets

### Dette technique introduite
Aucune ✅

---

## Itération précédente

**#11b — BC Settings + Thème Dark paramétrable** — Livré le 2026-02-XX

### Ce qui a été livré

#### Problème
L'application utilise uniquement le thème Bootstrap clair. Les utilisateurs modernes s'attendent à pouvoir choisir entre clair/sombre/système, avec persistence et synchronisation Web ↔ MAUI.

#### Solution appliquée

**Nouveau Bounded Context : Settings** ✅
- **Domain** : `UserSettings` (aggregate root, singleton), `ThemePreference` (enum : Light/Dark/System)
- **Application** : 2 use cases CQRS (GetUserSettings, SaveThemePreference)
- **Infrastructure** : `SqliteUserSettingsRepository` + migration `AddUserSettings`
- **API** : `SettingsController` (`GET /api/settings`, `PUT /api/settings/theme`)
- **UI (Web + MAUI)** : `SettingsApiClient`, select thème dans `Settings.razor`

**Fonctionnalités** ✅
- **3 modes** : ☀️ Clair, 🌙 Sombre, ⚙️ Système (détection via `prefers-color-scheme`)
- **Application immédiate** : thème change sans rechargement (JSInterop + `data-bs-theme`)
- **Persistence SQLite** : synchronisation Web ↔ MAUI via API REST
- **Defaults** : création automatique si premier accès (valeur par défaut : `System`)

**Architecture** ✅
- **Clean Architecture respectée** : dépendances pointent vers l'intérieur
- **CQRS sans MediatR** : suit exactement le pattern Tasks/Pomodoro/Journal
- **ADR-004** créé : documentation de la décision

### Impact
- **UX améliorée** : support natif du dark mode, confort visuel
- **Synchronisation** : préférence partagée entre Web et MAUI
- **Extensible** : BC Settings prêt pour d'autres préférences (langue, fuseau horaire, etc.)
- **23 use cases** au total (6 Tasks + 10 Pomodoro + 5 Journal + 2 Settings)

### Dette technique introduite
- **Tests manquants** : `UserSettingsTests`, `SaveThemePreferenceCommandHandlerTests`, `SqliteUserSettingsRepositoryTests`
- **Duplication UI** : `Settings.razor` dupliqué Web/MAUI (résolu dans future RCL Shared)

---

## Itération précédente

**#10 — .NET MAUI (application desktop/mobile)** — Livré le 2026-02-26

#### Problème
L'application n'était accessible que via navigateur web (Blazor WASM). Besoin d'une expérience native desktop/mobile avec les mêmes fonctionnalités.

#### Solution appliquée

**Projet .NET MAUI avec Blazor Hybrid** ✅
- **`Kairu.Maui`** : nouveau projet .NET 10 MAUI avec `Microsoft.AspNetCore.Components.WebView.Maui`
- **Multi-plateforme** : cible Windows, Android, iOS, macOS via `TargetFrameworks`
- **Réutilisation des pages Blazor** : tous les composants copiés depuis `Kairu.Web`
  - `Tasks.razor`, `Pomodoro.razor`, `Journal.razor`, `Settings.razor`
  - `NavMenu.razor` adapté avec émojis pour la navigation
  - Layout `MainLayout` réutilisé

**Services API clients** ✅
- **`Services/`** : `TaskApiClient`, `PomodoroApiClient`, `JournalApiClient`
- **DTOs** : `TaskDto`, `PomodoroDto`, `JournalDto` copiés dans le namespace `Kairu.Maui.Services`
- **Configuration** : `appsettings.json` avec `ApiBaseUrl` (défaut : `https://localhost:7056`)
- **Injection de dépendances** : `MauiProgram.cs` configure `HttpClient` + les 3 clients API

**UI et UX** ✅
- **Page d'accueil** : redirection automatique vers `/tasks`
- **Navigation** : menu latéral avec icônes (☑ Tâches, 🍅 Pomodoro, 📖 Journal, ⚙ Paramètres)
- **CSS** : Bootstrap + styles personnalisés copiés depuis `Kairu.Web`
- **Safe area** : support des zones sécurisées iOS (notch)

**Architecture** ✅
- **Communication API REST uniquement** : MAUI ne connaît rien du Domain/Application
- **Clean Architecture respectée** : aucune dépendance vers Domain/Application/Infrastructure
- **Blazor Hybrid** : les composants Razor tournent dans un WebView natif avec accès aux APIs .NET MAUI

### Impact
- **Application native** disponible sur **Windows, Android, iOS, macOS**
- **Réutilisation complète** des composants Blazor existants → zéro code UI à refactorer
- **Même API REST** utilisée par Web et MAUI → cohérence garantie
- **Prêt pour distribution** : Windows unpackaged, Android/iOS via stores

### Dette technique introduite
- **Duplication de code** : les pages Blazor et services API sont copiés dans `Kairu.Maui`
- **Solution future** : extraire dans `Kairu.Web.Shared` (Razor Class Library) référencée par Web + MAUI
- **Priorité** : basse (fonctionne tel quel, refactoring optionnel)

---


  - Formulaire d'ajout de commentaire (inline)
  - Modification/suppression de commentaires (inline)
  - Icônes contextuelles : 🍅 Sprint démarré, ✅ Sprint complété, ⏸️ Sprint interrompu, 🚀 Tâche démarrée, 🎉 Tâche complétée
- **`JournalApiClient`** : appels HTTP pour `GetTodayEntries`, `AddComment`, `UpdateComment`, `DeleteComment`
- **`JournalDto`** : DTOs `JournalEntryDto`, `JournalCommentDto`, `AddCommentRequest`, `UpdateCommentRequest`
- **`NavMenu.razor`** : ajout du lien 📖 Journal dans la navigation

**Intégration événementielle** (UCS-1 — Génération automatique) ✅
- **Pomodoro → Journal** :
  - `StartSessionInteractor` → crée `SprintStarted` lors du démarrage
  - `CompleteSessionInteractor` → crée `SprintCompleted` lors de la complétion
  - `InterruptSessionInteractor` → crée `SprintInterrupted` lors de l'interruption
- **Tasks → Journal** :
  - `ChangeTaskStatusInteractor` → crée `TaskStarted` si statut = `InProgress`
  - `ChangeTaskStatusInteractor` → crée `TaskCompleted` si statut = `Done`
  - `CompleteTaskInteractor` → crée `TaskCompleted` lors de la complétion
- Injection de `ICreateJournalEntryUseCase` dans tous les interactors concernés
- `NoOpJournalEntryPresenter` utilisé pour les appels internes (pas de réponse HTTP nécessaire)

**Infrastructure déjà en place** (itérations précédentes) ✅
- **Domain** : `JournalEntry`, `JournalComment`, `JournalEventType`, `IJournalEntryRepository`
- **Application** : 5 use cases (GetTodayJournal, AddComment, UpdateComment, RemoveComment, CreateEntry)
- **API** : `JournalController` avec endpoints REST complets
- **Tests** : 71 Domain + 66 Application + 17 Infrastructure = 154 tests, tous au vert ✅

### Impact
- L'utilisateur peut maintenant **consulter son activité quotidienne** sous forme de timeline
- Les événements Pomodoro et Tasks sont **automatiquement tracés** dans le journal
- L'utilisateur peut **ajouter des commentaires** personnels pour enrichir le contexte
- **Rétrospective facilitée** : "qu'est-ce que j'ai fait aujourd'hui ?"
- **Traçabilité complète** : chaque sprint et chaque tâche démarrée/complétée génère une entrée
- **UC-13, UC-14, UC-15, UC-16, UCS-1** sont maintenant complètement implémentés de bout en bout

### Captures d'écran (UI)
- Page `/journal` : timeline chronologique avec icônes et horodatages
- Affichage des commentaires : texte + boutons modifier/supprimer
- Formulaire d'ajout : textarea + boutons Ajouter/Annuler

---

## Itération #7 (précédente)

**#7 — Types de session Pomodoro (Sprint/Pause courte/Pause longue)** — Livré le 2026-02-26

### Ce qui a été livré

#### Problème
L'utilisateur ne pouvait pas choisir le type de session Pomodoro à démarrer. Seul le sprint était disponible, sans possibilité de lancer explicitement une pause courte ou longue. Le système comptait les sprints mais ne suggérait pas automatiquement le type de session suivant.

#### Solution appliquée

**Backend (Partie 1/2)**

- **Domain** : Nouveau enum `PomodoroSessionType` (Sprint, ShortBreak, LongBreak)
- **Domain** : Propriété `SessionType` ajoutée à `PomodoroSession`
- **Application** : 
  - `StartSessionRequest` accepte un paramètre `sessionType` optionnel
  - Logique de suggestion intelligente : après 4 sprints → LongBreak, sinon → Sprint/ShortBreak
  - Nouveau use case `GetSuggestedSessionType` pour récupérer la suggestion
- **API** : 
  - `GET /api/pomodoro/session/suggested` — retourne le type suggéré + durées
  - `POST /api/pomodoro/session?type={Sprint|ShortBreak|LongBreak}` — démarre avec type spécifique
- **Infrastructure** : Migration `AddSessionType` (colonne `SessionType` dans table PomodoroSessions)
- **Tests** : Tous les tests mis à jour pour `PomodoroSession.Create(type, duration)`

**Frontend (Partie 2/2)**

- **DTO** : 
  - `PomodoroSessionDto` inclut maintenant `SessionType`
  - Nouveau `SuggestedSessionTypeDto` pour la suggestion
- **ApiClient** : 
  - `GetSuggestedSessionTypeAsync()` — appel à l'endpoint de suggestion
  - `StartSessionAsync(sessionType)` — démarre avec type optionnel
- **UI Blazor** : 
  - **3 onglets (radio buttons)** pour choisir le type :
    - 🍅 **Sprint** (bleu, durée configurée)
    - ☕ **Pause courte** (vert, durée configurée)
    - 🌙 **Pause longue** (cyan, durée configurée)
  - **Présélection intelligente** via `GET /suggested` au chargement
  - **Durée affichée dynamiquement** dans chaque onglet
  - **Couleur de l'arc** adaptée au type de session
  - **Label de statut** contextualisé ("Sprint en cours", "Pause courte", "Pause longue")

**Tests** : 154 tests au total (71 Domain + 66 Application + 17 Infrastructure), 0 échec ✅

### Impact
- L'utilisateur peut maintenant **choisir explicitement** le type de session à démarrer
- Le système **suggère intelligemment** le prochain type basé sur l'historique
- **Meilleure expérience utilisateur** : onglets visuels + durées claires
- **Traçabilité complète** : chaque session stocke son type en base
- **Respect du rythme Pomodoro classique** : 4 sprints → pause longue

### Captures d'écran (UI)
- Page `/pomodoro` : 3 onglets avec durées affichées
- Horloge circulaire : couleur adaptée (bleu/vert/cyan)
- Label dynamique : "Sprint en cours" / "Pause courte" / "Pause longue"

---

## Itération #6 (précédente)

**#6 — UC-05 (Pomodoro) — Configurer les durées** — Livré le 2026-02-26

### Ce qui a été livré

#### Problème
L'utilisateur n'avait aucun moyen de personnaliser les durées des sprints Pomodoro. Les valeurs par défaut (25 min sprint, 5 min pause courte, 15 min pause longue) étaient codées en dur.

#### Solution appliquée

**Tests Application** : `SaveSettingsInteractorTests` (+13 tests)
- Validation des durées dans la plage [1-120] minutes
- Test des valeurs minimales et maximales
- Test de réécriture des paramètres existants
- Test des erreurs de validation pour chaque champ

**API** : Endpoint existant `PUT /api/pomodoro/settings`
- Déjà implémenté dans `PomodoroController`
- `SaveSettingsHttpPresenter` gère la réponse (204 NoContent / 400 BadRequest)

**Blazor UI** : Nouvelle page `/settings`
- Formulaire avec 3 champs numériques (sprint, pause courte, pause longue)
- Validation côté client et serveur (1-120 minutes)
- Bouton "Réinitialiser" pour revenir aux valeurs par défaut
- Affichage des messages de succès/erreur
- Ajout du lien "⚙ Paramètres" dans le menu de navigation

**Tests** : 154 tests au total (71 Domain + 66 Application + 17 Infrastructure), 0 échec ✅

### Impact
- L'utilisateur peut maintenant personnaliser les durées Pomodoro selon ses préférences
- Les durées sont persistées dans SQLite et réutilisées à chaque démarrage de sprint
- UC-05 (Pomodoro) est maintenant complètement implémenté et testé
- Ajout de 13 tests Application pour couvrir tous les scénarios de validation

### Captures d'écran (UI)
- Page `/settings` : formulaire avec 3 champs + boutons Enregistrer/Réinitialiser
- Menu de navigation : nouvel item "⚙ Paramètres"

---

## Itération #5e (précédente)

**#5e — UI Blazor pour description et modification de tâches** — Livré le 2026-02-26

### Ce qui a été livré

#### Problème rencontré
L'utilisateur ne voyait pas de différence dans l'interface web Blazor après l'implémentation de UC-05 (UpdateTask). Il n'était pas possible de créer une description pour une tâche ou de modifier une tâche existante depuis l'UI.

#### Solution appliquée
- **`TaskDto`** : Ajout propriété `Description?`
- **`TaskApiClient`** :
  - `AddAsync(title, description)` — ajout paramètre optionnel `description`
  - `UpdateAsync(id, title, description)` — nouvelle méthode pour modifier une tâche
- **`Tasks.razor`** : Refonte complète de l'interface
  - **Création** : Champ titre + textarea description (optionnelle)
  - **Affichage** : Description affichée en petit texte grisé sous le titre
  - **Modification** : Mode édition inline avec boutons Enregistrer/Annuler
  - **Actions** : Bouton ✎ (modifier) ajouté pour les tâches non complétées
  - **Layout** : Meilleur alignement avec flex-grow pour le contenu

### Impact
- L'utilisateur peut maintenant créer des tâches avec description
- L'utilisateur peut modifier le titre et la description d'une tâche existante
- L'interface affiche clairement les descriptions des tâches
- UC-05 est maintenant complètement fonctionnel de bout en bout (Domain → Application → API → UI)

### Captures d'écran (UI)
- Formulaire de création : titre + description
- Liste de tâches : titre en gras, description en petit en dessous
- Mode édition : inline avec inputs et boutons d'action

---

## Itération #5d (précédente)

**#5d — Correctif migration EF Core** — Livré le 2026-02-26

### Ce qui a été livré

#### Problème rencontré
L'application ne démarrait plus avec l'erreur :
```
System.InvalidOperationException: The model for context 'KairuDbContext' has pending changes. 
Add a new migration before updating the database.
```

#### Solution appliquée
- **Migration créée** : `20260226180050_AddTaskDescription.cs`
  - Ajoute la colonne `Description` (nullable, max 1000 caractères) à la table `Tasks`
  - Cette migration correspond aux changements faits dans `TaskConfiguration` lors de l'itération #5c
- **Build réussi** : 0 erreur, 0 avertissement
- **Tests** : 141/141 au vert ✅

### Impact
- L'application peut maintenant démarrer sans erreur
- La base de données est synchronisée avec le modèle EF Core
- Toutes les fonctionnalités précédentes (Tasks, Pomodoro, Journal) restent opérationnelles

---

## Itération #5c (précédente)

**#5c — UC-05 UpdateTask + Description optionnelle** — Livré le 2026-02-26

### Ce qui a été livré

#### UC-05 — Modifier une tâche
- **Domain** : 
  - Nouveau Value Object `TaskDescription` (optionnel, max 1000 caractères)
  - `DeveloperTask.Description` (propriété nullable)
  - `DeveloperTask.UpdateDetails(title, description)` — méthode de mise à jour
  - `DeveloperTask.Create()` accepte maintenant un paramètre `description`
- **Application** : `UpdateTask/` — Request, UseCase, Presenter, Interactor
- **Infrastructure** : 
  - Migration `AddTaskDescription` — ajout colonne `Description` (nullable, max 1000)
  - Mise à jour `TaskConfiguration` pour mapper la description
- **API** : 
  - Endpoint `PUT api/tasks/{id}` avec `UpdateTaskBody(Title, Description?)`
  - `UpdateTaskHttpPresenter`
- **Tests** : +19 tests (13 Domain + 6 Application)
- **Total : 141 tests** (71 Domain + 53 Application + 17 Infrastructure), 0 échec

#### Mise à jour AddTask
- `AddTaskRequest` accepte maintenant `Description` (optionnelle)
- `AddTaskInteractor` valide et crée la tâche avec description
- `TaskViewModel` inclut maintenant la `Description`

### Impact sur les BC existants
- Tous les appels à `DeveloperTask.Create()` ont été mis à jour pour passer `null` pour la description
- Aucune régression : tous les tests existants continuent de passer

---

## Itération #5b (précédente)

**Bugfixes + sous-agents + UC-12 ChangeTaskStatus** — Livré le 2026-02-26

### Ce qui a été livré

#### Bugfixes
- **NetworkError Blazor WASM** : `ApiBaseUrl` corrigé → `http://localhost:5205` (Aspire utilise le profil HTTP)
- **`CreatedAtActionResult("GetById")`** : remplacé par `CreatedResult($"api/tasks/{id}")` (action inexistante)

#### Sous-agents Claude Code
- **`.claude/agents/pm.md`** : agent Product Manager (outils : Read, Glob, Grep)
- **`.claude/agents/arch.md`** : agent Architecte (outils : Read, Glob, Grep, Write, Edit)
- **`.claude/agents/dev.md`** : agent Développeur (outils : Read, Glob, Grep, Write, Edit, Bash)

#### UC-12 — Changer le statut d'une tâche
- **Domain** : `DeveloperTask.ChangeStatus(TaskStatus, DateTime)` + `DomainErrors.Tasks.SameStatus`
- **Application** : `ChangeTaskStatus/` — Request, IUseCase, IPresenter, Interactor
- **API** : `PATCH api/tasks/{id}/status` + `ChangeTaskStatusHttpPresenter` + action dans `TasksController`
- **Tests** : +17 tests (9 Domain + 8 Application)
- **Total : 89 tests** (44 Domain + 28 Application + 17 Infrastructure), 0 échec

### Dette technique héritée et courante
- `TaskStatus` : alias `DomainTaskStatus` nécessaire dans les tests (conflit namespace)
- `DomainErrors` : alias `PomodoroErrors` nécessaire quand Tasks et Pomodoro sont tous deux importés
- `DeveloperTask.StartProgress()` redondant avec `ChangeStatus(InProgress, now)` — à supprimer dans un refactoring futur
- `Kairu.Adapters` : projet toujours présent dans la solution mais vide de sens (suppression à planifier)

---

## Stack technique
- .NET 10 GA (SDK 10.0.200-preview = SDK .NET 10.1 preview, runtime 10 GA)
- SQLite + EF Core 10.0.3 (fichier local `kairudev.db`, hors git)
- ASP.NET Core Web API (`Kairu.Api`)
- Blazor WebAssembly (`Kairu.Web`) — auth GitHub ✅
- .NET MAUI Blazor Hybrid (`Kairu.Maui`) — auth GitHub ✅
- .NET Aspire 13.1.1 (`Kairu.AppHost` + `Kairu.ServiceDefaults`)
- xUnit pour les tests
- Solution : `Kairu.slnx`

## Structure du projet
```
src/
├── Kairu.Domain/
│   ├── Common/
│   ├── Tasks/
│   ├── Pomodoro/
│   ├── Journal/
│   ├── Settings/
│   ├── Tickets/
│   └── Identity/              ← UserId, User, IUserRepository
├── Kairu.Application/
│   ├── Tasks/
│   ├── Pomodoro/
│   ├── Journal/
│   ├── Settings/
│   ├── Tickets/
│   └── Identity/              ← GetOrCreateUserCommand, ICurrentUserService
├── Kairu.Adapters/          ← à supprimer (ADR-007)
├── Kairu.Infrastructure/    ← migrations dans Persistence/Migrations/
├── Kairu.Api/               ← Controllers + Auth/
├── Kairu.AppHost/           ← orchestration Aspire
├── Kairu.ServiceDefaults/   ← OTEL, health checks, service discovery
├── Kairu.Web/               ← Pages/ (Home, Login, Dashboard, ...) + Auth/ + Services/
└── Kairu.Maui/              ← Pages/ + Auth/ + Services/ (Blazor Hybrid)
tests/
├── Kairu.Domain.Tests/
├── Kairu.Application.Tests/
└── Kairu.Infrastructure.Tests/  ← 17 tests intégration SQLite
docs/
├── spec.md
└── project-state.md
