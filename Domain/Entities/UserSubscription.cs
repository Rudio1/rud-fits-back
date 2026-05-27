using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class UserSubscription : BaseEntity
{
    private UserSubscription()
    {
    }

    public UserSubscription(Guid id, Guid userId, Guid subscriptionPlanId)
        : this()
    {
        Id = id;
        UserId = userId;
        SubscriptionPlanId = subscriptionPlanId;
        Status = SubscriptionStatus.None;
        BillingType = BillingType.None;
    }

    public Guid UserId { get; private set; }

    public Guid SubscriptionPlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public BillingType BillingType { get; private set; }

    public string? AsaasCustomerId { get; private set; }

    public string? AsaasSubscriptionId { get; private set; }

    public string? AsaasPaymentId { get; private set; }

    public string? AsaasPixAuthorizationId { get; private set; }

    public DateTime? CurrentPeriodStart { get; private set; }

    public DateTime? CurrentPeriodEnd { get; private set; }

    public DateTime? CanceledAt { get; private set; }

    public User User { get; private set; } = null!;

    public SubscriptionPlan SubscriptionPlan { get; private set; } = null!;

    public void SetAsaasCustomerId(string asaasCustomerId)
    {
        AsaasCustomerId = asaasCustomerId;
    }

    public void SetAsaasSubscriptionId(string asaasSubscriptionId)
    {
        AsaasSubscriptionId = asaasSubscriptionId;
    }

    public void SetAsaasPaymentId(string asaasPaymentId)
    {
        AsaasPaymentId = asaasPaymentId;
    }

    public void SetAsaasPixAuthorizationId(string asaasPixAuthorizationId)
    {
        AsaasPixAuthorizationId = asaasPixAuthorizationId;
    }

    public void SetBillingType(BillingType billingType)
    {
        BillingType = billingType;
    }

    public void Activate(
        BillingType billingType,
        DateTime? periodStartUtc,
        DateTime? periodEndUtc)
    {
        BillingType = billingType;
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = periodStartUtc;
        CurrentPeriodEnd = periodEndUtc;
        CanceledAt = null;
    }

    public void MarkPastDue()
    {
        Status = SubscriptionStatus.PastDue;
    }

    public void Cancel(DateTime canceledAtUtc)
    {
        Status = SubscriptionStatus.Canceled;
        CanceledAt = canceledAtUtc;
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
    }

    public void ChangePlan(Guid subscriptionPlanId)
    {
        SubscriptionPlanId = subscriptionPlanId;
    }

    public void SetPendingCheckout(
        BillingType billingType,
        string? asaasCustomerId = null,
        string? asaasSubscriptionId = null,
        string? asaasPaymentId = null)
    {
        BillingType = billingType;
        Status = SubscriptionStatus.Pending;
        if (!string.IsNullOrWhiteSpace(asaasCustomerId))
        {
            AsaasCustomerId = asaasCustomerId;
        }

        if (!string.IsNullOrWhiteSpace(asaasSubscriptionId))
        {
            AsaasSubscriptionId = asaasSubscriptionId;
        }

        if (!string.IsNullOrWhiteSpace(asaasPaymentId))
        {
            AsaasPaymentId = asaasPaymentId;
        }
    }
}
