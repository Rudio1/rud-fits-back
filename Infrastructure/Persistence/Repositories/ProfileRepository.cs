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
}
