namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class DailyProgressSnapshotDto
{
    public required decimal CaloriesPercent { get; init; }

    public required decimal ProteinPercent { get; init; }

    public required decimal CarbsPercent { get; init; }

    public required decimal FatPercent { get; init; }
}
