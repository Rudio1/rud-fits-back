using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;

namespace RudFitAI.Infrastructure.Asaas;

public sealed class AsaasClient : IAsaasClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly AsaasOptions _options;

    public AsaasClient(HttpClient httpClient, IOptions<AsaasOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> CreateCustomerAsync(
        string name,
        string email,
        string? cpfCnpj,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Asaas API key não configurada.");
        }

        Dictionary<string, object?> body = new()
        {
            ["name"] = name,
            ["email"] = email
        };

        if (!string.IsNullOrWhiteSpace(cpfCnpj))
        {
            body["cpfCnpj"] = cpfCnpj;
        }

        using HttpRequestMessage request = new(HttpMethod.Post, "customers");
        request.Headers.Add("access_token", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao criar cliente no Asaas: {responseBody}");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        string? customerId = document.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new InvalidOperationException("Resposta do Asaas sem id de cliente.");
        }

        return customerId;
    }
}
