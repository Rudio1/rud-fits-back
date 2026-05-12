using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Application.DTOs.Meals.Responses;

namespace RudFitAI.Application.Services.Interfaces.Meals;

public interface IMealLogService
{
    Task<MealLogResponseDto> CreateManualAsync(
        Guid userId,
        CreateMealLogRequest request,
        CancellationToken cancellationToken);

    Task<MealLogResponseDto> CreateFromDetectedFoodsAsync(
        Guid userId,
        CreateMealLogFromDetectedFoodsRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MealLogResponseDto>> ListByDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken);
}
