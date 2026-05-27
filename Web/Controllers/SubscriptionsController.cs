using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Subscriptions.Requests;
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

    [HttpPost("card")]
    public async Task<ActionResult<StartCheckoutResponseDto>> StartCard(
        [FromBody] StartCardSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            StartCheckoutResponseDto response =
                await _subscriptionService.StartCardSubscriptionAsync(userId, request, cancellationToken);

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

    [HttpPost("pix")]
    public async Task<ActionResult<StartCheckoutResponseDto>> StartPix(
        [FromBody] StartPixSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            StartCheckoutResponseDto response =
                await _subscriptionService.StartPixSubscriptionAsync(userId, request, cancellationToken);

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

    [HttpPost("lifetime")]
    public async Task<ActionResult<StartCheckoutResponseDto>> StartLifetime(
        [FromBody] StartLifetimeSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            StartCheckoutResponseDto response =
                await _subscriptionService.StartLifetimeSubscriptionAsync(userId, request, cancellationToken);

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

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            await _subscriptionService.CancelCurrentSubscriptionAsync(userId, cancellationToken);
            return NoContent();
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
