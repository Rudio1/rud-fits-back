using RudFitAI.Application.DTOs.Admin.Requests;
using RudFitAI.Application.DTOs.Admin.Responses;

namespace RudFitAI.Application.Services.Interfaces.Admin;

public interface IAdminUserService
{
    Task<InviteUserResponseDto?> InviteUserAsync(
        Guid adminUserId,
        InviteUserRequest request,
        CancellationToken cancellationToken);
}
