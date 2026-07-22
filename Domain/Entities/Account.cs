using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class Account : BaseEntity
{
    private Account()
    {
    }

    public Account(Guid id, Guid userId, string passwordHash, LoginProvider loginProvider, bool isAdmin = false)
        : this()
    {
        Id = id;
        UserId = userId;
        PasswordHash = passwordHash;
        EmailVerified = false;
        LoginProvider = loginProvider;
        IsTwoFactorEnabled = false;
        IsFirstAccess = true;
        IsAdmin = isAdmin;
    }

    public Guid UserId { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public bool EmailVerified { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiresAt { get; private set; }

    public LoginProvider LoginProvider { get; private set; }

    public bool IsTwoFactorEnabled { get; private set; }

    public bool IsFirstAccess { get; private set; }

    public bool IsAdmin { get; private set; }

    public User User { get; private set; } = null!;

    public void RecordLogin(DateTime utcNow)
    {
        LastLoginAt = utcNow;
    }

    public void CompleteFirstAccess()
    {
        IsFirstAccess = false;
    }

    public void MarkAsAdmin()
    {
        IsAdmin = true;
    }

    public void MarkEmailVerified()
    {
        EmailVerified = true;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
