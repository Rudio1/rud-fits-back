using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Domain.DomainServices;

namespace RudFitAI.Web.Filters;

public sealed class RequireMealAccessFilter : IAsyncActionFilter
{
    private readonly IEntitlementService _entitlementService;

    public RequireMealAccessFilter(IEntitlementService entitlementService)
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

        if (hasPremium)
        {
            await next();
            return;
        }

        bool allowFreeScanner = context.ActionDescriptor.EndpointMetadata
            .OfType<AllowFreeScannerAttribute>()
            .Any();

        if (allowFreeScanner)
        {
            bool consumed = await _entitlementService.TryConsumeFreeScannerUseAsync(
                userId,
                context.HttpContext.RequestAborted);

            if (consumed)
            {
                await next();
                return;
            }

            context.Result = new ObjectResult(new
            {
                message = $"Limite gratuito de {SubscriptionDomainService.FreeScannerLifetimeLimit} análises por foto atingido. Assine o Premium para continuar."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        context.Result = new ObjectResult(new
        {
            message = "Assinatura Premium necessária para usar este recurso."
        })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
