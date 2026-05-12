using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface IFoodRepository
{
    Task<IReadOnlyCollection<Food>> GetByIdsAsync(
        IReadOnlyCollection<Guid> foodIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, Food>> GetActiveByNormalizedNamesAsync(
        IReadOnlyCollection<string> normalizedNames,
        CancellationToken cancellationToken);

    Task<Food> AddOrGetActiveAiFoodAsync(Food candidate, CancellationToken cancellationToken);
}
