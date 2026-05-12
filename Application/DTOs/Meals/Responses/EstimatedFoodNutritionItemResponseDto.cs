namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class EstimatedFoodNutritionItemResponseDto
{
    public required Guid FoodId { get; init; }

    public required string Name { get; init; }

    public int EstimatedQuantityGrams { get; init; }

    public decimal CaloriesKcal { get; init; }

    public decimal CarbohydratesGrams { get; init; }

    public decimal FatGrams { get; init; }
}
