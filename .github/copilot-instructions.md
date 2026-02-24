# Kairudev — Instructions GitHub Copilot

## Démarrage de chaque session

1. Lis `docs/project-state.md` — état courant du projet.
2. Lis `docs/spec.md` — use cases, modèle du domaine, ADR.
3. Résume en 3 lignes : où on en est, ce qui était prévu.
4. Demande : *"On continue avec ce qui était prévu, ou tu as une nouvelle priorité ?"*
5. Attends la réponse. Aucune action sans confirmation.

---

## Contexte produit

Kairudev est une application de gestion d'activité quotidienne pour développeurs :
- Todo list de micro-tâches
- Journal de bord
- Intégration tickets (Jira, Linear, GitHub Issues)
- Sessions Pomodoro

## Stack technique
- .NET 10, C#
- Clean Architecture + DDD
- SQLite + EF Core (fichier local `kairudev.db`, hors git)
- ASP.NET Core (API REST)
- Blazor WebAssembly (UI web)
- .NET MAUI (UI mobile/desktop, itération future)
- xUnit (tests)

## Règles fondamentales
- Les dépendances pointent uniquement vers l'intérieur
- Jamais le Domain ne connaît l'Infrastructure
- Chaque Use Case = une classe Interactor avec Input/Output Boundary
- Tests nommés `Should_[résultat]_When_[contexte]`
- Langue du code : anglais. Langue des échanges : français.

## Use Case — template standard
```
UC-XX — Nom
Acteur principal / Préconditions / Postconditions (succès)
Scénario nominal (étapes numérotées)
Scénarios alternatifs / Scénarios d'exception
Critères d'acceptance (cases à cocher)
```
Les diagrammes Mermaid s'insèrent dans la spec là où ils apportent de la clarté.

## Séparation des responsabilités documentaires
- `docs/spec.md` — fonctionnel et architectural : use cases, modèle du domaine, ADR, diagrammes
- `docs/project-state.md` — état d'avancement : itération courante, statut, dette technique

## Fichiers clés
- `docs/project-state.md` — état courant
- `docs/spec.md` — spécification
- `CLAUDE.md` / `AGENTS.md` — instructions complètes
