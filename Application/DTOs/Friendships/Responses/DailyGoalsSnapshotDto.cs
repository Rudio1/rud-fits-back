namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class DailyGoalsSnapshotDto
{
    public required int DailyCaloriesGoal { get; init; }

    public required int DailyProteinGoal { get; init; }

    public required int DailyCarbsGoal { get; init; }

    public required int DailyFatGoal { get; init; }
}
