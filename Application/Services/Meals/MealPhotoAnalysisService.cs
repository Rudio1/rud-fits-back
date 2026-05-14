using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Meals;

namespace RudFitAI.Application.Services.Meals;

public sealed class MealPhotoAnalysisService : IMealPhotoAnalysisService
{
    private static readonly CultureInfo PtBrCulture = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IMealPhotoVisionClient _visionClient;
    private readonly OpenAiOptions _openAiOptions;

    public MealPhotoAnalysisService(
        IMealPhotoVisionClient visionClient,
        IOptions<OpenAiOptions> openAiOptions)
    {
        _visionClient = visionClient;
        _openAiOptions = openAiOptions.Value;
    }

    public async Task<MealPhotoAnalysisResponseDto> AnalyzePhotoAsync(
        byte[] imageBytes,
        string imageMimeType,
        CancellationToken cancellationToken)
    {
        int timeoutSeconds = _openAiOptions.RequestTimeoutSeconds <= 0 ? 10 : _openAiOptions.RequestTimeoutSeconds;
        timeoutSeconds = Math.Clamp(timeoutSeconds, 1, 120);
        using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(timeoutSeconds));
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        string rawContent = await _visionClient.GetMealAnalysisJsonAsync(
            imageBytes,
            imageMimeType,
            linked.Token);

        string jsonPayload = ExtractJsonPayload(rawContent);
        OpenAiFoodsEnvelope? envelope = JsonSerializer.Deserialize<OpenAiFoodsEnvelope>(jsonPayload, JsonOptions);
        if (envelope?.Foods is null || envelope.Foods.Count == 0)
        {
            throw new InvalidOperationException("Não foi possível identificar alimentos na imagem.");
        }

        List<MealPhotoAnalysisFoodItemDto> items = new();
        foreach (OpenAiFoodItem item in envelope.Foods)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                continue;
            }

            int grams = NormalizeGrams(item.EstimatedQuantityGrams);
            if (grams <= 0)
            {
                continue;
            }

            items.Add(
                new MealPhotoAnalysisFoodItemDto
                {
                    Name = FormatFoodDisplayName(item.Name),
                    EstimatedQuantityGrams = grams
                });
        }

        if (items.Count == 0)
        {
            throw new InvalidOperationException("Não foi possível identificar alimentos na imagem.");
        }

        return new MealPhotoAnalysisResponseDto { Foods = items };
    }

    private static string FormatFoodDisplayName(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return PtBrCulture.TextInfo.ToTitleCase(trimmed.ToLower(PtBrCulture));
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

    private static int NormalizeGrams(JsonElement gramsElement)
    {
        if (gramsElement.ValueKind == JsonValueKind.Number && gramsElement.TryGetInt32(out int intValue))
        {
            return intValue;
        }

        if (gramsElement.ValueKind == JsonValueKind.Number && gramsElement.TryGetDouble(out double doubleValue))
        {
            return (int)Math.Round(doubleValue, MidpointRounding.AwayFromZero);
        }

        if (gramsElement.ValueKind == JsonValueKind.String
            && double.TryParse(
                gramsElement.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double parsed))
        {
            return (int)Math.Round(parsed, MidpointRounding.AwayFromZero);
        }

        return 0;
    }

    private sealed class OpenAiFoodsEnvelope
    {
        [JsonPropertyName("foods")]
        public List<OpenAiFoodItem>? Foods { get; init; }
    }

    private sealed class OpenAiFoodItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("estimatedQuantityGrams")]
        public JsonElement EstimatedQuantityGrams { get; init; }
    }
}
