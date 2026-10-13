namespace OrderManagement.Application.Dtos.Customers;

/// <summary>
/// نمای عمومی یک مشتری.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// شناسه‌ی یکتای مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل یکتا را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// آدرس پستی اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
