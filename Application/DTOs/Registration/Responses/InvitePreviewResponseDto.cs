namespace RudFitAI.Application.DTOs.Registration.Responses;

public sealed class InvitePreviewResponseDto
{
    public required string Email { get; init; }

    public required bool IsValid { get; init; }

    public string? Message { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }
}
