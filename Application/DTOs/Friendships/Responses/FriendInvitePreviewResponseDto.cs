namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class FriendInvitePreviewResponseDto
{
    public required Guid UserId { get; init; }

    public required string Name { get; init; }

    public string? Username { get; init; }

    public string? ProfileImageUrl { get; init; }
}
