using RudFitAI.Application.DTOs.Meals.Responses;

namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class FriendDaySnapshotDto
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public string? ProfileImageUrl { get; init; }

    public required DailyGoalsSnapshotDto Goals { get; init; }

    public required DailyMealConsumptionSummaryResponseDto Consumption { get; init; }

    public required DailyProgressSnapshotDto Progress { get; init; }
}
