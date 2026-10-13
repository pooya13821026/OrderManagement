using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="Customer"/>.
/// </summary>
public interface ICustomerRepository : IRepository<Customer, Guid>
{
    /// <summary>
    /// مشتری را بر اساس ایمیل یکتایش برمی‌گرداند، یا اگر چنین مشتری‌ای وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="email">آدرس ایمیلی که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا مشتری‌ای با این ایمیل از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="email">آدرس ایمیلی که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای از مشتریان را برمی‌گرداند، به‌صورت اختیاری با فیلتر جست‌وجوی متنی آزاد
    /// که روی نام و ایمیل تطبیق داده می‌شود.
    /// </summary>
    /// <param name="searchTerm">عبارت جست‌وجوی اختیاری؛ اگر <c>null</c> یا خالی باشد همه‌ی مشتریان برمی‌گردند.</param>
    /// <param name="pageNumber">شماره‌ی صفحه که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">تعداد اقلام در هر صفحه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<PagedEntities<Customer>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
