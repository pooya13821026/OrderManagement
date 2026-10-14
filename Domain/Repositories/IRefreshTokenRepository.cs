using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="RefreshToken"/>.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>
    /// توکن refresh ذخیره‌شده را با هش توکن ارائه‌شده برمی‌گرداند،
    /// یا اگر چنین توکنی وجود نداشته باشد <c>null</c>.
    /// </summary>
    /// <param name="tokenHash">هش توکن refresh.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// همه‌ی توکن‌های refresh ذخیره‌شده‌ی کاربر داده‌شده را باطل می‌کند.
    /// </summary>
    /// <param name="userId">شناسه‌ی کاربر.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
