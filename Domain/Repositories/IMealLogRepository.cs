using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IMealLogRepository
{
    Task AddMealLogAsync(MealLog mealLog, CancellationToken cancellationToken);

    Task<MealLog?> GetActiveByIdAsync(
        Guid userId,
        Guid mealLogId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MealLog>> ListByConsumedAtRangeAsync(
        Guid userId,
        DateTime startInclusive,
        DateTime endExclusive,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
