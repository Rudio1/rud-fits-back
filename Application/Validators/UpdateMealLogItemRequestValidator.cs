using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class UpdateMealLogItemRequestValidator : AbstractValidator<UpdateMealLogItemRequest>
{
    public UpdateMealLogItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do alimento é obrigatório.")
            .MaximumLength(150)
            .WithMessage("Nome do alimento deve ter no máximo 150 caracteres.");

        RuleFor(x => x.EstimatedQuantityGrams)
            .GreaterThan(0)
            .WithMessage("Quantidade do alimento deve ser maior que zero.");
    }
}
