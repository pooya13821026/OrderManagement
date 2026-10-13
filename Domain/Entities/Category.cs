namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک دسته‌بندی محصول در کاتالوگ است. نام دسته در میان همه‌ی دسته‌ها یکتاست.
/// محصولات می‌توانند اختیاری به یک دسته متصل باشند.
/// </summary>
public class Category
{
    /// <summary>
    /// شناسه‌ی یکتای دسته.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام نمایشی دسته. یکتاست و برای جست‌وجو و بررسی موارد تکراری نیز استفاده می‌شود.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات بلند اختیاری دسته.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// نشان می‌دهد دسته فعال است و محصولات می‌توانند به آن متصل شوند. مقدار پیش‌فرض <c>true</c> است.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد رکورد دسته.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی رکورد دسته.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// مجموعه‌ی محصولاتی که به این دسته متصل‌اند. اتصال اختیاری است؛ محصول بدون دسته هم معتبر است.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
