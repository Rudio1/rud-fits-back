using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class UserInviteRepository : IUserInviteRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public UserInviteRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserInvite?> GetPendingByEmailAsync(
        string normalizedEmail,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserInvites
            .FirstOrDefaultAsync(
                invite => invite.Email == normalizedEmail
                    && invite.ConsumedAt == null
                    && invite.ExpiresAt > utcNow,
                cancellationToken);
    }

    public async Task<UserInvite?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbContext.UserInvites
            .FirstOrDefaultAsync(invite => invite.Token == token, cancellationToken);
    }

    public async Task AddAsync(UserInvite invite, CancellationToken cancellationToken)
    {
        await _dbContext.UserInvites.AddAsync(invite, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
