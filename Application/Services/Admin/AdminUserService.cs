using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Admin.Requests;
using RudFitAI.Application.DTOs.Admin.Responses;
using RudFitAI.Application.Email;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Admin;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Admin;

public sealed class AdminUserService : IAdminUserService
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    private readonly IAuthRepository _authRepository;
    private readonly IUserInviteRepository _userInviteRepository;
    private readonly AuthDomainService _authDomainService;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;

    public AdminUserService(
        IAuthRepository authRepository,
        IUserInviteRepository userInviteRepository,
        AuthDomainService authDomainService,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions)
    {
        _authRepository = authRepository;
        _userInviteRepository = userInviteRepository;
        _authDomainService = authDomainService;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
    }

    public async Task<InviteUserResponseDto?> InviteUserAsync(
        Guid adminUserId,
        InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = _authDomainService.NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("E-mail é obrigatório.", nameof(request));
        }

        bool emailTaken = await _authRepository.ExistsWithEmailAsync(normalizedEmail, cancellationToken);
        if (emailTaken)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_emailOptions.InviteBaseUrl))
        {
            throw new InvalidOperationException(
                "Email:InviteBaseUrl não configurado. Defina a URL base do convite no appsettings.");
        }

        DateTime utcNow = DateTime.UtcNow;
        DateTime expiresAt = utcNow.Add(InviteLifetime);
        string token = Guid.NewGuid().ToString("N");

        UserInvite? pending = await _userInviteRepository.GetPendingByEmailAsync(
            normalizedEmail,
            utcNow,
            cancellationToken);

        if (pending is null)
        {
            UserInvite invite = new(
                Guid.NewGuid(),
                normalizedEmail,
                token,
                adminUserId,
                expiresAt);
            await _userInviteRepository.AddAsync(invite, cancellationToken);
        }
        else
        {
            pending.Refresh(token, expiresAt);
            await _userInviteRepository.SaveChangesAsync(cancellationToken);
        }

        string baseUrl = _emailOptions.InviteBaseUrl.TrimEnd('/') + "/";
        string inviteUrl = baseUrl + token;
        string html = UserInviteEmailTemplate.BuildHtml(inviteUrl, normalizedEmail);

        await _emailSender.SendAsync(
            normalizedEmail,
            "Convite para o RudFit AI",
            html,
            cancellationToken);

        return new InviteUserResponseDto
        {
            Email = normalizedEmail,
            ExpiresAtUtc = expiresAt
        };
    }
}
