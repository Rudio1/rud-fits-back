using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Auth.Requests;
using RudFitAI.Application.DTOs.Auth.Responses;
using RudFitAI.Application.Services.Interfaces.Auth;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        AuthResponseDto? result = await _authService.RegisterAsync(request, cancellationToken);
        if (result == null)
        {
            return Conflict(new { message = "Não foi possível concluir o cadastro. E-mail ou username já está em uso." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        AuthResponseDto? result = await _authService.LoginAsync(request, cancellationToken);
        if (result == null)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        return Ok(result);
    }
}
