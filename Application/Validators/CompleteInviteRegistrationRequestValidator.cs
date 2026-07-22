using FluentValidation;
using RudFitAI.Application.DTOs.Registration.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CompleteInviteRegistrationRequestValidator
    : AbstractValidator<CompleteInviteRegistrationRequest>
{
    public CompleteInviteRegistrationRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Nome completo é obrigatório.")
            .MaximumLength(120)
            .WithMessage("Nome completo deve ter no máximo 120 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Senha é obrigatória.")
            .MinimumLength(8)
            .WithMessage("Senha deve ter no mínimo 8 caracteres.");
    }
}
