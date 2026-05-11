using RudFitAI.Application.DTOs.Auth.Requests;
using RudFitAI.Application.DTOs.Auth.Responses;

namespace RudFitAI.Application.Services.Interfaces.Auth;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResponseDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
