namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class EstimateDetectedFoodsNutritionResponseDto
{
    public required IReadOnlyList<EstimatedFoodNutritionItemResponseDto> Foods { get; init; }
}
