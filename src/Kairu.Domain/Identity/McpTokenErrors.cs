namespace Kairu.Domain.Identity;

public static class McpTokenErrors
{
    public const string NoTokenFound = "mcp_token.not_found";
    public const string Expired      = "mcp_token.expired";
    public const string InvalidHash  = "mcp_token.invalid_hash";
}
