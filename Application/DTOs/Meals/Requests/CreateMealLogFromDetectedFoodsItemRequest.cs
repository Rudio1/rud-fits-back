namespace RudFitAI.Application.DTOs.Meals.Requests;

public sealed class CreateMealLogFromDetectedFoodsItemRequest
{
    public Guid FoodId { get; init; }

    public int EstimatedQuantityGrams { get; init; }
}
