using System.Text.Json.Serialization;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Meals.Responses;

public sealed class MealLogResponseDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required MealType MealType { get; init; }

    public required MealSourceType SourceType { get; init; }

    [JsonPropertyName("consumedAt")]
    public required DateTime ConsumedAt { get; init; }

    public string? Notes { get; init; }

    public required int TotalCalories { get; init; }

    public required decimal TotalProtein { get; init; }

    public required decimal TotalCarbs { get; init; }

    public required decimal TotalFat { get; init; }

    public required IReadOnlyCollection<MealLogItemResponseDto> Items { get; init; }
}
