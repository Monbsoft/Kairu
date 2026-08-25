using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace KairuFocus.Application.Gamification.Commands.CreditSprintXp;

/// <summary>
/// Minimal command (SessionId only): the handler reloads the session so the command
/// is re-dispatchable as-is (replay after incident) and the persisted state stays
/// the single source of truth (ADR-023).
/// </summary>
public sealed record CreditSprintXpCommand(Guid SessionId) : ICommand<CreditSprintXpResult>;
