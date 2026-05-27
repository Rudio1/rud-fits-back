using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Subscriptions.Responses;
using RudFitAI.Application.Services.Interfaces.Subscriptions;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanResponseDto>>> GetPlans(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SubscriptionPlanResponseDto> plans =
            await _subscriptionService.GetActivePlansAsync(cancellationToken);

        return Ok(plans);
    }

    [HttpGet("me")]
    public async Task<ActionResult<SubscriptionStatusResponseDto>> GetCurrent(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        SubscriptionStatusResponseDto response =
            await _subscriptionService.GetStatusForUserAsync(userId, cancellationToken);

        return Ok(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? userIdRaw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdRaw, out userId);
    }
}
