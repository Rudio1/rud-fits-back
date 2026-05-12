using RudFitAI.Application.DTOs.Meals.Responses;

namespace RudFitAI.Application.Services.Interfaces.Meals;

public interface IMealPhotoAnalysisService
{
    Task<MealPhotoAnalysisResponseDto> AnalyzePhotoAsync(
        byte[] imageBytes,
        string imageMimeType,
        CancellationToken cancellationToken);
}
