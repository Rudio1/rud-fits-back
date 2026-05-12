using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class OnboardingRepository : IOnboardingRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public OnboardingRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(Account? Account, UserProfile? UserProfile)> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        Account? account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        UserProfile? userProfile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return (account, userProfile);
    }

    public async Task AddUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken)
    {
        await _dbContext.UserProfiles.AddAsync(userProfile, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
