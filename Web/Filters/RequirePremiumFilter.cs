using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RudFitAI.Application.Services.Interfaces.Subscriptions;

namespace RudFitAI.Web.Filters;

public sealed class RequirePremiumFilter : IAsyncActionFilter
{
    private readonly IEntitlementService _entitlementService;

    public RequirePremiumFilter(IEntitlementService entitlementService)
    {
        _entitlementService = entitlementService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ClaimsPrincipal user = context.HttpContext.User;
        string? userIdRaw =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdRaw, out Guid userId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Usuário não autenticado." });
            return;
        }

        bool hasPremium = await _entitlementService.HasPremiumAsync(
            userId,
            context.HttpContext.RequestAborted);

        if (!hasPremium)
        {
            context.Result = new ObjectResult(new
            {
                message = "Assinatura Premium necessária para usar este recurso."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
