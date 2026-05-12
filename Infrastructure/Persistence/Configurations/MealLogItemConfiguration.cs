using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class MealLogItemConfiguration : IEntityTypeConfiguration<MealLogItem>
{
    public void Configure(EntityTypeBuilder<MealLogItem> builder)
    {
        builder.ToTable("MealLogItems");
        builder.HasKey(mealLogItem => mealLogItem.Id);

        builder.Property(mealLogItem => mealLogItem.FoodName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.FoodId)
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.Quantity)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.UnitType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.Calories)
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.Protein)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.Carbs)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mealLogItem => mealLogItem.Fat)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasOne(mealLogItem => mealLogItem.Food)
            .WithMany()
            .HasForeignKey(mealLogItem => mealLogItem.FoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mealLogItem => mealLogItem.MealLogId);
        builder.HasIndex(mealLogItem => mealLogItem.FoodId);
    }
}
