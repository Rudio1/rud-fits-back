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
    private readonly IMealLogRepository _mealLogRepository;
    private readonly MealLogDomainService _mealLogDomainService;
    private readonly PersistenceOptions _persistenceOptions;

    public MealLogService(
        IFoodRepository foodRepository,
        IMealLogRepository mealLogRepository,
        MealLogDomainService mealLogDomainService,
        IOptions<PersistenceOptions> persistenceOptions)
    {
        _foodRepository = foodRepository;
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
        string mealName = request.Name.Trim();
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

    public async Task<IReadOnlyCollection<MealLogResponseDto>> ListByDateAsync(
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
                if (food.BaseQuantity <= 0m)
                {
                    throw new ArgumentException("Alimento com quantidade base inválida.");
                }

                decimal multiplier = portion.Quantity / food.BaseQuantity;
                int calories = (int)decimal.Round(food.Calories * multiplier, 0, MidpointRounding.AwayFromZero);
                decimal protein = decimal.Round(food.Protein * multiplier, NutritionScale, MidpointRounding.AwayFromZero);
                decimal carbs = decimal.Round(food.Carbs * multiplier, NutritionScale, MidpointRounding.AwayFromZero);
                decimal fat = decimal.Round(food.Fat * multiplier, NutritionScale, MidpointRounding.AwayFromZero);

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
        List<MealLogItemResponseDto> items = mealLog.Items
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
}
