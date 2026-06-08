namespace RudFitAI.Application.DTOs.Friendships.Responses;

public sealed class FriendshipResponseDto
{
    public required Guid FriendshipId { get; init; }

    public required Guid FriendUserId { get; init; }

    public required string Name { get; init; }

    public string? Username { get; init; }

    public string? ProfileImageUrl { get; init; }

    public required DateTime EstablishedAt { get; init; }
}
