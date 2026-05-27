using System.Globalization;
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

    public Task<string> CreateCustomerAsync(
        string name,
        string email,
        string? cpfCnpj,
        CancellationToken cancellationToken)
    {
        Dictionary<string, object?> body = new()
        {
            ["name"] = name,
            ["email"] = email
        };

        if (!string.IsNullOrWhiteSpace(cpfCnpj))
        {
            body["cpfCnpj"] = cpfCnpj;
        }

        return PostAndReadIdAsync("customers", body, cancellationToken);
    }

    public async Task UpdateCustomerCpfCnpjAsync(
        string customerId,
        string cpfCnpj,
        CancellationToken cancellationToken)
    {
        Dictionary<string, object?> body = new()
        {
            ["cpfCnpj"] = cpfCnpj
        };

        await PutAsync($"customers/{customerId}", body, cancellationToken);
    }

    public async Task<AsaasCheckoutResult> CreateCreditCardSubscriptionAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly nextDueDate,
        AsaasCreditCardDto creditCard,
        AsaasCreditCardHolderInfoDto holderInfo,
        CancellationToken cancellationToken)
    {
        Dictionary<string, object?> body = BuildSubscriptionBody(customerId, value, description, nextDueDate, "CREDIT_CARD");
        body["creditCard"] = new Dictionary<string, object?>
        {
            ["holderName"] = creditCard.HolderName,
            ["number"] = creditCard.Number,
            ["expiryMonth"] = creditCard.ExpiryMonth,
            ["expiryYear"] = creditCard.ExpiryYear,
            ["ccv"] = creditCard.Ccv
        };
        body["creditCardHolderInfo"] = BuildHolderInfoBody(holderInfo);

        using JsonDocument document = await PostAsync("subscriptions", body, cancellationToken);
        return MapSubscriptionCheckout(document.RootElement);
    }

    public async Task<AsaasCheckoutResult> CreatePixSubscriptionAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly nextDueDate,
        CancellationToken cancellationToken)
    {
        Dictionary<string, object?> body = BuildSubscriptionBody(customerId, value, description, nextDueDate, "PIX");
        using JsonDocument document = await PostAsync("subscriptions", body, cancellationToken);
        AsaasCheckoutResult result = MapSubscriptionCheckout(document.RootElement);

        if (string.IsNullOrWhiteSpace(result.SubscriptionId))
        {
            throw new InvalidOperationException("Resposta do Asaas sem id de assinatura.");
        }

        string? paymentId = ReadFirstPaymentId(document.RootElement);
        if (string.IsNullOrWhiteSpace(paymentId))
        {
            paymentId = await GetFirstSubscriptionPaymentIdAsync(result.SubscriptionId, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(paymentId))
        {
            throw new InvalidOperationException(
                "Cobrança PIX da assinatura ainda não está disponível no Asaas. Tente novamente em instantes.");
        }

        AsaasCheckoutResult pixDetails = await GetPixQrCodeAsync(paymentId, cancellationToken);
        return MergePix(result, pixDetails);
    }

    public async Task<string?> GetFirstSubscriptionPaymentIdAsync(
        string subscriptionId,
        CancellationToken cancellationToken)
    {
        using JsonDocument document = await GetAsync($"subscriptions/{subscriptionId}/payments", cancellationToken);
        return ReadFirstPixPaymentIdFromData(document.RootElement);
    }

    public async Task<AsaasCheckoutResult> CreatePaymentAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly dueDate,
        string billingType,
        AsaasCreditCardDto? creditCard,
        AsaasCreditCardHolderInfoDto? holderInfo,
        CancellationToken cancellationToken)
    {
        Dictionary<string, object?> body = new()
        {
            ["customer"] = customerId,
            ["billingType"] = billingType.ToUpperInvariant(),
            ["value"] = value,
            ["dueDate"] = dueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["description"] = description
        };

        if (string.Equals(billingType, "CREDIT_CARD", StringComparison.OrdinalIgnoreCase)
            && creditCard is not null
            && holderInfo is not null)
        {
            body["creditCard"] = new Dictionary<string, object?>
            {
                ["holderName"] = creditCard.HolderName,
                ["number"] = creditCard.Number,
                ["expiryMonth"] = creditCard.ExpiryMonth,
                ["expiryYear"] = creditCard.ExpiryYear,
                ["ccv"] = creditCard.Ccv
            };
            body["creditCardHolderInfo"] = BuildHolderInfoBody(holderInfo);
        }

        using JsonDocument document = await PostAsync("payments", body, cancellationToken);
        AsaasCheckoutResult result = MapPaymentCheckout(document.RootElement);

        if (string.Equals(billingType, "PIX", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(result.PaymentId))
        {
            AsaasCheckoutResult pixDetails = await GetPixQrCodeAsync(result.PaymentId, cancellationToken);
            return MergePix(result, pixDetails);
        }

        return result;
    }

    public async Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using HttpRequestMessage request = new(HttpMethod.Delete, $"subscriptions/{subscriptionId}");
        request.Headers.Add("access_token", _options.ApiKey);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao cancelar assinatura no Asaas: {responseBody}");
        }
    }

    private async Task<AsaasCheckoutResult> GetPixQrCodeAsync(string paymentId, CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using HttpRequestMessage request = new(HttpMethod.Get, $"payments/{paymentId}/pixQrCode");
        request.Headers.Add("access_token", _options.ApiKey);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha ao obter QR Code Pix no Asaas: {responseBody}");
        }

        using JsonDocument document = JsonDocument.Parse(responseBody);
        JsonElement root = document.RootElement;

        return new AsaasCheckoutResult
        {
            PaymentId = paymentId,
            PixQrCodeBase64 = ReadString(root, "encodedImage"),
            PixCopiaECola = ReadString(root, "payload")
        };
    }

    private static Dictionary<string, object?> BuildSubscriptionBody(
        string customerId,
        decimal value,
        string description,
        DateOnly nextDueDate,
        string billingType)
    {
        return new Dictionary<string, object?>
        {
            ["customer"] = customerId,
            ["billingType"] = billingType,
            ["value"] = value,
            ["nextDueDate"] = nextDueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["cycle"] = "MONTHLY",
            ["description"] = description
        };
    }

    private static Dictionary<string, object?> BuildHolderInfoBody(AsaasCreditCardHolderInfoDto holderInfo)
    {
        return new Dictionary<string, object?>
        {
            ["name"] = holderInfo.Name,
            ["email"] = holderInfo.Email,
            ["cpfCnpj"] = holderInfo.CpfCnpj,
            ["postalCode"] = holderInfo.PostalCode,
            ["addressNumber"] = holderInfo.AddressNumber,
            ["phone"] = holderInfo.Phone
        };
    }

    private async Task<string> PostAndReadIdAsync(
        string path,
        Dictionary<string, object?> body,
        CancellationToken cancellationToken)
    {
        using JsonDocument document = await PostAsync(path, body, cancellationToken);
        string? id = ReadString(document.RootElement, "id");
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException($"Resposta do Asaas sem id em {path}.");
        }

        return id;
    }

    private async Task<JsonDocument> GetAsync(string path, CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Add("access_token", _options.ApiKey);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha na chamada Asaas ({path}): {responseBody}");
        }

        return JsonDocument.Parse(responseBody);
    }

    private async Task<JsonDocument> PostAsync(
        string path,
        Dictionary<string, object?> body,
        CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using HttpRequestMessage request = new(HttpMethod.Post, path);
        request.Headers.Add("access_token", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha na chamada Asaas ({path}): {responseBody}");
        }

        return JsonDocument.Parse(responseBody);
    }

    private async Task PutAsync(
        string path,
        Dictionary<string, object?> body,
        CancellationToken cancellationToken)
    {
        EnsureApiKey();

        using HttpRequestMessage request = new(HttpMethod.Put, path);
        request.Headers.Add("access_token", _options.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Falha na chamada Asaas ({path}): {responseBody}");
        }
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Asaas API key não configurada.");
        }
    }

    private static AsaasCheckoutResult MapSubscriptionCheckout(JsonElement root)
    {
        return new AsaasCheckoutResult
        {
            SubscriptionId = ReadString(root, "id"),
            Status = ReadString(root, "status"),
            InvoiceUrl = ReadString(root, "invoiceUrl")
        };
    }

    private static AsaasCheckoutResult MapPaymentCheckout(JsonElement root)
    {
        return new AsaasCheckoutResult
        {
            PaymentId = ReadString(root, "id"),
            Status = ReadString(root, "status"),
            InvoiceUrl = ReadString(root, "invoiceUrl")
        };
    }

    private static AsaasCheckoutResult MergePix(AsaasCheckoutResult baseResult, AsaasCheckoutResult pix)
    {
        return new AsaasCheckoutResult
        {
            SubscriptionId = baseResult.SubscriptionId,
            PaymentId = pix.PaymentId ?? baseResult.PaymentId,
            Status = baseResult.Status,
            InvoiceUrl = baseResult.InvoiceUrl,
            PixQrCodeBase64 = pix.PixQrCodeBase64,
            PixCopiaECola = pix.PixCopiaECola
        };
    }

    private static string? ReadFirstPaymentId(JsonElement root)
    {
        if (root.TryGetProperty("payments", out JsonElement payments)
            && payments.ValueKind == JsonValueKind.Array)
        {
            return ReadFirstPixPaymentIdFromArray(payments);
        }

        return ReadFirstPixPaymentIdFromData(root);
    }

    private static string? ReadFirstPixPaymentIdFromData(JsonElement root)
    {
        if (!root.TryGetProperty("data", out JsonElement data)
            || data.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return ReadFirstPixPaymentIdFromArray(data);
    }

    private static string? ReadFirstPixPaymentIdFromArray(JsonElement payments)
    {
        string? fallbackId = null;

        foreach (JsonElement payment in payments.EnumerateArray())
        {
            string? id = ReadString(payment, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            string? billingType = ReadString(payment, "billingType");
            if (string.Equals(billingType, "PIX", StringComparison.OrdinalIgnoreCase))
            {
                return id;
            }

            fallbackId ??= id;
        }

        return fallbackId;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }
}
