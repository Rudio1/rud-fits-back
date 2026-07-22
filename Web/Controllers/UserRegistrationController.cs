using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Auth.Responses;
using RudFitAI.Application.DTOs.Registration.Requests;
using RudFitAI.Application.DTOs.Registration.Responses;
using RudFitAI.Application.Services.Interfaces.Registration;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/registration/invites")]
public sealed class UserRegistrationController : ControllerBase
{
    private readonly IUserRegistrationService _userRegistrationService;

    public UserRegistrationController(IUserRegistrationService userRegistrationService)
    {
        _userRegistrationService = userRegistrationService;
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<InvitePreviewResponseDto>> GetInvitePreview(
        string token,
        CancellationToken cancellationToken)
    {
        InvitePreviewResponseDto preview =
            await _userRegistrationService.GetInvitePreviewAsync(token, cancellationToken);
        return Ok(preview);
    }

    [HttpPost("{token}/complete")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> CompleteInvite(
        string token,
        [FromBody] CompleteInviteRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto? result = await _userRegistrationService.CompleteInviteAsync(
                token,
                request,
                cancellationToken);

            if (result is null)
            {
                return BadRequest(new
                {
                    message = "Não foi possível concluir o cadastro. Convite inválido, expirado ou e-mail já utilizado."
                });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
