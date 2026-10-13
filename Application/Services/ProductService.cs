using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Services;

/// <summary>
/// Implements product catalog management: creation with SKU uniqueness, paged retrieval,
/// update, delete and stock adjustment. All persistence goes through <see cref="IUnitOfWork"/>.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used for all persistence.</param>
    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Sku, nameof(request.Sku));

        string normalizedSku = request.Sku.Trim();

        // Best-effort duplicate guard; the unique index in the database remains the
        // authoritative protection against concurrent inserts.
        if (await _unitOfWork.Products.ExistsBySkuAsync(normalizedSku, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Product), $"A product with SKU '{normalizedSku}' already exists.");
        }

        if (request.Price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        if (request.StockQuantity < 0)
        {
            throw new DomainException("Product stock quantity must not be negative.");
        }

        DateTime now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Sku = normalizedSku,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    /// <inheritdoc />
    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Product product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), id);

        return product.ToDto();
    }

    /// <inheritdoc />
    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterParams filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        filter.Normalize();

        PagedEntities<Product> page = await _unitOfWork.Products.GetPagedAsync(
            string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(),
            filter.IsActive,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        IReadOnlyList<ProductDto> items = page.Items.Select(product => product.ToDto()).ToList();
        return new PagedResult<ProductDto>(items, filter.PageNumber, filter.PageSize, page.TotalCount);
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Sku, nameof(request.Sku));

        Product product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), id);

        if (request.Price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        string normalizedSku = request.Sku.Trim();
        if (!string.Equals(product.Sku, normalizedSku, StringComparison.OrdinalIgnoreCase)
            && await _unitOfWork.Products.ExistsBySkuAsync(normalizedSku, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Product), $"A product with SKU '{normalizedSku}' already exists.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Sku = normalizedSku;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(id, nameof(id));

        Product product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), id);

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductDto> AdjustStockAsync(Guid productId, int quantityChange, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(productId, nameof(productId));

        if (quantityChange == 0)
        {
            throw new DomainException("Quantity change must not be zero.");
        }

        Product product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), productId);

        int resultingQuantity = product.StockQuantity + quantityChange;
        if (resultingQuantity < 0)
        {
            // quantityChange is negative here, so the absolute withdrawal is -quantityChange.
            throw new InsufficientStockException(productId, product.StockQuantity, -quantityChange);
        }

        product.StockQuantity = resultingQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }
}
