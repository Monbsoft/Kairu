# Kairudev — État du projet

> Ce fichier est mis à jour après chaque itération.
> Il est lu par Claude au démarrage de chaque session.

---

## Résumé état actuel

**Dernière itération : #6 — UC-05 (Pomodoro) Configurer les durées** (2026-02-26)

**Bounded Contexts opérationnels :**
- **Tasks** : 6 use cases (Add, List, Complete, Delete, Update, ChangeStatus) — **UI complète** ✅
- **Pomodoro** : 8 use cases — **UC-05 SaveSettings implémenté** ✅
  - Démarrer/Terminer/Interrompre sprint
  - Lier tâche, Créer tâche, Mettre à jour statut
  - **Configurer les durées (sprint, pause courte, pause longue)** ✅
- **Journal** : 5 use cases (consultation, ajout/modification/suppression commentaires)

**Tests :** 154 au total (71 Domain + 66 Application + 17 Infrastructure), tous au vert ✅

**Infrastructure :** API REST, Blazor WASM, SQLite + EF Core, .NET Aspire orchestration

**Migrations :** 4 migrations (InitialCreate, AddPomodoro, AddJournalEntry, AddTaskDescription)

---

## Itérations

| # | Contenu | Statut | Date |
|---|---|---|---|
| ~~#1~~ | ~~BC Tasks — Domain, Application, Adapters, Infrastructure, SQLite, 23 tests~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2~~ | ~~API REST (ASP.NET Core) + UI Blazor WebAssembly~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#2b~~ | ~~Réécriture spec.md (use cases + diagrammes Mermaid) + prompts agents~~ | ~~✅ Livré~~ | ~~2026-02-24~~ |
| ~~#3~~ | ~~BC Pomodoro — sessions de focus, chrono circulaire, lien avec Tasks~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#3b~~ | ~~.NET Aspire 13.1.1 — AppHost + ServiceDefaults~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#4~~ | ~~Tests d'intégration SQLite (`Kairudev.Infrastructure.Tests`)~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#5~~ | ~~Configuration externalisée — URL API via `appsettings.json`~~ | ~~✅ Livré~~ | ~~2026-02-25~~ |
| ~~#5b~~ | ~~Bugfixes (NetworkError + CreatedAtAction) + sous-agents Claude + UC-12 ChangeTaskStatus~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5c~~ | ~~UC-05 UpdateTask + Description optionnelle sur les tâches~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5d~~ | ~~Correctif migration EF Core — AddTaskDescription~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#5e~~ | ~~UI Blazor — Description et modification de tâches~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#6~~ | ~~UC-05 (Pomodoro) — Configurer les durées (sprint, pause courte/longue)~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| #7 | BC Journal — log d'activité quotidien alimenté par les sprints | 📋 Planifié | — |
| #8 | BC Tickets — intégration Jira / Linear / GitHub Issues | 📋 Planifié | — |
| #9 | .NET MAUI — application desktop/mobile | 📋 Planifié | — |

---

## Dernière itération livrée

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
System.InvalidOperationException: The model for context 'KairudevDbContext' has pending changes. 
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
- `Kairudev.Adapters` : projet toujours présent dans la solution mais vide de sens (suppression à planifier)

---

## Stack technique
- .NET 10 GA (SDK 10.0.200-preview = SDK .NET 10.1 preview, runtime 10 GA)
- SQLite + EF Core 10.0.3 (fichier local `kairudev.db`, hors git)
- ASP.NET Core Web API (`Kairudev.Api`)
- Blazor WebAssembly (`Kairudev.Web`)
- .NET Aspire 13.1.1 (`Kairudev.AppHost` + `Kairudev.ServiceDefaults`)
- .NET MAUI — itération future
- xUnit pour les tests
- Solution : `Kairudev.slnx`

## Structure du projet
```
src/
├── Kairudev.Domain/
│   ├── Tasks/
│   └── Pomodoro/
├── Kairudev.Application/
│   ├── Tasks/
│   └── Pomodoro/
├── Kairudev.Adapters/          ← à supprimer (ADR-007)
├── Kairudev.Infrastructure/    ← migrations dans Persistence/Migrations/
├── Kairudev.Api/               ← Tasks/ + Pomodoro/
├── Kairudev.AppHost/           ← orchestration Aspire
├── Kairudev.ServiceDefaults/   ← OTEL, health checks, service discovery
└── Kairudev.Web/               ← Services/ + Pages/ + wwwroot/appsettings.json
tests/
├── Kairudev.Domain.Tests/      ← Tasks/ + Pomodoro/
├── Kairudev.Application.Tests/ ← Tasks/ + Pomodoro/
└── Kairudev.Infrastructure.Tests/  ← Tasks/ + Pomodoro/ (17 tests intégration SQLite)
docs/
├── spec.md
└── project-state.md
```
