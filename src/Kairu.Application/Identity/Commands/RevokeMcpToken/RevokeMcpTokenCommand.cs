using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Identity.Commands.RevokeMcpToken;

/// <summary>
/// Revokes the active MCP token for the user.
/// Returns Failure(NoTokenFound) if no active token exists.
/// </summary>
public sealed record RevokeMcpTokenCommand(UserId UserId) : ICommand<Result<RevokeMcpTokenResult>>;
