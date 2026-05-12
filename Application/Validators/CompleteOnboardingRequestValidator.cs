using FluentValidation;
using RudFitAI.Application.DTOs.Onboarding.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CompleteOnboardingRequestValidator : AbstractValidator<CompleteOnboardingRequest>
{
    public CompleteOnboardingRequestValidator()
    {
        RuleFor(x => x.Age)
            .InclusiveBetween(12, 100)
            .WithMessage("Idade deve estar entre 12 e 100 anos.");

        RuleFor(x => x.Height)
            .InclusiveBetween(100m, 250m)
            .WithMessage("Altura deve estar entre 100 e 250 cm.");

        RuleFor(x => x.Weight)
            .GreaterThan(0m)
            .WithMessage("Peso deve ser maior que zero.");

        RuleFor(x => x.StartingWeight)
            .GreaterThan(0m)
            .WithMessage("Peso inicial deve ser maior que zero.");

        RuleFor(x => x.TargetWeight)
            .GreaterThan(0m)
            .WithMessage("Peso alvo deve ser maior que zero.");

        RuleFor(x => x.DailyRoutineLevel)
            .InclusiveBetween(1, 4)
            .WithMessage("Nível de rotina diária deve estar entre 1 e 4.");

        RuleFor(x => x.GoalIntensity)
            .InclusiveBetween(1, 3)
            .WithMessage("Intensidade do objetivo deve estar entre 1 e 3.");
    }
}
