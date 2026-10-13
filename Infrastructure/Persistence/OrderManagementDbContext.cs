using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the order management store.
/// Maps the Domain entities onto SQL Server tables, applies every
/// <c>IEntityTypeConfiguration&lt;T&gt;</c> declared in this assembly and stamps
/// UTC creation/modification timestamps before each save.
/// </summary>
public class OrderManagementDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderManagementDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options (provider and connection string).</param>
    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the customers of the store.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets the products of the catalog.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets the customer orders.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// Gets the order line items.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Gets the application user accounts used for authentication.
    /// </summary>
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    /// <summary>
    /// Gets the stored refresh token hashes.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Builds the model by applying the base conventions and then every entity type
    /// configuration from this assembly, so all mapping lives next to the persistence code.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderManagementDbContext).Assembly);
    }

    /// <summary>
    /// Stamps <c>CreatedAt</c>/<c>UpdatedAt</c> with the current UTC time and then
    /// persists all pending changes. Entities that already carry an explicit timestamp
    /// keep it; every modified entity gets its <c>UpdatedAt</c> refreshed.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the store.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyUtcTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Walks the change tracker and assigns UTC timestamps:
    /// added entities get <c>CreatedAt</c> (and <c>UpdatedAt</c>) when the value was left
    /// unset, modified entities get a fresh <c>UpdatedAt</c>. Entities without such a
    /// property (for example <see cref="RefreshToken"/>) are skipped.
    /// </summary>
    private void ApplyUtcTimestamps()
    {
        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetTimestampIfPresent(entry, "CreatedAt", now, onlyWhenUnset: true);
                SetTimestampIfPresent(entry, "UpdatedAt", now, onlyWhenUnset: true);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetTimestampIfPresent(entry, "UpdatedAt", now, onlyWhenUnset: false);
            }
        }
    }

    /// <summary>
    /// Assigns <paramref name="value"/> to the given timestamp property when the entity type
    /// declares it, optionally only when the current value is still the type default.
    /// </summary>
    /// <param name="entry">The tracked entity.</param>
    /// <param name="propertyName">The timestamp property name.</param>
    /// <param name="value">The UTC timestamp to assign.</param>
    /// <param name="onlyWhenUnset">Whether an already set value should be preserved.</param>
    private static void SetTimestampIfPresent(EntityEntry entry, string propertyName, DateTime value, bool onlyWhenUnset)
    {
        // Not every entity has both timestamps (users and tokens have no UpdatedAt).
        if (entry.Metadata.FindProperty(propertyName) is null)
        {
            return;
        }

        if (onlyWhenUnset
            && entry.Property(propertyName).CurrentValue is DateTime current
            && current != default)
        {
            return;
        }

        entry.Property(propertyName).CurrentValue = value;
    }
}
