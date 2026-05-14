namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class UpdateMealLogItemRequest
{
    public Guid? Id { get; init; }

    public required string Name { get; init; }

    public int EstimatedQuantityGrams { get; init; }
}
