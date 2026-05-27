namespace RudFitAI.Application.Options;

public sealed class AsaasOptions
{
    public const string SectionName = "Asaas";

    public string BaseUrl { get; init; } = "https://sandbox.asaas.com/api/v3";

    public string ApiKey { get; init; } = string.Empty;

    public string WebhookAccessToken { get; init; } = string.Empty;

    public int DefaultDueDays { get; init; } = 3;

    /// <summary>
    /// Obrigatório pela API Asaas (ex.: "RudFitAI/1.0 (seu@email.com)").
    /// </summary>
    public string UserAgent { get; init; } = "RudFitAI/1.0";
}
