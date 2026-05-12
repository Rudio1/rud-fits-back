namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class EstimateDetectedFoodsNutritionRequest
{
    public required IReadOnlyList<EstimateDetectedFoodPortionDto> Foods { get; init; }
}
