using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class UpdateMealLogRequest
{
    public string? Name { get; init; }

    public MealType MealType { get; init; }

    public required IReadOnlyCollection<UpdateMealLogItemRequest> Items { get; init; }
}
