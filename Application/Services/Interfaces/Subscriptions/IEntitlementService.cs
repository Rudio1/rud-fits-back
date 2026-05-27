namespace RudFitAI.Application.Services.Interfaces.Subscriptions;

public interface IEntitlementService
{
    Task<bool> HasPremiumAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> TryConsumeFreeScannerUseAsync(Guid userId, CancellationToken cancellationToken);
}
