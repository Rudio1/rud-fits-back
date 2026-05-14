using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class UpdateMealLogRequestValidator : AbstractValidator<UpdateMealLogRequest>
{
    public UpdateMealLogRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome da refeição é obrigatório.")
            .MaximumLength(120)
            .WithMessage("Nome da refeição deve ter no máximo 120 caracteres.");

        RuleFor(x => x.MealType)
            .IsInEnum()
            .WithMessage("Tipo de refeição inválido.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Itens da refeição são obrigatórios.")
            .Must(items => items.Count > 0)
            .WithMessage("A refeição deve ter ao menos um item.");

        RuleFor(x => x.Items)
            .Must(items => items
                .Where(item => item.Id.HasValue)
                .Select(item => item.Id!.Value)
                .Distinct()
                .Count() == items.Count(item => item.Id.HasValue))
            .WithMessage("Itens duplicados não são permitidos na edição.");

        RuleForEach(x => x.Items)
            .SetValidator(new UpdateMealLogItemRequestValidator());
    }
}
