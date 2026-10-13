using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOrderRepository"/>: detail loading with includes,
/// existence checks and the filtered/sorted/paged list query. Display queries use
/// <c>AsNoTracking</c>.
/// </summary>
public class OrderRepository : EfRepository<Order, Guid>, IOrderRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public OrderRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Tracked on purpose: besides the detail view this lookup feeds the status
        // transition, which mutates and saves the returned graph - loading it tracked
        // keeps identity resolution intact instead of attaching a second instance.
        return await Context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return null;
        }

        return await Context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Order>> GetPagedAsync(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = Context.Orders.AsNoTracking();

        if (customerId.HasValue)
        {
            query = query.Where(order => order.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        // Inclusive bounds: an order placed exactly on fromDate/toDate is included.
        if (fromDate.HasValue)
        {
            query = query.Where(order => order.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            // A bound without a time component (a pure date such as 2026-10-01) covers
            // the whole day, so "to 2026-10-01" includes orders placed during that day.
            // Timestamps with a time component are applied exactly as given.
            DateTime to = toDate.Value;
            DateTime upperBound = to == to.Date
                ? to.Date.AddDays(1).AddTicks(-1)
                : to;
            query = query.Where(order => order.OrderDate <= upperBound);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(term)
                || (order.ShippingAddress != null && order.ShippingAddress.Contains(term))
                || (order.Notes != null && order.Notes.Contains(term)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sortBy, sortDescending);

        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        IReadOnlyList<Order> items = await query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Order>(items, totalCount);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return false;
        }

        return await Context.Orders
            .AnyAsync(order => order.OrderNumber == orderNumber, cancellationToken);
    }

    /// <summary>
    /// Applies a whitelisted sort column to the order query. Only <c>orderdate</c>,
    /// <c>totalamount</c>, <c>status</c> and <c>ordernumber</c> are accepted; anything
    /// else (including <c>null</c>) falls back to the default sort by order date, so a
    /// client-supplied value can never influence the query shape.
    /// </summary>
    /// <param name="query">The filtered query.</param>
    /// <param name="sortBy">The requested sort column name.</param>
    /// <param name="descending">Whether to sort descending instead of ascending.</param>
    /// <returns>The ordered query.</returns>
    private static IOrderedQueryable<Order> ApplySort(IQueryable<Order> query, string? sortBy, bool descending)
    {
        string key = sortBy?.Trim().ToLowerInvariant() ?? string.Empty;

        return key switch
        {
            "totalamount" => descending
                ? query.OrderByDescending(order => order.TotalAmount)
                : query.OrderBy(order => order.TotalAmount),
            "status" => descending
                ? query.OrderByDescending(order => order.Status)
                : query.OrderBy(order => order.Status),
            "ordernumber" => descending
                ? query.OrderByDescending(order => order.OrderNumber)
                : query.OrderBy(order => order.OrderNumber),
            "orderdate" => descending
                ? query.OrderByDescending(order => order.OrderDate)
                : query.OrderBy(order => order.OrderDate),
            // Unknown or missing sort column: default to order date.
            _ => descending
                ? query.OrderByDescending(order => order.OrderDate)
                : query.OrderBy(order => order.OrderDate)
        };
    }
}
