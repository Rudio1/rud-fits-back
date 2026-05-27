using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RudFitAI.Application.Services.Auth;
using RudFitAI.Application.Services.Interfaces.Auth;
using RudFitAI.Application.Services.Interfaces.Meals;
using RudFitAI.Application.Services.Interfaces.Onboarding;
using RudFitAI.Application.Services.Interfaces.Profile;
using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Application.Services.Meals;
using RudFitAI.Application.Services.Onboarding;
using RudFitAI.Application.Services.Profile;
using RudFitAI.Application.Services.Subscriptions;
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
        services.AddScoped<SubscriptionDomainService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMealLogService, MealLogService>();
        services.AddScoped<IMealPhotoAnalysisService, MealPhotoAnalysisService>();
        services.AddScoped<IMealDetectedFoodsNutritionEstimationService, MealDetectedFoodsNutritionEstimationService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<IAsaasWebhookProcessor, AsaasWebhookProcessor>();
        return services;
    }
}
