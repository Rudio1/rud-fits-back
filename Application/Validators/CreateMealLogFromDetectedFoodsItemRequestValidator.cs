using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;

namespace RudFitAI.Application.Validators;

public sealed class CreateMealLogFromDetectedFoodsItemRequestValidator : AbstractValidator<CreateMealLogFromDetectedFoodsItemRequest>
{
    public CreateMealLogFromDetectedFoodsItemRequestValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("FoodId é obrigatório.");

        RuleFor(x => x.EstimatedQuantityGrams)
            .GreaterThan(0)
            .WithMessage("Quantidade em gramas deve ser maior que zero.");
    }
}
