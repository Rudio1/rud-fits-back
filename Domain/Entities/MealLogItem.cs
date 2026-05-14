using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class MealLogItem : BaseEntity
{
    private MealLogItem()
    {
    }

    public MealLogItem(
        Guid id,
        Guid mealLogId,
        Guid foodId,
        string foodName,
        decimal quantity,
        UnitType unitType,
        int calories,
        decimal protein,
        decimal carbs,
        decimal fat)
        : this()
    {
        Id = id;
        MealLogId = mealLogId;
        FoodId = foodId;
        FoodName = foodName;
        Quantity = quantity;
        UnitType = unitType;
        Calories = calories;
        Protein = protein;
        Carbs = carbs;
        Fat = fat;
        IsDeleted = false;
    }

    public Guid MealLogId { get; private set; }

    public Guid FoodId { get; private set; }

    public string FoodName { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public UnitType UnitType { get; private set; }

    public int Calories { get; private set; }

    public decimal Protein { get; private set; }

    public decimal Carbs { get; private set; }

    public decimal Fat { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public Food Food { get; private set; } = null!;

    public MealLog MealLog { get; private set; } = null!;

    public void Update(
        Guid foodId,
        string foodName,
        decimal quantity,
        UnitType unitType,
        int calories,
        decimal protein,
        decimal carbs,
        decimal fat)
    {
        FoodId = foodId;
        FoodName = foodName.Trim();
        Quantity = quantity;
        UnitType = unitType;
        Calories = calories;
        Protein = protein;
        Carbs = carbs;
        Fat = fat;
    }

    public void SoftDelete(DateTime deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
    }
}
