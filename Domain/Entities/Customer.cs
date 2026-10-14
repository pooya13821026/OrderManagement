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
    /// شناسه‌ی حساب کاربری متصل به این پروفایل مشتری، یا <c>null</c> اگر حسابی نداشته باشد.
    /// عمداً nullable است: ردیف‌های قدیمی و داده‌کاشت‌شده پیش از ایجاد این پیوند ساخته شده‌اند
    /// و نباید با مقداردهی اجباری بشکنند؛ فقط ثبت‌نام‌های تازه و اتصال seed آن را پر می‌کنند.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// حساب کاربری متصل به این پروفایل مشتری، یا <c>null</c> اگر پیوندی وجود نداشته باشد.
    /// </summary>
    public ApplicationUser? User { get; set; }

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
