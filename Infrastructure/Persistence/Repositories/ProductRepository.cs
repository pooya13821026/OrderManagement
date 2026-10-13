using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="IProductRepository"/>. پرس‌وجوهای نمایشی ردیابی
/// نمی‌شوند؛ <see cref="GetByIdsAsync"/> عمداً موجودیت‌های tracked برمی‌گرداند چون نتایج
/// آن ایجاد سفارش را تغذیه می‌کند که گرافی با ارجاع به همان محصولات متصل می‌کند.
/// </summary>
public class ProductRepository : EfRepository<Product, Guid>, IProductRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="ProductRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
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
        Guid? categoryId,
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

        if (categoryId.HasValue)
        {
            // فقط محصولات همین دسته (نه محصولات بدون دسته)؛ null CategoryId با null مقایسه نمی‌شود
            // چون == روی Guid? در LINQ-to-Entities به IS NULL تبدیل نمی‌شود.
            query = query.Where(product => product.CategoryId == categoryId.Value);
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

        // بررسی موجودی و ایجاد سفارش بعداً این محصولات را تغییر و ذخیره می‌کنند، پس باید
        // tracked باشند: در غیر این صورت متصل کردن گراف سفارش جدید، محصولات قابل‌دسترس
        // (detached) را موجودیت‌های کاملاً جدید فرض می‌کند و سعی می‌کند دوباره درجشان کند.
        return await Context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }
}
