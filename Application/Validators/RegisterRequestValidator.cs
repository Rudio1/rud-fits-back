using FluentValidation;
using RudFitAI.Application.DTOs.Auth.Requests;

namespace RudFitAI.Application.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Nome completo é obrigatório.")
            .MaximumLength(120)
            .WithMessage("Nome completo deve ter no máximo 120 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("Informe um e-mail válido.");

        When(
            x => !string.IsNullOrWhiteSpace(x.Username),
            () =>
            {
                RuleFor(x => x.Username!)
                    .MinimumLength(3)
                    .WithMessage("Username deve ter no mínimo 3 caracteres.")
                    .MaximumLength(50)
                    .WithMessage("Username deve ter no máximo 50 caracteres.")
                    .Matches(@"^[a-zA-Z0-9_]+$")
                    .WithMessage("Username pode conter apenas letras, números e underscore.");
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(8)
            .WithMessage("Senha deve ter no mínimo 8 caracteres.");
    }
}
