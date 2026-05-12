using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class DailyGoalsDomainService
{
    public (int Calories, int Protein, int Carbs, int Fat) Calculate(UserProfile profile)
    {
        if (profile.Age is null
            || profile.Weight is null
            || profile.Height is null
            || profile.Gender is null
            || profile.Goal is null
            || profile.ActivityLevel is null
            || profile.DailyRoutineLevel is null
            || profile.GoalIntensity is null)
        {
            throw new InvalidOperationException("Perfil incompleto para cálculo de metas diárias.");
        }

        decimal weight = profile.Weight.Value;
        decimal height = profile.Height.Value;
        int age = profile.Age.Value;

        decimal bmr = (10m * weight) + (6.25m * height) - (5m * age) + GetGenderFactor(profile.Gender.Value);
        decimal dailyRoutineMultiplier = GetDailyRoutineMultiplier(profile.DailyRoutineLevel.Value);
        decimal baseTdee = bmr * dailyRoutineMultiplier;
        decimal workoutCalories = GetWorkoutCalories(profile.ActivityLevel.Value);
        decimal caloriesBeforeGoal = baseTdee + workoutCalories;
        decimal dailyCalories = caloriesBeforeGoal + GetGoalAdjustment(profile.Goal.Value, profile.GoalIntensity.Value);

        if (dailyCalories < 1200m)
        {
            dailyCalories = 1200m;
        }

        decimal proteinPerKg = GetProteinPerKg(profile.Goal.Value);
        decimal proteinGrams = weight * proteinPerKg;
        decimal proteinCalories = proteinGrams * 4m;

        decimal fatCalories = dailyCalories * 0.25m;
        decimal fatGrams = fatCalories / 9m;

        decimal carbsCalories = dailyCalories - proteinCalories - fatCalories;
        if (carbsCalories < 0m)
        {
            carbsCalories = 0m;
        }

        decimal carbsGrams = carbsCalories / 4m;

        return (
            Calories: Convert.ToInt32(Math.Round(dailyCalories, MidpointRounding.AwayFromZero)),
            Protein: Convert.ToInt32(Math.Round(proteinGrams, MidpointRounding.AwayFromZero)),
            Carbs: Convert.ToInt32(Math.Round(carbsGrams, MidpointRounding.AwayFromZero)),
            Fat: Convert.ToInt32(Math.Round(fatGrams, MidpointRounding.AwayFromZero))
        );
    }

    private static decimal GetGenderFactor(GenderType gender)
    {
        return gender switch
        {
            GenderType.Male => 5m,
            GenderType.Female => -161m,
            GenderType.Other => -78m,
            _ => -78m
        };
    }

    private static decimal GetDailyRoutineMultiplier(int dailyRoutineLevel)
    {
        return dailyRoutineLevel switch
        {
            1 => 1.2m,
            2 => 1.35m,
            3 => 1.5m,
            4 => 1.7m,
            _ => 1.2m
        };
    }

    private static decimal GetWorkoutCalories(ActivityLevelType activityLevel)
    {
        return activityLevel switch
        {
            ActivityLevelType.Sedentary => 0m,
            ActivityLevelType.LightlyActive => 100m,
            ActivityLevelType.ModeratelyActive => 250m,
            ActivityLevelType.VeryActive => 400m,
            ActivityLevelType.Athlete => 600m,
            _ => 0m
        };
    }

    private static decimal GetGoalAdjustment(GoalType goal, int goalIntensity)
    {
        return goal switch
        {
            GoalType.LoseWeight => GetLoseWeightAdjustment(goalIntensity),
            GoalType.GainMuscle => GetGainMuscleAdjustment(goalIntensity),
            GoalType.MaintainWeight => 0m,
            GoalType.BodyRecomposition => GetBodyRecompositionAdjustment(goalIntensity),
            _ => 0m
        };
    }

    private static decimal GetProteinPerKg(GoalType goal)
    {
        return goal switch
        {
            GoalType.LoseWeight => 2.1m,
            GoalType.GainMuscle => 1.9m,
            GoalType.MaintainWeight => 1.7m,
            GoalType.BodyRecomposition => 2.0m,
            _ => 1.7m
        };
    }

    private static decimal GetLoseWeightAdjustment(int goalIntensity)
    {
        return goalIntensity switch
        {
            1 => -250m,
            2 => -450m,
            3 => -700m,
            _ => -250m
        };
    }

    private static decimal GetGainMuscleAdjustment(int goalIntensity)
    {
        return goalIntensity switch
        {
            1 => 200m,
            2 => 300m,
            3 => 400m,
            _ => 200m
        };
    }

    private static decimal GetBodyRecompositionAdjustment(int goalIntensity)
    {
        return goalIntensity switch
        {
            1 => -150m,
            2 => -250m,
            3 => -350m,
            _ => -150m
        };
    }
}
