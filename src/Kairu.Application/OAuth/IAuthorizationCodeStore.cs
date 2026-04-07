using Kairu.Domain.Identity;

namespace Kairu.Application.OAuth;

public sealed record AuthorizationCodeEntry(
    UserId UserId,
    string CodeChallenge,
    string RedirectUri,
    DateTime ExpiresAt);

public interface IAuthorizationCodeStore
{
    Task StoreAsync(string code, AuthorizationCodeEntry entry, CancellationToken cancellationToken = default);
    Task<AuthorizationCodeEntry?> ConsumeAsync(string code, CancellationToken cancellationToken = default);
}
