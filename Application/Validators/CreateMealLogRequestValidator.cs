using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CreateMealLogRequestValidator : AbstractValidator<CreateMealLogRequest>
{
    public CreateMealLogRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(120)
            .WithMessage("Nome da refeição deve ter no máximo 120 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.MealType)
            .IsInEnum()
            .WithMessage("Tipo de refeição inválido.");

        RuleFor(x => x.ConsumedAt)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("Data de consumo é obrigatória.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Observações devem ter no máximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Itens da refeição são obrigatórios.")
            .Must(items => items.Count > 0)
            .WithMessage("A refeição deve ter ao menos um item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateMealLogItemRequestValidator());
    }
}
