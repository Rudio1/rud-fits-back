namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class CreateMealLogItemRequest
{
    public Guid FoodId { get; init; }

    public decimal Quantity { get; init; }
}
