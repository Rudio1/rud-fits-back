using RudFitAI.Domain.Common;

namespace RudFitAI.Domain.Entities;

public sealed class User : BaseEntity
{
    private User()
    {
    }

    public User(Guid id, string name, string email, string? username, bool isActive)
        : this()
    {
        Id = id;
        Name = name;
        Email = email;
        Username = username;
        ProfileImageUrl = null;
        IsActive = isActive;
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Username { get; private set; }

    public string? ProfileImageUrl { get; private set; }

    public bool IsActive { get; private set; }

    public Account? Account { get; private set; }

    public void SetProfileImageUrl(string? url)
    {
        ProfileImageUrl = url;
    }
}
