using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Auth.Requests;
using RudFitAI.Application.DTOs.Auth.Responses;
using RudFitAI.Application.Services.Interfaces.Auth;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly AuthDomainService _authDomainService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IAuthRepository authRepository,
        AuthDomainService authDomainService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _authRepository = authRepository;
        _authDomainService = authDomainService;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        string normalizedEmail = _authDomainService.NormalizeEmail(request.Email);
        string? normalizedUsername = _authDomainService.NormalizeUsername(request.Username);
        string fullName = request.FullName.Trim();

        _authDomainService.EnsureRegistration(fullName, normalizedEmail, normalizedUsername, request.Password);

        bool emailTaken = await _authRepository.ExistsWithEmailAsync(normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            return null;
        }

        if (normalizedUsername != null)
        {
            bool usernameTaken = await _authRepository.ExistsWithUsernameAsync(normalizedUsername, cancellationToken);
            if (usernameTaken)
            {
                return null;
            }
        }

        Guid userId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();
        User user = new(userId, fullName, normalizedEmail, normalizedUsername, isActive: true);
        string passwordHash = _passwordHasher.Hash(request.Password);
        Account account = new(accountId, userId, passwordHash, LoginProvider.Local);

        await _authRepository.AddUserAndAccountAsync(user, account, cancellationToken);

        DateTime utcNow = DateTime.UtcNow;
        string token = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, utcNow);
        DateTime expiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(utcNow);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        string normalizedEmail = _authDomainService.NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrEmpty(request.Password))
        {
            return null;
        }

        (User User, Account Account)? row =
            await _authRepository.GetUserWithAccountByEmailAsync(normalizedEmail, cancellationToken);
        if (row == null)
        {
            return null;
        }

        User user = row.Value.User;
        Account account = row.Value.Account;

        if (!user.IsActive)
        {
            return null;
        }

        if (account.LoginProvider != LoginProvider.Local)
        {
            return null;
        }

        bool valid = _passwordHasher.Verify(request.Password, account.PasswordHash);
        if (!valid)
        {
            return null;
        }

        DateTime utcNow = DateTime.UtcNow;
        await _authRepository.UpdateAccountLastLoginAsync(account.Id, utcNow, cancellationToken);

        string token = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, utcNow);
        DateTime expiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(utcNow);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
