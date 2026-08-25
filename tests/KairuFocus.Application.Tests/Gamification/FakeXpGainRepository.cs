using KairuFocus.Domain.Gamification;
using KairuFocus.Domain.Identity;

namespace KairuFocus.Application.Tests.Gamification;

internal sealed class FakeXpGainRepository : IXpGainRepository
{
    public List<XpGain> Gains { get; } = [];
    public int AddAsyncCallCount { get; private set; }

    public Task AddAsync(XpGain gain, CancellationToken cancellationToken = default)
    {
        AddAsyncCallCount++;
        Gains.Add(gain);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Gains.Any(g => g.SessionId == sessionId));

    public Task<int> GetTotalXpAsync(UserId userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Gains.Where(g => g.OwnerId == userId).Sum(g => g.Amount));

    public Task<int> GetXpBetweenAsync(UserId userId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult(Gains
            .Where(g => g.OwnerId == userId
                        && g.EarnedAtUtc >= startUtc
                        && g.EarnedAtUtc < endUtc)
            .Sum(g => g.Amount));
}
