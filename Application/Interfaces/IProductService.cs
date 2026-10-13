using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Products;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Application service contract for product catalog management.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product after verifying the SKU is not already taken.
    /// </summary>
    /// <param name="request">The product creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created product as a DTO.</returns>
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The product as a DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the product does not exist.</exception>
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a filtered and paginated page of products.
    /// </summary>
    /// <param name="filter">The search, active-flag and pagination inputs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of product DTOs.</returns>
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product, keeping the SKU unique. Stock is not changed here;
    /// use <see cref="AdjustStockAsync"/> for stock movements.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated product as a DTO.</returns>
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product after verifying it exists.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the product does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the stock quantity of a product by the given delta (positive to restock,
    /// negative to withdraw). The resulting quantity must not be negative.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="quantityChange">The signed change applied to the stock quantity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated product as a DTO.</returns>
    Task<ProductDto> AdjustStockAsync(Guid productId, int quantityChange, CancellationToken cancellationToken = default);
}
