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
/// پیاده‌سازی مدیریت کاتالوگ محصولات: ایجاد با یکتایی SKU، بازیابی صفحه‌بندی‌شده،
/// به‌روزرسانی، حذف و تنظیم موجودی. همه‌ی ذخیره‌سازی از طریق <see cref="IUnitOfWork"/> انجام می‌شود.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="ProductService"/> می‌سازد.
    /// </summary>
    /// <param name="unitOfWork">واحد کاری که برای همه‌ی ذخیره‌سازی‌ها استفاده می‌شود.</param>
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

        // نگهبان تکراری به‌صورت best-effort؛ ایندکس یکتای دیتابیس همچنان حافظ
        // مقتدرانه در برابر درج‌های همزمان است.
        if (await _unitOfWork.Products.ExistsBySkuAsync(normalizedSku, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Product), $"محصولی با SKU '{normalizedSku}' از قبل وجود دارد.");
        }

        if (request.Price <= 0)
        {
            throw new DomainException("قیمت محصول باید بزرگ‌تر از صفر باشد.");
        }

        if (request.StockQuantity < 0)
        {
            throw new DomainException("موجودی محصول نباید منفی باشد.");
        }

        Guid? categoryId = await ResolveCategoryIdAsync(request.CategoryId, cancellationToken);

        DateTime now = DateTime.UtcNow;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Sku = normalizedSku,
            CategoryId = categoryId,
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
            filter.CategoryId,
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
            throw new DomainException("قیمت محصول باید بزرگ‌تر از صفر باشد.");
        }

        Guid? categoryId = await ResolveCategoryIdAsync(request.CategoryId, cancellationToken);

        string normalizedSku = request.Sku.Trim();
        if (!string.Equals(product.Sku, normalizedSku, StringComparison.OrdinalIgnoreCase)
            && await _unitOfWork.Products.ExistsBySkuAsync(normalizedSku, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(Product), $"محصولی با SKU '{normalizedSku}' از قبل وجود دارد.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Sku = normalizedSku;
        product.CategoryId = categoryId;
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
            throw new DomainException("تغییر تعداد نباید صفر باشد.");
        }

        Product product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), productId);

        int resultingQuantity = product.StockQuantity + quantityChange;
        if (resultingQuantity < 0)
        {
            // quantityChange در اینجا منفی است، پس برداشت مطلق -quantityChange است.
            throw new InsufficientStockException(productId, product.StockQuantity, -quantityChange);
        }

        product.StockQuantity = resultingQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    /// <summary>
    /// شناسه‌ی دسته‌بندی درخواستی را اعتبارسنجی می‌کند: <c>null</c> بدون دسته معتبر است؛
    /// اگر مقدار داده شده باشد باید دسته‌ای با همان شناسه وجود داشته باشد، وگرنه
    /// <see cref="EntityNotFoundException"/> پرتاب می‌شود.
    /// </summary>
    /// <param name="categoryId">شناسه‌ی اختیاری دسته‌بندی درخواستی.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>شناسه‌ی معتبر دسته، یا <c>null</c> وقتی بدون دسته باشد.</returns>
    /// <exception cref="EntityNotFoundException">وقتی دسته‌ای با این شناسه وجود نداشته باشد پرتاب می‌شود.</exception>
    private async Task<Guid?> ResolveCategoryIdAsync(Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return null;
        }

        _ = await _unitOfWork.Categories.GetByIdAsync(categoryId.Value, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Category), categoryId.Value);

        return categoryId;
    }
}
