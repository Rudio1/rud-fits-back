namespace RudFitAI.Application.DTOs.Subscriptions.Responses;

public sealed class SubscriptionPlanResponseDto
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int PriceCents { get; init; }

    public string Interval { get; init; } = string.Empty;

    public string Kind { get; init; } = string.Empty;
}
