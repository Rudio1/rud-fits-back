using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Onboarding.Requests;
using RudFitAI.Application.DTOs.Onboarding.Responses;
using RudFitAI.Application.Services.Interfaces.Onboarding;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingController(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    [HttpPost]
    public async Task<ActionResult<CompleteOnboardingResponseDto>> Complete(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        CompleteOnboardingResponseDto? result =
            await _onboardingService.CompleteAsync(userId, request, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = "Conta do usuário não encontrada." });
        }

        return Ok(result);
    }

    [HttpPost("calculate-daily-goals")]
    public async Task<ActionResult<CalculateDailyGoalsResponseDto>> CalculateDailyGoals(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            CalculateDailyGoalsResponseDto? result =
                await _onboardingService.CalculateDailyGoalsAsync(userId, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = "Perfil de onboarding não encontrado." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("daily-goals")]
    public async Task<ActionResult<CalculateDailyGoalsResponseDto>> GetDailyGoals(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        CalculateDailyGoalsResponseDto? result =
            await _onboardingService.GetDailyGoalsAsync(userId, cancellationToken);

        if (result == null)
        {
            return NotFound(new { message = "Metas diárias ainda não foram calculadas." });
        }

        return Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? userIdRaw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdRaw, out userId);
    }
}
