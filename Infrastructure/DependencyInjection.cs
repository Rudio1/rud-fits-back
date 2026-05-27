using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;
using RudFitAI.Domain.Repositories;
using RudFitAI.Infrastructure.Asaas;
using RudFitAI.Infrastructure.OpenAI;
using RudFitAI.Infrastructure.Persistence;
using RudFitAI.Infrastructure.Persistence.Repositories;

namespace RudFitAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString =
                "Server=(localdb)\\mssqllocaldb;Database=RudFitAI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        }

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<AsaasOptions>(configuration.GetSection(AsaasOptions.SectionName));
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services
            .AddHttpClient("OpenAi")
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                OpenAiOptions openAi = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
                int seconds = openAi.RequestTimeoutSeconds <= 0 ? 10 : openAi.RequestTimeoutSeconds;
                seconds = Math.Clamp(seconds, 1, 120);
                client.Timeout = TimeSpan.FromSeconds(seconds);
            });

        services
            .AddHttpClient<IAsaasClient, AsaasClient>((serviceProvider, client) =>
            {
                AsaasOptions asaas = serviceProvider.GetRequiredService<IOptions<AsaasOptions>>().Value;
                string baseUrl = string.IsNullOrWhiteSpace(asaas.BaseUrl)
                    ? "https://sandbox.asaas.com/api/v3/"
                    : asaas.BaseUrl.TrimEnd('/') + "/";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);

                string userAgent = string.IsNullOrWhiteSpace(asaas.UserAgent)
                    ? "RudFitAI/1.0"
                    : asaas.UserAgent.Trim();
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
            });

        services.AddDbContext<RudFitAIDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IMealPhotoVisionClient, OpenAiMealPhotoClient>();
        services.AddScoped<IMealNutritionEstimationChatClient, OpenAiMealNutritionEstimationClient>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IMealLogRepository, MealLogRepository>();
        services.AddScoped<IOnboardingRepository, OnboardingRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }
}
