using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository providing the basic CRUD operations of
/// <see cref="IRepository{TEntity,TKey}"/> on top of the shared scoped
/// <see cref="OrderManagementDbContext"/>.
/// Key lookups (<see cref="GetByIdAsync"/>) return <b>tracked</b> entities because the
/// application services mutate and save them, and because a service may attach a new
/// entity graph that references them (an order references its already loaded customer
/// and products - a detached reference would be re-inserted). List/lookup queries that
/// only feed display code are implemented by the derived repositories with
/// <c>AsNoTracking</c>.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public abstract class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EfRepository{TEntity,TKey}"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    protected EfRepository(OrderManagementDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the database context shared by all repositories of the same scope.
    /// </summary>
    protected OrderManagementDbContext Context { get; }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        // Deliberately tracked (FindAsync): callers either modify + Update() the result or
        // reference it from a new entity graph that is handed to Add().
        return await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Read-only listing for display: no change tracking overhead.
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        Context.Set<TEntity>().RemoveRange(entities);
    }
}
