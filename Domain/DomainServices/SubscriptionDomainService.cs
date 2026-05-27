using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class SubscriptionDomainService
{
    public void EnsurePlanCode(string planCode)
    {
        if (string.IsNullOrWhiteSpace(planCode))
        {
            throw new ArgumentException("Código do plano é obrigatório.", nameof(planCode));
        }
    }

    public bool HasPremiumAccess(UserSubscription subscription, SubscriptionPlan plan, DateTime utcNow)
    {
        if (subscription.Status != SubscriptionStatus.Active
            && subscription.Status != SubscriptionStatus.Trialing)
        {
            return false;
        }

        if (plan.Kind == PlanKind.OneTime || plan.Interval == SubscriptionPlanInterval.Lifetime)
        {
            return subscription.Status == SubscriptionStatus.Active;
        }

        return subscription.CurrentPeriodEnd.HasValue && subscription.CurrentPeriodEnd.Value > utcNow;
    }

    public DateTime CalculateMonthlyPeriodEndUtc(DateTime periodStartUtc)
    {
        return periodStartUtc.AddMonths(1);
    }

    public void EnsureCanMarkPastDue(UserSubscription subscription)
    {
        if (subscription.Status != SubscriptionStatus.Active
            && subscription.Status != SubscriptionStatus.Trialing
            && subscription.Status != SubscriptionStatus.PastDue)
        {
            throw new InvalidOperationException("Assinatura não pode ser marcada como inadimplente no status atual.");
        }
    }

    public void EnsureCanActivateFromPayment(UserSubscription subscription)
    {
        if (subscription.Status == SubscriptionStatus.Canceled)
        {
            throw new InvalidOperationException("Assinatura cancelada não pode ser reativada sem novo checkout.");
        }
    }
}
