namespace RudFitAI.Application.Services.Interfaces.Subscriptions;

public interface IEntitlementService
{
    Task<bool> HasPremiumAsync(Guid userId, CancellationToken cancellationToken);
}
