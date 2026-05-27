using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class SubscriptionPlan : BaseEntity
{
    public const string PremiumMonthlyCode = "premium_monthly";
    public const string PremiumLifetimeCode = "premium_lifetime";

    private SubscriptionPlan()
    {
    }

    public SubscriptionPlan(
        Guid id,
        string code,
        string name,
        int priceCents,
        SubscriptionPlanInterval interval,
        PlanKind kind,
        bool isActive)
        : this()
    {
        Id = id;
        Code = code;
        Name = name;
        PriceCents = priceCents;
        Interval = interval;
        Kind = kind;
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int PriceCents { get; private set; }

    public SubscriptionPlanInterval Interval { get; private set; }

    public PlanKind Kind { get; private set; }

    public bool IsActive { get; private set; }
}
