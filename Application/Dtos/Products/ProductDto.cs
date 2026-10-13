namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// نمای عمومی یک محصول.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// شناسه‌ی یکتای محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// نام نمایشی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// قیمت واحد را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// تعداد واحدهای موجود در انبار را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// کد SKU یکتا را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// مقداری که نشان می‌دهد محصول فعال است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
