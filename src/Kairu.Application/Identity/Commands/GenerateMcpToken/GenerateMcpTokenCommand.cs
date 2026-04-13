using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Monbsoft.BrilliantMediator.Abstractions.Commands;

namespace Kairu.Application.Identity.Commands.GenerateMcpToken;

/// <summary>
/// Revokes any existing MCP token for the user and generates a new one.
/// The raw token is returned exactly once in the result.
/// </summary>
public sealed record GenerateMcpTokenCommand(UserId UserId) : ICommand<Result<GenerateMcpTokenResult>>;
