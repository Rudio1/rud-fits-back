using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Onboarding.Requests;

public sealed class CompleteOnboardingRequest
{
    public GoalType Goal { get; init; }

    public GenderType Gender { get; init; }

    public int Age { get; init; }

    public decimal Height { get; init; }

    public decimal Weight { get; init; }

    public decimal StartingWeight { get; init; }

    public decimal TargetWeight { get; init; }

    public ActivityLevelType ActivityLevel { get; init; }

    public int DailyRoutineLevel { get; init; }

    public int GoalIntensity { get; init; }
}
