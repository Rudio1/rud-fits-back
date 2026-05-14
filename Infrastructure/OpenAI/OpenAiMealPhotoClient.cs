using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;

namespace RudFitAI.Infrastructure.OpenAI;

public sealed class OpenAiMealPhotoClient : IMealPhotoVisionClient
{
    private const string UserPrompt = """
    Analise a imagem da refeição e retorne apenas JSON válido.

    Objetivo:
    Identificar os alimentos presentes no prato e estimar a quantidade aproximada em gramas.

    Regras:
    - Retorne SOMENTE JSON válido.
    - Não escreva explicações.
    - Não use markdown.
    - Não use texto antes ou depois do JSON.

    Idioma:
    - Todos os nomes dos alimentos devem estar em português do Brasil (pt-BR).
    - Utilize nomes populares e naturais.
    - Nunca retorne nomes em inglês.
    - Não traduza literalmente nomes incomuns.
    - Use capitalização natural para exibição.

    Estimativa:
    - A quantidade deve ser estimada em gramas.
    - Use valores numéricos inteiros.
    - Considere apenas alimentos visíveis na imagem.

    Formato esperado:
    {
    "foods": [
        {
        "name": "string",
        "estimatedQuantityGrams": 0
        }
    ]
    }
    """;

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiMealPhotoClient(IHttpClientFactory httpClientFactory, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAi");
        _options = options.Value;
    }

    public async Task<string> GetMealAnalysisJsonAsync(
        byte[] imageBytes,
        string imageMimeType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI não está configurada. Defina OpenAI:ApiKey.");
        }

        string dataUrl = $"data:{imageMimeType};base64,{Convert.ToBase64String(imageBytes)}";
        int maxTokens = _options.MaxCompletionTokens <= 0 ? 512 : _options.MaxCompletionTokens;
        maxTokens = Math.Clamp(maxTokens, 64, 4096);
        Dictionary<string, object> requestDict = new()
        {
            ["model"] = _options.Model,
            ["max_tokens"] = maxTokens,
            ["messages"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = new List<object>
                    {
                        new Dictionary<string, object> { ["type"] = "text", ["text"] = UserPrompt },
                        new Dictionary<string, object>
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new Dictionary<string, object> { ["url"] = dataUrl }
                        }
                    }
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
            throw new InvalidOperationException("A OpenAI não retornou conteúdo para a imagem.");
        }

        return content;
    }
}
