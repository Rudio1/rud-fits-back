namespace RudFitAI.Application.Abstractions;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, DateTime utcNow, bool isAdmin = false);

    DateTime GetAccessTokenExpiryUtc(DateTime utcNow);
}
