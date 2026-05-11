namespace RudFitAI.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, DateTime utcNow);

    DateTime GetAccessTokenExpiryUtc(DateTime utcNow);
}
