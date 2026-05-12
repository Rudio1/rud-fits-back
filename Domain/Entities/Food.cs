using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class Food : BaseEntity
{
    private Food()
    {
    }

    public Food(
        Guid id,
        string name,
        string? category,
        string? sourceCode,
        FoodSourceType sourceType,
        decimal baseQuantity,
        UnitType unitType,
        int calories,
        decimal protein,
        decimal carbs,
        decimal fat,
        bool isActive)
        : this()
    {
        Id = id;
        Name = SanitizeName(name);
        NormalizedName = NormalizeForLookup(Name);
        Category = category;
        SourceCode = sourceCode;
        SourceType = sourceType;
        BaseQuantity = baseQuantity;
        UnitType = unitType;
        Calories = calories;
        Protein = protein;
        Carbs = carbs;
        Fat = fat;
        IsActive = isActive;
    }

    public static string NormalizeForLookup(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return name.Trim().ToLowerInvariant();
    }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? SourceCode { get; private set; }

    public FoodSourceType SourceType { get; private set; }

    public decimal BaseQuantity { get; private set; }

    public UnitType UnitType { get; private set; }

    public int Calories { get; private set; }

    public decimal Protein { get; private set; }

    public decimal Carbs { get; private set; }

    public decimal Fat { get; private set; }

    public bool IsActive { get; private set; }

    private static string SanitizeName(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length > 150)
        {
            return trimmed.Substring(0, 150);
        }

        return trimmed;
    }
}
