using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RudFitAI.Application.Services.Auth;
using RudFitAI.Application.Services.Interfaces.Auth;
using RudFitAI.Application.Services.Interfaces.Meals;
using RudFitAI.Application.Services.Interfaces.Onboarding;
using RudFitAI.Application.Services.Meals;
using RudFitAI.Application.Services.Onboarding;
using RudFitAI.Domain.DomainServices;

namespace RudFitAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<AuthDomainService>();
        services.AddScoped<DailyGoalsDomainService>();
        services.AddScoped<MealLogDomainService>();
        services.AddScoped<OnboardingDomainService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMealLogService, MealLogService>();
        services.AddScoped<IMealPhotoAnalysisService, MealPhotoAnalysisService>();
        services.AddScoped<IMealDetectedFoodsNutritionEstimationService, MealDetectedFoodsNutritionEstimationService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        return services;
    }
}
