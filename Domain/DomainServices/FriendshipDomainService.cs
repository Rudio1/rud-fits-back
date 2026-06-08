using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class FriendshipDomainService
{
    public (Guid UserLowId, Guid UserHighId) NormalizePair(Guid userAId, Guid userBId)
    {
        return userAId.CompareTo(userBId) <= 0
            ? (userAId, userBId)
            : (userBId, userAId);
    }

    public void EnsureNotSelf(Guid userId, Guid otherUserId)
    {
        if (userId == otherUserId)
        {
            throw new InvalidOperationException("Não é possível adicionar a si mesmo como amigo.");
        }
    }

    public void EnsureCanAcceptInvite(Guid inviterUserId, Guid acceptorUserId, FriendInviteToken inviteToken)
    {
        EnsureNotSelf(acceptorUserId, inviterUserId);

        if (!inviteToken.IsActive)
        {
            throw new InvalidOperationException("Este link de convite está desativado.");
        }
    }

    public void EnsureInviterIsActive(User inviter)
    {
        if (!inviter.IsActive)
        {
            throw new InvalidOperationException("Este usuário não está disponível para amizade.");
        }
    }

    public void EnsureActiveFriendship(Friendship? friendship)
    {
        if (friendship is null || friendship.Status != FriendshipStatus.Active)
        {
            throw new InvalidOperationException("Amizade não encontrada ou inativa.");
        }
    }

    public void EnsureNotAlreadyFriends(Friendship? existingFriendship)
    {
        if (existingFriendship?.Status == FriendshipStatus.Active)
        {
            throw new InvalidOperationException("Vocês já são amigos.");
        }
    }

    public string GenerateToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
