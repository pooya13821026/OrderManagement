namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک محصول قابل فروش است. کد SKU در میان همه‌ی محصولات یکتاست.
/// </summary>
public class Product
{
    /// <summary>
    /// شناسه‌ی یکتای محصول.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام نمایشی محصول.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات بلند اختیاری محصول.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// قیمت واحد محصول به واحد پول فروشگاه.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// تعداد واحدهای موجود در انبار.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// کد نگه‌داری انبار (SKU). یکتاست و برای جست‌وجوی محصول و بررسی موارد تکراری استفاده می‌شود.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// شناسه‌ی اختیاری دسته‌بندی محصول. <c>null</c> یعنی محصول هنوز دسته‌بندی نشده است.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// دسته‌بندی اختیاری که محصول به آن تعلق دارد.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// نشان می‌دهد محصول فعال است و می‌توان آن را سفارش داد. مقدار پیش‌فرض <c>true</c> است.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد رکورد محصول.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی رکورد محصول.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
