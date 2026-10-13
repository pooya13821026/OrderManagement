using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="Order"/> entities.
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
    /// <summary>
    /// Gets an order by identifier including the customer and all items with their products,
    /// or <c>null</c> when no such order exists.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by its unique order number, or <c>null</c> when no such order exists.
    /// </summary>
    /// <param name="orderNumber">The order number to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of orders with optional filters for customer, status, date range and a free-text
    /// search term matched against order number and shipping address, with configurable sorting.
    /// </summary>
    /// <param name="customerId">Optional customer identifier filter.</param>
    /// <param name="status">Optional order status filter.</param>
    /// <param name="fromDate">Optional inclusive lower bound on the order date.</param>
    /// <param name="toDate">Optional inclusive upper bound on the order date.</param>
    /// <param name="searchTerm">Optional free-text search term; <c>null</c> or empty skips the text filter.</param>
    /// <param name="sortBy">Optional sort column name; <c>null</c> falls back to the default sort (order date).</param>
    /// <param name="sortDescending">Whether to sort descending instead of ascending.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<PagedEntities<Order>> GetPagedAsync(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether an order with the given order number already exists.
    /// </summary>
    /// <param name="orderNumber">The order number to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}
