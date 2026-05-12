using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class FoodRepository : IFoodRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public FoodRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Food>> GetByIdsAsync(
        IReadOnlyCollection<Guid> foodIds,
        CancellationToken cancellationToken)
    {
        if (foodIds.Count == 0)
        {
            return Array.Empty<Food>();
        }

        List<Food> foods = await _dbContext.Foods
            .Where(food => food.IsActive && foodIds.Contains(food.Id))
            .ToListAsync(cancellationToken);

        return foods;
    }

    public async Task<IReadOnlyDictionary<string, Food>> GetActiveByNormalizedNamesAsync(
        IReadOnlyCollection<string> normalizedNames,
        CancellationToken cancellationToken)
    {
        List<string> keys = normalizedNames
            .Where(key => !string.IsNullOrEmpty(key))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (keys.Count == 0)
        {
            return new Dictionary<string, Food>(StringComparer.Ordinal);
        }

        List<Food> rows = await _dbContext.Foods
            .AsNoTracking()
            .Where(food => food.IsActive && keys.Contains(food.NormalizedName))
            .ToListAsync(cancellationToken);

        Dictionary<string, Food> bestByKey = new(StringComparer.Ordinal);
        foreach (Food row in rows)
        {
            if (!bestByKey.TryGetValue(row.NormalizedName, out Food? current)
                || (int)row.SourceType < (int)current.SourceType)
            {
                bestByKey[row.NormalizedName] = row;
            }
        }

        return bestByKey;
    }

    public async Task<Food> AddOrGetActiveAiFoodAsync(Food candidate, CancellationToken cancellationToken)
    {
        Food? existingAi = await _dbContext.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                food => food.IsActive
                        && food.NormalizedName == candidate.NormalizedName
                        && food.SourceType == FoodSourceType.Ai,
                cancellationToken);

        if (existingAi is not null)
        {
            return existingAi;
        }

        await _dbContext.Foods.AddAsync(candidate, cancellationToken);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return candidate;
        }
        catch (DbUpdateException)
        {
            if (_dbContext.Entry(candidate).State != EntityState.Detached)
            {
                _dbContext.Entry(candidate).State = EntityState.Detached;
            }

            Food? resolved = await _dbContext.Foods
                .AsNoTracking()
                .Where(food => food.IsActive && food.NormalizedName == candidate.NormalizedName)
                .OrderBy(food => (int)food.SourceType)
                .FirstOrDefaultAsync(cancellationToken);

            if (resolved is null)
            {
                throw;
            }

            return resolved;
        }
    }
}
