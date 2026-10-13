using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Aggregates all repositories plus a single SaveChanges and transaction boundary,
/// so a service can coordinate several repositories atomically without knowing EF Core.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for customer entities.
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Gets the repository for product entities.
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Gets the repository for order entities.
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// Gets the repository for order item entities.
    /// </summary>
    IOrderItemRepository OrderItems { get; }

    /// <summary>
    /// Gets the repository for application user entities.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Gets the repository for refresh token entities.
    /// </summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// Persists all pending changes made through the repositories.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction. Must be paired with
    /// <see cref="CommitAsync"/> or <see cref="RollbackAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
