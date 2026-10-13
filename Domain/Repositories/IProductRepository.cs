using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="Product"/> entities.
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    /// <summary>
    /// Gets a product by its unique SKU, or <c>null</c> when no such product exists.
    /// </summary>
    /// <param name="sku">The SKU to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a product with the given SKU already exists.
    /// </summary>
    /// <param name="sku">The SKU to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of products, optionally filtered by a free-text search term (name or SKU)
    /// and by active flag.
    /// </summary>
    /// <param name="searchTerm">Optional search term; <c>null</c> or empty skips the text filter.</param>
    /// <param name="isActive">Optional active flag filter; <c>null</c> returns both active and inactive products.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<PagedEntities<Product>> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the products matching the given identifiers.
    /// </summary>
    /// <param name="ids">The product identifiers to fetch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
