using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// Aggregates the scoped repositories plus one shared <see cref="OrderManagementDbContext"/>
/// and the database transaction boundary, so application services can coordinate several
/// repositories atomically. Registered as scoped; the context instance is the same one used
/// by every repository of the scope.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderManagementDbContext _context;
    private readonly ICustomerRepository _customers;
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IOrderItemRepository _orderItems;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;

    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    /// <param name="customers">The customer repository.</param>
    /// <param name="products">The product repository.</param>
    /// <param name="orders">The order repository.</param>
    /// <param name="orderItems">The order item repository.</param>
    /// <param name="users">The user repository.</param>
    /// <param name="refreshTokens">The refresh token repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
    public UnitOfWork(
        OrderManagementDbContext context,
        ICustomerRepository customers,
        IProductRepository products,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _customers = customers ?? throw new ArgumentNullException(nameof(customers));
        _products = products ?? throw new ArgumentNullException(nameof(products));
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _orderItems = orderItems ?? throw new ArgumentNullException(nameof(orderItems));
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _refreshTokens = refreshTokens ?? throw new ArgumentNullException(nameof(refreshTokens));
    }

    /// <inheritdoc />
    public ICustomerRepository Customers => _customers;

    /// <inheritdoc />
    public IProductRepository Products => _products;

    /// <inheritdoc />
    public IOrderRepository Orders => _orders;

    /// <inheritdoc />
    public IOrderItemRepository OrderItems => _orderItems;

    /// <inheritdoc />
    public IUserRepository Users => _users;

    /// <inheritdoc />
    public IRefreshTokenRepository RefreshTokens => _refreshTokens;

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when a transaction is already active.</exception>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("A database transaction is already active.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when no transaction is active.</exception>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No active database transaction to commit.");
        }

        IDbContextTransaction transaction = _transaction;
        _transaction = null;

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            // Disposing a transaction that failed to commit rolls it back on the server.
            await transaction.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // Deliberately a no-op when no transaction is active: callers roll back from a
        // catch block, and throwing here would mask the original failure (for example
        // when the commit itself already disposed the transaction).
        if (_transaction is null)
        {
            return;
        }

        IDbContextTransaction transaction = _transaction;
        _transaction = null;

        await transaction.RollbackAsync(cancellationToken);
        await transaction.DisposeAsync();
    }

    /// <summary>
    /// Disposes the active transaction (if any) and the underlying database context.
    /// The context is owned by this unit of work's scope; disposing it twice is harmless.
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
