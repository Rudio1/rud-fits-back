using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RudFitAI.Application.Options;

namespace RudFitAI.Web.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddRudFitAiJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        IConfigurationSection jwtSection = configuration.GetSection(JwtOptions.SectionName);
        string issuer = jwtSection["Issuer"] ?? string.Empty;
        string audience = jwtSection["Audience"] ?? string.Empty;
        string signingKey = jwtSection["SigningKey"] ?? string.Empty;

        byte[] signingKeyBytes = Encoding.UTF8.GetBytes(signingKey);
        SymmetricSecurityKey key = new(signingKeyBytes);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
