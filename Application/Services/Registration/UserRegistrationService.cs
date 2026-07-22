using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Auth.Responses;
using RudFitAI.Application.DTOs.Registration.Requests;
using RudFitAI.Application.DTOs.Registration.Responses;
using RudFitAI.Application.Services.Interfaces.Registration;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Registration;

public sealed class UserRegistrationService : IUserRegistrationService
{
    private readonly IUserInviteRepository _userInviteRepository;
    private readonly IAuthRepository _authRepository;
    private readonly AuthDomainService _authDomainService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public UserRegistrationService(
        IUserInviteRepository userInviteRepository,
        IAuthRepository authRepository,
        AuthDomainService authDomainService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userInviteRepository = userInviteRepository;
        _authRepository = authRepository;
        _authDomainService = authDomainService;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<InvitePreviewResponseDto> GetInvitePreviewAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new InvitePreviewResponseDto
            {
                Email = string.Empty,
                IsValid = false,
                Message = "Convite inválido."
            };
        }

        UserInvite? invite = await _userInviteRepository.GetByTokenAsync(token.Trim(), cancellationToken);
        DateTime utcNow = DateTime.UtcNow;

        if (invite is null)
        {
            return new InvitePreviewResponseDto
            {
                Email = string.Empty,
                IsValid = false,
                Message = "Convite não encontrado."
            };
        }

        if (invite.ConsumedAt is not null)
        {
            return new InvitePreviewResponseDto
            {
                Email = MaskEmail(invite.Email),
                IsValid = false,
                Message = "Este convite já foi utilizado."
            };
        }

        if (invite.ExpiresAt <= utcNow)
        {
            return new InvitePreviewResponseDto
            {
                Email = MaskEmail(invite.Email),
                IsValid = false,
                Message = "Este convite expirou. Peça um novo ao administrador."
            };
        }

        return new InvitePreviewResponseDto
        {
            Email = invite.Email,
            IsValid = true,
            ExpiresAtUtc = invite.ExpiresAt
        };
    }

    public async Task<AuthResponseDto?> CompleteInviteAsync(
        string token,
        CompleteInviteRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        UserInvite? invite = await _userInviteRepository.GetByTokenAsync(token.Trim(), cancellationToken);
        DateTime utcNow = DateTime.UtcNow;

        if (invite is null || !invite.IsPending(utcNow))
        {
            return null;
        }

        string fullName = request.FullName.Trim();
        string normalizedEmail = invite.Email;

        _authDomainService.EnsureRegistration(fullName, normalizedEmail, null, request.Password);

        bool emailTaken = await _authRepository.ExistsWithEmailAsync(normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            return null;
        }

        Guid userId = Guid.NewGuid();
        Guid accountId = Guid.NewGuid();
        User user = new(userId, fullName, normalizedEmail, username: null, isActive: true);
        string passwordHash = _passwordHasher.Hash(request.Password);
        Account account = new(accountId, userId, passwordHash, LoginProvider.Local, isAdmin: false);
        account.MarkEmailVerified();

        await _authRepository.AddUserAndAccountAsync(user, account, cancellationToken);

        invite.MarkConsumed(utcNow);
        await _userInviteRepository.SaveChangesAsync(cancellationToken);

        string accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, utcNow, isAdmin: false);
        DateTime expiresAtUtc = _jwtTokenGenerator.GetAccessTokenExpiryUtc(utcNow);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc,
            IsFirstAccess = true,
            IsAdmin = false,
            Username = user.Username
        };
    }

    private static string MaskEmail(string email)
    {
        int at = email.IndexOf('@');
        if (at <= 1)
        {
            return email;
        }

        return email[0] + new string('*', Math.Min(at - 1, 4)) + email[at..];
    }
}
