using RudFitAI.Domain.Entities;

namespace RudFitAI.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken cancellationToken);

    Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken);

    Task<UserSubscription?> GetCurrentUserSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<UserSubscription?> GetUserSubscriptionByAsaasCustomerIdAsync(
        string asaasCustomerId,
        CancellationToken cancellationToken);

    Task<UserSubscription?> GetUserSubscriptionByAsaasSubscriptionIdAsync(
        string asaasSubscriptionId,
        CancellationToken cancellationToken);

    Task<UserSubscription?> GetUserSubscriptionByAsaasPaymentIdAsync(
        string asaasPaymentId,
        CancellationToken cancellationToken);

    Task<bool> PaymentEventExistsAsync(string externalEventId, CancellationToken cancellationToken);

    Task AddPaymentEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken);

    Task AddUserSubscriptionAsync(UserSubscription userSubscription, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
