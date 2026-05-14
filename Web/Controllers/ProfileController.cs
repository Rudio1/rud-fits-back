using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Onboarding.Requests;
using RudFitAI.Application.DTOs.Onboarding.Responses;
using RudFitAI.Application.DTOs.Profiles.Responses;
using RudFitAI.Application.Services.Interfaces.Onboarding;
using RudFitAI.Application.Services.Interfaces.Profile;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly IProfileService _profileService;

    public ProfileController(
        IProfileService profileService,
        IOnboardingService onboardingService)
    {
        _profileService = profileService;
        _onboardingService = onboardingService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDetailsResponseDto>> GetCurrent(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        UserProfileDetailsResponseDto? response =
            await _profileService.GetCurrentAsync(userId, cancellationToken);

        if (response is null)
        {
            return NotFound(new { message = "Perfil do usuário não encontrado." });
        }

        return Ok(response);
    }

    [HttpPost("me/recalculate-daily-goals")]
    public async Task<ActionResult<CalculateDailyGoalsResponseDto>> RecareporelculateDailyGoals(
        [FromBody] CompleteOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            CalculateDailyGoalsResponseDto? response =
                await _onboardingService.UpdateAndCalculateDailyGoalsAsync(userId, request, cancellationToken);

            if (response is null)
            {
                return NotFound(new { message = "Conta do usuário não encontrada." });
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? userIdRaw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdRaw, out userId);
    }
}
