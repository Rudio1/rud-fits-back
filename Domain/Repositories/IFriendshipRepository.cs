using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IFriendshipRepository
{
    Task<FriendInviteToken?> GetInviteTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<FriendInviteToken?> GetInviteTokenByTokenAsync(string token, CancellationToken cancellationToken);

    Task AddInviteTokenAsync(FriendInviteToken inviteToken, CancellationToken cancellationToken);

    Task<Friendship?> GetFriendshipByPairAsync(
        Guid userLowId,
        Guid userHighId,
        CancellationToken cancellationToken);

    Task<Friendship?> GetActiveFriendshipAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Friendship>> ListActiveFriendshipsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task AddFriendshipAsync(Friendship friendship, CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
