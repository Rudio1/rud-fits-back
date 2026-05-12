namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class MealPhotoAnalysisFoodItemDto
{
    public required string Name { get; init; }

    public required int EstimatedQuantityGrams { get; init; }
}
