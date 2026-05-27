using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Options;
using RudFitAI.Application.Time;
using RudFitAI.Domain.Common;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence;

public sealed class RudFitAIDbContext : DbContext
{
    private readonly PersistenceOptions _persistenceOptions;

    public RudFitAIDbContext(
        DbContextOptions<RudFitAIDbContext> options,
        IOptions<PersistenceOptions> persistenceOptions)
        : base(options)
    {
        _persistenceOptions = persistenceOptions.Value;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Food> Foods => Set<Food>();

    public DbSet<MealLog> MealLogs => Set<MealLog>();

    public DbSet<MealLogItem> MealLogItems => Set<MealLogItem>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();

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
        DateTime auditNow = PersistenceClock.GetWallClockNow(_persistenceOptions);
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = auditNow;
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = auditNow;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(BaseEntity.UpdatedAt)).CurrentValue = auditNow;
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
