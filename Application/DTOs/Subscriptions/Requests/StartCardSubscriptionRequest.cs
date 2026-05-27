namespace RudFitAI.Application.DTOs.Subscriptions.Requests;

public sealed class StartCardSubscriptionRequest
{
    public string PlanCode { get; init; } = SubscriptionPlanCodes.PremiumMonthly;

    public string HolderName { get; init; } = string.Empty;

    public string CardNumber { get; init; } = string.Empty;

    public string ExpiryMonth { get; init; } = string.Empty;

    public string ExpiryYear { get; init; } = string.Empty;

    public string Ccv { get; init; } = string.Empty;

    public string HolderEmail { get; init; } = string.Empty;

    public string HolderCpfCnpj { get; init; } = string.Empty;

    public string HolderPostalCode { get; init; } = string.Empty;

    public string HolderAddressNumber { get; init; } = string.Empty;

    public string HolderPhone { get; init; } = string.Empty;
}
