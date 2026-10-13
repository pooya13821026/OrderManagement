using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICustomerRepository"/>. All queries that feed
/// display code run without change tracking (<c>AsNoTracking</c>).
/// </summary>
public class CustomerRepository : EfRepository<Customer, Guid>, ICustomerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public CustomerRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Defensive: an empty lookup value can never match a stored address, so skip the query.
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await Context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await Context.Customers
            .AnyAsync(customer => customer.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Customer>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = Context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(customer =>
                customer.FirstName.Contains(term)
                || customer.LastName.Contains(term)
                || customer.Email.Contains(term));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // Clamp the paging inputs so a caller mistake can never produce a negative Skip/Take.
        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        // A deterministic sort (name, then the unique email as tie breaker) keeps pages stable
        // while paging - otherwise rows could appear or vanish between page requests.
        IReadOnlyList<Customer> items = await query
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ThenBy(customer => customer.Email)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Customer>(items, totalCount);
    }
}
