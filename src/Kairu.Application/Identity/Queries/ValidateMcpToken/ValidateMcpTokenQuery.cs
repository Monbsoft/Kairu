using Kairu.Domain.Common;
using Kairu.Domain.Identity;
using Monbsoft.BrilliantMediator.Abstractions.Queries;

namespace Kairu.Application.Identity.Queries.ValidateMcpToken;

/// <summary>
/// Internal query consumed by the MCP authentication middleware.
/// Validates a raw token extracted from the Authorization: Bearer header.
/// Not exposed as a REST endpoint.
/// </summary>
public sealed record ValidateMcpTokenQuery(McpRawToken RawToken) : IQuery<Result<ValidateMcpTokenResult>>;
