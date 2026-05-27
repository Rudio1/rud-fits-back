namespace RudFitAI.Application.DTOs.Subscriptions.Responses;

public sealed class SubscriptionStatusResponseDto
{
    public bool HasPremium { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? PlanCode { get; init; }

    public string? PlanName { get; init; }

    public DateTime? CurrentPeriodEnd { get; init; }

    public string? BillingType { get; init; }

    public int? FreeScannerUsesCount { get; init; }

    public int? FreeScannerUsesRemaining { get; init; }
}
