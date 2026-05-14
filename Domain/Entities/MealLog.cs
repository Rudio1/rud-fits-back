using System.ComponentModel.DataAnnotations.Schema;
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
        IsDeleted = false;
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

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public User User { get; private set; } = null!;

    public IReadOnlyCollection<MealLogItem> Items => _items;

    [NotMapped]
    public IReadOnlyCollection<MealLogItem> ActiveItems => _items.Where(item => !item.IsDeleted).ToList();

    public void UpdateDetails(string name, MealType mealType)
    {
        Name = name.Trim();
        MealType = mealType;
    }

    public void UpdateTotals(int totalCalories, decimal totalProtein, decimal totalCarbs, decimal totalFat)
    {
        TotalCalories = totalCalories;
        TotalProtein = totalProtein;
        TotalCarbs = totalCarbs;
        TotalFat = totalFat;
    }

    public void AddItem(MealLogItem item)
    {
        _items.Add(item);
    }

    public void SoftDelete(DateTime deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt;

        foreach (MealLogItem item in _items.Where(existingItem => !existingItem.IsDeleted))
        {
            item.SoftDelete(deletedAt);
        }
    }
}
