namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک حساب کاربردی برای احراز هویت و کنترل دسترسی است.
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// شناسه‌ی یکتای حساب کاربری.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام ورود یکتای کاربر.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل کاربر. در میان همه‌ی حساب‌ها یکتاست.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// گذرواژه‌ی هش‌شده‌ی کاربر. هرگز گذرواژه‌ی ساده (متنی) را ذخیره یا افشا نکنید.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// نام کاربر.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی کاربر.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری کاربر.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// نشان می‌دهد حساب فعال است و امکان ورود دارد. مقدار پیش‌فرض <c>true</c> است.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// نقش کاربر (برای نمونه <c>User</c> یا <c>Admin</c>). مقدار پیش‌فرض <c>User</c> است.
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد حساب.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین ورود موفق، یا <c>null</c> اگر کاربر هرگز وارد نشده باشد.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// مهر امنیتی که برای باطل کردن توکن‌ها هنگام تغییر حساب استفاده می‌شود.
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
