namespace OrderManagement.Domain.Enums;

/// <summary>
/// ثابت‌های نام نقش شناخته‌شده که برای بررسی‌های کنترل دسترسی استفاده می‌شوند.
/// </summary>
public static class Roles
{
    /// <summary>
    /// نام نقش مدیران. مدیران می‌توانند سفارش‌ها را لغو و کاتالوگ محصولات را مدیریت کنند.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// نام نقش کاربران عادی. نقش پیش‌فرض که هنگام ثبت‌نام اختصاص داده می‌شود.
    /// </summary>
    public const string User = "User";
}
