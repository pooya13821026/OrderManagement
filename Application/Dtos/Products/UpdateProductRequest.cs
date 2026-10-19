using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// بدنه‌ی درخواست برای به‌روزرسانی یک محصول موجود. تغییرات موجودی از طریق
/// عملیات جداگانه‌ی تنظیم موجودی انجام می‌شود، نه از طریق این درخواست.
/// </summary>
public class UpdateProductRequest
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
    /// مقداری که نشان می‌دهد محصول فعال است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
