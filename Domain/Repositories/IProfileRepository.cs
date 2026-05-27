using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IProfileRepository
{
    Task<User?> GetByIdWithProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> TryIncrementFreeScannerUsesAsync(
        Guid userId,
        int lifetimeLimit,
        CancellationToken cancellationToken);
}
