using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;

namespace RudFitAI.Infrastructure.OpenAI;

public sealed class OpenAiMealNutritionEstimationClient : IMealNutritionEstimationChatClient
{
    private const string UserPrompt = """
    You estimate nutritional values for foods based on the provided portion size.

    Input JSON:
    {
    "foods": [
        {
        "name": "string",
        "estimatedQuantityGrams": number
        }
    ]
    }

    Rules:
    - Return ONLY valid JSON.
    - Do not use markdown.
    - Do not include explanations or extra text.
    - Preserve the exact same array order.
    - Echo "name" and "estimatedQuantityGrams" exactly as received.
    - Nutritional values must represent the FULL portion informed in "estimatedQuantityGrams".
    - Values are approximate estimates.
    - Use 0 when a macro is negligible.

    Return format:
    {
    "foods": [
        {
        "name": "string",
        "estimatedQuantityGrams": number,
        "caloriesKcal": number,
        "proteinGrams": number,
        "carbohydratesGrams": number,
        "fatGrams": number
        }
    ]
    }

    Input:
    """;

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiMealNutritionEstimationClient(IHttpClientFactory httpClientFactory, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAi");
        _options = options.Value;
    }

    public async Task<string> GetNutritionEstimatesJsonAsync(
        string foodsInputJson,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI não está configurada. Defina OpenAI:ApiKey.");
        }

        if (string.IsNullOrWhiteSpace(foodsInputJson))
        {
            throw new ArgumentException("foodsInputJson is required.", nameof(foodsInputJson));
        }

        int maxTokens = _options.MaxCompletionTokens <= 0 ? 512 : _options.MaxCompletionTokens;
        maxTokens = Math.Clamp(maxTokens, 64, 4096);
        string userText = UserPrompt + foodsInputJson;
        Dictionary<string, object> requestDict = new()
        {
            ["model"] = _options.Model,
            ["max_tokens"] = maxTokens,
            ["messages"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = userText
                }
            }
        };

        string json = JsonSerializer.Serialize(requestDict);
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(_options.ChatCompletionsUrl, UriKind.Absolute));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Falha ao chamar a OpenAI ({(int)response.StatusCode}). Tente novamente mais tarde.");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("choices", out JsonElement choices) || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Resposta da OpenAI em formato inesperado.");
        }

        JsonElement message = choices[0].GetProperty("message");
        string? content = message.GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("A OpenAI não retornou conteúdo para a estimativa nutricional.");
        }

        return content;
    }
}
