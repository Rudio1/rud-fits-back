using RudFitAI.Application.DTOs.Onboarding.Requests;
using RudFitAI.Application.DTOs.Onboarding.Responses;
using RudFitAI.Application.Services.Interfaces.Onboarding;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Onboarding;

public sealed class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly OnboardingDomainService _onboardingDomainService;
    private readonly DailyGoalsDomainService _dailyGoalsDomainService;

    public OnboardingService(
        IOnboardingRepository onboardingRepository,
        OnboardingDomainService onboardingDomainService,
        DailyGoalsDomainService dailyGoalsDomainService)
    {
        _onboardingRepository = onboardingRepository;
        _onboardingDomainService = onboardingDomainService;
        _dailyGoalsDomainService = dailyGoalsDomainService;
    }

    public async Task<CompleteOnboardingResponseDto?> CompleteAsync(
        Guid userId,
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        _onboardingDomainService.EnsureValidAnswers(
            request.Goal,
            request.Gender,
            request.Age,
            request.Height,
            request.Weight,
            request.StartingWeight,
            request.TargetWeight,
            request.ActivityLevel,
            request.DailyRoutineLevel,
            request.GoalIntensity);

        (Account? account, UserProfile? userProfile) =
            await _onboardingRepository.GetByUserIdAsync(userId, cancellationToken);

        if (account == null)
        {
            return null;
        }

        UserProfile profile = userProfile ?? new UserProfile(Guid.NewGuid(), userId);

        profile.CompleteOnboarding(
            request.Goal,
            request.Gender,
            request.Age,
            request.Height,
            request.Weight,
            request.StartingWeight,
            request.TargetWeight,
            request.ActivityLevel,
            request.DailyRoutineLevel,
            request.GoalIntensity);

        if (userProfile == null)
        {
            await _onboardingRepository.AddUserProfileAsync(profile, cancellationToken);
        }

        account.CompleteFirstAccess();
        await _onboardingRepository.SaveChangesAsync(cancellationToken);

        return new CompleteOnboardingResponseDto
        {
            Completed = true,
            IsFirstAccess = account.IsFirstAccess
        };
    }

    public async Task<CalculateDailyGoalsResponseDto?> CalculateDailyGoalsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        (Account? account, UserProfile? userProfile) =
            await _onboardingRepository.GetByUserIdAsync(userId, cancellationToken);

        if (account == null || userProfile == null)
        {
            return null;
        }

        (int calories, int protein, int carbs, int fat) = _dailyGoalsDomainService.Calculate(userProfile);

        userProfile.UpdateDailyGoals(calories, protein, carbs, fat);
        await _onboardingRepository.SaveChangesAsync(cancellationToken);

        return new CalculateDailyGoalsResponseDto
        {
            DailyCaloriesGoal = calories,
            DailyProteinGoal = protein,
            DailyCarbsGoal = carbs,
            DailyFatGoal = fat
        };
    }

    public async Task<CalculateDailyGoalsResponseDto?> GetDailyGoalsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        (Account? account, UserProfile? userProfile) =
            await _onboardingRepository.GetByUserIdAsync(userId, cancellationToken);

        if (account == null
            || userProfile == null
            || userProfile.DailyCaloriesGoal is null
            || userProfile.DailyProteinGoal is null
            || userProfile.DailyCarbsGoal is null
            || userProfile.DailyFatGoal is null)
        {
            return null;
        }

        return new CalculateDailyGoalsResponseDto
        {
            DailyCaloriesGoal = userProfile.DailyCaloriesGoal.Value,
            DailyProteinGoal = userProfile.DailyProteinGoal.Value,
            DailyCarbsGoal = userProfile.DailyCarbsGoal.Value,
            DailyFatGoal = userProfile.DailyFatGoal.Value
        };
    }
}
