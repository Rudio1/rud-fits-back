using Microsoft.Extensions.Options;
using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Meals;
using RudFitAI.Application.Time;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Meals;

public sealed class MealLogService : IMealLogService
{
    private const int NutritionScale = 2;

    private readonly IFoodRepository _foodRepository;
    private readonly IMealDetectedFoodsNutritionEstimationService _mealDetectedFoodsNutritionEstimationService;
    private readonly IMealLogRepository _mealLogRepository;
    private readonly MealLogDomainService _mealLogDomainService;
    private readonly PersistenceOptions _persistenceOptions;

    public MealLogService(
        IFoodRepository foodRepository,
        IMealDetectedFoodsNutritionEstimationService mealDetectedFoodsNutritionEstimationService,
        IMealLogRepository mealLogRepository,
        MealLogDomainService mealLogDomainService,
        IOptions<PersistenceOptions> persistenceOptions)
    {
        _foodRepository = foodRepository;
        _mealDetectedFoodsNutritionEstimationService = mealDetectedFoodsNutritionEstimationService;
        _mealLogRepository = mealLogRepository;
        _mealLogDomainService = mealLogDomainService;
        _persistenceOptions = persistenceOptions.Value;
    }

    public async Task<MealLogResponseDto> CreateManualAsync(
        Guid userId,
        CreateMealLogRequest request,
        CancellationToken cancellationToken)
    {
        Guid mealLogId = Guid.NewGuid();
        DateTime consumedAt =
            DateTime.SpecifyKind(request.ConsumedAt.DateTime, DateTimeKind.Unspecified);
        string mealName = ResolveMealName(request.Name, request.MealType);
        string? notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        IReadOnlyCollection<(Guid FoodId, decimal Quantity)> portions = request.Items
            .Select(item => (item.FoodId, item.Quantity))
            .ToList();

        IReadOnlyCollection<MealLogItem> items =
            await BuildItemsFromCatalogAsync(mealLogId, portions, cancellationToken);

        return await PersistMealLogAsync(
            userId,
            mealLogId,
            mealName,
            request.MealType,
            consumedAt,
            MealSourceType.Manual,
            notes,
            items,
            cancellationToken);
    }

    public async Task<MealLogResponseDto> CreateFromDetectedFoodsAsync(
        Guid userId,
        CreateMealLogFromDetectedFoodsRequest request,
        CancellationToken cancellationToken)
    {
        Guid mealLogId = Guid.NewGuid();
        DateTime consumedAt = request.ConsumedAt.HasValue
            ? DateTime.SpecifyKind(request.ConsumedAt.Value.DateTime, DateTimeKind.Unspecified)
            : PersistenceClock.GetWallClockNow(_persistenceOptions);
        string mealName = ResolveMealName(request.Name, request.MealType);
        string? notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        IReadOnlyCollection<(Guid FoodId, decimal Quantity)> portions = request.Foods
            .Select(food => (food.FoodId, (decimal)food.EstimatedQuantityGrams))
            .ToList();

        IReadOnlyCollection<MealLogItem> items =
            await BuildItemsFromCatalogAsync(mealLogId, portions, cancellationToken);

        return await PersistMealLogAsync(
            userId,
            mealLogId,
            mealName,
            request.MealType,
            consumedAt,
            MealSourceType.Ai,
            notes,
            items,
            cancellationToken);
    }

