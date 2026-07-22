using FluentValidation;
using RudFitAI.Application.DTOs.Admin.Requests;

namespace RudFitAI.Application.Validators;

public sealed class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("Informe um e-mail válido.");
    }
}
