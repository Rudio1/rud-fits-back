using RudFitAI.Domain.Common;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Domain.Entities;

public sealed class MealLog : BaseEntity
{
    private readonly List<MealLogItem> _items = new();

    private MealLog()
    {
    }

    public MealLog(
        Guid id,
        Guid userId,
        string name,
        MealType mealType,
        DateTime consumedAt,
        MealSourceType sourceType,
        string? notes,
        int totalCalories,
        decimal totalProtein,
        decimal totalCarbs,
        decimal totalFat,
        IReadOnlyCollection<MealLogItem> items)
        : this()
    {
        Id = id;
        UserId = userId;
        Name = name;
        MealType = mealType;
        ConsumedAt = consumedAt;
        SourceType = sourceType;
        Notes = notes;
        TotalCalories = totalCalories;
        TotalProtein = totalProtein;
        TotalCarbs = totalCarbs;
        TotalFat = totalFat;
        _items.AddRange(items);
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public MealType MealType { get; private set; }

    public DateTime ConsumedAt { get; private set; }

    public MealSourceType SourceType { get; private set; }

    public string? Notes { get; private set; }

    public int TotalCalories { get; private set; }

    public decimal TotalProtein { get; private set; }

    public decimal TotalCarbs { get; private set; }

    public decimal TotalFat { get; private set; }

    public User User { get; private set; } = null!;

    public IReadOnlyCollection<MealLogItem> Items => _items;
}
