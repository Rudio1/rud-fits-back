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

        bool isFirstAccess = account.IsFirstAccess;
        DateTime utcNow = DateTime.UtcNow;
        await _authRepository.UpdateAccountLastLoginAsync(account.Id, utcNow, cancellationToken);

        string token = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, utcNow, account.IsAdmin);
        DateTime expiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(utcNow);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            IsFirstAccess = isFirstAccess,
            IsAdmin = account.IsAdmin,
            Username = user.Username
        };
    }
}
