using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class UserProfile : BaseEntity
{
    private UserProfile()
    {
    }

    public UserProfile(Guid id, Guid userId)
        : this()
    {
        Id = id;
        UserId = userId;
    }

    public Guid UserId { get; private set; }

    public int? Age { get; private set; }

    public decimal? Weight { get; private set; }

    public decimal? Height { get; private set; }

    public GenderType? Gender { get; private set; }

    public GoalType? Goal { get; private set; }

    public ActivityLevelType? ActivityLevel { get; private set; }

    public int? DailyCaloriesGoal { get; private set; }

    public int? DailyProteinGoal { get; private set; }

    public int? DailyCarbsGoal { get; private set; }

    public int? DailyFatGoal { get; private set; }

    public int? DailyRoutineLevel { get; private set; }

    public int? GoalIntensity { get; private set; }

    public decimal? TargetWeight { get; private set; }

    public decimal? StartingWeight { get; private set; }

    public User User { get; private set; } = null!;

    public void CompleteOnboarding(
        GoalType goal,
        GenderType gender,
        int age,
        decimal height,
        decimal weight,
        decimal startingWeight,
        decimal targetWeight,
        ActivityLevelType activityLevel,
        int dailyRoutineLevel,
        int goalIntensity)
    {
        Goal = goal;
        Gender = gender;
        Age = age;
        Height = height;
        Weight = weight;
        StartingWeight = startingWeight;
        TargetWeight = targetWeight;
        ActivityLevel = activityLevel;
        DailyRoutineLevel = dailyRoutineLevel;
        GoalIntensity = goalIntensity;
    }

    public void UpdateDailyGoals(
        int dailyCaloriesGoal,
        int dailyProteinGoal,
        int dailyCarbsGoal,
        int dailyFatGoal)
    {
        DailyCaloriesGoal = dailyCaloriesGoal;
        DailyProteinGoal = dailyProteinGoal;
        DailyCarbsGoal = dailyCarbsGoal;
        DailyFatGoal = dailyFatGoal;
    }
}
