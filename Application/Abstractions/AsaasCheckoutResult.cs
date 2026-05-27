namespace RudFitAI.Application.Abstractions;

public sealed class AsaasCheckoutResult
{
    public string? SubscriptionId { get; init; }

    public string? PaymentId { get; init; }

    public string? InvoiceUrl { get; init; }

    public string? PixQrCodeBase64 { get; init; }

    public string? PixCopiaECola { get; init; }

    public string? Status { get; init; }
}
