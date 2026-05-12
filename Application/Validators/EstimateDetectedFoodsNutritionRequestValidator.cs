using FluentValidation;
using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Application.Validators;

public sealed class EstimateDetectedFoodsNutritionRequestValidator : AbstractValidator<EstimateDetectedFoodsNutritionRequest>
{
    public EstimateDetectedFoodsNutritionRequestValidator()
    {
        RuleFor(x => x.Foods)
            .NotNull()
            .WithMessage("A lista foods é obrigatória.")
            .NotEmpty()
            .WithMessage("Informe ao menos um alimento.")
            .Must(foods => foods.Count <= 40)
            .WithMessage("No máximo 40 alimentos por requisição.")
            .Must(
                foods => foods.Select(f => Food.NormalizeForLookup(f.Name)).Distinct(StringComparer.Ordinal).Count()
                         == foods.Count)
            .WithMessage("Não repita o mesmo alimento na mesma requisição (nome equivalente).");

        RuleForEach(x => x.Foods)
            .ChildRules(
                item =>
                {
                    item.RuleFor(i => i.Name)
                        .NotEmpty()
                        .WithMessage("Nome do alimento é obrigatório.")
                        .MaximumLength(200)
                        .WithMessage("Nome do alimento deve ter no máximo 200 caracteres.");

                    item.RuleFor(i => i.EstimatedQuantityGrams)
                        .InclusiveBetween(1, 10_000)
                        .WithMessage("Quantidade estimada deve estar entre 1 e 10000 gramas.");
                });
    }
}
