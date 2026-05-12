using System.Text.Json.Serialization;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class CreateMealLogFromDetectedFoodsRequest
{
    public MealType MealType { get; init; }

    public string? Name { get; init; }

    [JsonPropertyName("consumedAt")]
    public DateTimeOffset? ConsumedAt { get; init; }

    public string? Notes { get; init; }

    public required IReadOnlyCollection<CreateMealLogFromDetectedFoodsItemRequest> Foods { get; init; }
}
