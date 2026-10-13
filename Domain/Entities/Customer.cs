namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک مشتری فروشگاه است. ایمیل در میان همه‌ی مشتریان یکتاست.
/// </summary>
public class Customer
{
    /// <summary>
    /// شناسه‌ی یکتای مشتری.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام مشتری.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی مشتری.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل مشتری. یکتاست و برای جست‌وجو و بررسی موارد تکراری نیز استفاده می‌شود.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// شماره تلفن اختیاری مشتری.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// آدرس پستی اختیاری مشتری.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد رکورد مشتری.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی رکورد مشتری.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
