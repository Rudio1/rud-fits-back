using RudFitAI.Application.DTOs.Subscriptions.Requests;
using RudFitAI.Application.DTOs.Subscriptions.Responses;

namespace RudFitAI.Application.Services.Interfaces.Subscriptions;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanResponseDto>> GetActivePlansAsync(CancellationToken cancellationToken);

    Task<SubscriptionStatusResponseDto> GetStatusForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<StartCheckoutResponseDto> StartCardSubscriptionAsync(
        Guid userId,
        StartCardSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task<StartCheckoutResponseDto> StartPixSubscriptionAsync(
        Guid userId,
        StartPixSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task<StartCheckoutResponseDto> StartLifetimeSubscriptionAsync(
        Guid userId,
        StartLifetimeSubscriptionRequest request,
        CancellationToken cancellationToken);

    Task CancelCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken);
}
