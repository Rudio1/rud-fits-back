namespace RudFitAI.Application.DTOs.Subscriptions.Requests;

public sealed class StartPixSubscriptionRequest
{
    public string PlanCode { get; init; } = SubscriptionPlanCodes.PremiumMonthly;

    public string CpfCnpj { get; init; } = string.Empty;
}
