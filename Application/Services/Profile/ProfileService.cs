using RudFitAI.Application.DTOs.Profiles.Responses;
using RudFitAI.Application.Services.Interfaces.Profile;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Profile;

public sealed class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;

    public ProfileService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<UserProfileDetailsResponseDto?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        User? user = await _profileRepository.GetByIdWithProfileAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        UserProfile? profile = user.UserProfile;

        return new UserProfileDetailsResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Username = user.Username,
            ProfileImageUrl = user.ProfileImageUrl,
            IsActive = user.IsActive,
            Age = profile?.Age,
            Weight = profile?.Weight,
            Height = profile?.Height,
            Gender = profile?.Gender,
            Goal = profile?.Goal,
            ActivityLevel = profile?.ActivityLevel,
            DailyRoutineLevel = profile?.DailyRoutineLevel,
            GoalIntensity = profile?.GoalIntensity,
            StartingWeight = profile?.StartingWeight,
            TargetWeight = profile?.TargetWeight,
            DailyCaloriesGoal = profile?.DailyCaloriesGoal,
            DailyProteinGoal = profile?.DailyProteinGoal,
            DailyCarbsGoal = profile?.DailyCarbsGoal,
            DailyFatGoal = profile?.DailyFatGoal
        };
    }
}
