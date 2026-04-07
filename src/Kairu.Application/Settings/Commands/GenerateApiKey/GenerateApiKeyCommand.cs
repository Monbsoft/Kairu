using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.GenerateApiKey;

/// <summary>Génère (ou régénère) une API Key pour l'utilisateur courant.</summary>
public sealed record GenerateApiKeyCommand : ICommand<GenerateApiKeyResult>;
