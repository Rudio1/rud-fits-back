using RudFitAI.Application.DTOs.Subscriptions.Responses;
using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Subscriptions;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IEntitlementService _entitlementService;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IEntitlementService entitlementService)
    {
        _subscriptionRepository = subscriptionRepository;
        _entitlementService = entitlementService;
    }

    public async Task<IReadOnlyList<SubscriptionPlanResponseDto>> GetActivePlansAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<SubscriptionPlan> plans =
            await _subscriptionRepository.GetActivePlansAsync(cancellationToken);

        List<SubscriptionPlanResponseDto> result = new(plans.Count);
        foreach (SubscriptionPlan plan in plans)
        {
            result.Add(MapPlan(plan));
        }

        return result;
    }

    public async Task<SubscriptionStatusResponseDto> GetStatusForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        bool hasPremium = await _entitlementService.HasPremiumAsync(userId, cancellationToken);

        UserSubscription? subscription =
            await _subscriptionRepository.GetCurrentUserSubscriptionByUserIdAsync(userId, cancellationToken);

        if (subscription is null)
        {
            return new SubscriptionStatusResponseDto
            {
                HasPremium = false,
                Status = SubscriptionStatus.None.ToString()
            };
        }

        return new SubscriptionStatusResponseDto
        {
            HasPremium = hasPremium,
            Status = subscription.Status.ToString(),
            PlanCode = subscription.SubscriptionPlan.Code,
            PlanName = subscription.SubscriptionPlan.Name,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            BillingType = subscription.BillingType == BillingType.None
                ? null
                : subscription.BillingType.ToString()
        };
    }

    private static SubscriptionPlanResponseDto MapPlan(SubscriptionPlan plan)
    {
        return new SubscriptionPlanResponseDto
        {
            Code = plan.Code,
            Name = plan.Name,
            PriceCents = plan.PriceCents,
            Interval = plan.Interval.ToString(),
            Kind = plan.Kind.ToString()
        };
    }
}
