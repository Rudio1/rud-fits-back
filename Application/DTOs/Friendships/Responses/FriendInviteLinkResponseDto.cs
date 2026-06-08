namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class FriendInviteLinkResponseDto
{
    public required string Token { get; init; }

    public required string Url { get; init; }

    public required bool IsActive { get; init; }
}
