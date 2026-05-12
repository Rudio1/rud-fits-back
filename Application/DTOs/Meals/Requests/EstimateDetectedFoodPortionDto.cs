namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class EstimateDetectedFoodPortionDto
{
    public required string Name { get; init; }

    public required int EstimatedQuantityGrams { get; init; }
}
