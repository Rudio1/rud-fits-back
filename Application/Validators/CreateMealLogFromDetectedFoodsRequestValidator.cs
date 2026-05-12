using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CreateMealLogFromDetectedFoodsRequestValidator : AbstractValidator<CreateMealLogFromDetectedFoodsRequest>
{
    public CreateMealLogFromDetectedFoodsRequestValidator()
    {
        RuleFor(x => x.MealType)
            .IsInEnum()
            .WithMessage("Tipo de refeição inválido.");

        RuleFor(x => x.Name)
            .MaximumLength(120)
            .WithMessage("Nome da refeição deve ter no máximo 120 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Observações devem ter no máximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.Foods)
            .NotNull()
            .WithMessage("Lista de alimentos é obrigatória.")
            .Must(foods => foods.Count > 0)
            .WithMessage("A refeição deve ter ao menos um alimento.");

        RuleForEach(x => x.Foods)
            .SetValidator(new CreateMealLogFromDetectedFoodsItemRequestValidator());
    }
}
