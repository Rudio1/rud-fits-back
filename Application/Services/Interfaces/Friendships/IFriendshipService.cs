using RudFitAI.Application.DTOs.Friendships.Requests;
using RudFitAI.Application.DTOs.Friendships.Responses;
using RudFitAI.Application.DTOs.Meals.Responses;

namespace RudFitAI.Application.Services.Interfaces.Friendships;

public interface IFriendshipService
{
    Task<FriendInviteLinkResponseDto> GetOrCreateInviteLinkAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<FriendInviteLinkResponseDto> RegenerateInviteLinkAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<FriendInviteLinkResponseDto> UpdateInviteLinkAsync(
        Guid userId,
        UpdateInviteLinkRequest request,
        CancellationToken cancellationToken);

    Task<FriendInvitePreviewResponseDto?> GetInvitePreviewAsync(
        string token,
        CancellationToken cancellationToken);

    Task<FriendshipResponseDto> AcceptInviteAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<FriendshipResponseDto>> ListFriendsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<FriendshipResponseDto?> GetFriendAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken);

    Task<bool> RemoveFriendAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken);

    Task<FriendDailyComparisonResponseDto?> GetDailyComparisonAsync(
        Guid userId,
        Guid friendUserId,
        DateOnly? date,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MealLogResponseDto>?> GetFriendMealLogsByDateAsync(
        Guid userId,
        Guid friendUserId,
        DateOnly date,
        CancellationToken cancellationToken);
}
