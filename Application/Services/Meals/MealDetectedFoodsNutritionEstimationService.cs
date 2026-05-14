using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Meals;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Meals;

public sealed class MealDetectedFoodsNutritionEstimationService : IMealDetectedFoodsNutritionEstimationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions SerializePromptOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IFoodRepository _foodRepository;
    private readonly IMealNutritionEstimationChatClient _chatClient;
    private readonly OpenAiOptions _openAiOptions;

    public MealDetectedFoodsNutritionEstimationService(
        IFoodRepository foodRepository,
        IMealNutritionEstimationChatClient chatClient,
        IOptions<OpenAiOptions> openAiOptions)
    {
        _foodRepository = foodRepository;
        _chatClient = chatClient;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<EstimateDetectedFoodsNutritionResponseDto> EstimateAsync(
        EstimateDetectedFoodsNutritionRequest request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<EstimateDetectedFoodPortionDto> portions = request.Foods;

        List<string> normalizedKeys = portions
            .Select(portion => Food.NormalizeForLookup(portion.Name))
            .Where(key => key.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        IReadOnlyDictionary<string, Food> catalog =
            await _foodRepository.GetActiveByNormalizedNamesAsync(normalizedKeys, cancellationToken);

        List<int> needAiIndices = new();
        for (int i = 0; i < portions.Count; i++)
        {
            string key = Food.NormalizeForLookup(portions[i].Name);
            if (!catalog.ContainsKey(key))
            {
                needAiIndices.Add(i);
            }
        }

        OpenAiNutritionEnvelope? aiEnvelope = null;
        if (needAiIndices.Count > 0)
        {
            int timeoutSeconds = _openAiOptions.RequestTimeoutSeconds <= 0 ? 10 : _openAiOptions.RequestTimeoutSeconds;
            timeoutSeconds = Math.Clamp(timeoutSeconds, 1, 120);
            using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(timeoutSeconds));
            using CancellationTokenSource linked =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            EstimateDetectedFoodsNutritionRequest subRequest = new()
            {
                Foods = needAiIndices.Select(index => portions[index]).ToList()
            };

            string inputJson = JsonSerializer.Serialize(subRequest, SerializePromptOptions);
            string rawContent = await _chatClient.GetNutritionEstimatesJsonAsync(inputJson, linked.Token);

            string jsonPayload = ExtractJsonPayload(rawContent);
            aiEnvelope = JsonSerializer.Deserialize<OpenAiNutritionEnvelope>(jsonPayload, JsonOptions);
            if (aiEnvelope?.Foods is null || aiEnvelope.Foods.Count != needAiIndices.Count)
            {
                throw new InvalidOperationException(
                    "Não foi possível obter estimativas nutricionais ou a resposta da IA não corresponde aos itens solicitados.");
            }
        }

        List<EstimatedFoodNutritionItemResponseDto> items = new(portions.Count);
        int aiPosition = 0;
        for (int i = 0; i < portions.Count; i++)
        {
            EstimateDetectedFoodPortionDto portion = portions[i];
            string key = Food.NormalizeForLookup(portion.Name);
            string displayName = portion.Name.Trim();
            int grams = portion.EstimatedQuantityGrams;

            if (catalog.TryGetValue(key, out Food? foodHit))
            {
                items.Add(MapFromFood(foodHit, displayName, grams));
                continue;
            }

            if (aiEnvelope?.Foods is null || aiPosition >= aiEnvelope.Foods.Count)
            {
                throw new InvalidOperationException("Não foi possível obter estimativas nutricionais.");
            }

            OpenAiNutritionFoodItem aiItem = aiEnvelope.Foods[aiPosition];
            aiPosition++;

            decimal kcal = ParseDecimal(aiItem.CaloriesKcal);
            decimal protein = ParseDecimal(aiItem.ProteinGrams);
            decimal carbs = ParseDecimal(aiItem.CarbohydratesGrams);
            decimal fat = ParseDecimal(aiItem.FatGrams);

            Food newFood = CreateAiFoodFromPortionEstimate(displayName, grams, kcal, protein, carbs, fat);
            Food persisted = await _foodRepository.AddOrGetActiveAiFoodAsync(newFood, cancellationToken);
            items.Add(MapFromFood(persisted, displayName, grams));
        }

        return new EstimateDetectedFoodsNutritionResponseDto { Foods = items };
    }

    private static Food CreateAiFoodFromPortionEstimate(
        string displayName,
        int portionGrams,
        decimal portionKcal,
        decimal portionProtein,
        decimal portionCarbs,
        decimal portionFat)
    {
        if (portionGrams <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(portionGrams));
        }

        decimal factor = 100m / portionGrams;
        int calories100 = (int)decimal.Round(portionKcal * factor, 0, MidpointRounding.AwayFromZero);
        decimal protein100 = decimal.Round(portionProtein * factor, 2, MidpointRounding.AwayFromZero);
        decimal carbs100 = decimal.Round(portionCarbs * factor, 2, MidpointRounding.AwayFromZero);
        decimal fat100 = decimal.Round(portionFat * factor, 2, MidpointRounding.AwayFromZero);

        return new Food(
            Guid.NewGuid(),
            displayName,
            category: null,
            sourceCode: null,
            FoodSourceType.Ai,
            baseQuantity: 100m,
            UnitType.Gram,
            calories: calories100,
            protein: protein100,
            carbs: carbs100,
            fat: fat100,
            isActive: true);
    }

    private static EstimatedFoodNutritionItemResponseDto MapFromFood(Food food, string displayName, int portionGrams)
    {
        if (food.BaseQuantity <= 0m)
        {
            throw new InvalidOperationException("Alimento do catálogo com quantidade base inválida.");
        }

        decimal mult = portionGrams / food.BaseQuantity;
        decimal kcal = decimal.Round(food.Calories * mult, 2, MidpointRounding.AwayFromZero);
        decimal protein = decimal.Round(food.Protein * mult, 2, MidpointRounding.AwayFromZero);
        decimal carbs = decimal.Round(food.Carbs * mult, 2, MidpointRounding.AwayFromZero);
        decimal fat = decimal.Round(food.Fat * mult, 2, MidpointRounding.AwayFromZero);

        return new EstimatedFoodNutritionItemResponseDto
        {
            FoodId = food.Id,
            Name = displayName,
            EstimatedQuantityGrams = portionGrams,
            CaloriesKcal = kcal,
            ProteinGrams = protein,
            CarbohydratesGrams = carbs,
            FatGrams = fat
        };
    }

    private static string ExtractJsonPayload(string assistantContent)
    {
        string trimmed = assistantContent.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        int firstLineBreak = trimmed.IndexOf('\n');
        int lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstLineBreak < 0 || lastFence <= firstLineBreak)
        {
            return trimmed;
        }

        return trimmed.Substring(firstLineBreak + 1, lastFence - firstLineBreak - 1).Trim();
    }

    private static decimal ParseDecimal(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out decimal direct))
        {
            return decimal.Round(direct, 2, MidpointRounding.AwayFromZero);
        }

        if (element.ValueKind == JsonValueKind.String
            && decimal.TryParse(
                element.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out decimal parsed))
        {
            return decimal.Round(parsed, 2, MidpointRounding.AwayFromZero);
        }

        return 0m;
    }

    private sealed class OpenAiNutritionEnvelope
    {
        [JsonPropertyName("foods")]
        public List<OpenAiNutritionFoodItem>? Foods { get; init; }
    }

    private sealed class OpenAiNutritionFoodItem
    {
        [JsonPropertyName("caloriesKcal")]
        public JsonElement CaloriesKcal { get; init; }

        [JsonPropertyName("proteinGrams")]
        public JsonElement ProteinGrams { get; init; }

        [JsonPropertyName("carbohydratesGrams")]
        public JsonElement CarbohydratesGrams { get; init; }

        [JsonPropertyName("fatGrams")]
        public JsonElement FatGrams { get; init; }
    }
}
