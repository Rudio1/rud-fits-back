using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class MealLogItemResponseDto
{
    public required Guid Id { get; init; }

    public required Guid FoodId { get; init; }

    public required string FoodName { get; init; }

    public required decimal Quantity { get; init; }

    public required UnitType UnitType { get; init; }

    public required int Calories { get; init; }

    public required decimal Protein { get; init; }

    public required decimal Carbs { get; init; }

    public required decimal Fat { get; init; }
}
