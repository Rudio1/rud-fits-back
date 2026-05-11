namespace RudFitAI.Application.DTOs.Auth.Responses;

public sealed class AuthResponseDto
{
    public required string AccessToken { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }
}
