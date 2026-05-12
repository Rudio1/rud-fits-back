using System.Text.Json.Serialization;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class CreateMealLogRequest
{
    public required string Name { get; init; }

    public MealType MealType { get; init; }

    [JsonPropertyName("consumedAt")]
    public DateTimeOffset ConsumedAt { get; init; }

    public string? Notes { get; init; }

    public required IReadOnlyCollection<CreateMealLogItemRequest> Items { get; init; }
}
