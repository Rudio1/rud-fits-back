namespace RudFitAI.Application.DTOs.Auth.Requests;

public sealed class RegisterRequest
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Username { get; init; }

    public string Password { get; init; } = string.Empty;
}