    public async Task<MealLogResponseDto?> UpdateAsync(
        Guid userId,
        Guid mealLogId,
        UpdateMealLogRequest request,
        CancellationToken cancellationToken)
    {
        MealLog? mealLog =
            await _mealLogRepository.GetActiveByIdAsync(userId, mealLogId, cancellationToken);

        if (mealLog is null)
        {
            return null;
        }

        Dictionary<Guid, MealLogItem> existingItemsById = mealLog.ActiveItems
            .ToDictionary(item => item.Id);

        HashSet<Guid> requestedExistingItemIds = request.Items
            .Where(item => item.Id.HasValue)
            .Select(item => item.Id!.Value)
            .ToHashSet();

        if (requestedExistingItemIds.Any(itemId => !existingItemsById.ContainsKey(itemId)))
        {
            throw new ArgumentException("Um ou mais itens da refeição não foram encontrados.");
        }

        IReadOnlyList<ResolvedMealLogItemInput> resolvedItems =
            await ResolveUpdatedItemsAsync(request.Items.ToList(), existingItemsById, cancellationToken);

        DateTime auditNow = PersistenceClock.GetWallClockNow(_persistenceOptions);

        foreach (MealLogItem existingItem in existingItemsById.Values)
        {
            if (!requestedExistingItemIds.Contains(existingItem.Id))
            {
                existingItem.SoftDelete(auditNow);
            }
        }

        foreach (ResolvedMealLogItemInput resolvedItem in resolvedItems)
        {
            if (resolvedItem.ExistingItemId.HasValue)
            {
                MealLogItem existingItem = existingItemsById[resolvedItem.ExistingItemId.Value];
                existingItem.Update(
                    resolvedItem.FoodId,
                    resolvedItem.FoodName,
                    resolvedItem.Quantity,
                    resolvedItem.UnitType,
                    resolvedItem.Calories,
                    resolvedItem.Protein,
                    resolvedItem.Carbs,
                    resolvedItem.Fat);

                continue;
            }

            MealLogItem newItem = new(
                Guid.NewGuid(),
                mealLog.Id,
                resolvedItem.FoodId,
                resolvedItem.FoodName,
                resolvedItem.Quantity,
                resolvedItem.UnitType,
                resolvedItem.Calories,
                resolvedItem.Protein,
                resolvedItem.Carbs,
                resolvedItem.Fat);

            mealLog.AddItem(newItem);
        }

        mealLog.UpdateDetails(ResolveMealName(request.Name, request.MealType), request.MealType);

        IReadOnlyCollection<MealLogItem> activeItems = mealLog.ActiveItems;
        _mealLogDomainService.EnsureValidMealLog(mealLog.MealType, mealLog.ConsumedAt, activeItems);

        (int totalCalories, decimal totalProtein, decimal totalCarbs, decimal totalFat) =
            _mealLogDomainService.CalculateTotals(activeItems);

        mealLog.UpdateTotals(totalCalories, totalProtein, totalCarbs, totalFat);

        await _mealLogRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(mealLog);
    }

