namespace RudFitAI.Application.DTOs.Onboarding.Responses;

public sealed class CompleteOnboardingResponseDto
{
    public required bool Completed { get; init; }

    public required bool IsFirstAccess { get; init; }
}
