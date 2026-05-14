namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class DailyMealConsumptionSummaryResponseDto
{
    public required DateOnly Date { get; init; }

    public required int MealsCount { get; init; }

    public required int TotalCalories { get; init; }

    public required decimal TotalProtein { get; init; }

    public required decimal TotalCarbs { get; init; }

    public required decimal TotalFat { get; init; }
}
