using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Settings.Queries.GetApiKey;

/// <summary>Retourne le statut de la clé API de l'utilisateur courant (jamais le hash).</summary>
public sealed record GetApiKeyQuery : IQuery<GetApiKeyResult>;
