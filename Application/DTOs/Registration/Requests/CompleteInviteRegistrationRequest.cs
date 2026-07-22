namespace RudFitAI.Application.DTOs.Registration.Requests;

public sealed class CompleteInviteRegistrationRequest
{
    public string FullName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
