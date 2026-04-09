using Kairu.Domain.Identity;

namespace Kairu.Application.Identity.Queries.ValidateMcpToken;

/// <summary>
/// The UserId associated with the validated token.
/// Consumed by the MCP authentication handler to build the ClaimsPrincipal.
/// </summary>
public sealed record ValidateMcpTokenResult(UserId UserId);
