using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class SubscriptionDomainService
{
    public const int FreeScannerLifetimeLimit = 15;

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
        if (subscription.Status == SubscriptionStatus.Canceled
            || subscription.Status == SubscriptionStatus.Expired)
        {
            throw new InvalidOperationException("Assinatura encerrada não pode ser reativada sem novo checkout.");
        }
    }

    public void EnsureRecurringPlan(SubscriptionPlan plan)
    {
        if (plan.Kind != PlanKind.Recurring)
        {
            throw new InvalidOperationException("Este fluxo é válido apenas para plano recorrente mensal.");
        }
    }

    public void EnsureOneTimePlan(SubscriptionPlan plan)
    {
        if (plan.Kind != PlanKind.OneTime)
        {
            throw new InvalidOperationException("Este fluxo é válido apenas para plano permanente.");
        }
    }

    public bool CanUseFreeScanner(UserProfile? profile)
    {
        if (profile is null)
        {
            return false;
        }

        return profile.CanUseFreeScanner(FreeScannerLifetimeLimit);
    }

    public string NormalizeCpfCnpj(string cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            throw new ArgumentException("CPF ou CNPJ é obrigatório para pagamento via PIX.");
        }

        string digits = new string(cpfCnpj.Where(char.IsDigit).ToArray());
        if (digits.Length is not 11 and not 14)
        {
            throw new ArgumentException("Informe um CPF (11 dígitos) ou CNPJ (14 dígitos) válido.");
        }

        return digits;
    }
}
