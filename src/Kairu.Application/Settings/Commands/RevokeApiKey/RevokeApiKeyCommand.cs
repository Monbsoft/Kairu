using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Settings.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand : ICommand<RevokeApiKeyResult>;
