using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RudFitAI.Application.Services.Auth;
using RudFitAI.Application.Services.Interfaces.Auth;
using RudFitAI.Domain.DomainServices;

namespace RudFitAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<AuthDomainService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
