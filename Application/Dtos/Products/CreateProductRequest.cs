using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد یک محصول جدید.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// نام نمایشی محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات اختیاری محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// قیمت واحد محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// موجودی اولیه‌ی محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>
    /// کد SKU یکتای محصول را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// شناسه‌ی اختیاری دسته‌بندی محصول را می‌گیرد یا تنظیم می‌کند؛ <c>null</c> یعنی بدون دسته.
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد محصول فعال است را می‌گیرد یا تنظیم می‌کند. مقدار پیش‌فرض <c>true</c> است.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
