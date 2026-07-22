using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IUserInviteRepository
{
    Task<UserInvite?> GetPendingByEmailAsync(string normalizedEmail, DateTime utcNow, CancellationToken cancellationToken);

    Task<UserInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task AddAsync(UserInvite invite, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
