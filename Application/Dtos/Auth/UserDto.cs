namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// نمای عمومی یک حساب کاربری. هرگز هش گذرواژه را افشا نمی‌کند.
/// </summary>
public class UserDto
{
    /// <summary>
    /// شناسه‌ی یکتای کاربر را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام کاربری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// نام را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد حساب فعال است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// نام نقش کاربر را می‌گیرد یا تنظیم می‌کند (برای نمونه <c>User</c> یا <c>Admin</c>).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد حساب را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین ورود موفق، در صورت وجود، را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// شناسه‌ی پروفایل مشتری متصل به این حساب، یا <c>null</c> اگر حساب پروفایلی نداشته باشد
    /// (کاربران بدون پروفایل یا ردیف‌های قدیمی). لایه‌ی API همین مقدار را برای ایجاد سفارش
    /// به کاربر برمی‌گرداند.
    /// </summary>
    public Guid? CustomerId { get; set; }
}
