namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// نتیجه‌ی یک عملیات احراز هویت موفق (ثبت‌نام، ورود یا refresh).
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// توکن دسترسی JWT امضاشده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// توکن refresh خام را می‌گیرد یا تنظیم می‌کند. فقط هش آن در سمت سرور ذخیره می‌شود.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// زمان انقضای UTC توکن دسترسی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// اطلاعات پروفایل کاربر احراز هویت‌شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public UserDto User { get; set; } = null!;
}
