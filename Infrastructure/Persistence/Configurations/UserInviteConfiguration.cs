using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class UserInviteConfiguration : IEntityTypeConfiguration<UserInvite>
{
    public void Configure(EntityTypeBuilder<UserInvite> builder)
    {
        builder.ToTable("UserInvites");
        builder.HasKey(invite => invite.Id);

        builder.Property(invite => invite.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(invite => invite.Token)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(invite => invite.InvitedByUserId)
            .IsRequired();

        builder.Property(invite => invite.ExpiresAt)
            .IsRequired();

        builder.Property(invite => invite.ConsumedAt);

        builder.HasOne(invite => invite.InvitedByUser)
            .WithMany()
            .HasForeignKey(invite => invite.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(invite => invite.Token)
            .IsUnique();

        builder.HasIndex(invite => invite.Email);
    }
}
