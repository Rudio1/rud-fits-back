namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class FriendDailyComparisonResponseDto
{
    public required DateOnly Date { get; init; }

    public required FriendDaySnapshotDto Me { get; init; }

    public required FriendDaySnapshotDto Friend { get; init; }
}
