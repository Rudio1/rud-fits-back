namespace RudFitAI.Application.DTOs.Onboarding.Responses;

public sealed class CalculateDailyGoalsResponseDto
{
    public required int DailyCaloriesGoal { get; init; }

    public required int DailyProteinGoal { get; init; }

    public required int DailyCarbsGoal { get; init; }

    public required int DailyFatGoal { get; init; }
}
