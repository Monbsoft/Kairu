using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Identity.Queries.GetMcpTokenStatus;

/// <summary>
/// Returns the status and expiry date of the user's MCP token.
/// Never returns the hash or the raw token.
/// </summary>
public sealed record GetMcpTokenStatusQuery(UserId UserId) : IQuery<Result<GetMcpTokenStatusResult>>;
