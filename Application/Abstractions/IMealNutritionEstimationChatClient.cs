namespace RudFitAI.Application.Abstractions;

public interface IMealNutritionEstimationChatClient
{
    Task<string> GetNutritionEstimatesJsonAsync(string foodsInputJson, CancellationToken cancellationToken);
}
