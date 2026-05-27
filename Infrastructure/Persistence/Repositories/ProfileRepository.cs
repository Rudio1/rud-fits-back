using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public ProfileRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdWithProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .Include(existingUser => existingUser.UserProfile)
            .AsSplitQuery()
            .FirstOrDefaultAsync(existingUser => existingUser.Id == userId, cancellationToken);

        return user;
    }

    public async Task<bool> TryIncrementFreeScannerUsesAsync(
        Guid userId,
        int lifetimeLimit,
        CancellationToken cancellationToken)
    {
        int rowsAffected = await _dbContext.UserProfiles
            .Where(profile => profile.UserId == userId && profile.FreeScannerUsesCount < lifetimeLimit)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    profile => profile.FreeScannerUsesCount,
                    profile => profile.FreeScannerUsesCount + 1),
                cancellationToken);

        return rowsAffected > 0;
    }
}
