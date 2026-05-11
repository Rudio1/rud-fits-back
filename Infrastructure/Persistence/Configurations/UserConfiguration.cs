using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Name)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(user => user.Email)
            .HasMaxLength(200)
            .IsRequired();
        builder.HasIndex(user => user.Email)
            .IsUnique();
        builder.Property(user => user.Username)
            .HasMaxLength(50);
        builder.HasIndex(user => user.Username)
            .IsUnique()
            .HasFilter("[Username] IS NOT NULL");
        builder.Property(user => user.ProfileImageUrl)
            .HasMaxLength(500);
        builder.Property(user => user.IsActive)
            .IsRequired();
    }
}
