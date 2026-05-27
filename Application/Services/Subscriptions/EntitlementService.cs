using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Subscriptions;

public sealed class EntitlementService : IEntitlementService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly SubscriptionDomainService _subscriptionDomainService;

    public EntitlementService(
        ISubscriptionRepository subscriptionRepository,
        SubscriptionDomainService subscriptionDomainService)
    {
        _subscriptionRepository = subscriptionRepository;
        _subscriptionDomainService = subscriptionDomainService;
    }

    public async Task<bool> HasPremiumAsync(Guid userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<UserSubscription> subscriptions =
            await _subscriptionRepository.GetUserSubscriptionsByUserIdAsync(userId, cancellationToken);

        DateTime utcNow = DateTime.UtcNow;
        foreach (UserSubscription subscription in subscriptions)
        {
            if (_subscriptionDomainService.HasPremiumAccess(
                    subscription,
                    subscription.SubscriptionPlan,
                    utcNow))
            {
                return true;
            }
        }

        return false;
    }
}
