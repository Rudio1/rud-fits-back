using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Friendships.Requests;
using RudFitAI.Application.DTOs.Friendships.Responses;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Services.Interfaces.Friendships;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/friendships")]
[Authorize]
public sealed class FriendshipsController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipsController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [HttpGet("invite-link")]
    public async Task<ActionResult<FriendInviteLinkResponseDto>> GetInviteLink(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        FriendInviteLinkResponseDto response =
            await _friendshipService.GetOrCreateInviteLinkAsync(userId, cancellationToken);

        return Ok(response);
    }

    [HttpPost("invite-link/regenerate")]
    public async Task<ActionResult<FriendInviteLinkResponseDto>> RegenerateInviteLink(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        FriendInviteLinkResponseDto response =
            await _friendshipService.RegenerateInviteLinkAsync(userId, cancellationToken);

        return Ok(response);
    }

    [HttpPatch("invite-link")]
    public async Task<ActionResult<FriendInviteLinkResponseDto>> UpdateInviteLink(
        UpdateInviteLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        FriendInviteLinkResponseDto response =
            await _friendshipService.UpdateInviteLinkAsync(userId, request, cancellationToken);

        return Ok(response);
    }

    [HttpGet("invites/{token}/preview")]
    public async Task<ActionResult<FriendInvitePreviewResponseDto>> GetInvitePreview(
        string token,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid _))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            FriendInvitePreviewResponseDto? response =
                await _friendshipService.GetInvitePreviewAsync(token, cancellationToken);

            if (response is null)
            {
                return NotFound(new { message = "Link de convite inválido." });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status410Gone, new { message = ex.Message });
        }
    }

    [HttpPost("invites/{token}/accept")]
    public async Task<ActionResult<FriendshipResponseDto>> AcceptInvite(
        string token,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            FriendshipResponseDto response =
                await _friendshipService.AcceptInviteAsync(userId, token, cancellationToken);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("já são amigos"))
            {
                return Conflict(new { message = ex.Message });
            }

            if (ex.Message.Contains("desativado"))
            {
                return StatusCode(StatusCodes.Status410Gone, new { message = ex.Message });
            }

            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FriendshipResponseDto>>> ListFriends(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        IReadOnlyCollection<FriendshipResponseDto> response =
            await _friendshipService.ListFriendsAsync(userId, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{friendUserId:guid}")]
    public async Task<ActionResult<FriendshipResponseDto>> GetFriend(
        Guid friendUserId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        FriendshipResponseDto? response =
            await _friendshipService.GetFriendAsync(userId, friendUserId, cancellationToken);

        if (response is null)
        {
            return NotFound(new { message = "Amizade não encontrada." });
        }

        return Ok(response);
    }

    [HttpDelete("{friendUserId:guid}")]
    public async Task<ActionResult> RemoveFriend(
        Guid friendUserId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            bool removed = await _friendshipService.RemoveFriendAsync(userId, friendUserId, cancellationToken);

            if (!removed)
            {
                return NotFound(new { message = "Amizade não encontrada." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{friendUserId:guid}/meal-logs")]
    public async Task<ActionResult<IReadOnlyCollection<MealLogResponseDto>>> GetFriendMealLogs(
        Guid friendUserId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (date is null)
        {
            return BadRequest(new { message = "Informe a data no formato YYYY-MM-DD." });
        }

        try
        {
            IReadOnlyCollection<MealLogResponseDto>? response =
                await _friendshipService.GetFriendMealLogsByDateAsync(
                    userId,
                    friendUserId,
                    date.Value,
                    cancellationToken);

            if (response is null)
            {
                return NotFound(new { message = "Amizade não encontrada." });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{friendUserId:guid}/daily-comparison")]
    public async Task<ActionResult<FriendDailyComparisonResponseDto>> GetDailyComparison(
        Guid friendUserId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            FriendDailyComparisonResponseDto? response =
                await _friendshipService.GetDailyComparisonAsync(
                    userId,
                    friendUserId,
                    date,
                    cancellationToken);

            if (response is null)
            {
                return NotFound(new { message = "Amizade não encontrada." });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? userIdRaw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdRaw, out userId);
    }
}
