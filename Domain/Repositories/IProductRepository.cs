using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="Product"/>.
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    /// <summary>
    /// محصول را بر اساس کد SKU یکتایش برمی‌گرداند، یا اگر چنین محصولی وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="sku">کد SKUای که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا محصولی با این کد SKU از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="sku">کد SKUای که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای از محصولات را برمی‌گرداند، به‌صورت اختیاری با فیلتر جست‌وجوی متنی آزاد (نام یا SKU)،
    /// فیلتر پرچم فعال بودن و فیلتر دسته‌بندی.
    /// </summary>
    /// <param name="searchTerm">عبارت جست‌وجوی اختیاری؛ <c>null</c> یا خالی فیلتر متنی را رد می‌کند.</param>
    /// <param name="isActive">فیلتر اختیاری پرچم فعال بودن؛ <c>null</c> هم محصولات فعال و هم غیرفعال را برمی‌گرداند.</param>
    /// <param name="categoryId">فیلتر اختیاری شناسه‌ی دسته‌بندی؛ <c>null</c> محصولات همه‌ی دسته‌ها (شامل بدون دسته) را برمی‌گرداند.</param>
    /// <param name="pageNumber">شماره‌ی صفحه که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">تعداد اقلام در هر صفحه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<PagedEntities<Product>> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        Guid? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// محصولات منطبق با شناسه‌های داده‌شده را برمی‌گرداند.
    /// </summary>
    /// <param name="ids">شناسه‌های محصولی که باید واکشی شوند.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
