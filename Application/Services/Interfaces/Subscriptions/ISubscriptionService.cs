using RudFitAI.Application.DTOs.Subscriptions.Responses;

namespace RudFitAI.Application.Services.Interfaces.Subscriptions;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanResponseDto>> GetActivePlansAsync(CancellationToken cancellationToken);

    Task<SubscriptionStatusResponseDto> GetStatusForUserAsync(Guid userId, CancellationToken cancellationToken);
}
