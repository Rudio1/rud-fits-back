using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IAuthRepository
{
    Task<bool> ExistsWithEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> ExistsWithUsernameAsync(string normalizedUsername, CancellationToken cancellationToken);

    Task<(User User, Account Account)?> GetUserWithAccountByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);

    Task AddUserAndAccountAsync(User user, Account account, CancellationToken cancellationToken);

    Task UpdateAccountLastLoginAsync(Guid accountId, DateTime utcNow, CancellationToken cancellationToken);
}
