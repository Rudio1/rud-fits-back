using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");
        builder.HasKey(friendship => friendship.Id);

        builder.Property(friendship => friendship.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(friendship => friendship.InitiatedByUserId)
            .IsRequired();

        builder.Property(friendship => friendship.EstablishedAt);

        builder.HasOne(friendship => friendship.UserLow)
            .WithMany()
            .HasForeignKey(friendship => friendship.UserLowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(friendship => friendship.UserHigh)
            .WithMany()
            .HasForeignKey(friendship => friendship.UserHighId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(friendship => new { friendship.UserLowId, friendship.UserHighId })
            .IsUnique();
    }
}
