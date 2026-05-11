using FluentValidation;
using RudFitAI.Application.DTOs.Auth.Requests;

namespace RudFitAI.Application.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        When(
            x => !string.IsNullOrWhiteSpace(x.Username),
            () =>
            {
                RuleFor(x => x.Username!)
                    .MinimumLength(3)
                    .MaximumLength(50)
                    .Matches(@"^[a-zA-Z0-9_]+$");
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
