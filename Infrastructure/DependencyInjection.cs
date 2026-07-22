using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;
using RudFitAI.Domain.Repositories;
using RudFitAI.Infrastructure.Email;
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
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.Configure<FriendshipOptions>(configuration.GetSection(FriendshipOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services
            .AddHttpClient("OpenAi")
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                OpenAiOptions openAi = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
                int seconds = openAi.RequestTimeoutSeconds <= 0 ? 10 : openAi.RequestTimeoutSeconds;
                seconds = Math.Clamp(seconds, 1, 120);
                client.Timeout = TimeSpan.FromSeconds(seconds);
            });

        services.AddDbContext<RudFitAIDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IMealPhotoVisionClient, OpenAiMealPhotoClient>();
        services.AddScoped<IMealNutritionEstimationChatClient, OpenAiMealNutritionEstimationClient>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IFoodRepository, FoodRepository>();
        services.AddScoped<IMealLogRepository, MealLogRepository>();
        services.AddScoped<IOnboardingRepository, OnboardingRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IUserInviteRepository, UserInviteRepository>();

        return services;
    }
}
