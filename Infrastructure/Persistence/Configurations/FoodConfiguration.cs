using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.ToTable("Foods");
        builder.HasKey(food => food.Id);

        builder.Property(food => food.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(food => food.NormalizedName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(food => food.Category)
            .HasMaxLength(100);

        builder.Property(food => food.SourceCode)
            .HasMaxLength(50);

        builder.Property(food => food.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(food => food.BaseQuantity)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(food => food.UnitType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(food => food.Calories)
            .IsRequired();

        builder.Property(food => food.Protein)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(food => food.Carbs)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(food => food.Fat)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(food => food.IsActive)
            .IsRequired();

        builder.HasIndex(food => food.Name);
        builder.HasIndex(food => food.NormalizedName)
            .HasDatabaseName("IX_Foods_NormalizedName");
        builder.HasIndex(food => food.NormalizedName)
            .IsUnique()
            .HasDatabaseName("UX_Foods_Ai_NormalizedName_Active")
            .HasFilter("[SourceType] = 4 AND [IsActive] = 1");
        builder.HasIndex(food => new { food.SourceType, food.SourceCode })
            .IsUnique()
            .HasFilter("[SourceCode] IS NOT NULL");
    }
}
