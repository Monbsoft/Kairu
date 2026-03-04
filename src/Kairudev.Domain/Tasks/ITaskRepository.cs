using Kairudev.Domain.Identity;

namespace Kairudev.Domain.Tasks;

public interface ITaskRepository
{
    Task AddAsync(DeveloperTask task, CancellationToken cancellationToken = default);
    Task<DeveloperTask?> GetByIdAsync(TaskId id, UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeveloperTask>> GetAllAsync(UserId userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(DeveloperTask task, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaskId id, UserId userId, CancellationToken cancellationToken = default);
}
