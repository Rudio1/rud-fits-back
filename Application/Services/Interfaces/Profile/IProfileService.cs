using RudFitAI.Application.DTOs.Profiles.Responses;

namespace RudFitAI.Application.Services.Interfaces.Profile;

public interface IProfileService
{
    Task<UserProfileDetailsResponseDto?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
}
