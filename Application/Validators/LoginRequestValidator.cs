using FluentValidation;
using RudFitAI.Application.DTOs.Auth.Requests;

namespace RudFitAI.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.");
    }
}
