using RudFitAI.Domain.Common;

namespace RudFitAI.Domain.Entities;

public sealed class UserInvite : BaseEntity
{
    private UserInvite()
    {
    }

    public UserInvite(
        Guid id,
        string email,
        string token,
        Guid invitedByUserId,
        DateTime expiresAt)
        : this()
    {
        Id = id;
        Email = email;
        Token = token;
        InvitedByUserId = invitedByUserId;
        ExpiresAt = expiresAt;
        ConsumedAt = null;
    }

    public string Email { get; private set; } = string.Empty;

    public string Token { get; private set; } = string.Empty;

    public Guid InvitedByUserId { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? ConsumedAt { get; private set; }

    public User InvitedByUser { get; private set; } = null!;

    public bool IsPending(DateTime utcNow)
    {
        return ConsumedAt is null && ExpiresAt > utcNow;
    }

    public void Refresh(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
        ConsumedAt = null;
    }

    public void MarkConsumed(DateTime utcNow)
    {
        ConsumedAt = utcNow;
    }
}
