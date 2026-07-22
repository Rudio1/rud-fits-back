namespace RudFitAI.Application.DTOs.Admin.Responses;

public sealed class InviteUserResponseDto
{
    public required string Email { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }
}
