using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CreateMealLogItemRequestValidator : AbstractValidator<CreateMealLogItemRequest>
{
    public CreateMealLogItemRequestValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("FoodId é obrigatório.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0m)
            .WithMessage("Quantidade deve ser maior que zero.");
    }
}
