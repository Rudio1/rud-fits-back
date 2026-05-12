using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IOnboardingRepository
{
    Task<(Account? Account, UserProfile? UserProfile)> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
