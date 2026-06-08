using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class FriendInviteTokenConfiguration : IEntityTypeConfiguration<FriendInviteToken>
{
    public void Configure(EntityTypeBuilder<FriendInviteToken> builder)
    {
        builder.ToTable("FriendInviteTokens");
        builder.HasKey(inviteToken => inviteToken.Id);

        builder.Property(inviteToken => inviteToken.Token)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(inviteToken => inviteToken.IsActive)
            .IsRequired();

        builder.HasOne(inviteToken => inviteToken.User)
            .WithMany()
            .HasForeignKey(inviteToken => inviteToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(inviteToken => inviteToken.UserId)
            .IsUnique();

        builder.HasIndex(inviteToken => inviteToken.Token)
            .IsUnique();
    }
}
