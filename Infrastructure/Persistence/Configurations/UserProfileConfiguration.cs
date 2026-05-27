using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Gender)
            .HasConversion<int>();
        builder.Property(profile => profile.Goal)
            .HasConversion<int>();
        builder.Property(profile => profile.ActivityLevel)
            .HasConversion<int>();
        builder.Property(profile => profile.Weight).HasColumnType("decimal(10,2)");
        builder.Property(profile => profile.Height).HasColumnType("decimal(10,2)");
        builder.Property(profile => profile.TargetWeight).HasColumnType("decimal(10,2)");
        builder.Property(profile => profile.StartingWeight).HasColumnType("decimal(10,2)");
        builder.Property(profile => profile.DailyRoutineLevel);
        builder.Property(profile => profile.GoalIntensity);
        builder.Property(profile => profile.FreeScannerUsesCount)
            .HasDefaultValue(0);

        builder.HasOne(profile => profile.User)
            .WithOne(user => user.UserProfile)
            .HasForeignKey<UserProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();
    }
}
