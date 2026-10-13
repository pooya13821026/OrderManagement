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
    /// کاربر را بر اساس نام کاربری یکتایش برمی‌گرداند، یا اگر چنین کاربری وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="userName">نام کاربری‌ای که باید جست‌وجو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// تعیین می‌کند که آیا کاربری با این ایمیل از قبل وجود دارد یا نه.
    /// </summary>
    /// <param name="email">آدرس ایمیلی که باید بررسی شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
