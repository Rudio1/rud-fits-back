namespace RudFitAI.Application.DTOs.Subscriptions.Responses;

public sealed class StartCheckoutResponseDto
{
    public Guid UserSubscriptionId { get; init; }

    public string PlanCode { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string? AsaasSubscriptionId { get; init; }

    public string? AsaasPaymentId { get; init; }

    public string? InvoiceUrl { get; init; }

    public string? PixQrCodeBase64 { get; init; }

    public string? PixCopiaECola { get; init; }
}
