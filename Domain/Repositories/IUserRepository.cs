using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="ApplicationUser"/>.
/// </summary>
public interface IUserRepository : IRepository<ApplicationUser, Guid>
{
    /// <summary>
    /// کاربر را بر اساس ایمیل یکتایش برمی‌گرداند، یا اگر چنین کاربری وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="email">آدرس ایمیلی که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا کاربری با این ایمیل از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="email">آدرس ایمیلی که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// صفحه‌ای از کاربران را برمی‌گرداند، به‌صورت اختیاری با فیلتر جست‌وجوی متنی آزاد
    /// که روی نام کاربری، ایمیل و نام/نام خانواده تطبیق داده می‌شود (فهرست مدیریت کاربران).
    /// </summary>
    /// <param name="searchTerm">عبارت جست‌وجوی اختیاری؛ اگر <c>null</c> یا خالی باشد همه‌ی کاربران برمی‌گردند.</param>
    /// <param name="pageNumber">شماره‌ی صفحه که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">تعداد اقلام در هر صفحه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<PagedEntities<ApplicationUser>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
