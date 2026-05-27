using Microsoft.EntityFrameworkCore;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly RudFitAIDbContext _dbContext;

    public SubscriptionRepository(RudFitAIDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .Where(plan => plan.IsActive)
            .OrderBy(plan => plan.PriceCents)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(plan => plan.Code == code && plan.IsActive, cancellationToken);
    }

    public async Task<UserSubscription?> GetCurrentUserSubscriptionByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        List<UserSubscription> subscriptions = await _dbContext.UserSubscriptions
            .Include(subscription => subscription.SubscriptionPlan)
            .Where(subscription => subscription.UserId == userId)
            .ToListAsync(cancellationToken);

        return SelectCurrentSubscription(subscriptions);
    }

    public async Task<IReadOnlyList<UserSubscription>> GetUserSubscriptionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserSubscriptions
            .Include(subscription => subscription.SubscriptionPlan)
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSubscription?> GetUserSubscriptionByAsaasCustomerIdAsync(
        string asaasCustomerId,
        CancellationToken cancellationToken)
    {
        List<UserSubscription> subscriptions = await _dbContext.UserSubscriptions
            .Include(subscription => subscription.SubscriptionPlan)
            .Where(subscription => subscription.AsaasCustomerId == asaasCustomerId)
            .ToListAsync(cancellationToken);

        return SelectCurrentSubscription(subscriptions);
    }

    public async Task<UserSubscription?> GetUserSubscriptionByAsaasSubscriptionIdAsync(
        string asaasSubscriptionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserSubscriptions
            .Include(subscription => subscription.SubscriptionPlan)
            .Where(subscription => subscription.AsaasSubscriptionId == asaasSubscriptionId)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserSubscription?> GetUserSubscriptionByAsaasPaymentIdAsync(
        string asaasPaymentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserSubscriptions
            .Include(subscription => subscription.SubscriptionPlan)
            .Where(subscription => subscription.AsaasPaymentId == asaasPaymentId)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> PaymentEventExistsAsync(string externalEventId, CancellationToken cancellationToken)
    {
        return await _dbContext.PaymentEvents
            .AnyAsync(paymentEvent => paymentEvent.ExternalEventId == externalEventId, cancellationToken);
    }

    public async Task AddPaymentEventAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)
    {
        await _dbContext.PaymentEvents.AddAsync(paymentEvent, cancellationToken);
    }

    public async Task AddUserSubscriptionAsync(UserSubscription userSubscription, CancellationToken cancellationToken)
    {
        await _dbContext.UserSubscriptions.AddAsync(userSubscription, cancellationToken);
    }

    public async Task<string?> GetLatestAsaasCustomerIdByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserSubscriptions
            .AsNoTracking()
            .Where(subscription => subscription.UserId == userId && subscription.AsaasCustomerId != null)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .Select(subscription => subscription.AsaasCustomerId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserSubscription? SelectCurrentSubscription(IReadOnlyList<UserSubscription> subscriptions)
    {
        if (subscriptions.Count == 0)
        {
            return null;
        }

        return subscriptions
            .OrderBy(subscription => GetStatusPriority(subscription.Status))
            .ThenByDescending(subscription => subscription.CreatedAt)
            .First();
    }

    private static int GetStatusPriority(SubscriptionStatus status)
    {
        return status switch
        {
            SubscriptionStatus.Active => 0,
            SubscriptionStatus.Trialing => 1,
            SubscriptionStatus.Pending => 2,
            SubscriptionStatus.PastDue => 3,
            SubscriptionStatus.Canceled => 4,
            SubscriptionStatus.Expired => 5,
            _ => 6
        };
    }
}