    public async Task<bool> SoftDeleteAsync(
        Guid userId,
        Guid mealLogId,
        CancellationToken cancellationToken)
    {
        MealLog? mealLog =
            await _mealLogRepository.GetActiveByIdAsync(userId, mealLogId, cancellationToken);

        if (mealLog is null)
        {
            return false;
        }

        DateTime deletedAt = PersistenceClock.GetWallClockNow(_persistenceOptions);
        mealLog.SoftDelete(deletedAt);

        await _mealLogRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<DailyMealConsumptionSummaryResponseDto> GetDailyConsumptionSummaryAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealLog> mealLogs =
            await GetMealLogsByDateAsync(userId, date, cancellationToken);

        int totalCalories = mealLogs.Sum(mealLog => mealLog.TotalCalories);
        decimal totalProtein = mealLogs.Sum(mealLog => mealLog.TotalProtein);
        decimal totalCarbs = mealLogs.Sum(mealLog => mealLog.TotalCarbs);
        decimal totalFat = mealLogs.Sum(mealLog => mealLog.TotalFat);

        return new DailyMealConsumptionSummaryResponseDto
        {
            Date = date,
            MealsCount = mealLogs.Count,
            TotalCalories = totalCalories,
            TotalProtein = decimal.Round(totalProtein, NutritionScale, MidpointRounding.AwayFromZero),
            TotalCarbs = decimal.Round(totalCarbs, NutritionScale, MidpointRounding.AwayFromZero),
            TotalFat = decimal.Round(totalFat, NutritionScale, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<IReadOnlyCollection<MealLogResponseDto>> ListByDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealLog> mealLogs =
            await GetMealLogsByDateAsync(userId, date, cancellationToken);

        List<MealLogResponseDto> response = mealLogs
            .OrderBy(log => log.ConsumedAt)
            .Select(ToResponse)
            .ToList();

        return response;
    }

    private async Task<IReadOnlyCollection<MealLogItem>> BuildItemsFromCatalogAsync(
        Guid mealLogId,
        IReadOnlyCollection<(Guid FoodId, decimal Quantity)> portions,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> requestedFoodIds = portions
            .Select(portion => portion.FoodId)
            .Distinct()
            .ToList();

        IReadOnlyCollection<Food> foods =
            await _foodRepository.GetByIdsAsync(requestedFoodIds, cancellationToken);

        Dictionary<Guid, Food> foodById = foods.ToDictionary(food => food.Id);
        if (foodById.Count != requestedFoodIds.Count)
        {
            throw new ArgumentException("Um ou mais alimentos não foram encontrados.");
        }

        List<MealLogItem> items = portions
            .Select(portion =>
            {
                Food food = foodById[portion.FoodId];
                (int calories, decimal protein, decimal carbs, decimal fat) =
                    CalculateNutrition(food, portion.Quantity);

                return new MealLogItem(
                    Guid.NewGuid(),
                    mealLogId,
                    food.Id,
                    food.Name,
                    portion.Quantity,
                    food.UnitType,
                    calories,
                    protein,
                    carbs,
                    fat);
            })
            .ToList();

        return items;
    }

    private async Task<IReadOnlyList<ResolvedMealLogItemInput>> ResolveUpdatedItemsAsync(
        IReadOnlyList<UpdateMealLogItemRequest> requestItems,
        IReadOnlyDictionary<Guid, MealLogItem> existingItemsById,
        CancellationToken cancellationToken)
    {
        Dictionary<int, Guid> resolvedFoodIdsByIndex = new();
        List<(int Index, EstimateDetectedFoodPortionDto Portion)> foodsToEstimate = new();

        for (int index = 0; index < requestItems.Count; index++)
        {
            UpdateMealLogItemRequest requestItem = requestItems[index];

            if (requestItem.Id.HasValue)
            {
                MealLogItem existingItem = existingItemsById[requestItem.Id.Value];
                bool nameChanged =
                    Food.NormalizeForLookup(requestItem.Name) != Food.NormalizeForLookup(existingItem.FoodName);

                if (!nameChanged)
                {
                    resolvedFoodIdsByIndex[index] = existingItem.FoodId;
                    continue;
                }
            }

            foodsToEstimate.Add(
                (index, new EstimateDetectedFoodPortionDto
                {
                    Name = requestItem.Name.Trim(),
                    EstimatedQuantityGrams = requestItem.EstimatedQuantityGrams
                }));
        }

        if (foodsToEstimate.Count > 0)
        {
            EstimateDetectedFoodsNutritionResponseDto estimatedFoods =
                await _mealDetectedFoodsNutritionEstimationService.EstimateAsync(
                    new EstimateDetectedFoodsNutritionRequest
                    {
                        Foods = foodsToEstimate.Select(item => item.Portion).ToList()
                    },
                    cancellationToken);

            for (int position = 0; position < foodsToEstimate.Count; position++)
            {
                resolvedFoodIdsByIndex[foodsToEstimate[position].Index] = estimatedFoods.Foods[position].FoodId;
            }
        }

        IReadOnlyCollection<Food> resolvedFoods =
            await _foodRepository.GetByIdsAsync(resolvedFoodIdsByIndex.Values.Distinct().ToList(), cancellationToken);

        Dictionary<Guid, Food> foodsById = resolvedFoods.ToDictionary(food => food.Id);
        List<ResolvedMealLogItemInput> resolvedItems = new(requestItems.Count);

        for (int index = 0; index < requestItems.Count; index++)
        {
            UpdateMealLogItemRequest requestItem = requestItems[index];
            Guid foodId = resolvedFoodIdsByIndex[index];

            if (!foodsById.TryGetValue(foodId, out Food? food))
            {
                throw new ArgumentException("Um ou mais alimentos não foram encontrados.");
            }

            (int calories, decimal protein, decimal carbs, decimal fat) =
                CalculateNutrition(food, requestItem.EstimatedQuantityGrams);

            resolvedItems.Add(
                new ResolvedMealLogItemInput(
                    requestItem.Id,
                    food.Id,
                    food.Name,
                    requestItem.EstimatedQuantityGrams,
                    food.UnitType,
                    calories,
                    protein,
                    carbs,
                    fat));
        }

        return resolvedItems;
    }

    private static (int Calories, decimal Protein, decimal Carbs, decimal Fat) CalculateNutrition(
        Food food,
        decimal quantity)
    {
        if (food.BaseQuantity <= 0m)
        {
            throw new ArgumentException("Alimento com quantidade base inválida.");
        }

        decimal multiplier = quantity / food.BaseQuantity;
        int calories = (int)decimal.Round(food.Calories * multiplier, 0, MidpointRounding.AwayFromZero);
        decimal protein = decimal.Round(food.Protein * multiplier, NutritionScale, MidpointRounding.AwayFromZero);
        decimal carbs = decimal.Round(food.Carbs * multiplier, NutritionScale, MidpointRounding.AwayFromZero);
        decimal fat = decimal.Round(food.Fat * multiplier, NutritionScale, MidpointRounding.AwayFromZero);

        return (calories, protein, carbs, fat);
    }

    private async Task<IReadOnlyCollection<MealLog>> GetMealLogsByDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        (DateTime startInclusive, DateTime endExclusive) = ResolveConsumedCalendarDayRange(date);

        IReadOnlyCollection<MealLog> mealLogs =
            await _mealLogRepository.ListByConsumedAtRangeAsync(
                userId,
                startInclusive,
                endExclusive,
                cancellationToken);

        return mealLogs;
    }

    private async Task<MealLogResponseDto> PersistMealLogAsync(
        Guid userId,
        Guid mealLogId,
        string mealName,
        MealType mealType,
        DateTime consumedAt,
        MealSourceType sourceType,
        string? notes,
        IReadOnlyCollection<MealLogItem> items,
        CancellationToken cancellationToken)
    {
        _mealLogDomainService.EnsureValidMealLog(mealType, consumedAt, items);

        (int totalCalories, decimal totalProtein, decimal totalCarbs, decimal totalFat) =
            _mealLogDomainService.CalculateTotals(items);

        MealLog mealLog = new(
            mealLogId,
            userId,
            mealName,
            mealType,
            consumedAt,
            sourceType,
            notes,
            totalCalories,
            totalProtein,
            totalCarbs,
            totalFat,
            items);

        await _mealLogRepository.AddMealLogAsync(mealLog, cancellationToken);
        await _mealLogRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(mealLog);
    }

    private static string ResolveMealName(string? providedName, MealType mealType)
    {
        if (!string.IsNullOrWhiteSpace(providedName))
        {
            return providedName.Trim();
        }

        return mealType switch
        {
            MealType.Breakfast => "Café da manhã",
            MealType.Lunch => "Almoço",
            MealType.Dinner => "Jantar",
            MealType.Snack => "Lanche",
            MealType.PreWorkout => "Pré-treino",
            MealType.PostWorkout => "Pós-treino",
            _ => mealType.ToString()
        };
    }

    private static MealLogResponseDto ToResponse(MealLog mealLog)
    {
        List<MealLogItemResponseDto> items = mealLog.ActiveItems
            .Select(item => new MealLogItemResponseDto
            {
                Id = item.Id,
                FoodId = item.FoodId,
                FoodName = item.FoodName,
                Quantity = item.Quantity,
                UnitType = item.UnitType,
                Calories = item.Calories,
                Protein = item.Protein,
                Carbs = item.Carbs,
                Fat = item.Fat
            })
            .ToList();

        return new MealLogResponseDto
        {
            Id = mealLog.Id,
            Name = mealLog.Name,
            MealType = mealLog.MealType,
            SourceType = mealLog.SourceType,
            ConsumedAt = mealLog.ConsumedAt,
            Notes = mealLog.Notes,
            TotalCalories = mealLog.TotalCalories,
            TotalProtein = mealLog.TotalProtein,
            TotalCarbs = mealLog.TotalCarbs,
            TotalFat = mealLog.TotalFat,
            Items = items
        };
    }

    private static (DateTime StartInclusive, DateTime EndExclusive) ResolveConsumedCalendarDayRange(DateOnly calendarDate)
    {
        DateTime startInclusive =
            DateTime.SpecifyKind(calendarDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        DateTime endExclusive = startInclusive.AddDays(1);
        return (startInclusive, endExclusive);
    }

    private sealed record ResolvedMealLogItemInput(
        Guid? ExistingItemId,
        Guid FoodId,
        string FoodName,
        decimal Quantity,
        UnitType UnitType,
        int Calories,
        decimal Protein,
        decimal Carbs,
        decimal Fat);
}
