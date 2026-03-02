# Kairudev — État du projet

> Ce fichier est mis à jour après chaque itération.
> Il est lu par Claude au démarrage de chaque session.

---

## Résumé état actuel

**Dernière itération : #12 — Journal : historique + numérotation sprints** (2026-03-02)

**Bounded Contexts opérationnels :**
- **Tasks** : 6 Commands/Queries — **Architecture CQRS** ✅
- **Pomodoro** : 10 Commands/Queries — **Architecture CQRS** ✅
- **Journal** : 6 Commands/Queries — **Architecture CQRS** ✅ (🆕 GetJournalByDate)
- **Settings** : 3 Commands/Queries — **Architecture CQRS** ✅

**Architecture Application Layer :**
- ✅ **CQRS sans MediatR** : Commands (écriture) + Queries (lecture)
- ✅ **Handlers** retournent directement des `Result` (plus de Presenters)
- ✅ **Injection directe** dans les Controllers (pas de mediator)
- ✅ **25 use cases** (6 Tasks + 10 Pomodoro + 6 Journal + 3 Settings)

**Fonctionnalités Journal :**
- ✅ Navigation historique : flèches ← → dans la page journal (Web + MAUI)
- ✅ Numérotation des sprints : Sprint #1 démarré, Sprint #2 complété, etc.
- ✅ Numérotation des pauses (déjà existant) + alignement MAUI

**Tests :** 128 au total ✅ (71 Domain + 40 Application + 17 Infrastructure)

**Infrastructure :** API REST, Blazor WASM, .NET MAUI, SQLite + EF Core, .NET Aspire

**Migrations :** 6 migrations (InitialCreate, AddPomodoro, AddJournalEntry, AddTaskDescription, AddSessionType, AddUserSettings)

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
| ~~#7~~ | ~~Types de session Pomodoro (Sprint/Pause courte/Pause longue) — UI onglets~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#8~~ | ~~BC Journal — log d'activité quotidien + génération automatique d'entrées~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#10~~ | ~~.NET MAUI — application desktop/mobile~~ | ~~✅ Livré~~ | ~~2026-02-26~~ |
| ~~#11~~ | ~~Migration CQRS sans MediatR — refactoring architectural~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~#11b~~ | ~~BC Settings + Thème Dark paramétrable~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~#11c~~ | ~~Nettoyage post-migration CQRS~~ | ~~✅ Livré~~ | ~~2026-02-XX~~ |
| ~~bugfix~~ | ~~Pomodoro UI bloqué après fin de session (timer 0:00 + bouton Interrompre persistant)~~ | ~~✅ Livré~~ | ~~2026-03-02~~ |
| ~~#12~~ | ~~Journal : navigation historique + numérotation sprints~~ | ~~✅ Livré~~ | ~~2026-03-02~~ |
| #13 | BC Tickets — intégration Jira / Linear / GitHub Issues | 📋 Planifié | — |

---

## Dernière itération livrée

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
- **`Kairudev.Maui`** : nouveau projet .NET 10 MAUI avec `Microsoft.AspNetCore.Components.WebView.Maui`
- **Multi-plateforme** : cible Windows, Android, iOS, macOS via `TargetFrameworks`
- **Réutilisation des pages Blazor** : tous les composants copiés depuis `Kairudev.Web`
  - `Tasks.razor`, `Pomodoro.razor`, `Journal.razor`, `Settings.razor`
  - `NavMenu.razor` adapté avec émojis pour la navigation
  - Layout `MainLayout` réutilisé

**Services API clients** ✅
- **`Services/`** : `TaskApiClient`, `PomodoroApiClient`, `JournalApiClient`
- **DTOs** : `TaskDto`, `PomodoroDto`, `JournalDto` copiés dans le namespace `Kairudev.Maui.Services`
- **Configuration** : `appsettings.json` avec `ApiBaseUrl` (défaut : `https://localhost:7056`)
- **Injection de dépendances** : `MauiProgram.cs` configure `HttpClient` + les 3 clients API

**UI et UX** ✅
- **Page d'accueil** : redirection automatique vers `/tasks`
- **Navigation** : menu latéral avec icônes (☑ Tâches, 🍅 Pomodoro, 📖 Journal, ⚙ Paramètres)
- **CSS** : Bootstrap + styles personnalisés copiés depuis `Kairudev.Web`
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
- **Duplication de code** : les pages Blazor et services API sont copiés dans `Kairudev.Maui`
- **Solution future** : extraire dans `Kairudev.Web.Shared` (Razor Class Library) référencée par Web + MAUI
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
