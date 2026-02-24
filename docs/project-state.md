# Kairudev — État du projet

> Ce fichier est mis à jour après chaque itération.
> Il est lu par Claude au démarrage de chaque session.

---

## Itérations

| # | Contenu | Statut | Date |
|---|---|---|---|
| ~~#1~~ | ~~BC Tasks — Domain, Application, Adapters, Infrastructure, SQLite, 23 tests~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2~~ | ~~API REST (ASP.NET Core) + UI Blazor WebAssembly~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2b~~ | ~~Réécriture spec.md (use cases + diagrammes Mermaid) + prompts agents~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| **#3** | **BC Pomodoro** — sessions de focus liées aux tâches | 🔜 Prévu | — |
| #4 | Tests d'intégration SQLite (`Kairudev.Infrastructure.Tests`) | 📋 Planifié | — |
| #5 | Configuration externalisée — URL API via `appsettings.json` | 📋 Planifié | — |
| #6 | BC Journal — log d'activité quotidien alimenté par les sprints | 📋 Planifié | — |
| #7 | BC Tickets — intégration Jira / Linear / GitHub Issues | 📋 Planifié | — |
| #8 | .NET MAUI — application desktop/mobile | 📋 Planifié | — |

---

## Dernière itération livrée

**#2b** — Livrée le 2026-02-24

### Ce qui a été livré
- `docs/spec.md` : réécriture complète (UC-01 à UC-04 avec template, diagrammes Mermaid par UC)
- `CLAUDE.md` / `AGENTS.md` : template use case dans SPÉCIFIER, note Mermaid
- `.github/copilot-instructions.md` : template use case + séparation spec/project-state

### Dette technique
- URL API hardcodée dans `Program.cs` du Web (`https://localhost:7056`)
- Pas de tests d'intégration SQLite (`Kairudev.Infrastructure.Tests` vide)
- `TaskStatus` : alias `DomainTaskStatus` nécessaire dans les tests (conflit namespace)
- `DeveloperTask.StartProgress()` codé dans le domaine, pas exposé en UC ni en endpoint

---

## Stack technique
- .NET 10 (preview)
- SQLite + EF Core 10 (fichier local `kairudev.db`, hors git)
- ASP.NET Core Web API (`Kairudev.Api`) — `https://localhost:7056`
- Blazor WebAssembly (`Kairudev.Web`) — `https://localhost:7204`
- .NET MAUI — itération future
- xUnit pour les tests

## Structure du projet
```
src/
├── Kairudev.Domain/
├── Kairudev.Application/
├── Kairudev.Adapters/
├── Kairudev.Infrastructure/    ← migrations dans Persistence/Migrations/
├── Kairudev.Api/               ← Tasks/TasksController + Tasks/Presenters/
└── Kairudev.Web/               ← Services/TaskApiClient + Pages/Tasks.razor
tests/
├── Kairudev.Domain.Tests/
├── Kairudev.Application.Tests/
└── Kairudev.Infrastructure.Tests/  ← vide, à compléter
docs/
├── spec.md
└── project-state.md
```
