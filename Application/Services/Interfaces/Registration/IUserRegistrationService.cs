using RudFitAI.Application.DTOs.Auth.Responses;
using RudFitAI.Application.DTOs.Registration.Requests;
using RudFitAI.Application.DTOs.Registration.Responses;

namespace RudFitAI.Application.Services.Interfaces.Registration;

public interface IUserRegistrationService
{
    Task<InvitePreviewResponseDto> GetInvitePreviewAsync(string token, CancellationToken cancellationToken);

    Task<AuthResponseDto?> CompleteInviteAsync(
        string token,
        CompleteInviteRegistrationRequest request,
        CancellationToken cancellationToken);
}
