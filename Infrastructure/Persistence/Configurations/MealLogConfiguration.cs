using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("MealLogs");
        builder.HasKey(mealLog => mealLog.Id);

        builder.Property(mealLog => mealLog.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(mealLog => mealLog.MealType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(mealLog => mealLog.ConsumedAt)
            .IsRequired();

        builder.Property(mealLog => mealLog.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(mealLog => mealLog.Notes)
            .HasMaxLength(500);

        builder.Property(mealLog => mealLog.TotalCalories)
            .IsRequired();

        builder.Property(mealLog => mealLog.TotalProtein)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mealLog => mealLog.TotalCarbs)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(mealLog => mealLog.TotalFat)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.HasOne(mealLog => mealLog.User)
            .WithMany()
            .HasForeignKey(mealLog => mealLog.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mealLog => mealLog.Items)
            .WithOne(item => item.MealLog)
            .HasForeignKey(item => item.MealLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mealLog => new { mealLog.UserId, mealLog.ConsumedAt });
    }
}
