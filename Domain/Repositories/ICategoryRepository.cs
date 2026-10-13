using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="Category"/>.
/// </summary>
public interface ICategoryRepository : IRepository<Category, Guid>
{
    /// <summary>
    /// دسته را بر اساس نام یکتایش برمی‌گرداند، یا اگر چنین دسته‌ای وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="name">نامی که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا دسته‌ای با این نام از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="name">نامی که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای از دسته‌ها را برمی‌گرداند، به‌صورت اختیاری با فیلتر جست‌وجوی متنی آزاد
    /// که روی نام و توضیحات تطبیق داده می‌شود و فیلتر پرچم فعال بودن.
    /// </summary>
    /// <param name="searchTerm">عبارت جست‌وجوی اختیاری؛ اگر <c>null</c> یا خالی باشد همه‌ی دسته‌ها برمی‌گردند.</param>
    /// <param name="isActive">فیلتر اختیاری پرچم فعال بودن؛ <c>null</c> هم دسته‌های فعال و هم غیرفعال را برمی‌گرداند.</param>
    /// <param name="pageNumber">شماره‌ی صفحه که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">تعداد اقلام در هر صفحه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<PagedEntities<Category>> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
