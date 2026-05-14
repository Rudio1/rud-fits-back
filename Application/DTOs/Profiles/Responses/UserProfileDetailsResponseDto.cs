using RudFitAI.Domain.Enums;

namespace RudFitAI.Application.DTOs.Profiles.Responses;

public sealed class UserProfileDetailsResponseDto
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public string? Username { get; init; }

    public string? ProfileImageUrl { get; init; }

    public required bool IsActive { get; init; }

    public int? Age { get; init; }

    public decimal? Weight { get; init; }

    public decimal? Height { get; init; }

    public GenderType? Gender { get; init; }

    public GoalType? Goal { get; init; }

    public ActivityLevelType? ActivityLevel { get; init; }

    public int? DailyRoutineLevel { get; init; }

    public int? GoalIntensity { get; init; }

    public decimal? StartingWeight { get; init; }

    public decimal? TargetWeight { get; init; }

    public int? DailyCaloriesGoal { get; init; }

    public int? DailyProteinGoal { get; init; }

    public int? DailyCarbsGoal { get; init; }

    public int? DailyFatGoal { get; init; }
}
