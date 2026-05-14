using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class MealLogRepository : IMealLogRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public MealLogRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddMealLogAsync(MealLog mealLog, CancellationToken cancellationToken)
    {
        await _dbContext.MealLogs.AddAsync(mealLog, cancellationToken);
    }

    public async Task<MealLog?> GetActiveByIdAsync(
        Guid userId,
        Guid mealLogId,
        CancellationToken cancellationToken)
    {
        MealLog? mealLog = await _dbContext.MealLogs
            .Include(existingMealLog => existingMealLog.Items)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                existingMealLog => existingMealLog.UserId == userId
                                   && existingMealLog.Id == mealLogId,
                cancellationToken);

        return mealLog;
    }

    public async Task<IReadOnlyCollection<MealLog>> ListByConsumedAtRangeAsync(
        Guid userId,
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken)
    {
        List<MealLog> mealLogs = await _dbContext.MealLogs
            .Include(mealLog => mealLog.Items)
            .AsSplitQuery()
            .Where(mealLog => mealLog.UserId == userId
                              && mealLog.ConsumedAt >= startInclusive
                              && mealLog.ConsumedAt < endExclusive)
            .OrderBy(mealLog => mealLog.ConsumedAt)
            .ToListAsync(cancellationToken);

        return mealLogs;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
