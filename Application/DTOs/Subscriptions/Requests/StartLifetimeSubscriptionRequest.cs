namespace RudFitAI.Application.DTOs.Subscriptions.Requests;

public sealed class StartLifetimeSubscriptionRequest
{
    public string BillingType { get; init; } = "PIX";

    public string? HolderName { get; init; }

    public string? CardNumber { get; init; }

    public string? ExpiryMonth { get; init; }

    public string? ExpiryYear { get; init; }

    public string? Ccv { get; init; }

    public string? HolderEmail { get; init; }

    public string? HolderCpfCnpj { get; init; }

    public string? HolderPostalCode { get; init; }

    public string? HolderAddressNumber { get; init; }

    public string? HolderPhone { get; init; }
}
