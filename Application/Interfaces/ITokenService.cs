using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد صدور توکن دسترسی JWT و کمک‌کننده‌های توکن refresh.
/// پیاده‌سازی مشخص JWT در لایه‌ی Infrastructure قرار دارد.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// طول عمر توکن دسترسی بر حسب دقیقه را برمی‌گرداند (همان مقداری که هنگام صدور
    /// داخل خود توکن می‌نشیند). سرویس احراز هویت از آن برای فیلد <c>ExpiresAt</c>
    /// پاسخ استفاده می‌کند تا ساعت اعلام‌شده با ساعت واقعی توکن یکی باشد.
    /// </summary>
    int AccessTokenLifetimeMinutes { get; }

    /// <summary>
    /// توکن دسترسی JWT امضاشده‌ای برای کاربر داده‌شده تولید می‌کند.
    /// </summary>
    /// <param name="user">کاربری که توکن برای او صادر می‌شود.</param>
    /// <returns>JWT امضاشده به‌صورت رشته.</returns>
    string GenerateAccessToken(ApplicationUser user);

    /// <summary>
    /// توکن refresh خام با تصادف رمزنگاری‌شده تولید می‌کند. فقط هش آن ذخیره می‌شود.
    /// </summary>
    /// <returns>توکن refresh خام.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// هش یک توکن refresh خام را برای ذخیره‌سازی و جست‌وجو محاسبه می‌کند.
    /// </summary>
    /// <param name="token">توکن refresh خام.</param>
    /// <returns>هش توکن.</returns>
    string HashRefreshToken(string token);
}
