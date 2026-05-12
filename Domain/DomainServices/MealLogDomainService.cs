using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.DomainServices;

public sealed class MealLogDomainService
{
    public void EnsureValidMealLog(MealType mealType, DateTime consumedAt, IReadOnlyCollection<MealLogItem> items)
    {
        if (!Enum.IsDefined(mealType))
        {
            throw new ArgumentException("Tipo de refeição inválido.", nameof(mealType));
        }

        if (consumedAt == default)
        {
            throw new ArgumentException("Data de consumo é obrigatória.", nameof(consumedAt));
        }

        if (items.Count == 0)
        {
            throw new ArgumentException("A refeição deve ter ao menos um item.", nameof(items));
        }

        foreach (MealLogItem item in items)
        {
            if (item.FoodId == Guid.Empty)
            {
                throw new ArgumentException("Identificador do alimento é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(item.FoodName))
            {
                throw new ArgumentException("Nome do alimento é obrigatório.");
            }

            if (item.Quantity <= 0m)
            {
                throw new ArgumentException("Quantidade do item deve ser maior que zero.");
            }

            if (!Enum.IsDefined(item.UnitType))
            {
                throw new ArgumentException("Unidade do item inválida.");
            }

            if (item.Calories < 0)
            {
                throw new ArgumentException("Calorias do item não podem ser negativas.");
            }

            if (item.Protein < 0m || item.Carbs < 0m || item.Fat < 0m)
            {
                throw new ArgumentException("Macros do item não podem ser negativos.");
            }
        }
    }

    public (int TotalCalories, decimal TotalProtein, decimal TotalCarbs, decimal TotalFat) CalculateTotals(
        IReadOnlyCollection<MealLogItem> items)
    {
        int totalCalories = 0;
        decimal totalProtein = 0m;
        decimal totalCarbs = 0m;
        decimal totalFat = 0m;

        foreach (MealLogItem item in items)
        {
            totalCalories += item.Calories;
            totalProtein += item.Protein;
            totalCarbs += item.Carbs;
            totalFat += item.Fat;
        }

        return (
            totalCalories,
            decimal.Round(totalProtein, 2, MidpointRounding.AwayFromZero),
            decimal.Round(totalCarbs, 2, MidpointRounding.AwayFromZero),
            decimal.Round(totalFat, 2, MidpointRounding.AwayFromZero));
    }
}
