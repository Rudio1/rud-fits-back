using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");
        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.Status).HasConversion<int>();
        builder.Property(subscription => subscription.BillingType).HasConversion<int>();
        builder.Property(subscription => subscription.AsaasCustomerId).HasMaxLength(50);
        builder.Property(subscription => subscription.AsaasSubscriptionId).HasMaxLength(50);
        builder.Property(subscription => subscription.AsaasPaymentId).HasMaxLength(50);
        builder.Property(subscription => subscription.AsaasPixAuthorizationId).HasMaxLength(50);

        builder.HasOne(subscription => subscription.User)
            .WithMany()
            .HasForeignKey(subscription => subscription.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(subscription => subscription.SubscriptionPlan)
            .WithMany()
            .HasForeignKey(subscription => subscription.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(subscription => subscription.UserId);
        builder.HasIndex(subscription => subscription.AsaasCustomerId);
        builder.HasIndex(subscription => subscription.AsaasSubscriptionId);
        builder.HasIndex(subscription => subscription.AsaasPaymentId);
    }
}
