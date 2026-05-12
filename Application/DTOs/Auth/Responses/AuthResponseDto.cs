namespace RudFitAI.Application.DTOs.Auth.Responses;

public sealed class AuthResponseDto
{
    public required string AccessToken { get; init; }

    public required DateTime ExpiresAtUtc { get; init; }

    public required bool IsFirstAccess { get; init; }

    public string? Username { get; init; }
}
