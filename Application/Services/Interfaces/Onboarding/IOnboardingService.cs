using RudFitAI.Application.DTOs.Onboarding.Requests;
using RudFitAI.Application.DTOs.Onboarding.Responses;

namespace RudFitAI.Application.Services.Interfaces.Onboarding;

public interface IOnboardingService
{
    Task<CompleteOnboardingResponseDto?> CompleteAsync(
        Guid userId,
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken);

    Task<CalculateDailyGoalsResponseDto?> CalculateDailyGoalsAsync(Guid userId, CancellationToken cancellationToken);

    Task<CalculateDailyGoalsResponseDto?> GetDailyGoalsAsync(Guid userId, CancellationToken cancellationToken);
}
