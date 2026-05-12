namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class MealPhotoAnalysisResponseDto
{
    public required IReadOnlyList<MealPhotoAnalysisFoodItemDto> Foods { get; init; }
}
