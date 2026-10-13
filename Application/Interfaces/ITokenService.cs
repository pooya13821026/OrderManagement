using System.Security.Claims;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد صدور توکن دسترسی JWT و کمک‌کننده‌های توکن refresh.
/// پیاده‌سازی مشخص JWT در لایه‌ی Infrastructure قرار دارد.
/// </summary>
public interface ITokenService
{
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

    /// <summary>
    /// <see cref="ClaimsPrincipal"/> را از یک توکن دسترسی منقضی‌شده می‌سازد
    /// بدون اعتبارسنجی طول عمر آن، تا بتوان از آن برای پیدا کردن
    /// کاربر هنگام refresh استفاده کرد.
    /// </summary>
    /// <param name="token">توکن دسترسی منقضی‌شده.</param>
    /// <returns><c>ClaimsPrincipal</c> استخراج‌شده از توکن.</returns>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
