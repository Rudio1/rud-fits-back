using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Admin.Requests;
using RudFitAI.Application.DTOs.Admin.Responses;
using RudFitAI.Application.Services.Interfaces.Admin;
using RudFitAI.Domain.Auth;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = AuthRoles.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpPost("invite")]
    public async Task<ActionResult<InviteUserResponseDto>> InviteUser(
        InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid adminUserId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            InviteUserResponseDto? result = await _adminUserService.InviteUserAsync(
                adminUserId,
                request,
                cancellationToken);

            if (result is null)
            {
                return Conflict(new { message = "Já existe uma conta com este e-mail." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? raw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(raw, out userId);
    }
}
