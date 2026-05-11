using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class AuthRepository : IAuthRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public AuthRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsWithEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsWithUsernameAsync(string normalizedUsername, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(user => user.Username == normalizedUsername, cancellationToken);
    }

    public async Task<(User User, Account Account)?> GetUserWithAccountByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .Include(u => u.Account)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user?.Account == null)
        {
            return null;
        }

        return (user, user.Account);
    }

    public async Task AddUserAndAccountAsync(User user, Account account, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.Accounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAccountLastLoginAsync(Guid accountId, DateTime utcNow, CancellationToken cancellationToken)
    {
        Account? account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account == null)
        {
            return;
        }

        account.RecordLogin(utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
