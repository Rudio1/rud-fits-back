using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.Options;

namespace RudFitAI.Web.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(Guid userId, string email, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer)
            || string.IsNullOrWhiteSpace(_options.Audience)
            || string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException("A configuração de JWT está incompleta.");
        }

        if (_options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("A chave de assinatura do JWT deve ter no mínimo 32 caracteres.");
        }

        byte[] keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        SymmetricSecurityKey securityKey = new(keyBytes);
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        DateTime expiresUtc = utcNow.AddMinutes(_options.AccessTokenMinutes);

        JwtSecurityToken token = new(
            _options.Issuer,
            _options.Audience,
            claims,
            utcNow,
            expiresUtc,
            credentials);

        JwtSecurityTokenHandler handler = new();
        return handler.WriteToken(token);
    }

    public DateTime GetAccessTokenExpiryUtc(DateTime utcNow)
    {
        return utcNow.AddMinutes(_options.AccessTokenMinutes);
    }
}
