using Kairu.Domain.Identity;

namespace Kairu.Application.OAuth;

public sealed record AuthorizationCodeEntry(
    UserId UserId,
    string DisplayName,
    string Login,
    string CodeChallenge,
    string RedirectUri,
    DateTime ExpiresAt);

/// <summary>
/// Stores OAuth 2.1 authorization codes (single-use, time-limited).
/// <para>
/// Contract: <see cref="ConsumeAsync"/> MUST return <c>null</c> for expired entries
/// (i.e. entries whose <see cref="AuthorizationCodeEntry.ExpiresAt"/> is in the past).
/// </para>
/// </summary>
public interface IAuthorizationCodeStore
{
    Task StoreAsync(string code, AuthorizationCodeEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically removes and returns the entry for <paramref name="code"/>.
    /// Returns <c>null</c> if the code is unknown, already consumed, or expired.
    /// </summary>
    Task<AuthorizationCodeEntry?> ConsumeAsync(string code, CancellationToken cancellationToken = default);
}
