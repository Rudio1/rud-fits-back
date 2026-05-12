using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Application.DTOs.Meals.Responses;

namespace RudFitAI.Application.Services.Interfaces.Meals;

public interface IMealDetectedFoodsNutritionEstimationService
{
    Task<EstimateDetectedFoodsNutritionResponseDto> EstimateAsync(
        EstimateDetectedFoodsNutritionRequest request,
        CancellationToken cancellationToken);
}
