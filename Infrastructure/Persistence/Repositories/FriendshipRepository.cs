using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class FriendshipRepository : IFriendshipRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public FriendshipRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FriendInviteToken?> GetInviteTokenByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FriendInviteTokens
            .FirstOrDefaultAsync(inviteToken => inviteToken.UserId == userId, cancellationToken);
    }

    public async Task<FriendInviteToken?> GetInviteTokenByTokenAsync(
        string token,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FriendInviteTokens
            .Include(inviteToken => inviteToken.User)
            .FirstOrDefaultAsync(inviteToken => inviteToken.Token == token, cancellationToken);
    }

    public async Task AddInviteTokenAsync(FriendInviteToken inviteToken, CancellationToken cancellationToken)
    {
        await _dbContext.FriendInviteTokens.AddAsync(inviteToken, cancellationToken);
    }

    public async Task<Friendship?> GetFriendshipByPairAsync(
        Guid userLowId,
        Guid userHighId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Friendships
            .FirstOrDefaultAsync(
                friendship => friendship.UserLowId == userLowId && friendship.UserHighId == userHighId,
                cancellationToken);
    }

    public async Task<Friendship?> GetActiveFriendshipAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Friendships
            .FirstOrDefaultAsync(
                friendship =>
                    friendship.Status == FriendshipStatus.Active
                    && ((friendship.UserLowId == userId && friendship.UserHighId == friendUserId)
                        || (friendship.UserLowId == friendUserId && friendship.UserHighId == userId)),
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Friendship>> ListActiveFriendshipsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<Friendship> friendships = await _dbContext.Friendships
            .Include(friendship => friendship.UserLow)
            .Include(friendship => friendship.UserHigh)
            .AsSplitQuery()
            .Where(
                friendship =>
                    friendship.Status == FriendshipStatus.Active
                    && (friendship.UserLowId == userId || friendship.UserHighId == userId))
            .OrderByDescending(friendship => friendship.EstablishedAt)
            .ToListAsync(cancellationToken);

        return friendships;
    }

    public async Task AddFriendshipAsync(Friendship friendship, CancellationToken cancellationToken)
    {
        await _dbContext.Friendships.AddAsync(friendship, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
