using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class Friendship : BaseEntity
{
    private Friendship()
    {
    }

    public Friendship(
        Guid id,
        Guid userLowId,
        Guid userHighId,
        Guid initiatedByUserId,
        DateTime establishedAt)
        : this()
    {
        Id = id;
        UserLowId = userLowId;
        UserHighId = userHighId;
        InitiatedByUserId = initiatedByUserId;
        Status = FriendshipStatus.Active;
        EstablishedAt = establishedAt;
    }

    public Guid UserLowId { get; private set; }

    public Guid UserHighId { get; private set; }

    public FriendshipStatus Status { get; private set; }

    public Guid InitiatedByUserId { get; private set; }

    public DateTime? EstablishedAt { get; private set; }

    public User UserLow { get; private set; } = null!;

    public User UserHigh { get; private set; } = null!;

    public void Activate(Guid initiatedByUserId, DateTime establishedAt)
    {
        Status = FriendshipStatus.Active;
        InitiatedByUserId = initiatedByUserId;
        EstablishedAt = establishedAt;
    }

    public void Remove()
    {
        Status = FriendshipStatus.Removed;
        EstablishedAt = null;
    }
}
