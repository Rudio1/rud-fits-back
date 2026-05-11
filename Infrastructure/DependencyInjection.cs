using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RudFitAI.Domain.Repositories;
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

        services.AddDbContext<RudFitAIDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAuthRepository, AuthRepository>();

        return services;
    }
}
