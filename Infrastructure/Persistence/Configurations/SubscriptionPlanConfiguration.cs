using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(plan => plan.Id);

        builder.Property(plan => plan.Code).HasMaxLength(50).IsRequired();
        builder.Property(plan => plan.Name).HasMaxLength(120).IsRequired();
        builder.Property(plan => plan.Interval).HasConversion<int>();
        builder.Property(plan => plan.Kind)
            .HasColumnName("PlanKind")
            .HasConversion<int>();

        builder.HasIndex(plan => plan.Code).IsUnique();
    }
}
