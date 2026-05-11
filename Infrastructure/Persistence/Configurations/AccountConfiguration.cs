using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.PasswordHash)
            .IsRequired();
        builder.Property(account => account.EmailVerified)
            .IsRequired();
        builder.Property(account => account.LastLoginAt);
        builder.Property(account => account.RefreshTokenExpiresAt);
        builder.Property(account => account.LoginProvider)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(account => account.IsTwoFactorEnabled)
            .IsRequired();

        builder.HasOne(account => account.User)
            .WithOne(user => user.Account)
            .HasForeignKey<Account>(account => account.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(account => account.UserId)
            .IsUnique();
    }
}
