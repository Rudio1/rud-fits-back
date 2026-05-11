using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using RudFitAI.Domain.Common;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence;

public sealed class RudFitAIDbContext : DbContext
{
    public RudFitAIDbContext(DbContextOptions<RudFitAIDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RudFitAIDbContext).Assembly);
        ApplyAuditableEntityMapping(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyAuditableEntityMapping(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            Type? clrType = entityType.ClrType;
            if (clrType is null || clrType == typeof(BaseEntity) || !typeof(BaseEntity).IsAssignableFrom(clrType))
            {
                continue;
            }

            modelBuilder.Entity(clrType, builder =>
            {
                builder.Property(nameof(BaseEntity.CreatedAt)).HasColumnName("CreatedAt").IsRequired();
                builder.Property(nameof(BaseEntity.UpdatedAt)).HasColumnName("UpdatedAt").IsRequired();
            });
        }
    }

    private void ApplyAuditTimestamps()
    {
        DateTime utcNow = DateTime.UtcNow;
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;
                default:
                    break;
            }
        }
    }
}
