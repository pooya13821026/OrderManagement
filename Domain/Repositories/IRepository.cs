namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Generic repository contract providing basic CRUD operations for an entity type.
/// Implementations live in the Infrastructure layer.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Gets an entity by its identifier, or <c>null</c> when it does not exist.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities of the type.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the unit of work. The change is not persisted until save is called.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a range of new entities to the unit of work. The changes are not persisted until save is called.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Marks an existing entity for deletion.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// Marks a range of existing entities for deletion.
    /// </summary>
    /// <param name="entities">The entities to remove.</param>
    void RemoveRange(IEnumerable<TEntity> entities);
}
