using RudFitAI.Domain.Common;

namespace RudFitAI.Domain.Entities;

public sealed class FriendInviteToken : BaseEntity
{
    private FriendInviteToken()
    {
    }

    public FriendInviteToken(Guid id, Guid userId, string token)
        : this()
    {
        Id = id;
        UserId = userId;
        Token = token;
        IsActive = true;
    }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public User User { get; private set; } = null!;

    public void RegenerateToken(string token)
    {
        Token = token;
        IsActive = true;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
