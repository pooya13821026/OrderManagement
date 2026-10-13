using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IProductRepository"/>. Display queries are not
/// tracked; <see cref="GetByIdsAsync"/> intentionally returns tracked entities because its
/// results feed order creation, which attaches a graph referencing those products.
/// </summary>
public class ProductRepository : EfRepository<Product, Guid>, IProductRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public ProductRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return null;
        }

        return await Context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Sku == sku, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        return await Context.Products
            .AnyAsync(product => product.Sku == sku, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Product>> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = Context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(product => product.Name.Contains(term) || product.Sku.Contains(term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(product => product.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        IReadOnlyList<Product> items = await query
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Sku)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Product>(items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        List<Guid> productIds = ids.Distinct().ToList();
        if (productIds.Count == 0)
        {
            return Array.Empty<Product>();
        }

        // Stock checks and order creation mutate/save these products later, so they must
        // be tracked: attaching a new order graph would otherwise treat the reachable
        // (detached) products as brand-new entities and try to insert them again.
        return await Context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }
}
